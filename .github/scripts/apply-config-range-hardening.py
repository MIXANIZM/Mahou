#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
path = ROOT / "Mahou/Classes/Configs.cs"
data = path.read_bytes()
old_lf = b'''        void NormalizeCriticalRanges() {
            NormalizeInt("Hidden", "AS_IngoreLSTimeout", 0, 600000, 5000);
            NormalizeInt("Hidden", "OverlayExcludedInterval", 100, 600000, 2500);
            NormalizeInt("Hidden", "AutoRestartMins", 0, 10080, 0);
            NormalizeInt("Updates", "Delay", 1, 300, 5);
            NormalizeInt("Timings", "SelectedTextGetMoreTriesCount", 1, 20, 5);
            NormalizeInt("Timings", "DelayAfterBackspaces", 0, 2000, 100);
            NormalizeInt("Timings", "CapsLockDisableRefreshRate", 10, 60000, 100);
            NormalizeInt("Timings", "ScrollLockStateRefreshRate", 10, 60000, 100);
            NormalizeInt("Timings", "FlagsInTrayRefreshRate", 10, 60000, 100);
            NormalizeInt("Timings", "DoubleHotkey2ndPressWait", 50, 5000, 350);
            NormalizeInt("Timings", "LangTooltipForCaretRefreshRate", 10, 60000, 25);
            NormalizeInt("Timings", "LangTooltipForMouseRefreshRate", 10, 60000, 25);
            NormalizeInt("PersistentLayout", "Layout1CheckInterval", 10, 60000, 50);
            NormalizeInt("PersistentLayout", "Layout2CheckInterval", 10, 60000, 50);
        }
'''
new_lf = b'''        void NormalizeCriticalRanges() {
            // Hidden timing controls: keep values inside the actual UI ranges.
            NormalizeInt("Hidden", "TrayHoverMahouMM", 0, 350000, 0);
            NormalizeInt("Hidden", "AS_IngoreLSTimeout", 0, 350000, 5000);
            NormalizeInt("Hidden", "OverlayExcludedInterval", 250, 10000, 2500);
            NormalizeInt("Hidden", "AutoRestartMins", 0, 500, 0);

            // ComboBox indices must be valid before the settings form loads.
            NormalizeInt("Layouts", "SpecificKeysType", 0, 1, 0);
            NormalizeInt("Functions", "WriteInputHistoryBackSpaceType", 0, 1, 0);

            // NumericUpDown and Timer values are normalized to their designer limits.
            NormalizeInt("TranslatePanel", "Transparency", 1, 100, 90);
            NormalizeInt("LangPanel", "Transparency", 1, 100, 90);
            NormalizeInt("LangPanel", "RefreshRate", 1, 2000, 25);
            NormalizeInt("Timings", "LangTooltipForMouseSkipMessages", 0, 1000, 5);
            NormalizeInt("Timings", "SelectedTextGetMoreTriesCount", 3, 20, 5);
            NormalizeInt("Timings", "DelayAfterBackspaces", 1, 900, 100);
            NormalizeInt("Timings", "CapsLockDisableRefreshRate", 1, 2000, 100);
            NormalizeInt("Timings", "ScrollLockStateRefreshRate", 1, 2000, 100);
            NormalizeInt("Timings", "FlagsInTrayRefreshRate", 1, 2000, 100);
            NormalizeInt("Timings", "DoubleHotkey2ndPressWait", 1, 2000, 350);
            NormalizeInt("Timings", "LangTooltipForCaretRefreshRate", 1, 2000, 25);
            NormalizeInt("Timings", "LangTooltipForMouseRefreshRate", 1, 2000, 25);
            NormalizeInt("PersistentLayout", "Layout1CheckInterval", 1, 99999, 50);
            NormalizeInt("PersistentLayout", "Layout2CheckInterval", 1, 99999, 50);

            // Display geometry remains flexible but cannot allocate absurd surfaces.
            NormalizeInt("Appearence", "CaretLTWidth", 1, 1000, 26);
            NormalizeInt("Appearence", "CaretLTHeight", 1, 1000, 14);
            NormalizeInt("Appearence", "MouseLTWidth", 1, 1000, 26);
            NormalizeInt("Appearence", "MouseLTHeight", 1, 1000, 14);
            NormalizeInt("Appearence", "Layout1Width", 1, 1000, 26);
            NormalizeInt("Appearence", "Layout1Height", 1, 1000, 14);
            NormalizeInt("Appearence", "Layout2Width", 1, 1000, 26);
            NormalizeInt("Appearence", "Layout2Height", 1, 1000, 14);
            NormalizeInt("Appearence", "CaretLTPositionX", -10000, 10000, 8);
            NormalizeInt("Appearence", "CaretLTPositionY", -10000, 10000, 12);
            NormalizeInt("Appearence", "MouseLTPositionX", -10000, 10000, 8);
            NormalizeInt("Appearence", "MouseLTPositionY", -10000, 10000, 0);
            NormalizeInt("Appearence", "Layout1PositionX", -10000, 10000, 8);
            NormalizeInt("Appearence", "Layout1PositionY", -10000, 10000, 0);
            NormalizeInt("Appearence", "Layout2PositionX", -10000, 10000, 8);
            NormalizeInt("Appearence", "Layout2PositionY", -10000, 10000, 0);

            NormalizeInt("Updates", "Delay", 1, 300, 5);
        }
'''
old_crlf = old_lf.replace(b"\n", b"\r\n")
new_crlf = new_lf.replace(b"\n", b"\r\n")
count = data.count(old_lf) + data.count(old_crlf)
if count != 1:
    raise RuntimeError("Expected one NormalizeCriticalRanges block, found %d" % count)
if old_crlf in data:
    data = data.replace(old_crlf, new_crlf, 1)
else:
    data = data.replace(old_lf, new_lf, 1)
path.write_bytes(data)
Path(__file__).unlink()
print("Expanded Mahou.ini range validation to UI, timer, combo and display limits.")
