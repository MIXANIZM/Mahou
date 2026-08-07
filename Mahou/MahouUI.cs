using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using System.Globalization;
using System.Net;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Reflection;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Web;

namespace Mahou {
	public partial class MahouUI : Form {
		#region Variables
		// Hotkeys, HKC => HotKey Convert
		public Hotkey Mainhk, ExitHk, HKCLast, HKCSelection, HKCLine, HKSymIgn, HKConMorWor,
			  	      HKTitleCase, HKRandomCase, HKSwapCase, HKTransliteration, HKRestart, 
			  	      HKToggleLP, HKShowST, HKToggleMahou, HKUpperCase, HKLowerCase, HKCycleCase,
			  	      HKSelCustConv, HKShCMenuUM;
		public List<Hotkey> SpecificSwitchHotkeys = new List<Hotkey>();
		/// <summary>
		/// Hotkey OK to fire action bools.
		/// </summary>
		public bool hksTTCOK, hksTRCOK, hksTSCOK, hksTrslOK, hkShWndOK, hkcwdsOK, hklOK, 
					hksOK, hklineOK, hkSIOK, hkExitOK, hkToglLPOK, hkShowTSOK, hkToggleMahouOK, hkUcOK, hklcOK, hkccOK,
					hkSCCok, hkSCMUM;
		public static string nPath = AppDomain.CurrentDomain.BaseDirectory, CustomSound, CustomSound2, Redefines;
		public static int ACT_Match = 0, TrayHoverMahouMM = 0, explorer_pid, explorer_not_found_tries = 0;
		public static bool LoggingEnabled, dummy, CapsLockDisablerTimer, LangPanelUpperArrow, mouseLTUpperArrow, caretLTUpperArrow,
						   ShiftInHotkey, AltInHotkey, CtrlInHotkey, WinInHotkey, AutoStartAsAdmin, UseJKL, AutoSwitchEnabled, SmartCapsEnabled, ReadOnlyNA,
						   SoundEnabled, UseCustomSound, SoundOnAutoSwitch, SoundOnConvLast, SoundOnLayoutSwitch,
						   UseCustomSound2, SoundOnAutoSwitch2, SoundOnConvLast2, SoundOnLayoutSwitch2, TrOnDoubleClick,
						   TrEnabled, TrBorderAero, OnceSpecific, WriteInputHistory, ExcludeCaretLD, UsePaste,
						   WriteInputHistoryByDate, WriteInputHistoryHourly, MahouMM = false,
						   hk_result, multi_continue = true, ZxZ = true, configs_loading;
		public static bool ENABLED = true;
		#region Timers
		static Timer overlay_excluder;
		static Timer tmr = new Timer();
		bool autoSwitchDictionaryTextUpdating;
		static Timer old = new Timer();
		static Timer stimer = new Timer();
		static Timer animate = new Timer();
		static Timer showUpdWnd = new Timer();
		public Timer ICheck = new Timer();
		public Timer ScrlCheck = new Timer();
		public Timer crtCheck = new Timer();
		public Timer capsCheck = new Timer();
		public Timer flagsCheck = new Timer();
		public Timer persistentLayout1Check = new Timer();
		public Timer persistentLayout2Check = new Timer();
		public Timer langPanelRefresh = new Timer();
		public Timer res = new Timer();
		public Timer resC = new Timer();
		#endregion
		#region [Hidden]
		public static bool nomemoryflush, LibreCtrlAltShiftV, CycleCaseReset,
							OVEXDisabled, ClipBackOnlyText, MahouMMTrayHoverLostFocusClose, CycleCaseSaveBase, cmdbackfix;
		public static string ReselectCustoms, AutoCopyTranslation = "", onlyAutoSwitchExcluded = "", CycleCaseBase;
		static string CycleCaseOrder = "TULSR", OverlayExcluded, tas, ncs;
		static int OverlayExcludedInerval, arm;
		static Timer armt = new Timer();
		#endregion
		static uint lastTrayFlagLayout = 0;
		public static Bitmap FLAG, ITEXT;
		int titlebar = 12;
		public static int AtUpdateShow, SpecKeySetCount, AutoSwitchCount, TrSetCount, InputHistoryBackSpaceWriteType;
		public int DoubleHKInterval = 200, SelectedTextGetMoreTriesCount, DelayAfterBackspaces;
		#region Temporary variables
		/// <summary> Translate Panel Colors</summary>
		public static Color TrFore, TrBack, TrBorder;
		public static Font TrText, TrTitle;
		public static int TrTransparency, Layout1ModifierKey, Layout2ModifierKey, LayoutDModifierKey, LayoutSModifierKey;
		public static uint LayoutSModifierLayout;
		/// <summary> In memory settings, for timers/hooks.</summary>
		public static bool DiffAppearenceForLayouts, LDForCaretOnChange, LDForMouseOnChange, ScrollTip, AddOneSpace,
					TrayFlags, TrayText, SymIgnEnabled, TrayIconVisible, ChangeLayouByKey, EmulateLS,
					RePress, BlockHKWithCtrl, blueIcon, SwitchBetweenLayouts, SelectedTextGetMoreTries, ReSelect,
					ConvertSelectionLS, ConvertSelectionLSPlus, MCDSSupport, OneLayoutWholeWord,
					MouseTTAlways, OneLayout, MouseLangTooltipEnabled, CaretLangTooltipEnabled, QWERTZ_fix, 
					ChangeLayoutInExcluded,
					AutoSwitchSpaceAfter, AutoSwitchSwitchToGuessLayout, GuessKeyCodeFix, Dowload_ASD_InZip, 
					LDForCaret, LDForMouse, LDUseWindowsMessages, RemapCapslockAsF18, Add1NL, PersistentLayoutOnWindowChange, PersistentLayoutOnlyOnce,
					PersistentLayoutForLayout1, PersistentLayoutForLayout2, UseDelayAfterBackspaces,
					ConvertSWLinExcl;
		/// <summary> Temporary modifiers of hotkeys. </summary>
		string Mainhk_tempMods, ExitHk_tempMods, HKCLast_tempMods, HKCSelection_tempMods, 
			    HKCLine_tempMods, HKSymIgn_tempMods, HKConMorWor_tempMods, HKTitleCase_tempMods,
 				HKRandomCase_tempMods, HKSwapCase_tempMods, HKTransliteration_tempMods, HKRestart_tempMods,
 				HKToggleLangPanel_tempMods, HKShowSelectionTranslate_tempMods, HKToggleMahou_tempMods, HKToUpper_tempMods,
 				HKToLower_tempMods, HKCycleCase_tempMods, HKSelCustConv_tempMods, HKShCMenuUM_tempMods;
		/// <summary> Temporary key of hotkeys. </summary>
		int Mainhk_tempKey, ExitHk_tempKey, HKCLast_tempKey, HKCSelection_tempKey,
			    HKCLine_tempKey, HKSymIgn_tempKey, HKConMorWor_tempKey, HKTitleCase_tempKey,
 				HKRandomCase_tempKey, HKSwapCase_tempKey, HKTransliteration_tempKey, HKRestart_tempKey,
 				HKToggleLangPanel_tempKey, HKShowSelectionTranslate_tempKey, HKToggleMahou_tempKey, HKToUpper_tempKey,
 				HKToLower_tempKey, HKCycleCase_tempKey, HKSelCustConv_tempKey, HKShCMenuUM_tempKey;
		/// <summary> Temporary Enabled of hotkeys. </summary>
		bool Mainhk_tempEnabled, ExitHk_tempEnabled, HKCLast_tempEnabled, HKCSelection_tempEnabled,
			    HKCLine_tempEnabled, HKSymIgn_tempEnabled, HKConMorWor_tempEnabled, HKTitleCase_tempEnabled,
 				HKRandomCase_tempEnabled, HKSwapCase_tempEnabled, HKTransliteration_tempEnabled, HKRestart_tempEnabled,
 				HKToggleLangPanel_tempEnabled, HKShowSelectionTranslate_tempEnabled, HKToggleMahou_tempEnabled,
 				HKToUpper_tempEnabled, HKToLower_tempEnabled, HKCycleCase_tempEnabled, HKShCMenuUM_tempEnabled;
		public static bool HKSelCustConv_tempEnabled;
		/// <summary> Temporary Double of hotkeys. </summary>
		bool Mainhk_tempDouble, ExitHk_tempDouble, HKCLast_tempDouble, HKCSelection_tempDouble,
			    HKCLine_tempDouble, HKSymIgn_tempDouble, HKConMorWor_tempDouble, HKTitleCase_tempDouble,
 				HKRandomCase_tempDouble, HKSwapCase_tempDouble, HKTransliteration_tempDouble,
 				HKToggleLangPanel_tempDouble, HKShowSelectionTranslate_tempDouble, HKToggleMahou_tempDouble,
 				HKToUpper_tempDouble, HKToLower_tempDouble, HKCycleCase_tempDouble, HKSelCustConv_tempDouble, HKShCMenuUM_tempDouble;
		/// <summary> Temporary colors of LangDisplays appearece. </summary>
		public static Color LDMouseFore_temp, LDCaretFore_temp, LDMouseBack_temp, LDCaretBack_temp, 
		 	  Layout1Fore_temp, Layout2Fore_temp, Layout1Back_temp, Layout2Back_temp;
		/// <summary> Temporary fonts of LangDisplays appearece. </summary>
		public static Font LDMouseFont_temp, LDCaretFont_temp, Layout1Font_temp, Layout2Font_temp;
		/// <summary> Temporary use flags of LangDisplays appearece. </summary>
		public static bool LDMouseUseFlags_temp, LDCaretUseFlags_temp;
		/// <summary> Temporary transparent backgrounds of LangDisplays appearece. </summary>
		public static bool LDMouseTransparentBack_temp, LDCaretTransparentBack_temp,
     		 Layout1TransparentBack_temp, Layout2TransparentBack_temp;
		/// <summary> Temporary positions of LangDisplays appearece. </summary>
		public static int LDMouseY_Pos_temp, LDCaretY_Pos_temp, LDMouseX_Pos_temp, LDCaretX_Pos_temp, 
		 	  Layout1Y_Pos_temp, Layout2Y_Pos_temp, Layout1X_Pos_temp, Layout2X_Pos_temp,
		 	  MCDS_Xpos_temp, MCDS_Ypos_temp, MCDS_TopIndent_temp, MCDS_BottomIndent_temp;
		/// <summary> Temporary sizes of LangDisplays appearece. </summary>
		public static int LDMouseHeight_temp, LDCaretHeight_temp, LDMouseWidth_temp, LDCaretWidth_temp, 
		 	  Layout1Height_temp, Layout2Height_temp, Layout1Width_temp, Layout2Width_temp;
		/// <summary>
		/// Temporary list boxes indexes before and after settings loaded.
		/// </summary>
		public int tmpHotkeysIndex, tmpLangTTAppearenceIndex;
		/// <summary> Temporary hotkey key of hotkey in txt_Hotkey. </summary>
		int txt_Hotkey_tempKey;
		/// <summary> Temporary hotkey modifiers of hotkey in txt_Hotkey. </summary>
		string txt_Hotkey_tempModifiers;
		/// <summary> Temporary persistent layout's processes. </summary>
		public string PersistentLayout1Processes, PersistentLayout2Processes;
		/// <summary> Temporary layouts, etc.. </summary>
		public static string Layout1, Layout2, Layout3, Layout4, 
			MainLayout1, MainLayout2, EmulateLSType, ExcludedPrograms, Layout1TText, Layout2TText;
		/// <summary> Temporary specific keys. </summary>
		public int Key1, Key2, Key3, Key4;
		/// <summary> LangPanel temporary bool variables. </summary>
		public static bool LangPanelDisplay, LangPanelBorderAero;
		/// <summary> LangPanel temporary int variables. </summary>
		public int LangPanelRefreshRate, LangPanelTransparency;
		/// <summary> LangPanel temporary color variables. </summary>
		public Color LangPanelForeColor, LangPanelBackColor, LangPanelBorderColor;
		/// <summary> LangPanel temporary position variable. </summary>
		public Point LangPanelPosition;
		/// <summary> LangPanel temporary font variable. </summary>
		public Font LangPanelFont;
		/// <summary> Static last layout for LangPanel. </summary>
		public static uint lastLayoutLangPanel = 0;
		ToolTip HelpMeUnderstand;
		#endregion
		public TrayIcon icon;
		Icon generatedTrayIcon;
		public LangDisplay mouseLangDisplay = new LangDisplay();
		public LangDisplay caretLangDisplay = new LangDisplay();
		public LangPanel _langPanel;
		public TranslatePanel _TranslatePanel;
		uint latestL = 0, latestCL = 0;
		static string decim = ",";
		public static uint currentLayout, GlobalLayout;
		public static uint MAIN_LAYOUT1, MAIN_LAYOUT2, CTRL_ALT_TemporaryLayout;
		bool onepass = true, onepassC = true;
		/// <summary>
		/// Has a lot of values/keys taken from dynamic controls:<br/>
		/// txt_keyN - HotkeyBox,<br/> 
		/// ^---> ADDONS: _mods - to get modifiers, _key - to get keyCode.<br/>
		/// chk_winN - Use Win modifier in hotkey, <br/>
		/// lbl_arrN - Arrow label, [has no values]<br/>
		/// cbb_typN - Switch type(To specific layout or switch between).
		/// </summary>
		public Dictionary<string, string> SpecKeySetsValues = new Dictionary<string, string>();
		// From more configs
		ColorDialog clrd = new ColorDialog();
		FontDialog fntd = new FontDialog();
		public static FontConverter fcv = new FontConverter();
		public static string AS_dictfile = Path.Combine(MahouUI.nPath, "AS_dict.txt");
		public static string mahou_folder_appd = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MIXANIZM Mahou");
		public static string latest_save_dir = "";
		public static string AutoSwitchDictionaryRaw = "";
		public static bool AutoSwitchDictionaryTooBig = false;
		public static Point LDC_lp = new Point(0,0);
		public static int LD_MouseSkipMessagesCount = 0;
		System.Threading.Thread uche;
		public static List<IntPtr> PERSISTENT_LAYOUT1_HWNDs = new List<IntPtr>(); 
		public static List<IntPtr> NOT_PERSISTENT_LAYOUT1_HWNDs = new List<IntPtr>(); 
		public static List<IntPtr> PERSISTENT_LAYOUT2_HWNDs = new List<IntPtr>(); 
		public static List<IntPtr> NOT_PERSISTENT_LAYOUT2_HWNDs = new List<IntPtr>(); 
		#endregion
		public MahouUI() {
			DeleteTrash();
			MMain.MAHOU_HANDLE = Handle;
			InitializeComponent();
			InitializeSmartTypingUi();
			HelpMeUnderstand = new ToolTip();			
			HelpMeUnderstand.AutoPopDelay = 20000;
			HelpMeUnderstand.InitialDelay = 500;
			HelpMeUnderstand.ReshowDelay = 100;
			HelpMeUnderstand.ShowAlways = true;
			HelpMeUnderstand.ToolTipIcon = ToolTipIcon.Info;
			HelpMeUnderstand.Popup += HelpMeUnderstandPopup;
			if (MMain.C_SWITCH) {
				chk_AppDataConfigs.Enabled = false;
			}
			// Switch to more secure connection.
			ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
			nud_LangTTPositionX.Minimum = nud_LangTTPositionY.Minimum = -100;
           	LoadConfigs();
			ApplySecurityPolicy();
			InitializeListBoxes();
			// Set minnimum values because they're ALWAYS restores to 0 after Form Editor is used.
		    nud_CapsLockRefreshRate.Minimum = nud_DoubleHK2ndPressWaitTime.Minimum =
		        nud_LangTTCaretRefreshRate.Minimum = nud_LangTTMouseRefreshRate.Minimum =
				nud_ScrollLockRefreshRate.Minimum =	nud_TrayFlagRefreshRate.Minimum = 
		    	nud_PersistentLayout1Interval.Minimum = nud_PersistentLayout2Interval.Minimum =	1;
			// Disable horizontal scroll
			pan_TrSets.AutoScroll = pan_KeySets.AutoScroll = false;
			pan_TrSets.HorizontalScroll.Maximum = pan_KeySets.HorizontalScroll.Maximum = 0;
			pan_TrSets.AutoScroll = pan_KeySets.AutoScroll = true;
			Text = "Mahou " + Assembly.GetExecutingAssembly().GetName().Version;
			Text += "-dev";
			if (____.commit != "") {
				Text += " <"+____.commit+">";
				MMain.MyConfs.Write("Updates", "LatestCommit", ____.commit);
				MMain.MyConfs.WriteToDisk();
			}
			else {
				var commit = MMain.MyConfs.Read("Updates", "LatestCommit");
				if (MMain.MyConfs.Read("Updates", "LatestCommit").Length == 7)
					Text += " <"+commit+">";
			}
			var mult = 4;
			if (KMHook.IfNW7())
				mult = 6;
			if (tabs.RowCount >= 2)
				mult++;
			if (tabs.RowCount >= 3)
				mult+=2;
			var pty = tabs.RowCount*mult;
			var lsy = btn_OK.Location.Y+pty;
			btn_Apply.Location = new Point(btn_Apply.Location.X, lsy);
			btn_Cancel.Location = new Point(btn_Cancel.Location.X, lsy);
			btn_OK.Location = new Point(btn_OK.Location.X, lsy);
			tabs.Height += pty;
			Height += pty;
			#if GITHUB_RELEASE
			Text = "Mahou " + Assembly.GetExecutingAssembly().GetName().Version;
			#endif
			RegisterHotkeys();
			RefreshAllIcons();
			//Background startup check for updates
			if (MMain.MyConfs.ReadBool("Functions", "StartupUpdatesCheck")) {
				uche = new System.Threading.Thread(StartupCheck);
				uche.Name = "Startup Check";
				uche.Start();
				showUpdWnd.Tick += (_, __) => {
					Logging.Log("Checking: " + AtUpdateShow);
					if (AtUpdateShow == 1) {
						if (MMain.MyConfs.ReadBool("Functions", "SilentUpdate")) {
							Btn_DownloadUpdateClick((object)0, new EventArgs());
							Logging.Log("Silent UPDATE!");
						}
						else {
							tabs.SelectedIndex = ïİ{öÚ$z{-®éÜj×vVB“°Ğ —Fõö6&"å6VÆV7FVD–æFW„6†ævVB³ÒæWrWfVçD†æFÆW"„6&%ôg%Fõ6VÆV7FVD–æFW„6†ævVB“°Ğ¢òğ–6&"ä—FV×2äFB„ÔÖ–âäÆæu´ÆæwVvW2äVÆVÖVçBå7v—F6„&WGvVVåÒ“°Ğ –g%ö6&"ä—FV×2äFE&ævR…G&ç6ÆFUæVÂäuDÆæw2“°Ğ —Fõö6&"ä—FV×2äFE&ævR…G&ç6ÆFUæVÂäuDÆæw2“°Ğ –g%ö6&"å6VÆV7FVD–æFW‚ÒFõö6&"å6VÆV7FVD–æFW‚Ò°Ğ •÷6WBä6öçG&öÇ2äFB†g%öÆ&Â“°Ğ •÷6WBä6öçG&öÇ2äFB†g%ö6&"“°Ğ •÷6WBä6öçG&öÇ2äFB†æWrÆ&VÂ‚—´ÆVgBÒö&6TÆVgB¶Æ&Å÷v–GF‚¶Æ&Åög'Fõ÷v–GF‚³#¶6&%÷v–GF‚ÂæÖSÒ&Æ&Åö'""µG%6WD6÷VçBÂv–GFƒÖÆ&Å÷v–GF‚ÂFW‡CÒ"Óâ"ÂF÷Ó'Ò“°Ğ •÷6WBä6öçG&öÇ2äFB‡FõöÆ&Â“°Ğ •÷6WBä6öçG&öÇ2äFB‡Fõö6&"“°Ğ¢òğ•7V4¶W•6WG5fÇVW5²&6&%ög""µG%6WD6÷VçB²%ö¶W’%ÒÒ7V4¶W•6WG5fÇVW5²'G‡Eö¶W’"µG%6WD6÷VçB²%öÖöG2%ÒÒ7V4¶W•6WG5fÇVW5²&6&%÷G—"µG%6WD6÷VçEÒÒ"#°Ğ —åõG%6WG2ä6öçG&öÇ2äFB…÷6WB“°Ğ –Æ&ÅõG%6WG46÷VçBäf÷&T6öÆ÷"Ò6öÆ÷"ä&Æ6³°Ğ –Æ&ÅõG%6WG46÷VçBåFW‡BÒ"2"µG%6WD6÷VçC°Ğ ––b…G%6WD6÷VçCã“‚’Æ&ÅõG%6WG46÷VçBäf÷&T6öÆ÷"Ò6öÆ÷"å&VC°Ğ —ĞĞ —fö–B'FåõG%7V%6WD6Æ–6²†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ ––b…G%6WD6÷VçBÂ’&WGW&ã°Ğ —åõG%6WG2ä6öçG&öÇ5²'6WEò"µG%6WD6÷VçEÒäF—7÷6R‚“°Ğ •G%6WD6÷VçBÒÓ°Ğ –Æ&ÅõG%6WG46÷VçBäf÷&T6öÆ÷"Ò6öÆ÷"ä&Æ6³°Ğ –Æ&ÅõG%6WG46÷VçBåFW‡BÒ"2"µG%6WD6÷VçC°Ğ ––b…G%6WD6÷VçBÂĞ –Æ&ÅõG%6WG46÷VçBäf÷&T6öÆ÷"Ò6öÆ÷"äÆ–v‡Dw&“°Ğ Ğ —ĞĞ —fö–B'FåôFE6WD6Æ–6²†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ ––b…7V4¶W•6WD6÷VçCã“‚’&WGW&ã°Ğ —f"÷6WBÒæWræVÂ‚“°Ğ •÷6WBåv–GF‚Ò‡åô¶W•6WG2åv–GF‚£“‚ó’Ó#°Ğ •7V4¶W•6WD6÷VçB²³°Ğ •÷6WBäæÖRÒ'6WEò"µ7V4¶W•6WD6÷VçC°Ğ —f"F÷Ò°Ğ ––b…7V4¶W•6WD6÷VçCãĞ —F÷Òåô¶W•6WG2ä6öçG&öÇ5²'6WEò"²…7V4¶W•6WD6÷VçBÓ•ÒåF÷³#S°Ğ •÷6WBä†V–v‡BÒ#3°Ğ •÷6WBåF÷ÒF÷°Ğ •÷6WBäÆVgBÒ°Ğ —f"ö&6TÆVgBÒ†–çB’‡åô¶W•6WG2åv–GF‚£"ó“°Ğ —f"G‡E÷v–GF‚Ò“°Ğ —f"6†µ÷v–GF‚ÒCS°Ğ —f"Æ&Å÷v–GF‚Ò#S°Ğ —f"6&%÷v–GF‚Ò“°Ğ •÷6WBä6öçG&öÇ2äFB†æWrÆ&VÂ‚—´ÆVgBÒö&6TÆVgBÂæÖSÒ&Æ&ÅöçVÒ"µ7V4¶W•6WD6÷VçBÂv–GFƒÖÆ&Å÷v–GF‚ÂFW‡CÕ7V4¶W•6WD6÷VçB²#¢"ÂF÷Ó'Ò“°Ğ —f"G‡BÒæWrFW‡D&÷‚‚—´ÆVgBÒö&6TÆVgB¶Æ&Å÷v–GF‚ÂæÖSÒ'G‡Eö¶W’"µ7V4¶W•6WD6÷VçBÂv–GFƒ×G‡E÷v–GF‚Â&6´6öÆ÷#Õ7—7FVÔ6öÆ÷'2åv–æF÷rÂ&VDöæÇ“×G'VWÓ°Ğ —G‡Bä¶W”F÷vâ³ÒæWr¶W”WfVçD†æFÆW"…G‡Eõ7V4†÷F¶W”F÷vâ“°Ğ —f"6†²ÒæWr6†V6´&÷‚‚—´ÆVgBÒö&6TÆVgB¶Æ&Å÷v–GF‚·G‡E÷v–GF‚³2ÂæÖSÒ&6†µ÷v–â"µ7V4¶W•6WD6÷VçBÂv–GFƒÖ6†µ÷v–GF‚ÂFW‡CÒ%v–â'Ó°Ğ –6†²ä6†V6¶VD6†ævVB³ÒæWrWfVçD†æFÆW"„6†µõ7V5v–ä6†V6¶VD6†ævVB“°Ğ —f"6&"ÒæWr6öÖ&ô&÷‚‚—´G&÷F÷vå7G–ÆRÒ6öÖ&ô&÷…7G–ÆRäG&÷F÷väÆ—7BÂÆVgBÒö&6TÆVgB¶Æ&Å÷v–GF‚·G‡E÷v–GF‚¶6†µ÷v–GF‚¶Æ&Å÷v–GF‚³’ÂæÖSÒ&6&%÷G—"µ7V4¶W•6WD6÷VçBÂv–GFƒÖ6&%÷v–GF‡Ó°Ğ –6&"å6VÆV7FVD–æFW„6†ævVB³ÒæWrWfVçD†æFÆW"„6&%õ7V5G—U6VÆV7FVD–æFW„6†ævVB“°Ğ –6&"ä—FV×2äFB„ÔÖ–âäÆæu´ÆæwVvW2äVÆVÖVçBå7v—F6„&WGvVVåÒ“°Ğ –6&"ä—FV×2äFE&ævR„ÔÖ–âæÆ6æÖ–BåFô'&’‚’“°Ğ •÷6WBä6öçG&öÇ2äFB‡G‡B“°Ğ •÷6WBä6öçG&öÇ2äFB†6†²“°Ğ •÷6WBä6öçG&öÇ2äFB†æWrÆ&VÂ‚—´ÆVgBÒö&6TÆVgB¶Æ&Å÷v–GF‚·G‡E÷v–GF‚¶6†µ÷v–GF‚³bÂæÖSÒ&Æ&Åö'""µ7V4¶W•6WD6÷VçBÂv–GFƒÖÆ&Å÷v–GF‚ÂFW‡CÒ"Óâ"ÂF÷Ó'Ò“°Ğ •÷6WBä6öçG&öÇ2äFB†6&"“°Ğ •7V4¶W•6WG5fÇVW5²'G‡Eö¶W’"µ7V4¶W•6WD6÷VçB²%ö¶W’%ÒÒ7V4¶W•6WG5fÇVW5²'G‡Eö¶W’"µ7V4¶W•6WD6÷VçB²%öÖöG2%ÒÒ7V4¶W•6WG5fÇVW5²&6&%÷G—"µ7V4¶W•6WD6÷VçEÒÒ"#°Ğ —åô¶W•6WG2ä6öçG&öÇ2äFB…÷6WB“°Ğ –Æ&Åõ6WG46÷VçBäf÷&T6öÆ÷"Ò6öÆ÷"ä&Æ6³°Ğ –Æ&Åõ6WG46÷VçBåFW‡BÒ"2"µ7V4¶W•6WD6÷VçC°Ğ ––b…7V4¶W•6WD6÷VçCã“‚’Æ&Åõ6WG46÷VçBäf÷&T6öÆ÷"Ò6öÆ÷"å&VC°Ğ —ĞĞ —fö–B'Fåõ7V%6WD6Æ–6²†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ ––b…7V4¶W•6WD6÷VçBÂ’&WGW&ã°Ğ —åô¶W•6WG2ä6öçG&öÇ5²'6WEò"µ7V4¶W•6WD6÷VçEÒäF—7÷6R‚“°Ğ •7V4¶W•6WD6÷VçBÒÓ°Ğ –Æ&Åõ6WG46÷VçBäf÷&T6öÆ÷"Ò6öÆ÷"ä&Æ6³°Ğ –Æ&Åõ6WG46÷VçBåFW‡BÒ"2"µ7V4¶W•6WD6÷VçC°Ğ ––b…7V4¶W•6WD6÷VçBÂĞ –Æ&Åõ6WG46÷VçBäf÷&T6öÆ÷"Ò6öÆ÷"äÆ–v‡Dw&“°Ğ Ğ —ĞĞ —fö–BG‡Eõ7V4†÷F¶W”F÷vâ†ö&¦V7B6VæFW"Â¶W”WfVçD&w2R’°Ğ —f"BÒ6VæFW"2FW‡D&÷ƒ°Ğ ––b†Rä¶W”6öFRÓÒ¶W—2ä&6²bbRäÖöF–f–W'2ÓÒ¶W—2äæöæR’°Ğ •7V4¶W•6WG5fÇVW5·BäæÖR²%ö¶W’%ÒÒ7V4¶W•6WG5fÇVW5·BäæÖR²%öÖöG2%ÒÒBåFW‡BÒ"#°Ğ —&WGW&ã°Ğ —ĞĞ ”FV'Vråw&—FTÆ–æR†Rä¶W”6öFR²"R"“°Ğ —BåFW‡BÒöVÕ&VF&ÆR‚†RäÖöF–f–W'2åFõ7G&–ær‚’å&WÆ6R‚"Â"Â"²"’²"²"°Ğ ’&VÖ¶R†Rä¶W”6öFR’’å&WÆ6R‚$æöæR²"Â""’“°Ğ •7V4¶W•6WG5fÇVW5·BäæÖR²%ö¶W’%ÒÒ‚†–çB–Rä¶W”6öFR’åFõ7G&–ær‚“°Ğ •7V4¶W•6WG5fÇVW5·BäæÖR²%öÖöG2%ÒÒRäÖöF–f–W'2åFõ7G&–ær‚’å&WÆ6R‚"Â"Â"²"“°Ğ —ĞĞ —fö–B6†µõ7V5v–ä6†V6¶VD6†ævVB†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ —f"2Ò6VæFW"26†V6´&÷ƒ°Ğ —f"¶W’Ò7V4¶W•6WG5fÇVW5²'G‡Eö¶W’"¶2äæÖRå&WÆ6R‚&6†µ÷v–â"Â""’²%öÖöG2%Ó°Ğ —f"†5v–âÒ¶W’ä6öçF–ç2‚%v–â"“°Ğ ––b††5v–âbb2ä6†V6¶VBĞ •7V4¶W•6WG5fÇVW5²'G‡Eö¶W’"¶2äæÖRå&WÆ6R‚&6†µ÷v–â"Â""’²%öÖöG2%ÒÒ¶W’å&WÆ6R‚%v–â"Â""“°Ğ ––b‚†5v–âbb2ä6†V6¶VBĞ •7V4¶W•6WG5fÇVW5²'G‡Eö¶W’"¶2äæÖRå&WÆ6R‚&6†µ÷v–â"Â""’²%öÖöG2%ÒÒ¶W’²"²v–â#°Ğ —ĞĞ —fö–B6&%õ7V5G—U6VÆV7FVD–æFW„6†ævVB†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ —f"6"Ò6VæFW"26öÖ&ô&÷ƒ°Ğ •7V4¶W•6WG5fÇVW5¶6"äæÖUÒÒ6"å6VÆV7FVD—FVÒåFõ7G&–ær‚“°Ğ —ĞĞ —fö–B6&%õG$ÖWF†öE6VÆV7FVD–æFW„6†ævVB†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ •G&ç6ÆFUæVÂçW6Tu2Ò†6&%õG$ÖWF†öBå6VÆV7FVD–æFW‚ÓÒ’òG'VR¢fÇ6S°Ğ •G&ç6ÆFUæVÂçW6TäÒ†6&%õG$ÖWF†öBå6VÆV7FVD–æFW‚ÓÒ"’òG'VR¢fÇ6S°Ğ —ĞĞ —fö–B6&%ôg%Fõ6VÆV7FVD–æFW„6†ævVB†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ —f"6"Ò6VæFW"26öÖ&ô&÷ƒ°Ğ •G%6WG5fÇVW5¶6"äæÖUÒÒG&ç6ÆFUæVÂäuDÆæw56…¶6"å6VÆV7FVD–æFW…Ó°Ğ¢òğ”FV'Vråw&—FTÆ–æR…G%6WG5fÇVW5¶6"äæÖUÒ“°Ğ —ĞĞ —fö–B6&%õ7V4¶W—5G—U6VÆV7FVD–æFW„6†ævVB†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ —f"öÆBÒ6&%õ7V4¶W—5G—Rå6VÆV7FVD–æFW‚ÓÒ°Ğ –Æ&Åô'&÷såf—6–&ÆRÒÆ&Åô'&÷s"åf—6–&ÆRÒÆ&Åô'&÷s2åf—6–&ÆRÒÆ&Åô'&÷sBåf—6–&ÆRÒw&%ôÆ–÷WG2åf—6–&ÆRÒw&%ô¶W—2åf—6–&ÆRÒöÆC°Ğ –Æ&Åõ6WG46÷VçBåf—6–&ÆRÒåô¶W•6WG2åf—6–&ÆRÒ'Fåõ7V%6WBåf—6–&ÆRÒ'FåôFE6WBåf—6–&ÆRÒöÆC°Ğ —ĞĞ —fö–B'Fåõ6VÆV7E6æD6Æ–6²†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ –Æ&Åô7W7FöÕ6÷VæBåFW‡BÒ6VÆV7DvWEvdf–ÆR‚“°Ğ ”†VÇÖUVæFW'7FæBå6WEFööÅF—†Æ&Åô7W7FöÕ6÷VæBÂÆ&Åô7W7FöÕ6÷VæBåFW‡B“°Ğ —ĞĞ —fö–B'Fåõ6VÆV7E6æC$6Æ–6²†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ –Æ&Åô7W7FöÕ6÷VæC"åFW‡BÒ6VÆV7DvWEvdf–ÆR‚“°Ğ ”†VÇÖUVæFW'7FæBå6WEFööÅF—†Æ&Åô7W7FöÕ6÷VæC"ÂÆ&Åô7W7FöÕ6÷VæC"åFW‡B“°Ğ —ĞĞ —fö–B'Fåö&6·W6Æ–6²†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ •7–æ4&6·W‚“°Ğ —ĞĞ —fö–B'Få÷&W7F÷&T6Æ–6²†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ •7–æ5&W7F÷&R‚“°Ğ —ĞĞ –&ööÂ7G&W2Â7F&·°Ğ —fö–B7D&·6÷”6Æ–6²†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ ––b‚7F&·’°Ğ —7F&·ÒG'VS°Ğ —f"BÒæWrF–ÖW"‚“°Ğ —BåF–6²³Ò…òÂõõò’Óâ°Ğ —7D&·6÷’ä&6¶w&÷VæD–ÖvRÒ&÷W'F–W2å&W6÷W&6W2æ6Æ—°Ğ —7F&·ÒfÇ6S°Ğ —Bå7F÷‚“°Ğ —BäF—7÷6R‚“°Ğ —Ó°Ğ —Bä–çFW'fÂÒƒ°Ğ ––b‚7G&–ærä—4çVÆÄ÷$V×G’‡G‡Eö&6·W–BåFW‡B’’°Ğ ”æF—fT6Æ—&ö&Bå6WEFW‡B‡G‡Eö&6·W–BåFW‡B“°Ğ —7D&·6÷’ä&6¶w&÷VæD–ÖvRÒ&÷W'F–W2å&W6÷W&6W2æ6Æ—ö³°Ğ —Bå7F'B‚“°Ğ —ÒVÇ6R°Ğ —7D&·6÷’ä&6¶w&÷VæD–ÖvRÒ&÷W'F–W2å&W6÷W&6W2æ6Æ—W'#°Ğ —Bå7F'B‚“°Ğ —ĞĞ —ĞĞ —ĞĞ —fö–B7E&W57FT6Æ–6²†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ ––b‚7G&W2’°Ğ —7G&W2ÒG'VS°Ğ —f"BÒæWrF–ÖW"‚“°Ğ —BåF–6²³Ò…òÂõõò’Óâ°Ğ —7E&W57FRä&6¶w&÷VæD–ÖvRÒ&÷W'F–W2å&W6÷W&6W2æ6Æ—°Ğ —7G&W2ÒfÇ6S°Ğ —Bå7F÷‚“°Ğ —BäF—7÷6R‚“°Ğ —Ó°Ğ —Bä–çFW'fÂÒƒ°Ğ —f"7G&’Ò´Ô†öö²ävWD6Æ—&ö&Bƒ2“°Ğ ––b‚7G&–ærä—4çVÆÄ÷$V×G’‡7G&’’’°Ğ —G‡E÷&W7F÷&T–BåFW‡BÒ7G&“°Ğ —7E&W57FRä&6¶w&÷VæD–ÖvRÒ&÷W'F–W2å&W6÷W&6W2æ6Æ—ö³°Ğ —Bå7F'B‚“°Ğ —ÒVÇ6R°Ğ —7E&W57FRä&6¶w&÷VæD–ÖvRÒ&÷W'F–W2å&W6÷W&6W2æ6Æ—W'#°Ğ —Bå7F'B‚“°Ğ —ĞĞ —ĞĞ —ĞĞ —fö–B6†µõ§…¤6†V6¶VD6†ævVB†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ •§…¢Ò‡6VæFW"26†V6´&÷‚’ä6†V6¶VC°Ğ —ĞĞ —7FF–27G&–ær†÷F¶W”f÷&Õö†÷F¶W’Ò"#°Ğ –6Æ72†÷F¶W”f÷&Ò¢f÷&Ò°Ğ ”Æ&VÂ†³°Ğ —V&Æ–2†÷F¶W”f÷&Ò‚’°Ğ —F†—2äf÷&Ô&÷&FW%7G–ÆRÒf÷&Ô&÷&FW%7G–ÆRäæöæS°Ğ —F†—2å6†÷t–åF6¶&"ÒfÇ6S°Ğ —F†—2ä&6´6öÆ÷"Ò6öÆ÷"äf÷&W7Dw&VVã°Ğ —F†—2æ†²ÒæWrÆ&VÂ‚“°Ğ —F†—2æ†²äWFõ6—¦RÒfÇ6S°Ğ —F†—2æ†²åv–GF‚Ò#3ƒ°Ğ —F†—2æ†²äföçBÒæWrföçB‡F†—2æ†²äföçBäföçDfÖ–Ç’ÂB“°Ğ —F†—2æ†²ä†V–v‡BÒ#c°Ğ —F†—2æ†²ä&6´6öÆ÷"Ò6öÆ÷"åv†—FS°Ğ —F†—2æ†²äf÷&T6öÆ÷"Ò6öÆ÷"ä÷&ævS°Ğ —F†—2æ†²äÆö6F–öâÒæWrö–çBƒÃ“°Ğ —F†—2ä6öçG&öÇ2äFB††²“°Ğ —F†—2äÖ–æ–×VÕ6—¦RÒæWr6—¦RƒSÃ“°Ğ —F†—2åv–GF‚Ò#C°Ğ —F†—2ä†V–v‡BÒ#ƒ°Ğ —F†—2ä6VçFW%Fõ67&VVâ‚“°Ğ —F†—2åF÷Ö÷7BÒG'VS°Ğ —ĞĞ –&ööÂö³²–çBÆ×6rÒ°Ğ —&÷FV7FVB÷fW'&–FRfö–BvæE&ö2‡&VbÖW76vRÒ’°Ğ ––b†Òä×6rÓÒ†–çB•v–ä’åtÕô´U•UÇÀĞ –Òä×6rÓÒ†–çB•v–ä’åtÕõ5•4´U•UÇÀĞ –Òä×6rÓÒ†–çB•v–ä’åtÕõ5•4´U”DõtâÇÀĞ –Òä×6rÓÒ†–çB•v–ä’åtÕô´U”Dõtâ’°Ğ —f"ÖöG2Ò´Ô†öö²ävWDÖöG57G"„´Ô†öö²æ7G&ÂÄ´Ô†öö²æ7G&Å÷"Â´Ô†öö²ç6†–gBÄ´Ô†öö²ç6†–gE÷"Ä´Ô†öö²æÇBÄ´Ô†öö²æÇE÷"Ä´Ô†öö²çv–âÄ´Ô†öö²çv–å÷"“°Ğ —f"²Ò„¶W—2–Òåu&ÒåFô–çC3"‚“°Ğ —f"ö²ÒfÇ6S°Ğ ––b†²Ò¶W—2äÄ6öçG&öÄ¶W’b`Ğ ’²Ò¶W—2å$6öçG&öÄ¶W’b`Ğ ’²Ò¶W—2äÅ6†–gD¶W’b`Ğ ’²Ò¶W—2å%6†–gD¶W’b`Ğ ’²Ò¶W—2äÄÖVçRb`Ğ ’²Ò¶W—2å$ÖVçRb`Ğ ’²Ò¶W—2å6†–gD¶W’b`Ğ ’²Ò¶W—2äÖVçRb`Ğ ’²Ò¶W—2ä6öçG&öÄ¶W’b`Ğ ’²Ò¶W—2äÅv–âb`Ğ ’²Ò¶W—2å%v–â’°Ğ –ÖöG2³Ö³°Ğ –ö²ÒG'VS°Ğ —ĞĞ —F†—2æ†²åFW‡BÒÖöG3°Ğ ––b†Æ×6rÓÒ’²Æ×6rÒÒä×6s²ĞĞ ––b†ö²’°Ğ –ö²ÒG'VS°Ğ ”†÷F¶W”f÷&Õö†÷F¶W’ÒF†—2æ†²åFW‡C°Ğ —ĞĞ ––b†ö²bbÆ×6rÒÒä×6r’°Ğ —F†—2ä6Æ÷6R‚“°Ğ —ĞĞ –Æ×6rÒÒä×6s°Ğ —ĞĞ –&6RåvæE&ö2‡&VbÒ“°Ğ —ĞĞ —ĞĞ —fö–B‡G‡Eõ&VFVf–æW4VçFW"†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ —f"BÒ…FW‡D&÷‚—6VæFW#°Ğ —Bä×VÇF–Æ–æRÒG'VS°Ğ —f"’ÒBå6VÆV7F–öå7F'C°Ğ —BåFW‡BÒBåFW‡Bå&WÆ6R‚'Â"ÂVçf—&öæÖVçBäæWtÆ–æR“°Ğ —Bå6VÆV7F–öå7F'BÒ“°Ğ ––b‡BäÆ–æW2äÆVæwF‚â’°Ğ —Bä†V–v‡BÒ†–çB’ƒR£#B“°Ğ —Bå67&öÆÄ&'2Ò67&öÆÄ&'2åfW'F–6Ã°Ğ —ĞĞ —ĞĞ —fö–B‡G‡Eõ&VFVf–æW4ÆVfR†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ —f"BÒ…FW‡D&÷‚—6VæFW#°Ğ —f"’ÒBå6VÆV7F–öå7F'C°Ğ —BåFW‡BÒBåFW‡Bå&WÆ6R„Vçf—&öæÖVçBäæWtÆ–æRÂ'Â"“°Ğ —Bå6VÆV7F–öå7F'BÒ“°Ğ —Bä†V–v‡BÒ#C°Ğ¢òğ—Bä×VÇF–Æ–æRÒfÇ6S°Ğ —Bå67&öÆÄ&'2Ò67&öÆÄ&'2äæöæS°Ğ —ĞĞ —fö–B‡G‡Eõ&VFVf–æW5FW‡D6†ævVB†ö&¦V7B6VæFW"ÂWfVçD&w2R’°Ğ —f"BÒ…FW‡D&÷‚—6VæFW#°Ğ ––b‡Bäfö7W6VB’°Ğ —f"’ÒBå6VÆV7F–öå7F'C°Ğ ––b‡BåFW‡BäÆVæwF‚â’°Ğ ––b‡BåFW‡E·BåFW‡BäÆVæwF‚ÓÒÓÒwÂr’’²³°Ğ —BåFW‡BÒBåFW‡Bå&WÆ6R‚'Â"ÂVçf—&öæÖVçBäæWtÆ–æR“°Ğ —ĞĞ —Bå6VÆV7F–öå7F'BÒ“°Ğ —Bå67&öÆÅFô6&WB‚“°Ğ —ĞĞ —ĞĞ ’6VæG&Vv–öàĞ ’7&Vv–öâ7–æ0Ğ —7G&–æuµÒ&VEFô&6·W‡7G&–ær–BÂ7G&–æræÖRÂ&ööÂ6†²Â&ööÂ&÷‡–rÒG'VR’°Ğ —f""Ò"#°Ğ —f"7FBÒ"#°Ğ —f"bÒF‚ä6öÖ&–æR†åF‚ÂæÖR“°Ğ ––b†6†²’°Ğ ––b‚f–ÆRäW†—7G2†b’’°Ğ —7FB³ÒæÖR²""²ÔÖ–âäÆæu´ÆæwVvW2äVÆVÖVçBäæ÷EÒåFôÆ÷vW"‚’²""²ÔÖ–âäÆæu´ÆæwVvW2äVÆVÖVçBäW†—7EÒåFôÆ÷vW"‚“°Ğ —&WGW&âæWrµÒ²""Â7FGÓ°Ğ —ĞĞ —f"f’ÒæWrf–ÆT–æfò†b“°Ğ ––b†f’äÆVæwF‚ãÒC’ Ğ —7FB³ÒæÖR²ÔÖ–âäÆæu´ÆæwVvW2äVÆVÖVçBåFöô&–uÓ°Ğ —G'’°Ğ —"³Ò"2ÒÒÒÒÒÓâ"¶–B´Vçf—&öæÖVçBäæWtÆ–æS°Ğ ––b†bä6öçF–ç2‚$Ö†÷Ræ–æ’"’bb&÷‡–rĞ —"³ÒÔÖ–âä×”6öæg2ävWE&uv—F†÷WDw&÷W‚%µ&÷‡•Ò"“°Ğ –VÇ6R Ğ —"³Òf–ÆRå&VDÆÅFW‡B†b“°Ğ —"³ÒVçf—&öæÖVçBäæWtÆ–æR²"2ÒÒÒÒÒÓâ"¶–B´Vçf—&öæÖVçBäæWtÆ–æS°Ğ —Ò6F6‚„W†6WF–öâR’°Ğ —7FB³ÒæÖR²#¢"²ÔÖ–âäÆæu´ÆæwVvW2äVÆVÖVçBä6ææ÷D&UÒåFôÆ÷vW"‚’²""²ÔÖ–âäÆæu´ÆæwVvW2äVÆVÖVçBå&VFVåÒåFôÆ÷vW"‚’²RäÖW76vS°Ğ —ĞĞ —ĞĞ —&WGW&âæWrµ×·"Â7FGÓ°Ğ —ĞĞ —7G&–ærw&—FU&W7F÷&Tf–ÆW2‡7G&–ær&rÂ&ööÂÖ–æ’Â&ööÂ‡G‡BÂ&ööÂGG‡BÂ&ööÂ&÷‡–rÒG'VRÂ&ööÂÖÒÒfÇ6R’°Ğ —f"7FBÒ"#°Ğ —f"BÒ&rå&WÆ6R‚%Ç""Â""“°Ğ —f"ÆÂÒBå7Æ—B‚uÆâr“°Ğ —f"&"ÒæWrµÒ²Ö–æ’Â‡G‡BÂGG‡BÂÖÒÓ°Ğ —f"FãÒ&GVÖ×’#°Ğ —f"7BÒfÇ6S°Ğ —f"BÒæWrF–7F–öæ'“Ç7G&–ærÂ7G&–æsâ‚“°Ğ –f÷"‡f"’Ò²’ÒÆÂäÆVæwF‚Ó²’²²’°Ğ —f"ÂÒÆÅ¶•Ó°Ğ —f"6öçBÒfÇ6S°Ğ —f"VæBÒfÇ6S°Ğ ––b‡7B’°Ğ ––b†’³ÃÒÆÂäÆVæwF‚Ó’°Ğ —f"Ç¢ÒÆÅ¶’³Ó°Ğ ––b†Ç¢å7F'G5v—F‚…5”ä5õ4UÂ7G&–æt6ö×&—6öâä–çf&–çD7VÇGW&R’’°Ğ ––b†Ç¢ÓÒ5”ä5õ4U·Fâ’°Ğ –VæBÒG'VS°Ğ —ĞĞ —ĞĞ —ĞĞ —ĞĞ ––b†Âå7F'G5v—F‚…5”ä5õ4UÂ7G&–æt6ö×&—6öâä–çf&–çD7VÇGW&R’’°Ğ –f÷&V6‚‡f"G—R–â5”ä5õE•U2’°Ğ ––b†ÂÓÒ5”ä5õ4U·G—R’°Ğ ––b‡FâÓÒG—R’ Ğ —FâÒ&GVÖ×’#°Ğ –VÇ6R°Ğ —Fã×G—S°Ğ —7C×G'VS°Ğ –6öçC×G'VS°Ğ —ĞĞ —ĞĞ —ĞĞ —ĞĞ ––b†6öçB’6öçF–çVS°Ğ —f"ÆâÒÂ²‚†’ÓÒÆÂäÆVæwF‚Ó'ÇÆVæB’ò""¢Vçf—&öæÖVçBäæWtÆ–æR“°Ğ ––b†Bä6öçF–ç4¶W’‡Fâ’’°Ğ —f"fÒE·FåÓ°Ğ –E·FåÒÒf¶Æã°Ğ —ÒVÇ6R Ğ –BäFB‡FâÂÆâ“°Ğ —ĞĞ —f"ô²Ò"#°Ğ —f"U%"Ò"#°Ğ –f÷"‡f"’Ò²’Ò&"äÆVæwFƒ²’²²’°Ğ —f""Ò&%¶•Ó°Ğ —f"G’Ò5”ä5õE•U5¶•Ó°Ğ ––b†"’°Ğ ––b†Bä6öçF–ç4¶W’‡G’’’°Ğ —G'’°Ğ ––b‡G’ÓÒ&–æ’"’°Ğ ––b‚&÷‡–r’ Ğ ”ÔÖ–âä×”6öæg2åô”ä’å&WÆ6U&r„ÔÖ–âä×”6öæg2ävWE&uv—F†÷WDw&÷W‚%µ&÷‡•Ò"ÂE·G•Ò’“°Ğ –VÇ6PĞ ”ÔÖ–âä×”6öæg2åô”ä’å&WÆ6U&r†E·G•Ò“°Ğ —ĞĞ —f"bÒF‚ä6öÖ&–æR†åF‚Â5”ä5ôäÔU5¶•Ò“°Ğ ”FV'Vråw&—FTÆ–æR‚%w&—F–æs¢"¶b“°Ğ ”FöÖ–4f–ÆRåw&—FTÆÅFW‡B†bÂE·G•Ò“°Ğ ”ô²³Ò""²5”ä5ôäÔU5¶•Ó°Ğ —Ò6F6‚„W†6WF–öâR’°Ğ —7FB³ÒG’²#¢"²RäÖW76vR²Vçf—&öæÖVçBäæWtÆ–æS°Ğ —ĞĞ —ÒVÇ6R°Ğ ”U%"³Ò""²5”ä5ôäÔU5¶•Ó°Ğ —ĞĞ —ĞĞ —ĞĞ —7FB³Ò$ô³¢"²ô²²„U%"Ò""ò„Vçf—&öæÖVçBäæWtÆ–æR²$U%#¢"²U%"’¢""“°Ğ —&WGW&â7FC°Ğ —ĞĞ —V&Æ–27FF–2&æFöÒ&æBÒæWr&æFöÒ‚“°Ğ —V&Æ–27FF–27G&–ærvWE&æFöÕ7G&–ær†–çBÆVæwF‚’°Ğ ’6öç7B7G&–ær6†'2Ò$$4DTdt„”¤´ÄÔäõ%5EUeu…•£#3CScsƒ’#°Ğ ’f"2Ò"#°Ğ ’f÷"†–çB’Ò²’ÂÆVæwFƒ²’²²Ğ ’ —2³Ò6†'5·&æBäæW‡B†6†'2äÆVæwF‚•Ó°Ğ ’&WGW&â3°Ğ —ĞĞ —7G&–ær7–æ5WÆöE§…¢‡7G&–ær6öçFVçB’°Ğ —&WGW&â7G&–æräV×G“°Ğ —ĞĞ Ğ —7G&–ær7–æ5WÆöD„"†'—FUµÒFFÂ&Vb7G&–æt'V–ÆFW"7FB’°Ğ —7FBä6ÆV"‚’äVæB„ÆVv7”æWGv÷&´F—6&ÆVDÖW76vR“°Ğ —&WGW&â7G&–æräV×G“°Ğ —ĞĞ Ğ —fö–B7–æ4&6·W‚’°Ğ —G‡Eö&6·W7FGW2åFW‡BÒÆVv7”æWGv÷&´F—6&ÆVDÖW76vS°Ğ —G‡Eö&6·W7FGW2åf—6–&ÆRÒG'VS°Ğ —ĞĞ Ğ —fö–B7–æ5&W7F÷&R‚’°Ğ —G‡E÷&W7F÷&U7FGW2åFW‡BÒÆVv7”æWGv÷&´F—6&ÆVDÖW76vS°Ğ —G‡E÷&W7F÷&U7FGW2åf—6–&ÆRÒG'VS°Ğ —ĞĞ Ğ —fö–B6WD&ööÇ2‡7G&–ærfÇVW2Â6†"6W&F÷"Â÷WB&ööÂ–æ’Â÷WB&ööÂ†—7F÷'’Â÷WB&ööÂF–7F–öæ'’Â÷WB&ööÂ&÷‡’Â÷WB&ööÂÖVçR’°Ğ —f"'G2Ò‡fÇVW2óò""’å7Æ—B‡6W&F÷"“°Ğ —f"ÆVv7’Ò'G2äÆVæwF‚ãÒc°Ğ ––æ’Ò'G2äÆVæwF‚âbb&öò‡'G5³Ò“°Ğ –†—7F÷'’Ò'G2äÆVæwF‚â†ÆVv7’ò"¢’bb&öò‡'G5¶ÆVv7’ò"¢Ò“°Ğ –F–7F–öæ'’Ò'G2äÆVæwF‚â†ÆVv7’ò2¢"’bb&öò‡'G5¶ÆVv7’ò2¢%Ò“°Ğ —&÷‡’Ò'G2äÆVæwF‚â†ÆVv7’òB¢2’bb&öò‡'G5¶ÆVv7’òB¢5Ò“°Ğ –ÖVçRÒ'G2äÆVæwF‚â†ÆVv7’òR¢B’bb&öò‡'G5¶ÆVv7’òR¢EÒ“°Ğ —ĞĞ Ğ –&ööÂ&öò‡7G&–ær2’°Ğ ––çB’Ò°Ğ –&ööÂ#°Ğ –&ööÂåG'•'6R‡2Â÷WB"“°Ğ ––çBåG'•'6R‡2Â÷WB’“°Ğ ––b†’â’ Ğ —&WGW&âG'VS°Ğ —&WGW&â#°Ğ —ĞĞ ––çB&–â†&ööÂ"’°Ğ ––b†"Ğ —&WGW&â°Ğ —&WGW&â°Ğ —ĞĞ ’6VæG&Vv–öàĞ —ĞĞ§ĞĞ 