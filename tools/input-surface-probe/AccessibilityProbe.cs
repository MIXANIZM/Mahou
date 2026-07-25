using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows.Automation;
using System.Windows.Automation.Text;
using Accessibility;

namespace Mahou.InputSurfaceProbe {
    internal static class AccessibilityProbe {
        private const uint EmGetSel = 0x00B0;
        private const uint WmGetTextLength = 0x000E;
        private const uint SmtoAbortIfHung = 0x0002;
        private const int ObjidClient = unchecked((int)0xFFFFFFFC);
        private const int ChildidSelf = 0;
        private const int TextPattern2Id = 10024;
        private static readonly Guid IidAccessible = new Guid("618736E0-3C3D-11CF-810C-00AA00389B71");
        private static readonly Guid IidAccessible2 = new Guid("E89F726E-C4F4-4C19-BB19-B647D7FA8478");
        private static readonly Guid IidAccessibleText = new Guid("24FD2FFB-3AAD-4A08-8335-A3AD89C0FB4B");
        private static readonly Guid IidAccessibleEditableText = new Guid("A59AA09A-7011-4B65-939D-32B1FB5547E3");

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SendMessageTimeout(IntPtr hWnd, uint message, IntPtr wParam, IntPtr lParam, uint flags, uint timeout, out IntPtr result);
        [DllImport("oleacc.dll")]
        private static extern int AccessibleObjectFromWindow(IntPtr hwnd, uint objectId, ref Guid interfaceId, [MarshalAs(UnmanagedType.Interface)] out object accessibleObject);

        internal static void ReadMetadata(uint processId, IntPtr focused, ProbeReport report, ProbeFacts facts) {
            ReadUia(processId, report.uia, facts);
            ReadMsaa(focused, report.msaa, report.ia2, facts);
        }

        private static void ReadUia(uint processId, UiaEvidence evidence, ProbeFacts facts) {
            try {
                var element = AutomationElement.FocusedElement;
                if (element == null || element.Current.ProcessId != (int)processId) return;
                evidence.element_available = true;
                evidence.control_type = Redaction.SafeIdentifier(element.Current.ControlType == null ? null : element.Current.ControlType.ProgrammaticName);
                evidence.framework_id = Redaction.SafeIdentifier(element.Current.FrameworkId);
                evidence.class_name = Redaction.SafeIdentifier(element.Current.ClassName);
                var automationId = element.Current.AutomationId;
                evidence.automation_id = String.IsNullOrEmpty(automationId) ? null : Redaction.RedactedToken(automationId);
                var name = element.Current.Name;
                evidence.name_present = !String.IsNullOrEmpty(name);
                evidence.name_length_bucket = Redaction.NameLengthBucket(name);
                evidence.is_password = element.Current.IsPassword;
                facts.UiaControlType = evidence.control_type;
                facts.UiaFrameworkId = evidence.framework_id;
                facts.UiaClassName = evidence.class_name;
                foreach (var pattern in element.GetSupportedPatterns()) {
                    var patternName = Redaction.SafeIdentifier(pattern.ProgrammaticName);
                    if (!String.IsNullOrEmpty(patternName)) evidence.supported_patterns.Add(patternName);
                }
                evidence.supported_patterns.Sort(StringComparer.Ordinal);
                object ignored;
                evidence.text_pattern_available = element.TryGetCurrentPattern(TextPattern.Pattern, out ignored);
                evidence.value_pattern_available = element.TryGetCurrentPattern(ValuePattern.Pattern, out ignored);
                var text2 = AutomationPattern.LookupById(TextPattern2Id);
                evidence.text_pattern2_available = text2 != null && element.TryGetCurrentPattern(text2, out ignored);
                facts.TextPatternAvailable = evidence.text_pattern_available;
                facts.TextPattern2Available = evidence.text_pattern2_available;
                facts.ValuePatternAvailable = evidence.value_pattern_available;
            } catch (ElementNotAvailableException) { }
              catch (InvalidOperationException) { }
              catch (UnauthorizedAccessException) { evidence.is_password = true; }
              catch (COMException) { }
        }

        private static void ReadMsaa(IntPtr focused, MsaaEvidence msaa, Ia2Evidence ia2, ProbeFacts facts) {
            object accessibleObject = null;
            try {
                var iid = IidAccessible;
                if (focused == IntPtr.Zero || AccessibleObjectFromWindow(focused, unchecked((uint)ObjidClient), ref iid, out accessibleObject) < 0 || accessibleObject == null) return;
                msaa.object_available = true;
                facts.MsaaAvailable = true;
                var accessible = accessibleObject as IAccessible;
                if (accessible != null) {
                    try { msaa.role = Variant(accessible.get_accRole(ChildidSelf)); } catch { }
                    try { msaa.state = Variant(accessible.get_accState(ChildidSelf)); } catch { }
                }
                ia2.iaccessible2 = HasInterface(accessibleObject, IidAccessible2);
                ia2.iaccessible_text = HasInterface(accessibleObject, IidAccessibleText);
                ia2.iaccessible_editable_text = HasInterface(accessibleObject, IidAccessibleEditableText);
                facts.IAccessible2 = ia2.iaccessible2;
                facts.IAccessibleText = ia2.iaccessible_text;
                facts.IAccessibleEditableText = ia2.iaccessible_editable_text;
            } finally {
                if (accessibleObject != null && Marshal.IsComObject(accessibleObject)) {
                    try { Marshal.ReleaseComObject(accessibleObject); } catch { }
                }
            }
        }

        private static string Variant(object value) {
            if (value == null) return null;
            try { return Convert.ToInt64(value, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture); }
            catch { return "variant-" + value.GetType().Name; }
        }

        private static bool HasInterface(object value, Guid iid) {
            IntPtr unknown = IntPtr.Zero;
            IntPtr queried = IntPtr.Zero;
            try {
                unknown = Marshal.GetIUnknownForObject(value);
                return Marshal.QueryInterface(unknown, ref iid, out queried) == 0 && queried != IntPtr.Zero;
            } catch { return false; }
            finally {
                if (queried != IntPtr.Zero) Marshal.Release(queried);
                if (unknown != IntPtr.Zero) Marshal.Release(unknown);
            }
        }

        internal static void ReadCapabilities(IntPtr focused, string focusedClass, ProbeReport report, ProbeFacts facts) {
            if (!ReadUiaCapabilities(report.uia, report.read_capabilities)) ReadNativeCapabilities(focused, focusedClass, report.read_capabilities);
            facts.CaretReadable = report.read_capabilities.caret_readable;
            facts.SelectionReadable = report.read_capabilities.selection_readable;
            facts.SelectionCollapsed = report.read_capabilities.selection_collapsed;
            facts.TextLengthReadable = report.read_capabilities.text_length_readable;
            facts.TextLength = report.read_capabilities.text_length;
            facts.TextLengthSource = report.read_capabilities.text_length_source;
        }

        private static bool ReadUiaCapabilities(UiaEvidence evidence, ReadCapabilities read) {
            if (evidence == null || !evidence.element_available || !evidence.text_pattern_available || evidence.is_password) return false;
            try {
                var element = AutomationElement.FocusedElement;
                object value;
                if (element == null || !element.TryGetCurrentPattern(TextPattern.Pattern, out value)) return false;
                var pattern = value as TextPattern;
                if (pattern == null) return false;
                var ranges = pattern.GetSelection();
                read.selection_readable = ranges != null;
                read.selection_collapsed = ranges != null && ranges.Length == 1 && IsCollapsed(ranges[0]);
                read.caret_readable = read.selection_collapsed || ReadCaret(element, evidence.text_pattern2_available);
                var length = CountCharacters(pattern.DocumentRange);
                if (length.HasValue) {
                    read.text_length_readable = true;
                    read.text_length = length;
                    read.text_length_source = "uia-text-pattern-range-count";
                }
                return true;
            } catch (ElementNotAvailableException) { return false; }
              catch (InvalidOperationException) { return false; }
              catch (UnauthorizedAccessException) { return false; }
              catch (COMException) { return false; }
        }

        private static bool ReadCaret(AutomationElement element, bool available) {
            if (!available) return false;
            try {
                object value;
                var patternId = AutomationPattern.LookupById(TextPattern2Id);
                if (patternId == null || !element.TryGetCurrentPattern(patternId, out value)) return false;
                dynamic pattern = value;
                bool active;
                TextPatternRange range = pattern.GetCaretRange(out active);
                return active && range != null && IsCollapsed(range);
            } catch { return false; }
        }

        private static bool IsCollapsed(TextPatternRange range) {
            return range != null && range.CompareEndpoints(TextPatternRangeEndpoint.Start, range, TextPatternRangeEndpoint.End) == 0;
        }

        private static int? CountCharacters(TextPatternRange range) {
            if (range == null) return null;
            var counter = range.Clone();
            counter.MoveEndpointByRange(TextPatternRangeEndpoint.End, counter, TextPatternRangeEndpoint.Start);
            var moved = counter.MoveEndpointByUnit(TextPatternRangeEndpoint.End, TextUnit.Character, ProbeConstants.MaximumCountedCharacters + 1);
            return moved <= ProbeConstants.MaximumCountedCharacters ? (int?)moved : null;
        }

        private static void ReadNativeCapabilities(IntPtr focused, string focusedClass, ReadCapabilities read) {
            if (focused == IntPtr.Zero || !(String.Equals(focusedClass, "Edit", StringComparison.OrdinalIgnoreCase) ||
                (!String.IsNullOrEmpty(focusedClass) && focusedClass.IndexOf("RichEdit", StringComparison.OrdinalIgnoreCase) >= 0))) return;
            int start;
            int end;
            if (NativeSelection(focused, out start, out end)) {
                read.selection_readable = true;
                read.selection_collapsed = start == end;
                read.caret_readable = start == end;
            }
            IntPtr length;
            if (SendMessageTimeout(focused, WmGetTextLength, IntPtr.Zero, IntPtr.Zero, SmtoAbortIfHung, 80, out length) != IntPtr.Zero &&
                length.ToInt64() >= 0 && length.ToInt64() <= ProbeConstants.MaximumCountedCharacters) {
                read.text_length_readable = true;
                read.text_length = (int)length.ToInt64();
                read.text_length_source = "bounded-native-length-message";
            }
        }

        private static bool NativeSelection(IntPtr focused, out int start, out int end) {
            start = 0;
            end = 0;
            IntPtr startPointer = IntPtr.Zero;
            IntPtr endPointer = IntPtr.Zero;
            try {
                startPointer = Marshal.AllocHGlobal(sizeof(int));
                endPointer = Marshal.AllocHGlobal(sizeof(int));
                Marshal.WriteInt32(startPointer, 0);
                Marshal.WriteInt32(endPointer, 0);
                IntPtr ignored;
                if (SendMessageTimeout(focused, EmGetSel, startPointer, endPointer, SmtoAbortIfHung, 80, out ignored) == IntPtr.Zero) return false;
                start = Marshal.ReadInt32(startPointer);
                end = Marshal.ReadInt32(endPointer);
                return start >= 0 && end >= start;
            } finally {
                if (startPointer != IntPtr.Zero) Marshal.FreeHGlobal(startPointer);
                if (endPointer != IntPtr.Zero) Marshal.FreeHGlobal(endPointer);
            }
        }
    }
}
