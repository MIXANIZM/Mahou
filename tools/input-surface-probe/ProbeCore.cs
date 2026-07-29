using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.IO;
using System.Web.Script.Serialization;

namespace Mahou.InputSurfaceProbe {
    internal static class ProbeConstants {
        internal const string SchemaVersion = "1";
        internal const int MaximumCountedCharacters = 1000000;
    }

    internal sealed class ProbeReport {
        public string schema_version { get; set; }
        public string probe_commit { get; set; }
        public string captured_at_utc { get; set; }
        public string status { get; set; }
        public string classification { get; set; }
        public string confidence { get; set; }
        public List<string> evidence { get; set; }
        public ReadCapabilities read_capabilities { get; set; }
        public List<string> write_capabilities_unverified { get; set; }
        public List<string> limitations { get; set; }
        public string capability_fingerprint { get; set; }
        public ProcessEvidence process { get; set; }
        public WindowEvidence window { get; set; }
        public UiaEvidence uia { get; set; }
        public MsaaEvidence msaa { get; set; }
        public Ia2Evidence ia2 { get; set; }
        public WordEvidence word_object_model { get; set; }
        public ProtectionEvidence protection { get; set; }
        public string error_code { get; set; }
    }

    internal sealed class ProcessEvidence {
        public string filename { get; set; }
        public string architecture { get; set; }
        public string product_version { get; set; }
        public string file_version { get; set; }
        public bool signer_present { get; set; }
        public string signer_name { get; set; }
        public string signer_thumbprint { get; set; }
        public string signer_validation { get; set; }
    }

    internal sealed class WindowEvidence {
        public string top_level_hwnd_class { get; set; }
        public string focused_hwnd_class { get; set; }
        public bool focused_hwnd_available { get; set; }
        public bool focus_belongs_to_foreground_process { get; set; }
    }

    internal sealed class UiaEvidence {
        public bool element_available { get; set; }
        public string control_type { get; set; }
        public string framework_id { get; set; }
        public string class_name { get; set; }
        public string automation_id { get; set; }
        public bool name_present { get; set; }
        public string name_length_bucket { get; set; }
        public bool is_password { get; set; }
        public List<string> supported_patterns { get; set; }
        public bool text_pattern_available { get; set; }
        public bool text_pattern2_available { get; set; }
        public bool value_pattern_available { get; set; }
    }

    internal sealed class MsaaEvidence {
        public bool object_available { get; set; }
        public string role { get; set; }
        public string state { get; set; }
    }

    internal sealed class Ia2Evidence {
        public bool iaccessible2 { get; set; }
        public bool iaccessible_text { get; set; }
        public bool iaccessible_editable_text { get; set; }
    }

    internal sealed class WordEvidence {
        public bool available { get; set; }
        public bool caret_or_selection_offsets_readable { get; set; }
        public bool selection_collapsed { get; set; }
        public bool text_length_readable { get; set; }
        public int? text_length { get; set; }
    }

    internal sealed class ProtectionEvidence {
        public bool protected_or_password { get; set; }
        public bool password_content_suppressed { get; set; }
        public List<string> signals { get; set; }
    }

    internal sealed class ReadCapabilities {
        public bool caret_readable { get; set; }
        public bool selection_readable { get; set; }
        public bool selection_collapsed { get; set; }
        public bool text_length_readable { get; set; }
        public int? text_length { get; set; }
        public string text_length_source { get; set; }
        public bool actual_text_collected { get; set; }
    }

    internal sealed class ProbeFacts {
        internal string ProcessFilename;
        internal string Architecture;
        internal string TopLevelClass;
        internal string FocusedClass;
        internal string UiaControlType;
        internal string UiaFrameworkId;
        internal string UiaClassName;
        internal bool WordObjectModelAvailable;
        internal bool TextPatternAvailable;
        internal bool TextPattern2Available;
        internal bool ValuePatternAvailable;
        internal bool MsaaAvailable;
        internal bool IAccessible2;
        internal bool IAccessibleText;
        internal bool IAccessibleEditableText;
        internal bool Protected;
        internal bool CaretReadable;
        internal bool SelectionReadable;
        internal bool SelectionCollapsed;
        internal bool TextLengthReadable;
        internal int? TextLength;
        internal string TextLengthSource;
    }

    internal sealed class ClassificationResult {
        internal string Classification;
        internal string Confidence;
        internal List<string> Evidence;
    }

    internal static class Redaction {
        internal static string SafeIdentifier(string value) {
            if (String.IsNullOrWhiteSpace(value)) return null;
            value = value.Trim();
            if (value.Length > 160 || LooksSensitive(value)) return RedactedToken(value);
            for (var i = 0; i < value.Length; i++) {
                var c = value[i];
                if (Char.IsLetterOrDigit(c) || c == '.' || c == '_' || c == '-' || c == ':' || c == ' ' || c == '#' || c == '+')
                    continue;
                return RedactedToken(value);
            }
            return value;
        }

        internal static string SafeVersion(string value) {
            if (String.IsNullOrWhiteSpace(value)) return null;
            value = value.Trim();
            if (value.Length > 96 || LooksSensitive(value)) return RedactedToken(value);
            for (var i = 0; i < value.Length; i++) {
                var c = value[i];
                if (Char.IsLetterOrDigit(c) || c == '.' || c == '-' || c == '_' || c == '+' || c == ' ' || c == '(' || c == ')')
                    continue;
                return RedactedToken(value);
            }
            return value;
        }

        internal static string SafeSignerName(string value) {
            if (String.IsNullOrWhiteSpace(value)) return null;
            value = value.Trim();
            if (value.Length > 160 || LooksSensitive(value)) return RedactedToken(value);
            for (var i = 0; i < value.Length; i++) {
                var c = value[i];
                if (Char.IsLetterOrDigit(c) || Char.IsWhiteSpace(c) || c == '.' || c == ',' || c == '-' || c == '_' || c == '+' || c == '(' || c == ')' || c == '&')
                    continue;
                return RedactedToken(value);
            }
            return value;
        }

        internal static string NameLengthBucket(string value) {
            if (String.IsNullOrEmpty(value)) return "0";
            var length = value.Length;
            if (length <= 4) return "1-4";
            if (length <= 16) return "5-16";
            if (length <= 64) return "17-64";
            return "65+";
        }

        internal static bool LooksSensitive(string value) {
            if (String.IsNullOrEmpty(value)) return false;
            var lower = value.ToLowerInvariant();
            return lower.Contains("\\users\\") ||
                   lower.Contains("/users/") ||
                   lower.Contains("/home/") ||
                   lower.Contains(":\\") ||
                   lower.Contains("file://") ||
                   lower.Contains("http://") ||
                   lower.Contains("https://") ||
                   lower.Contains("www.") ||
                   lower.Contains("@") ||
                   lower.Contains("?url=") ||
                   lower.Contains("?q=");
        }

        internal static string RedactedToken(string value) {
            if (value == null) return null;
            return "[redacted:length=" + value.Length.ToString(CultureInfo.InvariantCulture) + ";sha256=" + ShortHash(value) + "]";
        }

        internal static string ShortHash(string value) {
            using (var sha = SHA256.Create()) {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(value ?? String.Empty));
                var builder = new StringBuilder(16);
                for (var i = 0; i < 8; i++) builder.Append(bytes[i].ToString("x2", CultureInfo.InvariantCulture));
                return builder.ToString();
            }
        }
    }

    internal static class Classifier {
        internal static ClassificationResult Classify(ProbeFacts facts) {
            if (facts == null) throw new ArgumentNullException("facts");
            var process = Normalize(facts.ProcessFilename);
            var focusedClass = Normalize(facts.FocusedClass);
            var topClass = Normalize(facts.TopLevelClass);
            var framework = Normalize(facts.UiaFrameworkId);
            var uiaClass = Normalize(facts.UiaClassName);
            var evidence = new List<string>();

            if (process == "winword.exe" && facts.WordObjectModelAvailable) {
                evidence.Add("foreground-process-is-winword");
                evidence.Add("word-object-model-read-probe-available");
                return Result("WORD_OBJECT_MODEL", "high", evidence);
            }

            if (focusedClass == "edit") {
                evidence.Add("focused-hwnd-class-exact-edit");
                return Result("CLASSIC_WIN32_EDIT", "high", evidence);
            }

            if (Contains(focusedClass, "richedit") || Contains(uiaClass, "richedit")) {
                evidence.Add("focused-control-identifies-richedit-family");
                if (facts.TextPatternAvailable) evidence.Add("uia-text-pattern-present");
                return Result("RICHEDIT", facts.TextPatternAvailable ? "high" : "medium", evidence);
            }

            if (framework == "wpf") {
                evidence.Add("uia-framework-id-wpf");
                return Result("WPF", "high", evidence);
            }

            if (framework == "xaml" || framework == "winui" ||
                Contains(topClass, "applicationframewindow") ||
                Contains(topClass, "windows.ui.core.corewindow") ||
                Contains(focusedClass, "desktopchildsitebridge") ||
                Contains(focusedClass, "xaml")) {
                evidence.Add("xaml-or-modern-windows-window-signature");
                if (!String.IsNullOrEmpty(framework)) evidence.Add("uia-framework-id-" + SafeEvidence(framework));
                return Result("WINUI_UWP", "medium", evidence);
            }

            var chromiumProvider = framework == "chrome" ||
                                   Contains(focusedClass, "chrome_renderwidgethosthwnd") ||
                                   Contains(uiaClass, "chrome_renderwidgethosthwnd");
            if (chromiumProvider && IsBrowserProcess(process)) {
                evidence.Add("chromium-accessibility-provider-signature");
                evidence.Add("recognized-browser-process-family");
                return Result("CHROMIUM_BROWSER", "high", evidence);
            }

            if (chromiumProvider || Contains(topClass, "chrome_widgetwin")) {
                evidence.Add("chromium-or-webview-host-signature");
                evidence.Add("application-specific-verification-required");
                return Result("ELECTRON_WEBVIEW", "medium", evidence);
            }

            if (framework == "qt" || Contains(framework, "qt") || Contains(focusedClass, "qt") || Contains(topClass, "qt")) {
                evidence.Add("qt-framework-or-window-signature");
                evidence.Add("custom-control-behavior-unverified");
                return Result("QT_CUSTOM", "medium", evidence);
            }

            evidence.Add("no-recognized-control-family-signature");
            if (facts.TextPatternAvailable) evidence.Add("uia-text-pattern-present-without-family-proof");
            if (facts.IAccessibleText) evidence.Add("iaccessibletext-present-without-family-proof");
            return Result("CUSTOM_UNKNOWN", "low", evidence);
        }

        internal static string Fingerprint(ProbeFacts facts, ClassificationResult result) {
            if (facts == null) throw new ArgumentNullException("facts");
            if (result == null) throw new ArgumentNullException("result");
            var canonical = String.Join("|", new[] {
                ProbeConstants.SchemaVersion,
                Normalize(result.Classification),
                Normalize(facts.ProcessFilename),
                Normalize(facts.Architecture),
                Normalize(facts.TopLevelClass),
                Normalize(facts.FocusedClass),
                Normalize(facts.UiaControlType),
                Normalize(facts.UiaFrameworkId),
                Normalize(facts.UiaClassName),
                Bool(facts.WordObjectModelAvailable),
                Bool(facts.TextPatternAvailable),
                Bool(facts.TextPattern2Available),
                Bool(facts.ValuePatternAvailable),
                Bool(facts.MsaaAvailable),
                Bool(facts.IAccessible2),
                Bool(facts.IAccessibleText),
                Bool(facts.IAccessibleEditableText),
                Bool(facts.Protected),
                Bool(facts.CaretReadable),
                Bool(facts.SelectionReadable),
                Bool(facts.SelectionCollapsed),
                Bool(facts.TextLengthReadable),
                Normalize(facts.TextLengthSource)
            });
            using (var sha = SHA256.Create()) {
                var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(canonical));
                var builder = new StringBuilder(hash.Length * 2);
                foreach (var value in hash) builder.Append(value.ToString("x2", CultureInfo.InvariantCulture));
                return builder.ToString();
            }
        }

        private static ClassificationResult Result(string classification, string confidence, List<string> evidence) {
            return new ClassificationResult {
                Classification = classification,
                Confidence = confidence,
                Evidence = evidence
            };
        }

        private static bool IsBrowserProcess(string process) {
            return process == "chrome.exe" || process == "msedge.exe" || process == "brave.exe" ||
                   process == "vivaldi.exe" || process == "opera.exe" || process == "firefox.exe";
        }

        private static string SafeEvidence(string value) {
            var builder = new StringBuilder();
            foreach (var c in value) {
                if (Char.IsLetterOrDigit(c) || c == '-' || c == '_') builder.Append(c);
            }
            return builder.Length == 0 ? "unknown" : builder.ToString();
        }

        private static bool Contains(string source, string value) {
            return !String.IsNullOrEmpty(source) && source.IndexOf(value, StringComparison.Ordinal) >= 0;
        }

        private static string Normalize(string value) {
            return String.IsNullOrWhiteSpace(value) ? "" : value.Trim().ToLowerInvariant();
        }

        private static string Bool(bool value) {
            return value ? "1" : "0";
        }
    }

    internal static class ReportPolicy {
        internal static void ApplyProtection(ProbeReport report) {
            if (report == null) throw new ArgumentNullException("report");
            if (report.protection == null || !report.protection.protected_or_password) return;

            if (report.read_capabilities == null) report.read_capabilities = new ReadCapabilities();
            report.read_capabilities.caret_readable = false;
            report.read_capabilities.selection_readable = false;
            report.read_capabilities.selection_collapsed = false;
            report.read_capabilities.text_length_readable = false;
            report.read_capabilities.text_length = null;
            report.read_capabilities.text_length_source = "suppressed-protected-control";
            report.read_capabilities.actual_text_collected = false;
            report.protection.password_content_suppressed = true;

            if (report.word_object_model != null) {
                report.word_object_model.caret_or_selection_offsets_readable = false;
                report.word_object_model.selection_collapsed = false;
                report.word_object_model.text_length_readable = false;
                report.word_object_model.text_length = null;
            }
        }

        internal static List<string> BuildWriteMetadata(ProbeFacts facts) {
            var result = new List<string>();
            if (facts == null) return result;
            if (facts.ValuePatternAvailable) result.Add("uia-value-pattern-present-metadata-only");
            if (facts.IAccessibleEditableText) result.Add("iaccessible-editable-text-interface-present-metadata-only");
            if (facts.WordObjectModelAvailable) result.Add("word-object-model-present-metadata-only");
            result.Add("no-write-capability-verified-by-this-probe");
            return result;
        }

        internal static List<string> DefaultLimitations() {
            return new List<string> {
                "read-only-one-shot-observation",
                "classification-is-evidence-not-write-authorization",
                "provider-and-application-versions-can-change-capabilities",
                "interface-presence-does-not-prove-safe-exact-range-writing",
                "no-actual-text-window-title-uia-name-url-or-clipboard-content-collected",
                "no-continuous-monitoring-or-hooks"
            };
        }
    }

    internal static class JsonReportWriter {
        internal static string Serialize(ProbeReport report) {
            if (report == null) throw new ArgumentNullException("report");
            var serializer = new JavaScriptSerializer { MaxJsonLength = 4 * 1024 * 1024 };
            return serializer.Serialize(report);
        }

        internal static void Write(string path, ProbeReport report) {
            if (String.IsNullOrWhiteSpace(path)) throw new ArgumentException("Output path is required.", "path");
            var fullPath = Path.GetFullPath(path);
            var directory = Path.GetDirectoryName(fullPath);
            if (!String.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, Serialize(report) + Environment.NewLine, new UTF8Encoding(false));
        }
    }
}
