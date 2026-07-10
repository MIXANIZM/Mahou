using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NLog;

namespace Mahou
{
    internal static class UnifiedTextConverter
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();
        private const int ClipboardRetries = 10;
        private const int ClipboardRetryDelayMs = 40;
        private const int SelectionWaitAttempts = 20;
        private const int SelectionWaitDelayMs = 25;

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
            ClipboardSnapshot snapshot;
            if (!ClipboardSnapshot.TryCapture(out snapshot))
            {
                Log.Warn("Selection conversion skipped because the clipboard could not be copied safely");
                return false;
            }

            string marker = "MIXANIZM-MAHOU-SELECTION-" + Guid.NewGuid().ToString("N");
            bool clipboardChanged = false;
            try
            {
                if (!RetryClipboard(delegate { Clipboard.SetText(marker, TextDataFormat.UnicodeText); }))
                    return false;

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
                    Thread.Sleep(SelectionWaitDelayMs);
                    if (GetClipboardSequenceNumber() == markerSequence)
                        continue;

                    clipboardChanged = true;
                    string value = null;
                    if (RetryClipboard(delegate { value = Clipboard.ContainsText() ? Clipboard.GetText(TextDataFormat.UnicodeText) : null; }) &&
                        !String.IsNullOrEmpty(value) &&
                        !String.Equals(value, marker, StringComparison.Ordinal))
                    {
                        selectedText = value;
                    }
                    break;
                }
            }
            finally
            {
                if (!snapshot.Restore())
                    Log.Error("Could not restore the clipboard after selection detection");
            }

            return clipboardChanged && !String.IsNullOrEmpty(selectedText);
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
                Thread.Sleep(50);
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

        private sealed class ClipboardSnapshot
        {
            private readonly DataObject data;
            private readonly bool wasEmpty;

            private ClipboardSnapshot(DataObject data, bool wasEmpty)
            {
                this.data = data;
                this.wasEmpty = wasEmpty;
            }

            internal static bool TryCapture(out ClipboardSnapshot snapshot)
            {
                snapshot = null;
                DataObject copy = null;
                bool empty = false;
                bool success = RetryClipboard(delegate
                {
                    IDataObject source = Clipboard.GetDataObject();
                    if (source == null)
                    {
                        empty = true;
                        copy = new DataObject();
                        return;
                    }

                    copy = new DataObject();
                    string[] formats = source.GetFormats(false);
                    if (formats == null || formats.Length == 0)
                    {
                        empty = true;
                        return;
                    }

                    foreach (string format in formats)
                    {
                        object value = source.GetData(format, false);
                        if (value == null)
                            throw new InvalidOperationException("Clipboard format could not be rendered: " + format);
                        copy.SetData(format, false, CloneClipboardValue(value));
                    }
                });

                if (!success || copy == null)
                    return false;

                snapshot = new ClipboardSnapshot(copy, empty);
                return true;
            }

            internal bool Restore()
            {
                return RetryClipboard(delegate
                {
                    if (wasEmpty)
                        Clipboard.Clear();
                    else
                        Clipboard.SetDataObject(data, true, ClipboardRetries, ClipboardRetryDelayMs);
                });
            }

            private static object CloneClipboardValue(object value)
            {
                var bytes = value as byte[];
                if (bytes != null) return (byte[])bytes.Clone();

                var strings = value as string[];
                if (strings != null) return (string[])strings.Clone();

                var stringCollection = value as StringCollection;
                if (stringCollection != null)
                {
                    var clone = new StringCollection();
                    clone.AddRange(ToArray(stringCollection));
                    return clone;
                }

                var bitmap = value as Bitmap;
                if (bitmap != null) return new Bitmap(bitmap);

                var image = value as Image;
                if (image != null) return image.Clone();

                var stream = value as Stream;
                if (stream != null)
                {
                    long originalPosition = stream.CanSeek ? stream.Position : 0;
                    if (stream.CanSeek) stream.Position = 0;
                    var clone = new MemoryStream();
                    stream.CopyTo(clone);
                    clone.Position = 0;
                    if (stream.CanSeek) stream.Position = originalPosition;
                    return clone;
                }

                return value;
            }

            private static string[] ToArray(StringCollection collection)
            {
                var values = new string[collection.Count];
                collection.CopyTo(values, 0);
                return values;
            }
        }

        [DllImport("user32.dll")]
        private static extern uint GetClipboardSequenceNumber();

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern short VkKeyScanEx(char ch, IntPtr keyboardLayout);

        [DllImport("user32.dll")]
        private static extern uint MapVirtualKeyEx(uint code, uint mapType, IntPtr keyboardLayout);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int ToUnicodeEx(uint virtualKey, uint scanCode, byte[] keyboardState, StringBuilder output, int outputCapacity, uint flags, IntPtr keyboardLayout);
    }
}
