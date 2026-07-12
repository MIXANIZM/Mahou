using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;

namespace Mahou {
    /// <summary>
    /// Fast, read-only selected-text state probe. It does not modify the clipboard.
    /// Actual conversion still uses Mahou's compatibility path after a selection is known.
    /// </summary>
    internal static class SelectionProbe {
        internal enum State {
            Unknown,
            None,
            Selected,
            Sensitive
        }

        const int GWL_STYLE = -16;
        const long ES_PASSWORD = 0x20;
        const uint EM_GETSEL = 0x00B0;
        const uint WM_GETTEXT = 0x000D;
        const uint WM_GETTEXTLENGTH = 0x000E;
        const uint SMTO_ABORTIFHUNG = 0x0002;

        [StructLayout(LayoutKind.Sequential)]
        struct GUITHREADINFO {
            public int cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public System.Drawing.Rectangle rcCaret;
        }

        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr processId);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool GetGUIThreadInfo(uint idThread, ref GUITHREADINFO info);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern int GetClassName(IntPtr hWnd, StringBuilder className, int maxCount);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern int GetWindowLong32(IntPtr hWnd, int index);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int index);

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SendMessageTimeout(
            IntPtr hWnd,
            uint msg,
            IntPtr wParam,
            IntPtr lParam,
            uint flags,
            uint timeout,
            out IntPtr result);

        [DllImport("user32.dll", EntryPoint = "SendMessageTimeoutW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr SendMessageTimeoutText(
            IntPtr hWnd,
            uint msg,
            IntPtr wParam,
            StringBuilder lParam,
            uint flags,
            uint timeout,
            out IntPtr result);

        static long GetStyle(IntPtr window) {
            return IntPtr.Size == 8 ? GetWindowLongPtr64(window, GWL_STYLE).ToInt64() : GetWindowLong32(window, GWL_STYLE);
        }

        internal static State GetState() {
            var automation = ProbeAutomation();
            if (automation != State.Unknown) return automation;
            return ProbeStandardEdit();
        }


        internal static bool TryGetSelectedText(int maxCharacters, out string selectedText) {
            selectedText = String.Empty;
            if (maxCharacters < 1) return false;
            if (TryGetAutomationSelectedText(maxCharacters, out selectedText)) return true;
            return TryGetStandardEditSelectedText(maxCharacters, out selectedText);
        }

        static bool TryGetAutomationSelectedText(int maxCharacters, out string selectedText) {
            selectedText = String.Empty;
            try {
                var focused = AutomationElement.FocusedElement;
                if (focused == null || focused.Current.IsPassword) return false;

                object patternObject;
                if (!focused.TryGetCurrentPattern(TextPattern.Pattern, out patternObject)) return false;
                var pattern = patternObject as TextPattern;
                if (pattern == null) return false;
                var ranges = pattern.GetSelection();
                if (ranges == null || ranges.Length == 0) return false;
                foreach (var range in ranges) {
                    if (range == null) continue;
                    var text = range.GetText(maxCharacters + 1);
                    if (String.IsNullOrEmpty(text)) continue;
                    selectedText = text;
                    return true;
                }
            } catch (ElementNotAvailableException) {
            } catch (InvalidOperationException) {
            } catch (COMException) {
            } catch (UnauthorizedAccessException) {
            }
            return false;
        }

        static bool TryGetStandardEditSelectedText(int maxCharacters, out string selectedText) {
            selectedText = String.Empty;
            IntPtr startPointer = IntPtr.Zero;
            IntPtr endPointer = IntPtr.Zero;
            try {
                var foreground = GetForegroundWindow();
                if (foreground == IntPtr.Zero) return false;
                var thread = GetWindowThreadProcessId(foreground, IntPtr.Zero);
                if (thread == 0) return false;

                var info = new GUITHREADINFO { cbSize = Marshal.SizeOf(typeof(GUITHREADINFO)) };
                if (!GetGUIThreadInfo(thread, ref info) || info.hwndFocus == IntPtr.Zero) return false;

                var className = new StringBuilder(128);
                if (GetClassName(info.hwndFocus, className, className.Capacity) <= 0) return false;
                var name = className.ToString();
                if (name.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) < 0) return false;
                if ((GetStyle(info.hwndFocus) & ES_PASSWORD) != 0) return false;

                startPointer = Marshal.AllocHGlobal(sizeof(int));
                endPointer = Marshal.AllocHGlobal(sizeof(int));
                Marshal.WriteInt32(startPointer, 0);
                Marshal.WriteInt32(endPointer, 0);
                IntPtr result;
                if (SendMessageTimeout(info.hwndFocus, EM_GETSEL, startPointer, endPointer,
                                       SMTO_ABORTIFHUNG, 40, out result) == IntPtr.Zero)
                    return false;
                var start = Marshal.ReadInt32(startPointer);
                var end = Marshal.ReadInt32(endPointer);
                if (start < 0 || end <= start) return true;
                if (end - start > maxCharacters) return true;

                IntPtr textLengthResult;
                if (SendMessageTimeout(info.hwndFocus, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero,
                                       SMTO_ABORTIFHUNG, 40, out textLengthResult) == IntPtr.Zero)
                    return false;
                var textLength = textLengthResult.ToInt32();
                if (textLength < end || textLength > 1024 * 1024) return false;

                var fullText = new StringBuilder(textLength + 1);
                IntPtr copiedResult;
                if (SendMessageTimeoutText(info.hwndFocus, WM_GETTEXT, (IntPtr)fullText.Capacity, fullText,
                                           SMTO_ABORTIFHUNG, 80, out copiedResult) == IntPtr.Zero)
                    return false;
                if (fullText.Length < end) return false;
                selectedText = fullText.ToString(start, end - start);
                return true;
            } catch {
                return false;
            } finally {
                if (startPointer != IntPtr.Zero) Marshal.FreeHGlobal(startPointer);
                if (endPointer != IntPtr.Zero) Marshal.FreeHGlobal(endPointer);
            }
        }

        static State ProbeAutomation() {
            try {
                var focused = AutomationElement.FocusedElement;
                if (focused == null) return State.Unknown;
                if (focused.Current.IsPassword) return State.Sensitive;

                object patternObject;
                if (!focused.TryGetCurrentPattern(TextPattern.Pattern, out patternObject)) return State.Unknown;
                var pattern = patternObject as TextPattern;
                if (pattern == null) return State.Unknown;
                var ranges = pattern.GetSelection();
                if (ranges == null || ranges.Length == 0) return State.None;
                foreach (var range in ranges) {
                    if (range == null) continue;
                    var selected = range.GetText(1);
                    if (!String.IsNullOrEmpty(selected)) return State.Selected;
                }
                return State.None;
            } catch (ElementNotAvailableException) {
                return State.Unknown;
            } catch (InvalidOperationException) {
                return State.Unknown;
            } catch (COMException) {
                return State.Unknown;
            } catch (UnauthorizedAccessException) {
                return State.Sensitive;
            }
        }

        static State ProbeStandardEdit() {
            IntPtr startPointer = IntPtr.Zero;
            IntPtr endPointer = IntPtr.Zero;
            try {
                var foreground = GetForegroundWindow();
                if (foreground == IntPtr.Zero) return State.Unknown;
                var thread = GetWindowThreadProcessId(foreground, IntPtr.Zero);
                if (thread == 0) return State.Unknown;

                var info = new GUITHREADINFO { cbSize = Marshal.SizeOf(typeof(GUITHREADINFO)) };
                if (!GetGUIThreadInfo(thread, ref info) || info.hwndFocus == IntPtr.Zero) return State.Unknown;

                var className = new StringBuilder(128);
                if (GetClassName(info.hwndFocus, className, className.Capacity) <= 0) return State.Unknown;
                var name = className.ToString();
                if (name.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) < 0) return State.Unknown;
                if ((GetStyle(info.hwndFocus) & ES_PASSWORD) != 0) return State.Sensitive;

                startPointer = Marshal.AllocHGlobal(sizeof(int));
                endPointer = Marshal.AllocHGlobal(sizeof(int));
                Marshal.WriteInt32(startPointer, 0);
                Marshal.WriteInt32(endPointer, 0);
                IntPtr result;
                if (SendMessageTimeout(info.hwndFocus, EM_GETSEL, startPointer, endPointer,
                                       SMTO_ABORTIFHUNG, 40, out result) == IntPtr.Zero)
                    return State.Unknown;
                return Marshal.ReadInt32(endPointer) > Marshal.ReadInt32(startPointer)
                    ? State.Selected
                    : State.None;
            } catch {
                return State.Unknown;
            } finally {
                if (startPointer != IntPtr.Zero) Marshal.FreeHGlobal(startPointer);
                if (endPointer != IntPtr.Zero) Marshal.FreeHGlobal(endPointer);
            }
        }
    }
}
