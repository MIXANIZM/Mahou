using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Mahou.InputSurfaceProbe {
    internal static class WindowsProbe {
        private const int GwlStyle = -16;
        private const long EsPassword = 0x20;
        private const uint ProcessQueryLimitedInformation = 0x1000;
        private const ushort MachineUnknown = 0;
        private const ushort MachineI386 = 0x014c;
        private const ushort MachineAmd64 = 0x8664;
        private const ushort MachineArm64 = 0xAA64;

        [StructLayout(LayoutKind.Sequential)]
        private struct GuiThreadInfo {
            internal int cbSize;
            internal uint flags;
            internal IntPtr hwndActive;
            internal IntPtr hwndFocus;
            internal IntPtr hwndCapture;
            internal IntPtr hwndMenuOwner;
            internal IntPtr hwndMoveSize;
            internal IntPtr hwndCaret;
            internal System.Drawing.Rectangle rcCaret;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool GetGUIThreadInfo(uint threadId, ref GuiThreadInfo info);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr hWnd, StringBuilder value, int capacity);
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern int GetWindowLong32(IntPtr hWnd, int index);
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        private static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int index);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr OpenProcess(uint access, bool inherit, uint processId);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool CloseHandle(IntPtr handle);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool IsWow64Process(IntPtr handle, out bool wow64);
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool IsWow64Process2(IntPtr handle, out ushort processMachine, out ushort nativeMachine);

        internal static ProbeReport Capture(string commit) {
            var report = NewReport(commit);
            var facts = new ProbeFacts();
            var foreground = GetForegroundWindow();
            if (foreground == IntPtr.Zero) {
                report.status = "NO_FOREGROUND_WINDOW";
                report.error_code = "foreground-window-unavailable";
                FinalizeReport(report, facts);
                return report;
            }

            uint processId;
            var threadId = GetWindowThreadProcessId(foreground, out processId);
            var focused = FocusedWindow(threadId, foreground);
            uint focusedProcessId;
            GetWindowThreadProcessId(focused, out focusedProcessId);

            report.window.top_level_hwnd_class = Redaction.SafeIdentifier(WindowClass(foreground));
            report.window.focused_hwnd_class = Redaction.SafeIdentifier(WindowClass(focused));
            report.window.focused_hwnd_available = focused != IntPtr.Zero;
            report.window.focus_belongs_to_foreground_process = focusedProcessId != 0 && focusedProcessId == processId;
            facts.TopLevelClass = report.window.top_level_hwnd_class;
            facts.FocusedClass = report.window.focused_hwnd_class;

            ReadProcess(processId, report.process, facts);
            AccessibilityProbe.ReadMetadata(processId, focused, report, facts);

            var nativePassword = String.Equals(facts.FocusedClass, "Edit", StringComparison.OrdinalIgnoreCase) &&
                                 (WindowStyle(focused) & EsPassword) != 0;
            report.protection.protected_or_password = nativePassword || report.uia.is_password;
            if (nativePassword) report.protection.signals.Add("native-password-style");
            if (report.uia.is_password) report.protection.signals.Add("uia-is-password");
            facts.Protected = report.protection.protected_or_password;

            ReadWord(report.process.filename, report.word_object_model, facts, facts.Protected);
            if (!facts.Protected) {
                AccessibilityProbe.ReadCapabilities(focused, facts.FocusedClass, report, facts);
                MergeWord(report.word_object_model, report.read_capabilities, facts);
            }

            ReportPolicy.ApplyProtection(report);
            FinalizeReport(report, facts);
            return report;
        }

        private static ProbeReport NewReport(string commit) {
            return new ProbeReport {
                schema_version = ProbeConstants.SchemaVersion,
                probe_commit = commit,
                captured_at_utc = DateTime.UtcNow.ToString("o", CultureInfo.InvariantCulture),
                status = "CAPTURED",
                classification = "CUSTOM_UNKNOWN",
                confidence = "low",
                evidence = new List<string>(),
                read_capabilities = new ReadCapabilities { actual_text_collected = false },
                write_capabilities_unverified = new List<string>(),
                limitations = ReportPolicy.DefaultLimitations(),
                process = new ProcessEvidence { signer_validation = "presence-only-not-trust-validated" },
                window = new WindowEvidence(),
                uia = new UiaEvidence { supported_patterns = new List<string>() },
                msaa = new MsaaEvidence(),
                ia2 = new Ia2Evidence(),
                word_object_model = new WordEvidence(),
                protection = new ProtectionEvidence { signals = new List<string>() }
            };
        }

        private static void FinalizeReport(ProbeReport report, ProbeFacts facts) {
            facts.Protected = report.protection.protected_or_password;
            facts.CaretReadable = report.read_capabilities.caret_readable;
            facts.SelectionReadable = report.read_capabilities.selection_readable;
            facts.SelectionCollapsed = report.read_capabilities.selection_collapsed;
            facts.TextLengthReadable = report.read_capabilities.text_length_readable;
            facts.TextLength = report.read_capabilities.text_length;
            facts.TextLengthSource = report.read_capabilities.text_length_source;
            var result = Classifier.Classify(facts);
            report.classification = result.Classification;
            report.confidence = result.Confidence;
            report.evidence = result.Evidence;
            report.write_capabilities_unverified = ReportPolicy.BuildWriteMetadata(facts);
            report.capability_fingerprint = Classifier.Fingerprint(facts, result);
        }

        private static IntPtr FocusedWindow(uint threadId, IntPtr fallback) {
            var info = new GuiThreadInfo { cbSize = Marshal.SizeOf(typeof(GuiThreadInfo)) };
            return threadId != 0 && GetGUIThreadInfo(threadId, ref info) && info.hwndFocus != IntPtr.Zero ? info.hwndFocus : fallback;
        }

        private static string WindowClass(IntPtr window) {
            var value = new StringBuilder(256);
            return window != IntPtr.Zero && GetClassName(window, value, value.Capacity) > 0 ? value.ToString() : null;
        }

        private static long WindowStyle(IntPtr window) {
            if (window == IntPtr.Zero) return 0;
            try { return IntPtr.Size == 8 ? GetWindowLongPtr64(window, GwlStyle).ToInt64() : GetWindowLong32(window, GwlStyle); }
            catch { return 0; }
        }

        private static void ReadProcess(uint processId, ProcessEvidence evidence, ProbeFacts facts) {
            string path = null;
            try {
                using (var process = Process.GetProcessById((int)processId)) {
                    try { path = process.MainModule == null ? null : process.MainModule.FileName; } catch { }
                    evidence.filename = Redaction.SafeIdentifier(String.IsNullOrEmpty(path) ? process.ProcessName + ".exe" : Path.GetFileName(path));
                }
            } catch { }
            evidence.architecture = Architecture(processId);
            facts.ProcessFilename = evidence.filename;
            facts.Architecture = evidence.architecture;
            if (String.IsNullOrEmpty(path)) return;
            try {
                var version = FileVersionInfo.GetVersionInfo(path);
                evidence.product_version = Redaction.SafeVersion(version.ProductVersion);
                evidence.file_version = Redaction.SafeVersion(version.FileVersion);
            } catch { }
            try {
                using (var certificate = new X509Certificate2(X509Certificate.CreateFromSignedFile(path))) {
                    evidence.signer_present = true;
                    evidence.signer_name = Redaction.SafeSignerName(certificate.GetNameInfo(X509NameType.SimpleName, false));
                    evidence.signer_thumbprint = Redaction.SafeIdentifier(certificate.Thumbprint);
                }
            } catch { evidence.signer_present = false; }
        }

        private static string Architecture(uint processId) {
            var handle = OpenProcess(ProcessQueryLimitedInformation, false, processId);
            if (handle == IntPtr.Zero) return "unknown";
            try {
                try {
                    ushort processMachine;
                    ushort nativeMachine;
                    if (IsWow64Process2(handle, out processMachine, out nativeMachine)) {
                        if (processMachine == MachineI386) return "x86";
                        if (processMachine == MachineAmd64) return "x64";
                        if (processMachine == MachineArm64) return "arm64";
                        if (processMachine == MachineUnknown) return MachineName(nativeMachine);
                    }
                } catch (EntryPointNotFoundException) { }
                bool wow64;
                return IsWow64Process(handle, out wow64) ? (wow64 ? "x86" : (Environment.Is64BitOperatingSystem ? "x64" : "x86")) : "unknown";
            } finally { CloseHandle(handle); }
        }

        private static string MachineName(ushort machine) {
            if (machine == MachineI386) return "x86";
            if (machine == MachineAmd64) return "x64";
            if (machine == MachineArm64) return "arm64";
            return "unknown";
        }

        private static void ReadWord(string filename, WordEvidence evidence, ProbeFacts facts, bool protectedControl) {
            if (!String.Equals(filename, "WINWORD.exe", StringComparison.OrdinalIgnoreCase)) return;
            object application = null;
            object selection = null;
            object document = null;
            object content = null;
            try {
                application = Marshal.GetActiveObject("Word.Application");
                dynamic app = application;
                selection = app.Selection;
                document = app.ActiveDocument;
                evidence.available = true;
                facts.WordObjectModelAvailable = true;
                if (protectedControl) return;
                dynamic selected = selection;
                var start = (int)selected.Start;
                var end = (int)selected.End;
                evidence.caret_or_selection_offsets_readable = start >= 0 && end >= start;
                evidence.selection_collapsed = start == end;
                dynamic doc = document;
                content = doc.Content;
                dynamic range = content;
                evidence.text_length = Math.Max(0, (int)range.End - 1);
                evidence.text_length_readable = true;
            } catch (UnauthorizedAccessException) { }
              catch (COMException) { }
              catch (InvalidOperationException) { }
            finally {
                ReleaseCom(content); ReleaseCom(document); ReleaseCom(selection); ReleaseCom(application);
            }
        }

        private static void ReleaseCom(object value) {
            if (value == null || !Marshal.IsComObject(value)) return;
            try { Marshal.ReleaseComObject(value); } catch { }
        }

        private static void MergeWord(WordEvidence word, ReadCapabilities read, ProbeFacts facts) {
            if (word == null || !word.available) return;
            if (word.caret_or_selection_offsets_readable) {
                read.caret_readable = word.selection_collapsed;
                read.selection_readable = true;
                read.selection_collapsed = word.selection_collapsed;
            }
            if (word.text_length_readable) {
                read.text_length_readable = true;
                read.text_length = word.text_length;
                read.text_length_source = "word-object-model-range-count";
            }
            facts.CaretReadable = read.caret_readable;
            facts.SelectionReadable = read.selection_readable;
            facts.SelectionCollapsed = read.selection_collapsed;
            facts.TextLengthReadable = read.text_length_readable;
            facts.TextLength = read.text_length;
            facts.TextLengthSource = read.text_length_source;
        }
    }
}
