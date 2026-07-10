using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NLog;
using ComIDataObject = System.Runtime.InteropServices.ComTypes.IDataObject;

namespace Mahou
{
    internal static class UnifiedTextConverter
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const int ClipboardRetries = 5;
        private const int ClipboardRetryDelayMs = 5;
        private const int SelectionWaitAttempts = 10;
        private const int SelectionWaitDelayMs = 6;
        private const uint EM_GETSEL = 0x00B0;

        internal static void ConvertSelectionOrLast(List<KMHook.YuKey> word)
        {
            bool previousSelf = KMHook.self;
            KMHook.self = true;
            try
            {
                string selectedText;
                if (TryReadSelectedText(out selectedText))
                {
                    string converted = ConvertText(selectedText);
                    if (!String.IsNullOrEmpty(converted) && !String.Equals(converted, selectedText, StringComparison.Ordinal))
                    {
                        SwitchConfiguredLayout();
                        KInputs.MakeInput(KInputs.AddString(converted));
                        Reselect(converted);
                        return;
                    }
                }

                ConvertLastWord(word);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Unified word/selection conversion failed");
            }
            finally
            {
                KMHook.self = previousSelf;
            }
        }

        private static void ConvertLastWord(List<KMHook.YuKey> word)
        {
            if (word == null || word.Count == 0)
                return;

            Locales.IfLessThan2();
            SwitchConfiguredLayout();

            for (int i = 0; i < word.Count; i++)
            {
                KInputs.MakeInput(new[]
                {
                    KInputs.AddKey(Keys.Back, true),
                    KInputs.AddKey(Keys.Back, false)
                });
            }

            foreach (KMHook.YuKey item in word)
            {
                if (item.altnum && item.numpads != null)
                {
                    KInputs.MakeInput(new[] { KInputs.AddKey(Keys.LMenu, true) });
                    foreach (Keys numpad in item.numpads)
                    {
                        KInputs.MakeInput(new[]
                        {
                            KInputs.AddKey(numpad, true),
                            KInputs.AddKey(numpad, false)
                        });
                    }
                    KInputs.MakeInput(new[] { KInputs.AddKey(Keys.LMenu, false) });
                    continue;
                }

                if (item.upper)
                    KInputs.MakeInput(new[] { KInputs.AddKey(Keys.LShiftKey, true) });

                KInputs.MakeInput(new[]
                {
                    KInputs.AddKey(item.yukey, true),
                    KInputs.AddKey(item.yukey, false)
                });

                if (item.upper)
                    KInputs.MakeInput(new[] { KInputs.AddKey(Keys.LShiftKey, false) });
            }
        }

        private static bool TryReadSelectedText(out string selectedText)
        {
            selectedText = null;

            bool nativeHasSelection;
            if (TryDetermineNativeSelection(out nativeHasSelection) && !nativeHasSelection)
                return false;

            OleClipboardSnapshot snapshot;
            if (!OleClipboardSnapshot.TryCapture(out snapshot))
            {
                Log.Warn("Selection detection skipped because the clipboard data object could not be retained");
                return false;
            }

            bool clipboardChanged = false;
            bool restored = false;
            string marker = "MIXANIZM-MAHOU-SELECTION-" + Guid.NewGuid().ToString("N");

            try
            {
                if (RetryClipboard(delegate { Clipboard.SetText(marker, TextDataFormat.UnicodeText); }))
                {
                    uint markerSequence = GetClipboardSequenceNumber();
                    KInputs.MakeInput(new[]
                    {
                        KInputs.AddKey(Keys.RControlKey, true),
                        KInputs.AddKey(Keys.Insert, true),
                        KInputs.AddKey(Keys.Insert, false),
                        KInputs.AddKey(Keys.RControlKey, false)
                    });

                    for (int attempt = 0; attempt < SelectionWaitAttempts; attempt++)
                    {
                        if (attempt > 0)
                            Thread.Sleep(SelectionWaitDelayMs);

                        if (GetClipboardSequenceNumber() == markerSequence)
                            continue;

                        clipboardChanged = true;
                        string value = null;
                        bool textRead = RetryClipboard(delegate
                        {
                            value = Clipboard.ContainsText(TextDataFormat.UnicodeText)
                                ? Clipboard.GetText(TextDataFormat.UnicodeText)
                                : null;
                        });

                        if (textRead && !String.IsNullOrEmpty(value) && !String.Equals(value, marker, StringComparison.Ordinal))
                            selectedText = value;
                        break;
                    }
                }
            }
            finally
            {
                restored = snapshot.Restore();
                snapshot.Dispose();
                if (!restored)
                    Log.Error("Could not restore the original clipboard data object after selection detection");
            }

            if (!restored)
            {
                selectedText = null;
                return false;
            }

            return clipboardChanged && !String.IsNullOrEmpty(selectedText);
        }

        private static bool TryDetermineNativeSelection(out bool hasSelection)
        {
            hasSelection = false;
            var info = new GUITHREADINFO();
            info.cbSize = Marshal.SizeOf(typeof(GUITHREADINFO));
            if (!GetGUIThreadInfo(0, ref info) || info.hwndFocus == IntPtr.Zero)
                return false;

            var className = new StringBuilder(128);
            if (GetClassName(info.hwndFocus, className, className.Capacity) <= 0)
                return false;

            string value = className.ToString();
            bool supported = value.IndexOf("Edit", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("RichEdit", StringComparison.OrdinalIgnoreCase) >= 0 ||
                value.IndexOf("Scintilla", StringComparison.OrdinalIgnoreCase) >= 0;
            if (!supported)
                return false;

            int start;
            int end;
            SendMessage(info.hwndFocus, EM_GETSEL, out start, out end);
            hasSelection = start != end;
            return true;
        }

        private static string ConvertText(string text)
        {
            uint first = (uint)MMain.MyConfs.ReadInt("Locales", "locale1uId");
            uint second = (uint)MMain.MyConfs.ReadInt("Locales", "locale2uId");
            if (first == 0 || second == 0)
                return text;

            var result = new StringBuilder(text.Length);
            foreach (char character in text)
            {
                if (character == '\r' || character == '\n' || character == '\t')
                {
                    result.Append(character);
                    continue;
                }

                string mapped = MapCharacter(character, second, first);
                if (String.IsNullOrEmpty(mapped))
                    mapped = MapCharacter(character, first, second);
                result.Append(String.IsNullOrEmpty(mapped) ? character.ToString() : mapped);
            }
            return result.ToString();
        }

        private static string MapCharacter(char character, uint sourceLayout, uint targetLayout)
        {
            short keyState = VkKeyScanEx(character, new IntPtr(sourceLayout));
            if (keyState == -1)
                return String.Empty;

            uint virtualKey = (uint)(keyState & 0xff);
            int modifiers = (keyState >> 8) & 0xff;
            var keyboardState = new byte[256];
            if ((modifiers & 1) != 0) keyboardState[(int)Keys.ShiftKey] = 0x80;
            if ((modifiers & 2) != 0) keyboardState[(int)Keys.ControlKey] = 0x80;
            if ((modifiers & 4) != 0) keyboardState[(int)Keys.Menu] = 0x80;

            var output = new StringBuilder(8);
            uint scanCode = MapVirtualKeyEx(virtualKey, 0, new IntPtr(targetLayout));
            int count = ToUnicodeEx(virtualKey, scanCode, keyboardState, output, output.Capacity, 0, new IntPtr(targetLayout));
            return count > 0 ? output.ToString(0, Math.Min(count, output.Length)) : String.Empty;
        }

        private static void SwitchConfiguredLayout()
        {
            uint first = (uint)MMain.MyConfs.ReadInt("Locales", "locale1uId");
            uint second = (uint)MMain.MyConfs.ReadInt("Locales", "locale2uId");
            uint current = Locales.GetCurrentLocale();
            uint target = current == first ? second : first;
            if (target == 0 || target == current)
                return;

            IntPtr activeWindow = Locales.ActiveWindow();
            for (int attempt = 0; attempt < 4 && Locales.GetCurrentLocale() != target; attempt++)
            {
                KMHook.PostMessage(activeWindow, KInputs.WM_INPUTLANGCHANGEREQUEST, 0, target);
                Thread.Sleep(25);
            }
        }

        private static void Reselect(string text)
        {
            if (!MMain.MyConfs.ReadBool("Functions", "ReSelect"))
                return;

            int[] elements = StringInfo.ParseCombiningCharacters(text);
            if (elements.Length == 0)
                return;

            KInputs.MakeInput(new[] { KInputs.AddKey(Keys.LShiftKey, true) });
            for (int i = 0; i < elements.Length; i++)
            {
                KInputs.MakeInput(new[]
                {
                    KInputs.AddKey(Keys.Left, true),
                    KInputs.AddKey(Keys.Left, false)
                });
            }
            KInputs.MakeInput(new[] { KInputs.AddKey(Keys.LShiftKey, false) });
        }

        private static bool RetryClipboard(Action action)
        {
            for (int attempt = 0; attempt < ClipboardRetries; attempt++)
            {
                try
                {
                    action();
                    return true;
                }
                catch (ExternalException)
                {
                    Thread.Sleep(ClipboardRetryDelayMs);
                }
            }
            return false;
        }

        private sealed class OleClipboardSnapshot : IDisposable
        {
            private ComIDataObject dataObject;

            private OleClipboardSnapshot(ComIDataObject dataObject)
            {
                this.dataObject = dataObject;
            }

            internal static bool TryCapture(out OleClipboardSnapshot snapshot)
            {
                snapshot = null;
                for (int attempt = 0; attempt < ClipboardRetries; attempt++)
                {
                    ComIDataObject current;
                    int result = OleGetClipboard(out current);
                    if (result >= 0 && current != null)
                    {
                        snapshot = new OleClipboardSnapshot(current);
                        return true;
                    }
                    Thread.Sleep(ClipboardRetryDelayMs);
                }
                return false;
            }

            internal bool Restore()
            {
                return dataObject != null && OleSetClipboard(dataObject) >= 0;
            }

            public void Dispose()
            {
                if (dataObject == null)
                    return;

                try
                {
                    if (Marshal.IsComObject(dataObject))
                        Marshal.ReleaseComObject(dataObject);
                }
                catch
                {
                }
                dataObject = null;
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct GUITHREADINFO
        {
            public int cbSize;
            public uint flags;
            public IntPtr hwndActive;
            public IntPtr hwndFocus;
            public IntPtr hwndCapture;
            public IntPtr hwndMenuOwner;
            public IntPtr hwndMoveSize;
            public IntPtr hwndCaret;
            public RECT rcCaret;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("ole32.dll")]
        private static extern int OleGetClipboard([MarshalAs(UnmanagedType.Interface)] out ComIDataObject dataObject);

        [DllImport("ole32.dll")]
        private static extern int OleSetClipboard([MarshalAs(UnmanagedType.Interface)] ComIDataObject dataObject);

        [DllImport("user32.dll")]
        private static extern uint GetClipboardSequenceNumber();

        [DllImport("user32.dll")]
        private static extern bool GetGUIThreadInfo(uint threadId, ref GUITHREADINFO info);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int GetClassName(IntPtr window, StringBuilder className, int maxCount);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr window, uint message, out int start, out int end);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern short VkKeyScanEx(char ch, IntPtr keyboardLayout);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKeyEx(uint code, uint mapType, IntPtr keyboardLayout);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int ToUnicodeEx(uint virtualKey, uint scanCode, byte[] keyboardState, StringBuilder output, int outputCapacity, uint flags, IntPtr keyboardLayout);
    }
}
