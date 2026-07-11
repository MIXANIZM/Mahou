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

        static long GetStyle(IntPtr window) {
            return IntPtr.Size == 8 ? GetWindowLongPtr64(window, GWL_STYLE).ToInt64() : GetWindowLong32(window, GWL_STYLE);
        }

        internal static State GetState() {
            var automation = ProbeAutomation();
            if (automation != State.Unknown) return automation;
            return ProbeStandardEdit();
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
