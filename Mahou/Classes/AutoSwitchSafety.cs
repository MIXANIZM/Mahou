using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace Mahou {
    internal sealed class AutoSwitchSourceContext {
        internal readonly IntPtr Foreground;
        internal readonly IntPtr FocusedControl;
        internal readonly uint ProcessId;
        internal readonly string ProcessExecutable;
        internal readonly string ControlClass;
        internal readonly bool Protected;

        internal AutoSwitchSourceContext(IntPtr foreground, IntPtr focusedControl, uint processId,
                                         string processExecutable, string controlClass, bool protectedControl) {
            Foreground = foreground;
            FocusedControl = focusedControl;
            ProcessId = processId;
            ProcessExecutable = processExecutable;
            ControlClass = controlClass;
            Protected = protectedControl;
        }
    }

    internal static class AutoSwitchSafety {
        internal const string ModernNotepadExecutable = "notepad.exe";
        internal const string ModernNotepadControlClass = "RichEditD2DPT";
        const int GWL_STYLE = -16;
        const long ES_PASSWORD = 0x20;

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern int GetWindowLong32(IntPtr window, int index);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        static extern IntPtr GetWindowLongPtr64(IntPtr window, int index);

        internal static bool IsModernNotepadSurface(string processExecutable, string controlClass) {
            return String.Equals(processExecutable, ModernNotepadExecutable, StringComparison.OrdinalIgnoreCase) &&
                   String.Equals(controlClass, ModernNotepadControlClass, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsKnownContext(AutoSwitchSourceContext context) {
            return context != null &&
                   context.Foreground != IntPtr.Zero &&
                   context.FocusedControl != IntPtr.Zero &&
                   context.ProcessId != 0 &&
                   !String.IsNullOrEmpty(context.ProcessExecutable) &&
                   !String.IsNullOrEmpty(context.ControlClass);
        }

        internal static bool IsSameSource(AutoSwitchSourceContext expected, AutoSwitchSourceContext current) {
            return IsKnownContext(expected) &&
                   IsKnownContext(current) &&
                   expected.Foreground == current.Foreground &&
                   expected.FocusedControl == current.FocusedControl &&
                   expected.ProcessId == current.ProcessId &&
                   String.Equals(expected.ProcessExecutable, current.ProcessExecutable,
                                 StringComparison.OrdinalIgnoreCase) &&
                   String.Equals(expected.ControlClass, current.ControlClass,
                                 StringComparison.OrdinalIgnoreCase);
        }

        internal static bool CanMutate(AutoSwitchSourceContext expected, AutoSwitchSourceContext current) {
            return IsSameSource(expected, current) &&
                   !current.Protected &&
                   !IsModernNotepadSurface(current.ProcessExecutable, current.ControlClass);
        }

        internal static bool TryCaptureAllowedSource(out AutoSwitchSourceContext context) {
            if (!TryCapture(out context)) return false;
            if (IsModernNotepadSurface(context.ProcessExecutable, context.ControlClass)) {
                Logging.Log("[AS] > AutoSwitch suppressed for modern Notepad RichEditD2DPT.", 2);
                context = null;
                return false;
            }
            return true;
        }

        internal static bool CanMutateNow(AutoSwitchSourceContext expected) {
            AutoSwitchSourceContext current;
            if (!TryCapture(out current)) {
                Logging.Log("[AS] > AutoSwitch mutation cancelled because source identity is uncertain.", 2);
                return false;
            }
            if (!CanMutate(expected, current)) {
                Logging.Log("[AS] > AutoSwitch mutation cancelled because focus/control/source changed or is blocked.", 2);
                return false;
            }
            return true;
        }

        static bool TryCapture(out AutoSwitchSourceContext context) {
            context = null;
            try {
                var foreground = WinAPI.GetForegroundWindow();
                if (foreground == IntPtr.Zero) return false;

                uint foregroundProcessId;
                var thread = WinAPI.GetWindowThreadProcessId(foreground, out foregroundProcessId);
                if (thread == 0 || foregroundProcessId == 0) return false;

                var info = new WinAPI.GUITHREADINFO { cbSize = Marshal.SizeOf(typeof(WinAPI.GUITHREADINFO)) };
                if (!WinAPI.GetGUIThreadInfo(thread, ref info) || info.hwndFocus == IntPtr.Zero) return false;

                uint focusedProcessId;
                if (WinAPI.GetWindowThreadProcessId(info.hwndFocus, out focusedProcessId) == 0 ||
                    focusedProcessId == 0 ||
                    focusedProcessId != foregroundProcessId) return false;

                var className = new StringBuilder(128);
                if (WinAPI.GetClassName(info.hwndFocus, className, className.Capacity) <= 0) return false;

                using (var process = Process.GetProcessById((int)foregroundProcessId)) {
                    if (process == null || String.IsNullOrEmpty(process.ProcessName)) return false;
                    context = new AutoSwitchSourceContext(
                        foreground,
                        info.hwndFocus,
                        foregroundProcessId,
                        process.ProcessName + ".exe",
                        className.ToString(),
                        IsProtectedStandardEdit(info.hwndFocus, className.ToString()));
                }
                return IsKnownContext(context);
            } catch (Exception e) {
                Logging.Log("[AS] > Could not capture AutoSwitch source identity: " + e.Message, 2);
                context = null;
                return false;
            }
        }

        static bool IsProtectedStandardEdit(IntPtr focusedControl, string controlClass) {
            if (!String.Equals(controlClass, "Edit", StringComparison.OrdinalIgnoreCase)) return false;
            var style = IntPtr.Size == 8
                ? GetWindowLongPtr64(focusedControl, GWL_STYLE).ToInt64()
                : GetWindowLong32(focusedControl, GWL_STYLE);
            return (style & ES_PASSWORD) != 0;
        }
    }
}
