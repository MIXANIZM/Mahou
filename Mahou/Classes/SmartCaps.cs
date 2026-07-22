using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Mahou {
    /// <summary>
    /// Conservative, local-only correction for accidental capitals inside a freshly typed word.
    /// The service never selects text and never uses keyboard or clipboard mutation. It only asks the
    /// existing direct Edit/Word adapters to replace an exact, freshly typed word after its delimiter.
    /// </summary>
    internal static class SmartCaps {
        sealed class PendingWord {
            internal readonly string Original;
            internal readonly string Corrected;
            internal readonly Keys TriggerKey;
            internal readonly IntPtr Foreground;
            internal readonly int CreatedAt;
            internal readonly int Generation;

            internal PendingWord(string original, string corrected, Keys triggerKey, IntPtr foreground, int createdAt, int generation) {
                Original = original;
                Corrected = corrected;
                TriggerKey = triggerKey;
                Foreground = foreground;
                CreatedAt = createdAt;
                Generation = generation;
            }
        }

        sealed class PendingCorrection {
            internal readonly string Original;
            internal readonly string Corrected;
            internal readonly IntPtr Foreground;
            internal readonly int AppliedAt;
            internal readonly int Generation;

            internal PendingCorrection(string original, string corrected, IntPtr foreground, int appliedAt, int generation) {
                Original = original;
                Corrected = corrected;
                Foreground = foreground;
                AppliedAt = appliedAt;
                Generation = generation;
            }
        }

        const int MaxWordCharacters = 64;
        const int MaxCandidateAgeMs = 2500;
        const int MaxUndoAgeMs = 3000;
        const int RejectionsBeforeException = 2;
        const int MaxBoundaryCharacters = 4;

        static readonly object SyncRoot = new object();
        static readonly StringBuilder CurrentWord = new StringBuilder();
        static readonly HashSet<string> Exceptions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        static readonly Dictionary<string, int> Rejections = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        static bool enabled;
        static IntPtr typingForeground;
        static int lastPrintableAt;
        static bool currentUnsafe;
        static PendingWord pendingWord;
        static PendingCorrection pendingCorrection;
        static PendingCorrection pendingUndo;
        static int stateGeneration;
        static int correctionsThisSession;
        static int reversionsThisSession;

        internal static bool Enabled {
            get { lock (SyncRoot) return enabled; }
        }

        internal static int CorrectionsThisSession {
            get { lock (SyncRoot) return correctionsThisSession; }
        }

        internal static int ReversionsThisSession {
            get { lock (SyncRoot) return reversionsThisSession; }
        }

        internal static void Configure(bool isEnabled, string exceptionsRaw) {
            lock (SyncRoot) {
                enabled = isEnabled;
                ReplaceExceptionsUnlocked(exceptionsRaw);
                ResetTypingUnlocked();
                pendingWord = null;
                pendingCorrection = null;
                pendingUndo = null;
            }
        }

        internal static void SetEnabled(bool value) {
            lock (SyncRoot) {
                enabled = value;
                if (!enabled) {
                    ResetTypingUnlocked();
                    pendingWord = null;
                    pendingCorrection = null;
                    pendingUndo = null;
                }
            }
        }

        internal static void SetExceptions(string raw) {
            lock (SyncRoot) ReplaceExceptionsUnlocked(raw);
        }

        internal static string GetExceptionsForDisplay() {
            lock (SyncRoot) {
                return String.Join(Environment.NewLine, Exceptions.OrderBy(value => value, StringComparer.CurrentCultureIgnoreCase));
            }
        }

        internal static string GetExceptionsForConfig() {
            lock (SyncRoot) {
                return String.Join("|", Exceptions.OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
            }
        }

        internal static string NormalizeExceptionKey(string value) {
            if (String.IsNullOrWhiteSpace(value)) return String.Empty;
            return value.Trim().ToLower(CultureInfo.CurrentCulture);
        }

        enum SupportedScript {
            None,
            Latin,
            Cyrillic,
            Unsupported
        }

        static SupportedScript GetSupportedScript(char value) {
            if ((value >= '\u0041' && value <= '\u024F') || (value >= '\u1E00' && value <= '\u1EFF'))
                return SupportedScript.Latin;
            if ((value >= '\u0400' && value <= '\u052F') ||
                (value >= '\u2DE0' && value <= '\u2DFF') ||
                (value >= '\uA640' && value <= '\uA69F'))
                return SupportedScript.Cyrillic;
            return Char.IsLetter(value) ? SupportedScript.Unsupported : SupportedScript.None;
        }

        static bool HasSingleSupportedScript(string value) {
            var script = SupportedScript.None;
            for (var i = 0; i < value.Length; i++) {
                var current = GetSupportedScript(value[i]);
                if (current == SupportedScript.None) continue;
                if (current == SupportedScript.Unsupported) return false;
                if (script == SupportedScript.None) script = current;
                else if (script != current) return false;
            }
            return script != SupportedScript.None;
        }

        internal static bool TryBuildCorrection(string value, out string corrected) {
            corrected = String.Empty;
            if (String.IsNullOrEmpty(value) || value.Length < 3 || value.Length > MaxWordCharacters) return false;
            if (!HasSingleSupportedScript(value)) return false;

            var chars = value.ToCharArray();
            var segmentStart = true;
            var hasLowercaseLetter = false;
            var changed = false;

            for (var i = 0; i < value.Length; i++) {
                var current = value[i];
                if (Char.IsLetter(current)) {
                    if (Char.IsLower(current)) hasLowercaseLetter = true;
                    if (!segmentStart && Char.IsUpper(current)) {
                        var lowered = Char.ToLower(current, CultureInfo.CurrentCulture);
                        if (lowered != current) {
                            chars[i] = lowered;
                            changed = true;
                        }
                    }
                    segmentStart = false;
                    continue;
                }

                var category = CharUnicodeInfo.GetUnicodeCategory(current);
                if (category == UnicodeCategory.NonSpacingMark ||
                    category == UnicodeCategory.SpacingCombiningMark ||
                    category == UnicodeCategory.EnclosingMark)
                    continue;

                if ((current == '\'' || current == '’' || current == '-') &&
                    i > 0 && i + 1 < value.Length &&
                    Char.IsLetter(value[i - 1]) && Char.IsLetter(value[i + 1])) {
                    segmentStart = true;
                    continue;
                }

                return false;
            }

            // Preserve acronyms and words typed entirely in capitals. Intentional mixed-case names
            // remain reversible and can be learned through the personal-exception mechanism.
            if (!hasLowercaseLetter || !changed) return false;
            corrected = new String(chars);
            return !String.Equals(value, corrected, StringComparison.Ordinal);
        }

        internal static void HandlePrintable(Keys key, char value) {
            lock (SyncRoot) {
                if (!enabled) return;
                stateGeneration++;
                pendingCorrection = null;
                pendingUndo = null;

                var foreground = WinAPI.GetForegroundWindow();
                if (foreground == IntPtr.Zero) {
                    ResetTypingUnlocked();
                    return;
                }
                if (typingForeground != IntPtr.Zero && typingForeground != foreground)
                    ResetTypingUnlocked();
                typingForeground = foreground;

                if (IsTrackedWordCharacter(value)) {
                    if (CurrentWord.Length >= MaxWordCharacters) {
                        ResetTypingUnlocked();
                        return;
                    }
                    if (Char.IsDigit(value)) currentUnsafe = true;
                    CurrentWord.Append(value);
                    lastPrintableAt = Environment.TickCount;
                    return;
                }

                if (IsCodeOrAddressCharacter(value)) {
                    if (CurrentWord.Length >= MaxWordCharacters) {
                        ResetTypingUnlocked();
                        return;
                    }
                    currentUnsafe = true;
                    CurrentWord.Append(value);
                    lastPrintableAt = Environment.TickCount;
                    return;
                }

                if (currentUnsafe && !Char.IsWhiteSpace(value)) {
                    if (CurrentWord.Length >= MaxWordCharacters) {
                        ResetTypingUnlocked();
                        return;
                    }
                    CurrentWord.Append(value);
                    lastPrintableAt = Environment.TickCount;
                    return;
                }

                if (Char.IsWhiteSpace(value) || Char.IsPunctuation(value) || Char.IsSymbol(value))
                    pendingWord = CapturePendingWordUnlocked(key, foreground);
                else
                    ResetTypingUnlocked();
            }
        }

        internal static void HandleBoundaryKeyDown(Keys key) {
            lock (SyncRoot) {
                if (!enabled) return;
                stateGeneration++;
                pendingCorrection = null;
                pendingUndo = null;
                var foreground = WinAPI.GetForegroundWindow();
                pendingWord = CapturePendingWordUnlocked(key, foreground);
            }
            // The delimiter is not guaranteed to be committed to the target control until key-up.
            // The pending word is therefore applied by HandleKeyUp.
        }

        internal static void HandleBackspaceKeyDown() {
            lock (SyncRoot) {
                if (!enabled) return;
                stateGeneration++;
                if (pendingCorrection != null &&
                    unchecked(Environment.TickCount - pendingCorrection.AppliedAt) >= 0 &&
                    unchecked(Environment.TickCount - pendingCorrection.AppliedAt) <= MaxUndoAgeMs &&
                    WinAPI.GetForegroundWindow() == pendingCorrection.Foreground) {
                    pendingUndo = pendingCorrection;
                    pendingCorrection = null;
                    ResetTypingUnlocked();
                    return;
                }
                pendingUndo = null;
                if (CurrentWord.Length > 0) CurrentWord.Length -= 1;
                lastPrintableAt = Environment.TickCount;
            }
        }

        internal static void HandleKeyUp(Keys key) {
            PendingWord word = null;
            PendingCorrection undo = null;
            lock (SyncRoot) {
                if (!enabled) return;
                if (pendingWord != null && pendingWord.TriggerKey == key) {
                    word = pendingWord;
                    pendingWord = null;
                }
                if (key == Keys.Back && pendingUndo != null) {
                    undo = pendingUndo;
                    pendingUndo = null;
                }
            }
            if (word != null) QueueApply(word);
            if (undo != null) QueueUndo(undo);
        }

        internal static void ResetTypingContext() {
            lock (SyncRoot) {
                stateGeneration++;
                ResetTypingUnlocked();
                pendingWord = null;
                pendingCorrection = null;
                pendingUndo = null;
            }
        }

        internal static void ResetForMouseClick() {
            ResetTypingContext();
        }

        static PendingWord CapturePendingWordUnlocked(Keys key, IntPtr foreground) {
            var original = CurrentWord.ToString();
            var age = unchecked(Environment.TickCount - lastPrintableAt);
            var unsafeWord = currentUnsafe;
            ResetTypingUnlocked();
            if (unsafeWord || foreground == IntPtr.Zero || String.IsNullOrEmpty(original) || age < 0 || age > MaxCandidateAgeMs)
                return null;

            string corrected;
            if (!TryBuildCorrection(original, out corrected)) return null;
            if (Exceptions.Contains(NormalizeExceptionKey(original))) return null;
            return new PendingWord(original, corrected, key, foreground, Environment.TickCount, stateGeneration);
        }

        static bool IsTrackedWordCharacter(char value) {
            if (Char.IsLetterOrDigit(value)) return true;
            var category = CharUnicodeInfo.GetUnicodeCategory(value);
            if (category == UnicodeCategory.NonSpacingMark ||
                category == UnicodeCategory.SpacingCombiningMark ||
                category == UnicodeCategory.EnclosingMark)
                return true;
            return value == '\'' || value == '’' || value == '-';
        }


        static bool IsCodeOrAddressCharacter(char value) {
            return value == '@' || value == '.' || value == '/' || value == '\\' || value == ':' || value == '_' ||
                   value == '+' || value == '=' || value == '#' || value == '&';
        }

        static void ResetTypingUnlocked() {
            CurrentWord.Length = 0;
            typingForeground = IntPtr.Zero;
            lastPrintableAt = 0;
            currentUnsafe = false;
        }

        static void ReplaceExceptionsUnlocked(string raw) {
            Exceptions.Clear();
            Rejections.Clear();
            if (String.IsNullOrWhiteSpace(raw)) return;
            var values = raw.Replace("\r", "\n").Split(new[] { '\n', '|', ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var value in values) {
                var normalized = NormalizeExceptionKey(value);
                if (!String.IsNullOrEmpty(normalized)) Exceptions.Add(normalized);
            }
        }

        static void QueueApply(PendingWord word) {
            var main = MMain.mahou;
            if (main == null || main.IsDisposed || !main.IsHandleCreated) return;
            try {
                main.BeginInvoke((Action)(() => TryApply(word)));
            } catch (ObjectDisposedException) {
            } catch (InvalidOperationException) {
            }
        }

        static void QueueUndo(PendingCorrection correction) {
            var main = MMain.mahou;
            if (main == null || main.IsDisposed || !main.IsHandleCreated) return;
            try {
                main.BeginInvoke((Action)(() => TryUndo(correction)));
            } catch (ObjectDisposedException) {
            } catch (InvalidOperationException) {
            }
        }

        static void TryApply(PendingWord word) {
            if (word == null || !Enabled) return;
            lock (SyncRoot) {
                if (word.Generation != stateGeneration) return;
            }
            var age = unchecked(Environment.TickCount - word.CreatedAt);
            if (age < 0 || age > MaxCandidateAgeMs) return;
            if (WinAPI.GetForegroundWindow() != word.Foreground) return;
            if (KMHook.ExcludedProgram()) return;

            if (TryDirectReplace(word.Original, word.Corrected)) {
                int corrections;
                int reversions;
                lock (SyncRoot) {
                    pendingCorrection = new PendingCorrection(word.Original, word.Corrected, word.Foreground, Environment.TickCount, stateGeneration);
                    correctionsThisSession++;
                    corrections = correctionsThisSession;
                    reversions = reversionsThisSession;
                }
                NotifyUiStatus(corrections, reversions);
                Logging.Log("Smart Caps corrected a fresh word; length=" + word.Original.Length + ".");
            }
        }

        static void TryUndo(PendingCorrection correction) {
            if (correction == null || !Enabled) return;
            lock (SyncRoot) {
                if (stateGeneration != correction.Generation + 1) return;
            }
            var age = unchecked(Environment.TickCount - correction.AppliedAt);
            if (age < 0 || age > MaxUndoAgeMs) return;
            if (WinAPI.GetForegroundWindow() != correction.Foreground) return;
            if (KMHook.ExcludedProgram()) return;

            if (!TryDirectReplace(correction.Corrected, correction.Original)) return;
            int corrections;
            int reversions;
            lock (SyncRoot) {
                reversionsThisSession++;
                corrections = correctionsThisSession;
                reversions = reversionsThisSession;
            }
            NotifyUiStatus(corrections, reversions);
            RegisterRejection(correction.Original);
            Logging.Log("Smart Caps correction was explicitly reverted with Backspace; length=" + correction.Original.Length + ".");
        }

        static bool TryDirectReplace(string expected, string replacement) {
            SelectionProbe.StandardEditWord standardWord;
            var standardResult = SelectionProbe.TryGetStandardEditWordAroundCaret(MaxWordCharacters, out standardWord);
            if (standardResult == SelectionProbe.DirectWordResult.Sensitive) return false;
            if (standardResult == SelectionProbe.DirectWordResult.Ready) {
                if (!String.Equals(standardWord.Text, expected, StringComparison.Ordinal)) return false;
                return SelectionProbe.TryReplaceStandardEditWord(standardWord, replacement);
            }

            standardResult = SelectionProbe.TryGetStandardEditFreshTextBeforeCaret(
                expected, MaxBoundaryCharacters, out standardWord);
            if (standardResult == SelectionProbe.DirectWordResult.Sensitive) return false;
            if (standardResult == SelectionProbe.DirectWordResult.Ready)
                return SelectionProbe.TryReplaceStandardEditWord(standardWord, replacement);

            try {
                var process = Locales.ActiveWindowProcess();
                if (process == null || !String.Equals(process.ProcessName, "WINWORD", StringComparison.OrdinalIgnoreCase))
                    return false;
            } catch {
                return false;
            }

            int sourceLength;
            int replacementLength;
            var result = SelectionProbe.TryReplaceActiveWordRange(
                MaxWordCharacters,
                value => String.Equals(value, expected, StringComparison.Ordinal) ? replacement : null,
                out sourceLength,
                out replacementLength);
            if (result == SelectionProbe.DirectWordResult.Replaced) return true;
            result = SelectionProbe.TryReplaceActiveFreshTextBeforeCaret(
                expected, replacement, MaxBoundaryCharacters);
            return result == SelectionProbe.DirectWordResult.Replaced;
        }


        static void NotifyUiStatus(int corrections, int reversions) {
            var main = MMain.mahou;
            if (main == null || main.IsDisposed || !main.IsHandleCreated) return;
            Action update = () => main.UpdateSmartCapsStatusFromService(corrections, reversions);
            try {
                if (main.InvokeRequired) main.BeginInvoke(update);
                else update();
            } catch (ObjectDisposedException) {
            } catch (InvalidOperationException) {
            }
        }

        static void RegisterRejection(string original) {
            var normalized = NormalizeExceptionKey(original);
            if (String.IsNullOrEmpty(normalized)) return;
            string display = null;
            string config = null;
            lock (SyncRoot) {
                int count;
                Rejections.TryGetValue(normalized, out count);
                count++;
                Rejections[normalized] = count;
                if (count < RejectionsBeforeException || Exceptions.Contains(normalized)) return;
                Exceptions.Add(normalized);
                display = GetExceptionsForDisplayUnlocked();
                config = String.Join("|", Exceptions.OrderBy(value => value, StringComparer.OrdinalIgnoreCase));
            }

            try {
                MMain.MyConfs.WriteSave("SmartTyping", "SmartCapsExceptions", config);
            } catch (Exception error) {
                Logging.Log("Could not persist Smart Caps exception: " + error.Message, 1);
            }
            var main = MMain.mahou;
            if (main != null && !main.IsDisposed && main.IsHandleCreated) {
                try {
                    main.BeginInvoke((Action)(() => main.UpdateSmartCapsExceptionsFromService(display)));
                } catch { }
            }
        }

        static string GetExceptionsForDisplayUnlocked() {
            return String.Join(Environment.NewLine, Exceptions.OrderBy(value => value, StringComparer.CurrentCultureIgnoreCase));
        }
    }
}
