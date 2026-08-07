using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.Threading;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace Mahou {
	static class KMHook  { // Keyboard & Mouse Listeners & Event hook
		#region Variables
		public static string AS_IGN_RULES;
		public static bool win, alt, ctrl, shift,
			win_r, alt_r, ctrl_r, shift_r,
			shiftRP, ctrlRP, altRP, winRP, //RP = Re-Press
			awas, swas, cwas, wwas, afterEOS, afterEOL, //*was = alt/shift/ctrl was
			keyAfterCTRL, keyAfterALT, keyAfterALTGR, keyAfterSHIFT,
			keyAfterCTRLSHIFT, keyAfterALTSHIFT,
			clickAfterCTRL, clickAfterALT, clickAfterSHIFT,
			hotkeywithmodsfired, csdoing, incapt, waitfornum, 
			IsHotkey, ff_chr_wheeled, LMB_down, RMB_down, MMB_down,
			dbl_click, click, selfie, JKLERR, JKLERRchecking,
			AS_IGN_BACK, AS_IGN_DEL, AS_IGN_LS, was_back, was_del, was_ls, L_DOWN, 
			CLW_W_SPACE, CLW_W_ENTER, CTRL_ALT_changelayout_temporary, CTRL_ALT_Layout_loaded;
		public static uint CTRL_ALT_prev_layout;
	    public static string AS_END_symbols = "";
		public static System.Timers.Timer click_reset = new System.Timers.Timer();
		public static Keys skip_up = Keys.None, prev_up = Keys.None;
		public static System.Timers.Timer JKLERRT = new System.Timers.Timer();
		public static int skip_mouse_events, skip_spec_keys, cursormove = -1, guess_tries, skip_kbd_events, AS_IGN_TIMEOUT;
		static char sym = '\0'; static bool sym_upr = false;
		static uint as_lword_layout = 0;
		public static uint last_switch_layout = 0;
		static uint cs_layout_last = 0;
		static string busy_on = "", lastLWClearReason = "";
		public static NativeClipboard.OleSnapshot lastClip;
		static readonly object clipboardBackupSync = new object();
		static bool clipboardBackupPending;
		static int manualConversionInProgress;
		static int manualConversionCooldownUntil;
		const int ManualConversionCooldownMs = 120;
		const int WordManualConversionCooldownMs = 300;
		const int MaxCaretWordCharacters = 256;
		public static string symbolclear;
		static List<Keys> tempNumpads = new List<Keys>();
		static Keys preKey = Keys.None, prevKEY; //, seKeyDown = Keys.None, aseKeyDown = Keys.None;
		static Keys altwait = Keys.None;
		static readonly List<char> autoSwitchText = new List<char>();
		static string lastAutoSwitchText = "";
		public static System.Windows.Forms.Timer doublekey = new System.Windows.Forms.Timer();
		public static System.Timers.Timer AS_IGN_RESET = null;
		public static List<YuKey> c_word_backup = new List<YuKey>();
		public static List<YuKey> c_word_backup_last = new List<YuKey>();
		public static List<IntPtr> PLC_HWNDs = new List<IntPtr>();
		/// <summary> Created for faster check if program is excluded, when checkin too many times(in hooks, timers etc.). </summary>
		public static List<IntPtr> EXCLUDED_HWNDs = new List<IntPtr>(); 
		public static Stopwatch pif = new Stopwatch();
		public static List<IntPtr> NOT_EXCLUDED_HWNDs = new List<IntPtr>(); 
		public static List<IntPtr> AS_NOT_EXCLUDED_HWNDs = new List<IntPtr>(); 
		public static List<IntPtr> AS_EXCLUDED_HWNDs = new List<IntPtr>(); 
		public static List<IntPtr> ConHost_HWNDs = new List<IntPtr>();
		public static string[] as_wrongs;
		public static string[] as_corrects;
		static DICT<string,string> DefaultTransliterationDict = new DICT<string, string>( new Dictionary<string,string>() {
				{"Ð©", "SCH"}, {"Ñ‰", "sch"}, {"Ð§", "CH"}, {"Ð¨", "SH"}, {"Ð", "JO"}, {"Ð’Ð’", "W"},
				{"Ð„", "EH"}, {"ÑŽ", "yu"}, {"Ñ", "ya"}, {"Ñ”", "eh"}, {"Ð–", "ZH"},
				{"Ñ‡", "ch"}, {"Ñˆ", "sh"}, {"Ð™", "JJ"}, {"Ð¶", "zh"},
				{"Ð­", "EH"}, {"Ð®", "YU"}, {"Ð¯", "YA"}, {"Ð¹", "jj"}, {"Ñ‘", "jo"}, 
				{"Ñ", "eh"}, {"Ð²Ð²", "w"}, {"ÐºÑŒ", "q"}, {"ÐšÐ¬", "Q"},
				{"ÑŒ", "j"}, {"â„–", "#"}, {"Ð", "A"}, {"Ð‘", "B"},
				{"Ð’", "V"}, {"Ð“", "G"}, {"Ð”", "D"}, {"Ð•", "E"}, {"Ð—", "Z"}, 
				{"Ð˜", "I"}, {"Ðš", "K"}, {"Ð›", "L"}, {"Ðœ", "M"}, {"Ð", "N"},
				{"Ðž", "O"}, {"ÐŸ", "P"}, {"Ð ", "R"}, {"Ð¡", "S"}, {"Ð¢", "T"},
				{"Ð£", "U"}, {"Ð¤", "F"}, {"Ð¥", "H"}, {"Ð¦", "C"}, {"Ðª", "'"}, 
				{"Ð°", "a"}, {"Ð±", "b"}, {"Ð²", "v"}, {"Ð³", "g"}, {"Ð´", "d"},
				{"Ð·", "z"}, {"Ð¸", "i"}, {"Ðº", "k"}, {"Ð»", "l"}, {"Ð¼", "m"},
				{"Ð½", "n"}, {"Ð¾", "o"}, {"Ð¿", "p"}, {"Ñ€", "r"}, {"Ñ", "s"}, 
				{"Ñƒ", "u"}, {"Ñ„", "f"}, {"Ñ…", "h"}, {"Ñ†", "c"}, {"ÑŠ", ":"},
				{"Ð«", "Y"}, {"Ð¬", "J"}, {"Ðµ", "e"}, {"Ñ‚", "t"}, {"Ñ‹", "y"}
        });
		static DICT<string,string> LayReplDict = new DICT<string, string>(new Dictionary<string, string>() {
			{"Ã¤", "Ñ"},{"Ñ", "Ã¤"},{"Ã¶", "Ð¶"},{"Ð¶", "Ã¶"},
			{"Ã¼", "Ñ…"},{"Ñ…", "Ã¼"},{"Ã„", "Ð­"},{"Ð­", "Ã„"},
			{"Ã–", "Ð–"},{"Ð–", "Ã–"},{"Ãœ", "Ð¥"},{"Ð¥", "Ãœ"},
			{"Ð¯", "Y"},{"Y", "Ð¯"},{"Ð", "Z"},{"Z", "Ð"},
			{"Ñ", "y"},{"y", "Ñ"},{"Ð½", "z"},{"z", "Ð½"},
			{"-", "ÃŸ"}
        });
		static DICT<string, string> ASsymDiffDICT = new DICT<string, string>(new Dictionary<string, string>() {
     	    {"z", "y"}, { "Z", "Y" }
		});
		static DICT<string, string> CustomConversionDICT = new DICT<string, string>(new Dictionary<string, string>() {
        	{"abc", "xyz"}, { "s/^hold/bold/", "" } 
		});
		static DICT<string, string> transliterationDict = DefaultTransliterationDict;
		static DICT<int, DICT<int, int>> LKDict;
		#endregion
		#region Keyboard, Mouse & Event hooks callbacks
		public static void ListenKeyboard(int vkCode, uint MSG, short Flags = 0) {
			if (skip_kbd_events > 0) {
				skip_kbd_events--;
				return;
			}
			if (skip_up != Keys.None) {
				Debug.WriteLine("Skip up!"+skip_up);
				if (MSG == WinAPI.WM_KEYUP || MSG == WinAPI.WM_SYSKEYUP) {
					if (vkCode == (int)skip_up) {
						skip_up = Keys.None;
					}
				}
				return;
			}
			if (altwait != Keys.None) {
				if (alt) { alt = IsKDown(Keys.LMenu); }
				if (alt_r) { alt_r = IsKDown(Keys.RMenu); }
				if (shift) { shift = IsKDown(Keys.LShiftKey); }
				if (shift_r) { shift_r = IsKDown(Keys.RShiftKey); }
				if (ctrl) { ctrl = IsKDown(Keys.LControlKey); }
				if (ctrl_r) { ctrl_r = IsKDown(Keys.RControlKey); }
				if (win) { win = IsKDown(Keys.LWin); }
				if (win_r) { win_r = IsKDown(Keys.RWin); }
				if (!alt && !alt_r) { 
					altwait = Keys.None; } else {
					return;
				}
			}
			if (MahouUI.CaretLangTooltipEnabled)
				ff_chr_wheeled = false;
			if (vkCode > 254) return;
			var down = (MSG == WinAPI.WM_SYSKEYDOWN) || (MSG == WinAPI.WM_KEYDOWN);
			var Key = (Keys)vkCode; // "Key" will further be used instead of "(Keys)vkCode"
			if ((alt||alt_r) && Key == Keys.Tab) {
					altwait = Keys.None;
					if (alt) altwait = Keys.LMenu;
					if (alt_r) altwait = Keys.RMenu;
			}
			if (MMain.c_words.Count == 0) {
				MMain.c_words.Add(new List<YuKey>());
			}
			if ((Key < Keys.D0 || Key > Keys.D9) && waitfornum && (uint)Key != MMain.mahou.HKConMorWor.VirtualKeyCode && down)
				MMain.mahou.FlushConvertMoreWords();
			#region Checks modifiers that are down
			switch (Key) {
				case Keys.LShiftKey:   shift = down; break;
				case Keys.LControlKey: ctrl = down; break;
				case Keys.LMenu:       alt = down; break;
				case Keys.LWin:        win = down; break;
				case Keys.RShiftKey:   shift_r = down; break;
				case Keys.RControlKey: ctrl_r = down; break;
				case Keys.RMenu:       alt_r = down; break;
				case Keys.RWin:        win_r = down; break;
			}
//			shift = IsKDown(Keys.LShiftKey);
//			shift_r = IsKDown(Keys.RShiftKey);
//			ctrl = IsKDown(Keys.LControlKey);
//			ctrl_r = IsKDown(Keys.RControlKey);
//			alt = IsKDown(Keys.LMenu);
//			alt_r = IsKDown(Keys.RMenu);
//			win = IsKDown(Keys.LWin);
//			win_r = IsKDown(Keys.RWin);
			// Additional fix for scroll tip.
			if (MahouUI.ScrollTip && Key == Keys.Scroll && down) {
				DoSelf(() => {
					KeybdEvent(Keys.Scroll, 0);
					KeybdEvent(Keys.Scroll, 2);
	              }, "scroll_tip_fix");
			}
			uint mods = 0;
			if (alt || alt_r)
				mods += WinAPI.MOD_ALT;
			if (ctrl || ctrl_r)
				mods += WinAPI.MOD_CONTROL;
			if (shift || shift_r)
				mods += WinAPI.MOD_SHIFT;
			if (win || win_r)
				mods += WinAPI.MOD_WIN;
			if (MMain.mahou.HasHotkey(new Hotkey(false, (uint)Key, mods, 77))) {
				IsHotkey = true;
			} else
				IsHotkey = false;
//			Console.WriteLine("Pressed hotkey?: "+IsHotkey+" => ["+Key+"+"+mods+"] .");
			if ((Key >= Keys.D0 || Key <= Keys.D9) && waitfornum)
				IsHotkey = true;
			if (MahouUI.OnceSpecific && !down) {
				MahouUI.OnceSpecific = false;
			}
			var printable = ((Key >= Keys.D0 && Key <= Keys.Z) || // This is 0-9 & A-Z
			                 Key >= Keys.Oem1 && Key <= Keys.OemBackslash || // Other printable
							(Control.IsKeyLocked(Keys.NumLock) && ( // while numlock is on
						     Key >= Keys.NumPad0 && Key <= Keys.NumPad9)) || // Numpad numbers 
						     Key == Keys.Decimal || Key == Keys.Subtract || Key == Keys.Multiply ||
						     Key == Keys.Divide || Key == Keys.Add); // Numpad symbols
			var printable_mod = !win && !win_r && !alt && !alt_r && !ctrl && !ctrl_r; // E.g. only shift is PrintAble
			//Key log
			Logging.Log("[KEY] > Catched Key=[" + Key + "] with VKCode=[" + vkCode + "] and message=[" + (int)MSG + "], modifiers=[" + 
			            (shift ? "L-Shift" : "") + (shift_r ? "R-Shift" : "") + 
			            (alt ? "L-Alt" : "") + (alt_r ? "R-Alt" : "") + 
			            (ctrl ? "L-Ctrl" : "") + (ctrl_r ? "R-Ctrl" : "") + 
			            (win ? "L-Win" : "") + (win_r ? "R-Win" : "") + "].");
			// Anti C-A-DEL C & A stuck rule
			if (Key == Keys.Delete) {
				if (ctrl && alt)
					ctrl = alt = false;
				if (ctrl && alt_r)
					ctrl = alt_r = false;
				if (ctrl_r && alt_r)
					ctrl_r = alt_r = false;
				if (ctrl_r && alt)
					ctrl_r = alt = false;
			}
			// Anti win-stuck rule
			if (Key == Keys.L) {
				L_DOWN = MSG == WinAPI.WM_KEYDOWN || MSG == WinAPI.WM_SYSKEYDOWN;
			}
			if (Key == Keys.LWin) {
				win = MSG == WinAPI.WM_KEYDOWN || MSG == WinAPI.WM_SYSKEYDOWN;
			}
			if (Key == Keys.RWin) {
				win_r = MSG == WinAPI.WM_KEYDOWN || MSG == WinAPI.WM_SYSKEYDOWN;
			}
			if ((preKey == Keys.LWin && Key != Keys.LWin)  || (preKey == Keys.RWin && Key != Keys.RWin)) {
				preKey = Keys.None;
				Debug.WriteLine("Fix stuck preKey Win");
			}
			if (L_DOWN && (win || win_r)) {
				preKey = Keys.None;
				L_DOWN = win = win_r = false;
				LLHook.ClearModifiers();
				return;
			}
			// Clear currentLayout in MMain.mahou rule
			if (!MahouUI.UseJKL || KMHook.JKLERR)
				if (((win || alt || ctrl || win_r || alt_r || ctrl_r) && Key == Keys.Tab) ||
				    win && (Key != Keys.None && 
				            Key != Keys.LWin && 
				            Key != Keys.RWin)) // On any Win+[AnyKey] hotkey
					MahouUI.currentLayout = 0;
			if (!down && (
			    ((alt || ctrl || alt_r || ctrl_r) && (Key == Keys.Shift || Key == Keys.LShiftKey || Key == Keys.RShiftKey)) ||
			     shift && (Key == Keys.Menu || Key == Keys.LMenu || Key == Keys.RMenu) ||
			     (IfNW7() && (win || win_r) && Key == Keys.Space))) {
				if (!MahouUI.UseJKL || KMHook.JKLERR) {
					var time = 200;
					if (IfNW7())
						time = 50;
					MahouUI.currentLayout = 0;
					as_lword_layout = 0;
					DoLater(() => { MahouUI.GlobalLayout = MahouUI.currentLayout = Locales.GetCurrentLocale(); AS_IGN_fun(); }, time);
				}
			}
			#endregion
			if (MahouUI.CTRL_ALT_TemporaryLayout != 0) {
				if (down) {
					if (((Key == Keys.LMenu && ctrl) ||
					    ((Key == Keys.LControlKey) && alt)) && !CTRL_ALT_changelayout_temporary) {
						CTRL_ALT_prev_layout = (MahouUI.UseJKL && !JKLERR) ? MahouUI.currentLayout : Locales.GetCurrentLocale();
						uint sh1 = CTRL_ALT_prev_layout&0xffff, sh2 = MahouUI.CTRL_ALT_TemporaryLayout&0xffff;
						uint lo1 = CTRL_ALT_prev_layout>>16, lo2 = MahouUI.CTRL_ALT_TemporaryLayout >> 16;
						if (!(sh1 == sh2 && lo1 == lo2) && CTRL_ALT_prev_layout != 0) {
							CTRL_ALT_changelayout_temporary = true;
							var is_loaded = false;
							foreach (var l in MMain.locales) {
								if ((l.uId & 0xffff) == sh2 && (l.uId >> 16) == lo2) {
									is_loaded = true;
									break;
								}
							}
							if (!is_loaded) {
								var x = MahouUI.CTRL_ALT_TemporaryLayout.ToString("X");
								if (x.Length<7) {
									var zeroes = "";
									for(int i = 0; i!= 7-x.Length; i++) {
										zeroes += "0";
									}
									x = zeroes + x;
								}
								Logging.Log("[LCTRLLALT] > Loading layout: " +x);
								WinAPI.LoadKeyboardLayout(x, 1);
								Thread.Sleep(15);
								CTRL_ALT_Layout_loaded = true;
							}
							NormalChangeToLayout(Locales.ActiveWindow(), MahouUI.CTRL_ALT_TemporaryLayout);
							Logging.Log("[LCTRLLALT] > SWITCH TO LAYOUT" + MahouUI.CTRL_ALT_TemporaryLayout);
						}
					}
				}
				if (MSG == WinAPI.WM_KEYUP || MSG == WinAPI.WM_SYSKEYUP) {
					Debug.WriteLine("RELEASE: " +Key + " " +CTRL_ALT_prev_layout);
					if (CTRL_ALT_changelayout_temporary) {
						var swtch_back = false;
						if (Key == Keys.LControlKey) {
							if (prev_up == Keys.LMenu) {
								swtch_back = true;
							}
							prev_up = Key;
						}
						if (Key == Keys.LMenu) {
							if (prev_up == Keys.LControlKey) {
								swtch_back = true;
							}
							prev_up = Key;
						}
						if (swtch_back) {
							if (CTRL_ALT_prev_layout != 0) {
								if (CTRL_ALT_Layout_loaded) {
									var success = WinAPI.UnloadKeyboardLayout((IntPtr)MahouUI.CTRL_ALT_TemporaryLayout);
									Logging.Log("[LCTRLLALT] > Unload layout: "+MahouUI.CTRL_ALT_TemporaryLayout + " success: "+success);
								}
								Logging.Log("[LCTRLLALT] > SWITCH BACK TO LAYOUT" + CTRL_ALT_prev_layout);
								NormalChangeToLayout(Locales.ActiveWindow(), CTRL_ALT_prev_layout);
							}
							prev_up = Keys.None;
							CTRL_ALT_changelayout_temporary = false;
							CTRL_ALT_prev_layout = 0;
						}
					}
				}
			}
			#region
			if (MahouUI.LangPanelDisplay || MahouUI.MouseLangTooltipEnabled || MahouUI.CaretLangTooltipEnabled)
				if (MahouUI.LangPanelUpperArrow || MahouUI.mouseLTUpperArrow || MahouUI.caretLTUpperArrow) {
					sym = getSym(vkCode);
			}
			var ku = IsUpperInput();
			if (MahouUI.LangPanelDisplay)
				if (MahouUI.LangPanelUpperArrow)
					MMain.mahou._langPanel.DisplayUpper(ku);
			if (MahouUI.MouseLangTooltipEnabled)
				if (MahouUI.mouseLTUpperArrow)
					MMain.mahou.mouseLangDisplay.DisplayUpper(ku);
			if (MahouUI.CaretLangTooltipEnabled)
				if (MahouUI.caretLTUpperArrow)
					MMain.mahou.caretLangDisplay.DisplayUpper(ku);
			#endregion
			#region InputHistory
			if (MahouUI.WriteInputHistory) {
				if ((printable || Key == Keys.Enter || Key == Keys.Space) && printable_mod && down) {
					if (sym ==ãMµæÚ$z{-®éÜj×æv–ærÆ–÷WBFòæW‡Bv—F‚÷7DÖW76vRæBtÕô”åUDÄät4„ätU$UTU5BæBÅ&Ò„´ÅôäU…BàÐ ’òòòÂ÷7VÖÖ'“àÐ —V&Æ–27FF–2fö–B7–6ÆTÆ–÷WE7v—F6‚‚’°Ð ”FV'Vråw&—FTÆ–æR‚#ãâ4Å2"“°Ð ”Æövv–æräÆör‚$6†æv–ærÆ–÷WBW6–ær7–6ÆRÖöFR'’6VæF–ærÖW76vRµv–ä’åtÕô”åUDÄät4„ätU$UTU5EÒv—F‚Å&Ò´„´ÅôäU…EÒW6–ærv–ä’å÷7DÖW76vRFò7F—fUv–æF÷r"“°Ð ’òõW6Rv–ä’å÷7DÖW76vRFò7v—F6‚FòæW‡BÆ–÷W@Ð ”6†ævUFôÆ–÷WB„Æö6ÆW2ä7F—fUv–æF÷r‚’ÂvWDæW‡DÆ–÷WB‚’çT–B“°Ð —ÐÐ —7FF–26†"FõVæ–6öFTW„×VÇF’‡V–çBf²Â–çEG"Æ–÷WBÂ&ööÂWW"ÒfÇ6R’°Ð —f"fÆw2ÒæWrµ×³ÃÃ"Â"ÂÓ°Ð —f"2ÒæWr7G&–æt'V–ÆFW"ƒ“°Ð —f"'—BÒæWr'—FU³#SeÓ°Ð ––b‡WW"’°Ð –'—E²†–çB”¶W—2å6†–gD¶W•ÒÒ„dc°Ð —ÐÐ —f"g62Ò‡V–çB•v–ä’äÖf—'GVÄ¶W’‡f²Â“°Ð ––çBG"Ò°Ð –f÷&V6‚‡V–çBb–âfÆw2’°Ð —G"²³°Ð •v–ä’åFõVæ–6öFTW‚‡f²Âg62Â'—BÂ2Â2ä66—G’ÂbÂÆ–÷WB“°Ð ––b‡2äÆVæwF‚â’°Ð ”Æövv–æräÆör‚%µFõVæ”W…ÒG'“¢"·G"²"6†#¢"·2²"BfÆs¢"²b“°Ð —&WGW&â2åFõ7G&–ær‚•³Ó°Ð —ÐÐ —ÐÐ —&WGW&âuÃs°Ð —ÐÐ ’òòòÇ7VÖÖ'“àÐ ’òòò6öçfW'G26†&7FW"†2’g&öÒÆ–÷WB‡T”C’Fòæ÷F†W"Æ–÷WB‡T”C"’'’W6–ærv–ä’åFõVæ–6öFTW‚‚’àÐ ’òòòÂ÷7VÖÖ'“àÐ ’òòòÇ&ÒæÖSÒ&2#ä6†&7FW"Fò&R6öçfW'FVBãÂ÷&ÓàÐ ’òòòÇ&ÒæÖSÒ'T”C#äÆ–÷WB–B†g&öÒ’ãÂ÷&ÓàÐ ’òòòÇ&ÒæÖSÒ'T”C"#äÆ–÷WB–B"‡Fò“Â÷&ÓàÐ ’òòòÇ&WGW&ç3ãÂ÷&WGW&ç3àÐ —7FF–27G&–ær–äæ÷F†W"†6†"2ÂV–çBT”CÂV–çBT”C"’²òõ&VÖ¶W22g&öÒT”CFòT”C Ð —f"62Ò3°Ð —f"2Ò"#°Ð —f"6‡62Òv–ä’åf´¶W•66äW‚†62ÂT”C“°Ð ––b†6‡62ÓÒÓ’&WGW&â3°Ð —f"7FFRÒ†6‡62ãâ‚’b†fc°Ð Ð¢òð—f"'—BÒæWr'—FU³#SeÓ°Ð¢òð’òö—BæVVG2§W7B'WBÆç—v’ÆWB—B&RÂ’F†–æ²F†Bw2&WGFW Ð —f"’ÒFõVæ–6öFTW„×VÇF’‚‡V–çB–6‡62Â„–çEG"’‚†–çB—T”C"’Â7FFSÓÓ“°Ð ––b‡’ÒuÃr’2³×“°Ð¢òð’òô6†V6·2–bv6‡62r†fRWW"7FFPÐ¢òð––b‡7FFRÓÒ’°Ð¢òð–'—E²†–çB”¶W—2å6†–gD¶W•ÒÒ„dc°Ð¢òð—ÐÐ¢òð’òò$6öçfW'BÖv–>)Ê’"—2F†R7G&–ær&VÆ÷pÐ¢òð—f"çBÒv–ä’åFõVæ–6öFTW‚‚‡V–çB–6‡62Â‡V–çB–6‡62Â'—BÂ2Â2ä66—G’ÂÃÃ"Â„–çEG"—T”C"“°Ð —&WGW&â3°Ð —ÐÐ ’òòòÇ7VÖÖ'“àÐ ’òòò6–×Æ–f–VBv–ä’æ¶W–&EöWfVçB‚’v—F‚W‡FVæFVB&V6övæ—¦RfVGW&RàÐ ’òòòÂ÷7VÖÖ'“àÐ ’òòòÇ&ÒæÖSÒ&¶W’#ä¶W’Fò&R–çWGFVBãÂ÷&ÓàÐ ’òòòÇ&ÒæÖSÒ&fÆw2#äfÆw2‡7FFR’öb¶W’ãÂ÷&ÓàÐ —V&Æ–27FF–2fö–B¶W–&DWfVçB„¶W—2¶W’Â–çBfÆw2’²òò Ð ’òôFòæ÷B&VÖ÷fRF†—2Æ–æRÂ—BæVVFVBf÷"$ÆVgB6öçG&öÂ7v—F6‚Æ–÷WB"Fòv÷&²&÷W&ÇÐ¢òð•F‡&VBå6ÆVWƒR“°Ð —f"62Ò‡V–çB•v–ä’äÖf—'GVÄ¶W’‚‡V–çB–¶W’ÂB“°Ð ”FV'Vråw&—FTÆ–æR‚'66â"²‡63ãã‚’“°Ð •v–ä’æ¶W–&EöWfVçB‚†'—FR–¶W’Â†'—FR’‡62b†fb’ÂfÆw2Â‚‡62ãâ‚’Òò¢’Â“°Ð —ÐÐ —V&Æ–27FF–2fö–B&U&W74gFW"†–çBÖöG2’°Ð –7G&Å%Ò†÷F¶W’ä6öçF–ç4ÖöF–f–W"†ÖöG2Â†–çB•v–ä’äÔôEô4ôåE$ôÂ“°Ð —6†–gE%Ò†÷F¶W’ä6öçF–ç4ÖöF–f–W"†ÖöG2Â†–çB•v–ä’äÔôEõ4„”eB“°Ð –ÇE%Ò†÷F¶W’ä6öçF–ç4ÖöF–f–W"†ÖöG2Â†–çB•v–ä’äÔôEôÅB“°Ð —v–å%Ò†÷F¶W’ä6öçF–ç4ÖöF–f–W"†ÖöG2Â†–çB•v–ä’äÔôEõt”â“°Ð —ÐÐ —V&Æ–27FF–2&ööÂ—4´F÷vâ„¶W—2²’°Ð —&WGW&â…v–ä’ävWD7–æ4¶W•7FFR‚†–çB–²’bƒƒ’Ò°Ð —ÐÐ —V&Æ–27FF–2fö–Bv—D¶W“$'&VÆV6VB„¶W—2¶W’’°Ð –&ööÂ²ÒG'VS°Ð —v†–ÆR†²’°Ð –²Ò—4´F÷vâ†¶W’“°Ð •F‡&VBå6ÆVWƒR“°Ð ”FV'Vråw&—FTÆ–æR‚&²"¶²“°Ð —ÐÐ —ÐÐ ’òòòÇ7VÖÖ'“àÐ ’òòò6VæG2ÖöF–f–W'2W'’ÖöG7F÷W'&’â Ð ’òòòÂ÷7VÖÖ'“àÐ ’òòòÇ&ÒæÖSÒ&ÖöG7F÷W#ä'&’öbÖöF–f–W'2v†–6‚v–ÆÂ&R6VæBWâÒ7G&ÂÂÒ6†–gBÂ"ÒÇBãÂ÷&ÓàÐ —V&Æ–27FF–2fö–B6VæDÖöG5W†–çBÖöG7F÷WÂ&ööÂv—Gv–âÒfÇ6R’²òðÐ ’òõF†W6RF‡&VR&VÆ÷r&RæVVFVBFò&VÆV6RÆÂÖöF–f–W'2Â6òWfVâ–b–÷Rv–ÆÂ7F–ÆÂ†öÆBç’öb—@Ð ’òö—Bv–ÆÂ6¶—F†VÒæBFò2—B×W7BàÐ ––b†ÖöG7F÷WÃÒ’&WGW&ã°Ð ”FV'Vråw&—FTÆ–æR‚#ãâ4ÕS¢"²†÷F¶W’ävWDÖöG2†ÖöG7F÷W’“°Ð ”Fõ6VÆb‚‚’Óâ°Ð¢ –'—FUµÒ7FFRÒæWr'—FU³#SeÓ°Ð¢ •v–ä’ävWD¶W–&ö&E7FFR‡7FFR“°Ð ’ —f"ÖöG5UÒ"#°Ð ––b„†÷F¶W’ä6öçF–ç4ÖöF–f–W"†ÖöG7F÷WÂ†–çB•v–ä’äÔôEõt”â’’°Ð ’ ––b‡v—Gv–â—°Ð •v—D¶W“$'&VÆV6VB„¶W—2äÅv–â“°Ð •v—D¶W“$'&VÆV6VB„¶W—2å%v–â“°Ð —v–âÒv–å÷"ÒfÇ6S°Ð ”ÄÄ†öö²å6WDÖöF–f–W"…v–ä’äÔôEõt”âÂfÇ6R“°Ð ”ÄÄ†öö²å6WDÖöF–f–W"…v–ä’äÔôEõt”âÂfÇ6RÂfÇ6R“°Ð —7FFU²†–çB”¶W—2äÅv–åÒÃÒƒƒ°Ð —7FFU²†–çB”¶W—2å%v–åÒÃÒƒƒ°Ð –ÖöG5U³Ò$Åv–âÅ%v–âÂ#°Ð ’ —ÒVÇ6R°Ð ’ ––b„—4´F÷vâ„¶W—2äÅv–â’’°Ð ’ ”´–çWG2äÖ¶T–çWB„´–çWG2äFE&W72„¶W—2äÄ6öçG&öÄ¶W’’“°Ð ”´Ô†öö²ä¶W–&DWfVçB„¶W—2äÅv–âÂ"“²òòÆVgBv–âW Ð —7FFU²†–çB”¶W—2äÅv–åÒÃÒƒƒ°Ð —v–âÒfÇ6S°Ð ”ÄÄ†öö²å6WDÖöF–f–W"…v–ä’äÔôEõt”âÂfÇ6R“°Ð –ÖöG5U³Ò$Åv–âÂ#°Ð ’ —ÐÐ ’ ––b„—4´F÷vâ„¶W—2å%v–â’’°Ð ’ ”´–çWG2äÖ¶T–çWB„´–çWG2äFE&W72„¶W—2äÄ6öçG&öÄ¶W’’“°Ð ”´Ô†öö²ä¶W–&DWfVçB„¶W—2å%v–âÂ"“²òò&–v‡Bv–âW Ð —7FFU²†–çB”¶W—2å%v–åÒÃÒƒƒ°Ð —v–å÷"ÒfÇ6S°Ð ”ÄÄ†öö²å6WDÖöF–f–W"…v–ä’äÔôEõt”âÂfÇ6RÂfÇ6R“°Ð –ÖöG5U³Ò%%v–âÂ#°Ð ’ —ÐÐ ’ —ÐÐ —ÐÐ ––b„†÷F¶W’ä6öçF–ç4ÖöF–f–W"†ÖöG7F÷WÂ†–çB•v–ä’äÔôEõ4„”eB’’°Ð ’ ––b„—4´F÷vâ„¶W—2å%6†–gD¶W’’’°Ð ”´Ô†öö²ä¶W–&DWfVçB„¶W—2å%6†–gD¶W’Â"“²òò&–v‡B6†–gBW Ð —7FFU²†–çB”¶W—2å%6†–gD¶W•ÒÃÒƒƒ°Ð —6†–gE÷"ÒfÇ6S°Ð ”ÄÄ†öö²å6WDÖöF–f–W"…v–ä’äÔôEõ4„”eBÂfÇ6RÂfÇ6R“°Ð –ÖöG5U³Ò%%6†–gBÂ#°Ð ’ —ÐÐ ’ ––b„—4´F÷vâ„¶W—2äÅ6†–gD¶W’’’°Ð ”´Ô†öö²ä¶W–&DWfVçB„¶W—2äÅ6†–gD¶W’Â"“²òòÆVgB6†–gBW Ð —7FFU²†–çB”¶W—2äÅ6†–gD¶W•ÒÃÒƒƒ°Ð ”ÄÄ†öö²å6WDÖöF–f–W"…v–ä’äÔôEõ4„”eBÂfÇ6R“°Ð —6†–gBÒfÇ6S°Ð –ÖöG5U³Ò$Å6†–gBÂ#°Ð —ÐÐ —ÐÐ ––b„†÷F¶W’ä6öçF–ç4ÖöF–f–W"†ÖöG7F÷WÂ†–çB•v–ä’äÔôEô4ôåE$ôÂ’’°Ð ’ ––b„—4´F÷vâ„¶W—2å$6öçG&öÄ¶W’’’°Ð ”´Ô†öö²ä¶W–&DWfVçB„¶W—2å$6öçG&öÄ¶W’Â"“²òò&–v‡B6öçG&öÂW Ð —7FFU²†–çB”¶W—2å$6öçG&öÄ¶W•ÒÃÒƒƒ°Ð –7G&Å÷"ÒfÇ6S°Ð ”ÄÄ†öö²å6WDÖöF–f–W"…v–ä’äÔôEô4ôåE$ôÂÂfÇ6R“°Ð –ÖöG5U³Ò%$7G&ÂÂ#°Ð ’ —ÐÐ ’ ––b„—4´F÷vâ„¶W—2äÄ6öçG&öÄ¶W’’’°Ð ”´Ô†öö²ä¶W–&DWfVçB„¶W—2äÄ6öçG&öÄ¶W’Â"“²òòÆVgB6öçG&öÂW Ð —7FFU²†–çB”¶W—2äÄ6öçG&öÄ¶W•ÒÃÒƒƒ°Ð –7G&ÂÒfÇ6S°Ð ”ÄÄ†öö²å6WDÖöF–f–W"…v–ä’äÔôEô4ôåE$ôÂÂfÇ6RÂfÇ6R“°Ð –ÖöG5U³Ò$Ä7G&ÂÂ#°Ð ’ —ÐÐ —ÐÐ ––b„†÷F¶W’ä6öçF–ç4ÖöF–f–W"†ÖöG7F÷WÂ†–çB•v–ä’äÔôEôÅB’’°Ð ’ ––b„—4´F÷vâ„¶W—2å$ÖVçR’’°Ð ’ ”´–çWG2äÖ¶T–çWB„´–çWG2äFE&W72„¶W—2äÄ6öçG&öÄ¶W’’“°Ð ”´Ô†öö²ä¶W–&DWfVçB„¶W—2å$ÖVçRÂ"“²òò&–v‡BÇBW Ð —7FFU²†–çB”¶W—2å$ÖVçUÒÃÒƒƒ°Ð –ÇE÷"ÒfÇ6S°Ð ”ÄÄ†öö²å6WDÖöF–f–W"…v–ä’äÔôEôÅBÂfÇ6RÂfÇ6R“°Ð –ÖöG5U³Ò%$ÇBÂ#°Ð ’ —ÐÐ ’ ––b„—4´F÷vâ„¶W—2äÄÖVçR’’°Ð ’ ”´–çWG2äÖ¶T–çWB„´–çWG2äFE&W72„¶W—2äÄ6öçG&öÄ¶W’’“²òò7F÷ÖVçRfö7W0Ð ”´Ô†öö²ä¶W–&DWfVçB„¶W—2äÄÖVçRÂ"“²òòÆVgBÇBW Ð —7FFU²†–çB”¶W—2äÄÖVçUÒÃÒƒƒ°Ð –ÇBÒfÇ6S°Ð ”ÄÄ†öö²å6WDÖöF–f–W"…v–ä’äÔôEôÅBÂfÇ6R“°Ð –ÖöG5U³Ò$ÄÇBÂ#°Ð ’ —ÐÐ ’ —f"6Ç4äÒÒÆö6ÆW2ä7F—fUv–æF÷t6Æ74æÖRƒCÂv–ä’ävWDf÷&Vw&÷VæEv–æF÷r‚’“°Ð ’ ––b†6Ç4äÒå7F'G5v—F‚‚%CR"’’°Ð ’ •F‡&VBå6ÆVWƒsR“°Ð ’ —ÐÐ —ÐÐ •v–ä’å6WD¶W–&ö&E7FFR‡7FFR“°Ð ’ ”Æövv–æräÆör‚$ÖöF–f–W'2²"²‚†ÖöG5UäÆVæwF‚ã"’òÖöG5Uå7V'7G&–ærƒÆÖöG5UäÆVæwF‚Ó’¢""’²%Ò6VçBWâ"“°Ð¢ÒÂ'6VæFÖöG7W"“°Ð —ÐÐ ’òòòÇ7VÖÖ'“àÐ ’òòò6†V6·2–b¶W’—2ÖöF–f–W"ÂæB6ÆÇ26VæDÖöG5W‚’–b—B—2àÐ ’òòòÂ÷7VÖÖ'“àÐ ’òòòÇ&ÒæÖSÒ&¶W’#ä¶W’Fò&R6†V6¶VBãÂ÷&ÓàÐ —V&Æ–27FF–2fö–B–d¶W”—4ÖöB„¶W—2¶W’’°Ð —V–çBÖöG2Ò°Ð —7v—F6‚†¶W’’°Ð –66R¶W—2äÄ6öçG&öÄ¶W“ Ð –66R¶W—2å$6öçG&öÄ¶W“ Ð –ÖöG2³Òv–ä’äÔôEô4ôåE$ôÃ°Ð –'&V³°Ð –66R¶W—2äÅ6†–gD¶W“ Ð –66R¶W—2å%6†–gD¶W“ Ð –ÖöG2³Òv–ä’äÔôEõ4„”eC°Ð –'&V³°Ð –66R¶W—2äÄÖVçS Ð –66R¶W—2å$ÖVçS Ð –66R¶W—2äÇC Ð –ÖöG2³Òv–ä’äÔôEôÅC°Ð –'&V³°Ð –66R¶W—2äÅv–ã Ð –66R¶W—2å%v–ã Ð –ÖöG2³Òv–ä’äÔôEõt”ã°Ð –'&V³°Ð —ÐÐ ––b†ÖöG2âÐ •6VæDÖöG5W‚†–çB–ÖöG2ÂfÇ6R“°Ð —ÐÐ —V&Æ–27FF–2GWÆSÇ7G&–ærÂV–çCâv÷&DwVW74Æ–÷WB‡7G&–ærv÷&BÂV–çB÷F&vWBÒÂ&ööÂvÆf—‚ÒG'VR’°Ð —V–çBÆ–÷WBÒ°Ð —7G&–ærwVW72Ò"#°Ð —V–çBF&vWBÒ°Ð ––b…÷F&vWBÓÒ’°Ð ––b„Ö†÷UT’å7v—F6„&WGvVVäÆ–÷WG2’°Ð —f"7W"Ò„Ö†÷UT’åW6T¤´Âbb´Ô†öö²ä¤´ÄU%"’òÖ†÷UT’æ7W'&VçDÆ–÷WB¢Æö6ÆW2ävWD7W'&VçDÆö6ÆR‚“°Ð —F&vWBÒ7W"ÓÒÖ†÷UT’äÔ”åôÄ”õUCòÖ†÷UT’äÔ”åôÄ”õUC"¢Ö†÷UT’äÔ”åôÄ”õUC°Ð —ÒVÇ6R Ð —F&vWBÒvWDæW‡DÆ–÷WB‚’çT–C°Ð —ÒVÇ6RF&vWBÒ÷F&vWC°Ð –f÷"†–çB’Ò²’ÒÔÖ–âæÆö6ÆW2äÆVæwFƒ²’²²’°Ð ––b„ÔÖ–âæÆö6ÆW5¶•ÒäÆærä6öçF–ç2‚$Ö–7&÷6ögBöff–6R”ÔR"’’òòf¶RÆ–÷W@Ð –6öçF–çVS°Ð —f"ÂÒÔÖ–âæÆö6ÆW5¶•ÒçT–C°Ð ––b„Ö†÷UT’å7v—F6„&WGvVVäÆ–÷WG2’°Ð ––b†ÂÓÒF&vWBÇÂ†ÂÒÖ†÷UT’äÔ”åôÄ”õUCbbÂÒÖ†÷UT’äÔ”åôÄ”õUC"’’°Ð –6öçF–çVS°Ð —ÐÐ —ÐÐ —f"Ã"ÒF&vWC°Ð ––b†ÂÓÒF&vWB’6öçF–çVS°Ð ––çBv÷&DÄÖ–çW6W2Ò°Ð ––çBv÷&DÃ$Ö–çW6W2Ò°Ð ––çBÖ–æÖ–âÒ°Ð ––çBF†—6Ö–âÒ°Ð ––çBv÷&DÄdÖ–ä–æFW‚ÒÓ°Ð ––çBv÷&DÃ$dÖ–ä–æFW‚ÒÓ°Ð —V–çBÆ’Ò°Ð —f"v÷&DÂÒæWr7G&–æt'V–ÆFW"‚“°Ð —f"v÷&DÃ"ÒæWr7G&–æt'V–ÆFW"‚“°Ð —f"×WƒÒæWr7G&–æt'V–ÆFW"‚“°Ð —f"×Wƒ"ÒæWr7G&–æt'V–ÆFW"‚“°Ð —f"&W7VÇBÒæWr7G&–æt'V–ÆFW"‚“°Ð¢òð—f"æç’ÒfÇ6S°Ð ”FV'Vråw&—FTÆ–æR‚%FW7F–ær"·v÷&B²"v–ç7C¢"¶Â²"æB"¶Ã"“°Ð –f÷"†–çB’Ò²’×v÷&BäÆVæwFƒ²’²²’°Ð —f"2Òv÷&E´•Ó°Ð ––b„6†"ä—4çVÖ&W"†2’’°Ð —v÷&DÂäVæB†2“°Ð —v÷&DÃ"äVæB†2“°Ð –×WƒäVæB†2“°Ð –×Wƒ"äVæB†2“°Ð ”FV'Vråw&—FTÆ–æR‚$â×6¶—¢"²2“°Ð –6öçF–çVS°Ð —ÐÐ —f"6ÒÒfÇ6S°Ð ––b†2ÓÒ}[‚rÇÂ2ÓÒ}X‚r’°Ð ––b†2ÓÒ}[‚r’6ÒÒG'VS°Ð ––b‡v÷&BäÆVæwF‚â’³’°Ð ––b‡v÷&E´’³ÒÓÒ}h"r’°Ð —f"6‡'BÒÃ#ããc°Ð —f"÷6‡'BÒÃããc°Ð ––b‡6‡'BÓÒ32ÇÂ6‡'BÓÒC’°Ð —v÷&DÂäVæB‡6Òò'R"¢%R"“°Ð ”’²³²6öçF–çVS°Ð —ÐÐ ––b…÷6‡'BÓÒ32ÇÂ÷6‡'BÓÒC’°Ð —v÷&DÃ"äVæB‡6Òò'R"¢%R"“°Ð ”’²³²6öçF–çVS°Ð —ÐÐ ––b‡6‡'BÓÒC’’°Ð —v÷&DÂäVæB‡6Òò-2"¢-	2"“°Ð ”’²³²6öçF–çVS°Ð —ÐÐ ––b…÷6‡'BÓÒC’’°Ð —v÷&DÃ"äVæB‡6Òò-2"¢-	2"“°Ð ”’²³²6öçF–çVS°Ð —ÐÐ —ÐÐ —ÐÐ —ÐÐ ––b†vÆf—‚’°Ð —f"C2ÒvW&ÖäÆ–÷WDf—‚†2“°Ð ”FV'Vråw&—FTÆ–æR‚$tTd•ƒ¢"¶2²"C2"µC2“°Ð ––b…C2Ò""’°Ð —v÷&DÂäVæB…C2“°Ð —v÷&DÃ"äVæB…C2“°Ð –6öçF–çVS°Ð —ÐÐ —ÐÐ ––b„Ö†÷UT’å7–Ô–väVæ&ÆVB’°Ð ––b…7–Ö&öÄ–væ÷&U'VÆW2†2’’°Ð —v÷&DÂäVæB†2“°Ð —v÷&DÃ"äVæB†2“°Ð ”FV'Vråw&—FTÆ–æR‚%7–Ö&öÂ–væ÷&VC¢"²2“°Ð –6öçF–çVS°Ð —ÐÐ —ÐÐ ––b†2ÓÒuÆâr’°Ð —v÷&DÂäVæB‚%Æâ"“°Ð —v÷&DÃ"äVæB‚%Æâ"“°Ð –×WƒäVæB‚%Æâ"“°Ð –×Wƒ"äVæB‚%Æâ"“°Ð –6öçF–çVS°Ð —ÐÐ —f"CÒ–äæ÷F†W"†2ÂÂÂÃ"“°Ð —v÷&DÂäVæB…C“°Ð –×WƒäVæB…C“°Ð ––b…CÓÒ""’²v÷&DÄÖ–çW6W2²³²×WƒäVæB†2“²–b‡v÷&DÄdÖ–ä–æFW‚ÓÒÓ’²v÷&DÄdÖ–ä–æFW‚Ò“²ÒÐÐ —f"C"Ò–äæ÷F†W"†2ÂÃ"ÂÂ“°Ð —v÷&DÃ"äVæB…C"“°Ð –×Wƒ"äVæB…C"“°Ð ––b…C"ÓÒ""’²v÷&DÃ$Ö–çW6W2²³²×Wƒ"äVæB†2“²–b‡v÷&DÃ$dÖ–ä–æFW‚ÓÒÓ’²v÷&DÃ$dÖ–ä–æFW‚Ò“²ÒÐÐ ”FV'Vråw&—FTÆ–æR‚%C¢"²C²"ÂC#¢"²C"²"Â3¢"¶2“°Ð ––b…C"ÓÒ""bbCÓÒ""’°Ð¢òð–æç’ÒG'VS°Ð ”FV'Vråw&—FTÆ–æR‚$6†"²"¶2²%Ò—2æ÷B–âç’öbGvòÆ–÷WG2²"¶Â²%ÒÂ²"¶Ã"²%Ò§W7B&Ww&—F–ærâ"“°Ð —v÷&DÂäVæB‡v÷&E´•Ò“°Ð —v÷&DÃ"äVæB‡v÷&E´•Ò“°Ð —ÐÐ —ÐÐ ––b‡v÷&DÄÖ–çW6W2âv÷&DÃ$Ö–çW6W2’°Ð —F†—6Ö–âÒv÷&DÃ$Ö–çW6W3°Ð –Æ’ÒÃ#°Ð —&W7VÇBÒv÷&DÃ#°Ð ––b‡&W7VÇBäÆVæwF‚Âv÷&BäÆVæwF‚’°Ð ”FV'Vråw&—FTÆ–æR‚$×VÇBÖÆ–÷WBv÷&B×W†VB""“°Ð —&W7VÇBÒ×Wƒ#°Ð —ÐÐ —ÐÐ –VÇ6R°Ð —F†—6Ö–âÒv÷&DÄÖ–çW6W3°Ð –Æ’ÒÃ°Ð —&W7VÇBÒv÷&DÃ°Ð ––b‡&W7VÇBäÆVæwF‚Âv÷&BäÆVæwF‚’°Ð ”FV'Vråw&—FTÆ–æR‚$×VÇBÖÆ–÷WBv÷&B×W†VB"“°Ð —&W7VÇBÒ×Wƒ°Ð —ÐÐ —ÐÐ ”FV'Vråw&—FTÆ–æR‚$VæBÂ"²Æ’²'Â"·v÷&DÂ²"Â"²v÷&DÃ"²'Â"·v÷&DÄÖ–çW6W2²"Â"·v÷&DÃ$Ö–çW6W2²"×Wƒ¢"²×Wƒ²"Â×Wƒ#¢"²×Wƒ"“°Ð ––b‡v÷&DÄÖ–çW6W2ÓÒv÷&DÃ$Ö–çW6W2’°Ð ––b‡v÷&DÄÖ–çW6W2ÓÒbbv÷&DÃ$Ö–çW6W2ÓÒ’°Ð –Æ’ÒÃ°Ð —&W7VÇBÒv÷&DÃ°Ð —ÒVÇ6R°Ð —F†—6Ö–âÒv÷&DÄÖ–çW6W3°Ð –Æ’Ò°Ð —&W7VÇBä6ÆV"‚’äVæB‡v÷&B“°Ð –&ööÂöæRÒv÷&DÄdÖ–ä–æFW‚âv÷&DÃ$dÖ–ä–æFWƒ°Ð ––b†öæR’°Ð ––b†×WƒäÆVæwF‚ÓÒv÷&BäÆVæwF‚’°Ð —&W7VÇBÒ×Wƒ°Ð —ÐÐ —ÒVÇ6PÐ ––b†×Wƒ"äÆVæwF‚ÓÒv÷&BäÆVæwF‚’°Ð —&W7VÇBÒ×Wƒ#°Ð —ÐÐ ”FV'Vråw&—FTÆ–æR‚$ÄÖ–â"²v÷&DÄdÖ–ä–æFW‚²"bÃ$Ö–ã¢"²v÷&DÃ$dÖ–ä–æFW‚²"×W‚"²†öæSò##¢#""’²"Óâ"²&W7VÇB“°Ð —ÐÐ —ÐÐ ––b‡&W7VÇBäÆVæwF‚âwVW72äÆVæwF‚ÇÂ†Æ’ÒbbF†—6Ö–âÃÒÖ–æÖ–â’’°Ð –wVW72Ò&W7VÇBåFõ7G&–ær‚“°Ð –Æ–÷WBÒÆ“°Ð —ÐÐ ––b‡F†—6Ö–âÂÖ–æÖ–âÐ –Ö–æÖ–âÒF†—6Ö–ã°Ð ––b†Æ’ÓÒF&vWB’'&V³°Ð —ÐÐ ––b‡F&vWBÓÒÆ–÷WB’ Ð –wVW72Òv÷&C°Ð ––b†Æ–÷WBÓÒF&vWB’°Ð –wVW75÷G&–W2²³°Ð ”FV'Vråw&—FTÆ–æR‚%t$ä”ärwVW72G'’²2"¶wVW75÷G&–W2²%ÒÂF&vWBÆ–÷WBæBv÷&BÆ–÷WB&R6ÖRÂF¶–æræW‡BÆ–÷WB2F&vWB"“°Ð ––b†wVW75÷G&–W2ÂÔÖ–âæÆö6ÆW2äÆVæwF‚³’°Ð —F&vWBÒvWDæW‡DÆ–÷WB‡F&vWB’çT–C°Ð ”FV'Vråw&—FTÆ–æR‚%&WG'’v—Fƒ¢Æ–÷WC¢"¶Æ–÷WB²"ÂF&vWC¢"²F&vWB“°Ð —&WGW&âv÷&DwVW74Æ–÷WB‡v÷&BÂF&vWB“°Ð —ÒVÇ6R°Ð –wVW75÷G&–W2Ò°Ð —ÐÐ —ÒVÇ6R°Ð –wVW75÷G&–W2Ò°Ð —ÐÐ ”FV'Vråw&—FTÆ–æR‚%v÷&B"²v÷&B²"Æ–÷WB—2"²Æ–÷WB²"F&vWF–æs¢"²F&vWB²"wVW73¢"²wVW72“°Ð —&WGW&âGWÆRä7&VFR†wVW72ÂÆ–÷WB“°Ð —ÐÐ ––çFW&æÂ7FF–2WFõ7v—F6„F–7F–öæ'•'6U&W7VÇBÆöDWFõ7v—F6„F–7F–öæ'’‡7G&–ærF–7F–öæ'’’° —f"&W7VÇBÒWFõ7v—F6„F–7F–öæ'•'6W"å'6R†F–7F–öæ'’“° –5÷w&öæw2Ò&W7VÇBå7V66W72ò&W7VÇBå6÷W&6W2¢æWr7G&–æu³Ó° –5ö6÷'&V7G2Ò&W7VÇBå7V66W72ò&W7VÇBå&WÆ6VÖVçG2¢æWr7G&–æu³Ó° —&WGW&â&W7VÇC° —Ð  ––çFW&æÂ7FF–2WFõ7v—F6„F–7F–öæ'•'6U&W7VÇB&VÆöDWFõ7v—F6„F–7F–öæ'’‚’° ”WFõ7v—F6„F–7F–öæ'•'6U&W7VÇB&W7VÇC° ––b„Ö†÷UT’äWFõ7v—F6„Væ&ÆVB’° —&W7VÇBÒÆöDWFõ7v—F6„F–7F–öæ'’„Ö†÷UT’äWFõ7v—F6„F–7F–öæ'•&r“° ––b„Ö†÷UT’äWFõ7v—F6„F–7F–öæ'•Föô&–r’° ”Ö†÷UT’äWFõ7v—F6„F–7F–öæ'•&rÒçVÆÃ° —Ð —ÒVÇ6R° –5÷w&öæw2Ò5ö6÷'&V7G2ÒçVÆÃ° ”Ö†÷UT’äWFõ7v—F6„F–7F–öæ'•Föô&–rÒfÇ6S° —&W7VÇBÒWFõ7v—F6„F–7F–öæ'•'6W"äV×G•7V66W72‚“° —Ð ”6ÆV$WFõ7v—F6…G&6¶–ær‚“° —&WGW&â&W7VÇC° —Ð Ð —V&Æ–27FF–2fö–B6ÆV$WFõ7v—F6…G&6¶–ær‚’°Ð –WFõ7v—F6…FW‡Bä6ÆV"‚“°Ð –Æ7DWFõ7v—F6…FW‡BÒ"#°Ð —ÐÐ Ð ’òòòÇ7VÖÖ'“àÐ ’òòò6öçF–ç2¶W’„¶W—2¶W’’Â—B7FFR†&ööÂWW"’Â–b—B—2ÇBµ´çVÕEÒ†&ööÂÇFçVÒ’æB'&’öbçV×G2†Æ—7BöbçV×B¶W—2’àÐ ’òòòÂ÷7VÖÖ'“àÐ —V&Æ–27G'V7B—T¶W’°Ð —V&Æ–2¶W—2¶W“°Ð —V&Æ–2&ööÂWW#°Ð —V&Æ–2&ööÂÇFçVÓ°Ð —V&Æ–2Æ—7CÄ¶W—3âçV×G3°Ð —ÐÐ ’6VæG&Vv–öàÐ —ÐÐ§ÐÐ