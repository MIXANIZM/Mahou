using System;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Automation;

namespace Mahou {
    /// <summary>
    /// Read-only selection/caret probing plus bounded direct replacement for controls that expose safe native APIs.
    /// Clipboard-based conversion remains the compatibility fallback for controls without a writable text range.
    /// </summary>
    internal static class SelectionProbe {
        internal enum State {
            Unknown,
            None,
            Selected,
            Sensitive
        }

        internal enum DirectWordResult {
            Unavailable,
            NoWord,
            Sensitive,
            Ready,
            Replaced,
            Failed
        }

        internal sealed class StandardEditWord {
            internal readonly IntPtr ForegroundWindow;
            internal readonly IntPtr FocusedWindow;
            internal readonly int Start;
            internal readonly int End;
            internal readonly int Caret;
            internal readonly string Text;

            internal StandardEditWord(IntPtr foregroundWindow, IntPtr focusedWindow,
                                      int start, int end, int caret, string text) {
                ForegroundWindow = foregroundWindow;
                FocusedWindow = focusedWindow;
                Start = start;
                End = end;
                Caret = caret;
                Text = text;
            }
        }

        const int GWL_STYLE = -16;
        const long ES_PASSWORD = 0x20;
        const uint EM_GETSEL = 0x00B0;
        const uint EM_SETSEL = 0x00B1;
        const uint EM_REPLACESEL = 0x00C2;
        const uint WM_SETREDRAW = 0x000B;
        const uint WM_GETTEXT = 0x000D;
        const uint WM_GETTEXTLENGTH = 0x000E;
        const uint SMTO_ABORTIFHUNG = 0x0002;
        const int MaxDirectControlCharacters = 1024 * 1024;
        const int MaxAdjacentWhitespaceProbe = 8;

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

        [DllImport("user32.dll", EntryPoint = "SendMessageTimeoutW", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr SendMessageTimeoutString(
            IntPtr hWnd,
            uint msg,
            IntPtr wParam,
            string lParam,
            uint flags,
            uint timeout,
            out IntPtr result);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool InvalidateRect(IntPtr hWnd, IntPtr rect, bool erase);

        [DllImport("user32.dll", SetLastError = true)]
        static extern bool UpdateWindow(IntPtr hWnd);

        static long GetStyle(IntPtr window) {
            return IntPtr.Size == 8 ? GetWindowLongPtr64(window, GWL_STYLE).ToInt64() : GetWindowLong32(window, GWL_STYLE);
        }

        static bool TryGetFocusedStandardEdit(out IntPtr foreground, out IntPtr focused, out bool sensitive) {
            foreground = GetForegroundWindow();
            focused = IntPtr.Zero;
            sensitive = false;
            if (foreground == IntPtr.Zero) return false;
            var thread = GetWindowThreadProcessId(foreground, IntPtr.Zero);
            if (thread == 0) return false;

            var info = new GUITHREADINFO { cbSize = Marshal.SizeOf(typeof(GUITHREADINFO)) };
            if (!GetGUIThreadInfo(thread, ref info) || info.hwndFocus == IntPtr.Zero) return false;

            var className = new StringBuilder(128);
            if (GetClassName(info.hwndFocus, className, className.Capacity) <= 0) return false;
            if (className.ToString().IndexOf("Edit", StringComparison.OrdinalIgnoreCase) < 0) return false;
            focused = info.hwndFocus;
            sensitive = (GetStyle(focused) & ES_PASSWORD) != 0;
            return true;
        }

        static bool TryGetSelection(IntPtr focused, out int start, out int end) {
            start = 0;
            end = 0;
            IntPtr startPointer = IntPtr.Zero;
            IntPtr endPointer = IntPtr.Zero;
            try {
                startPointer = Marshal.AllocHGlobal(sizeof(int));
                endPointer = Marshal.AllocHGlobal(sizeof(int));
                Marshal.WriteInt32(startPointer, 0);
                Marshal.WriteInt32(endPointer, 0);
                IntPtr result;
                if (SendMessageTimeout(focused, EM_GETSEL, startPointer, endPointer,
                                       SMTO_ABORTIFHUNG, 40, out result) == IntPtr.Zero)
                    return false;
                start = Marshal.ReadInt32(startPointer);
                end = Marshal.ReadInt32(endPointer);
                return start >= 0 && end >= start;
            } finally {
                if (startPointer != IntPtr.Zero) Marshal.FreeHGlobal(startPointer);
                if (endPointer != IntPtr.Zero) Marshal.FreeHGlobal(endPointer);
            }
        }

        static bool TryGetStandardEditText(IntPtr focused, out string text) {
            text = String.Empty;
            IntPtr textLengthResult;
            if (SendMessageTimeout(focused, WM_GETTEXTLENGTH, IntPtr.Zero, IntPtr.Zero,
                                   SMTO_ABORTIFHUNG, 40, out textLengthResult) == IntPtr.Zero)
                return false;
            var textLength = textLengthResult.ToInt32();
            if (textLength < 0 || textLength > MaxDirectControlCharacters) return false;

            var fullText = new StringBuilder(textLength + 1);
            IntPtr copiedResult;
            if (SendMessageTimeoutText(focused, WM_GETTEXT, (IntPtr)fullText.Capacity, fullText,
                                       SMTO_ABORTIFHUNG, 80, out copiedResult) == IntPtr.Zero)
                return false;
            text = fullText.ToString();
            return true;
        }

        static bool IsCoreWordCharacter(char value) {
            if (Char.IsLetterOrDigit(value)) return true;
            var category = CharUnicodeInfo.GetUnicodeCategory(value);
            return category == UnicodeCategory.NonSpacingMark ||
                   category == UnicodeCategory.SpacingCombiningMark ||
                   category == UnicodeCategory.EnclosingMark;
        }

        static bool IsWordCharacter(string text, int index) {
            if (String.IsNullOrEmpty(text) || index < 0 || index >= text.Length) return false;
            var value = text[index];
            if (IsCoreWordCharacter(value)) return true;
            if (value != '\'' && value != '’' && value != '-' && value != '_') return false;
            return index > 0 && index + 1 < text.Length &&
                   IsCoreWordCharacter(text[index - 1]) && IsCoreWordCharacter(text[index + 1]);
        }

        static bool IsHorizontalWhitespace(char value) {
            return value == ' ' || value == '\t' || value == '\u00A0';
        }

        static bool IsWhitespaceOnly(string text, int start, int end) {
            if (String.IsNullOrEmpty(text) || start < 0 || end < start || end > text.Length) return false;
            for (var i = start; i < end; i++) {
                if (!IsHorizontalWhitespace(text[i])) return false;
            }
            return true;
        }

        static bool TryFindWordBounds(string text, int caret, int maxCharacters, out int start, out int end) {
            start = 0;
            end = 0;
            if (String.IsNullOrEmpty(text) || caret < 0 || caret > text.Length || maxCharacters < 1) return false;

            var probe = -1;
            var caretOnWord = caret < text.Length && IsWordCharacter(text, caret);
            var leftOnWord = caret > 0 && IsWordCharacter(text, caret - 1);
            var caretInsideWord = caretOnWord && leftOnWord;

            if (caretInsideWord) {
                probe = caret;
            } else if (leftOnWord) {
                probe = caret - 1;
            } else {
                // At an exact word-start boundary, prefer the previous word on the same line.
                // Only use the word to the right when no previous word exists.
                var left = caret - 1;
                var leftDistance = 0;
                while (left >= 0 && leftDistance < MaxAdjacentWhitespaceProbe && IsHorizontalWhitespace(text[left])) {
                    left--;
                    leftDistance++;
                }
                if (left >= 0 && IsWordCharacter(text, left)) {
                    probe = left;
                } else if (caretOnWord) {
                    probe = caret;
                } else {
                    var right = caret;
                    var rightDistance = 0;
                    while (right < text.Length && rightDistance < MaxAdjacentWhitespaceProbe && IsHorizontalWhitespace(text[right])) {
                        right++;
                        rightDistance++;
                    }
                    if (right < text.Length && IsWordCharacter(text, right)) probe = right;
                }
            }
            if (probe < 0) return false;

            start = probe;
            while (start > 0 && IsWordCharacter(text, start - 1)) start--;
            end = probe + 1;
            while (end < text.Length && IsWordCharacter(text, end)) end++;
            if (end <= start || end - start > maxCharacters) return false;

            for (var i = start; i < end; i++) {
                if (Char.IsLetter(text[i])) return true;
            }
            return false;
        }

        static bool IsSingleWord(string text, int maxCharacters) {
            if (String.IsNullOrEmpty(text) || text.Length > maxCharacters) return false;
            int start;
            int end;
            return TryFindWordBounds(text, 0, maxCharacters, out start, out end) && start == 0 && end == text.Length;
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

        internal static DirectWordResult TryGetStandardEditWordAroundCaret(int maxCharacters, out StandardEditWord word) {
            word = null;
            try {
                IntPtr foreground;
                IntPtr focused;
                bool sensitive;
                if (!TryGetFocusedStandardEdit(out foreground, out focused, out sensitive))
                    return DirectWordResult.Unavailable;
                if (sensitive) return DirectWordResult.Sensitive;

                int selectionStart;
                int selectionEnd;
                if (!TryGetSelection(focused, out selectionStart, out selectionEnd))
                    return DirectWordResult.Unavailable;
                if (selectionEnd > selectionStart) return DirectWordResult.Unavailable;

                string fullText;
                if (!TryGetStandardEditText(focused, out fullText)) return DirectWordResult.Unavailable;
                if (selectionStart > fullText.Length) return DirectWordResult.Unavailable;

                int wordStart;
                int wordEnd;
                if (!TryFindWordBounds(fullText, selectionStart, maxCharacters, out wordStart, out wordEnd))
                    return DirectWordResult.NoWord;
                word = new StandardEditWord(foreground, focused, wordStart, wordEnd,
                                            selectionStart, fullText.Substring(wordStart, wordEnd - wordStart));
                return DirectWordResult.Ready;
            } catch (UnauthorizedAccessException) {
                return DirectWordResult.Sensitive;
            } catch (Exception) {
                return DirectWordResult.Failed;
            }
        }

        internal static bool TryReplaceStandardEditWord(StandardEditWord word, string replacement) {
            if (word == null || replacement == null) return false;
            var redrawDisabled = false;
            var selectionApplied = false;
            var replacementApplied = false;
            try {
                IntPtr foreground;
                IntPtr focused;
                bool sensitive;
                if (!TryGetFocusedStandardEdit(out foreground, out focused, out sensitive) || sensitive) return false;
                if (foreground != word.ForegroundWindow || focused != word.FocusedWindow) return false;

                int selectionStart;
                int selectionEnd;
                if (!TryGetSelection(focused, out selectionStart, out selectionEnd) ||
                    selectionStart != word.Caret || selectionEnd != word.Caret)
                    return false;

                string currentText;
                if (!TryGetStandardEditText(focused, out currentText) ||
                    word.End > currentText.Length ||
                    !String.Equals(currentText.Substring(word.Start, word.End - word.Start), word.Text, StringComparison.Ordinal))
                    return false;
                if (String.Equals(word.Text, replacement, StringComparison.Ordinal)) return true;

                IntPtr ignored;
                if (SendMessageTimeout(focused, WM_SETREDRAW, IntPtr.Zero, IntPtr.Zero,
                                       SMTO_ABORTIFHUNG, 40, out ignored) != IntPtr.Zero)
                    redrawDisabled = true;
                if (SendMessageTimeout(focused, EM_SETSEL, (IntPtr)word.Start, (IntPtr)word.End,
                                       SMTO_ABORTIFHUNG, 40, out ignored) == IntPtr.Zero)
                    return false;
                selectionApplied = true;
                if (SendMessageTimeoutString(focused, EM_REPLACESEL, (IntPtr)1, replacement,
                                              SMTO_ABORTIFHUNG, 100, out ignored) == IntPtr.Zero) {
                    SendMessageTimeout(focused, EM_SETSEL, (IntPtr)word.Caret, (IntPtr)word.Caret,
                                       SMTO_ABORTIFHUNG, 40, out ignored);
                    return false;
                }
                replacementApplied = true;
                selectionApplied = false;

                var delta = replacement.Length - (word.End - word.Start);
                var newCaret = word.Caret <= word.Start
                    ? word.Start
                    : word.Caret >= word.End
                        ? word.Caret + delta
                        : word.Start + Math.Min(word.Caret - word.Start, replacement.Length);
                SendMessageTimeout(focused, EM_SETSEL, (IntPtr)newCaret, (IntPtr)newCaret,
                                   SMTO_ABORTIFHUNG, 40, out ignored);
                return true;
            } catch {
                return replacementApplied;
            } finally {
                if (selectionApplied && !replacementApplied) {
                    try {
                        IntPtr ignored;
                        SendMessageTimeout(word.FocusedWindow, EM_SETSEL, (IntPtr)word.Caret, (IntPtr)word.Caret,
                                           SMTO_ABORTIFHUNG, 40, out ignored);
                    } catch { }
                }
                if (redrawDisabled) {
                    try {
                        IntPtr ignored;
                        SendMessageTimeout(word.FocusedWindow, WM_SETREDRAW, (IntPtr)1, IntPtr.Zero,
                                           SMTO_ABORTIFHUNG, 40, out ignored);
                        InvalidateRect(word.FocusedWindow, IntPtr.Zero, false);
                        UpdateWindow(word.FocusedWindow);
                    } catch { }
                }
            }
        }

        internal static DirectWordResult TryReplaceActiveWordRange(int maxCharacters,
                                                                   Func<string, string> converter,
                                                                   out int sourceLength,
                                                                   out int replacementLength) {
            sourceLength = 0;
            replacementLength = 0;
            if (converter == null || maxCharacters < 1) return DirectWordResult.Unavailable;

            object applicationObject = null;
            object selectionObject = null;
            object documentObject = null;
            object contentObject = null;
            object contextRangeObject = null;
            object targetRangeObject = null;
            var replacementApplied = false;
            try {
                applicationObject = Marshal.GetActiveObject("Word.Application");
                dynamic application = applicationObject;
                selectionObject = application.Selection;
                dynamic selection = selectionObject;
                var selectionStart = (int)selection.Start;
                var selectionEnd = (int)selection.End;
                if (selectionEnd > selectionStart) return DirectWordResult.Unavailable;

                documentObject = application.ActiveDocument;
                dynamic document = documentObject;
                contentObject = document.Content;
                dynamic content = contentObject;
                var documentEnd = Math.Max(0, (int)content.End - 1);
                if (selectionStart < 0 || selectionStart > documentEnd) return DirectWordResult.Unavailable;

                var contextStart = Math.Max(0, selectionStart - maxCharacters - MaxAdjacentWhitespaceProbe);
                var contextEnd = Math.Min(documentEnd, selectionStart + maxCharacters + MaxAdjacentWhitespaceProbe);
                contextRangeObject = document.Range(contextStart, contextEnd);
                dynamic contextRange = contextRangeObject;
                var contextText = (string)contextRange.Text ?? String.Empty;
                var localCaret = selectionStart - contextStart;

                int localStart;
                int localEnd;
                if (!TryFindWordBounds(contextText, localCaret, maxCharacters, out localStart, out localEnd))
                    return DirectWordResult.NoWord;

                var targetStart = contextStart + localStart;
                var targetEnd = contextStart + localEnd;
                targetRangeObject = document.Range(targetStart, targetEnd);
                dynamic targetRange = targetRangeObject;
                var source = (string)targetRange.Text ?? String.Empty;
                if (!IsSingleWord(source, maxCharacters)) return DirectWordResult.NoWord;

                var replacement = converter(source);
                if (replacement == null) return DirectWordResult.Failed;
                sourceLength = source.Length;
                replacementLength = replacement.Length;
                if (!String.Equals(source, replacement, StringComparison.Ordinal)) targetRange.Text = replacement;
                replacementApplied = true;

                var delta = replacement.Length - source.Length;
                var newCaret = selectionStart <= targetStart
                    ? targetStart
                    : selectionStart >= targetEnd
                        ? selectionStart + delta
                        : targetStart + Math.Min(selectionStart - targetStart, replacement.Length);
                selection.SetRange(newCaret, newCaret);
                return DirectWordResult.Replaced;
            } catch (UnauthorizedAccessException) {
                return replacementApplied ? DirectWordResult.Replaced : DirectWordResult.Sensitive;
            } catch (COMException) {
                return replacementApplied ? DirectWordResult.Replaced : DirectWordResult.Unavailable;
            } catch (Exception) {
                return replacementApplied ? DirectWordResult.Replaced : DirectWordResult.Failed;
            } finally {
                ReleaseComObject(targetRangeObject);
                ReleaseComObject(contextRangeObject);
                ReleaseComObject(contentObject);
                ReleaseComObject(documentObject);
                ReleaseComObject(selectionObject);
                ReleaseComObject(applicationObject);
            }
        }

        static void ReleaseComObject(object value) {
            if (value == null) return;
            try {
                if (Marshal.IsComObject(value)) Marshal.ReleaseComObject(value);
            } catch { }
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
            try {
                IntPtr foreground;
                IntPtr focused;
                bool sensitive;
                if (!TryGetFocusedStandardEdit(out foreground, out focused, out sensitive) || sensitive) return false;

                int start;
                int end;
                if (!TryGetSelection(focused, out start, out end)) return false;
                if (end <= start) return true;
                if (end - start > maxCharacters) return true;

                string fullText;
                if (!TryGetStandardEditText(focused, out fullText) || end > fullText.Length) return false;
                selectedText = fullText.Substring(start, end - start);
                return true;
            } catch {
                return false;
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
            try {
                IntPtr foreground;
                IntPtr focused;
                bool sensitive;
                if (!TryGetFocusedStandardEdit(out foreground, out focused, out sensitive)) return State.Unknown;
                if (sensitive) return State.Sensitive;

                int start;
                int end;
                if (!TryGetSelection(focused, out start, out end)) return State.Unknown;
                return end > start ? State.Selected : State.None;
            } catch {
                return State.Unknown;
            }
        }
    }
}
