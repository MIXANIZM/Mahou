using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Mahou {
    public partial class MahouUI {
        const string LegacyNetworkDisabledMessage =
            "Legacy network update and public sync are disabled in MIXANIZM Mahou for security. " +
            "Updates are installed manually from verified releases.";

        void ApplySecurityPolicy() {
            MMain.MyConfs.Write("Functions", "StartupUpdatesCheck", "false");
            MMain.MyConfs.Write("Functions", "SilentUpdate", "false");
            MMain.MyConfs.Write("Functions", "AppDataConfigs", "true");

            chk_StartupUpdatesCheck.Checked = false;
            chk_StartupUpdatesCheck.Enabled = false;
            chk_SilentUpdate.Checked = false;
            chk_SilentUpdate.Enabled = false;
            btn_CheckForUpdates.Enabled = false;
            btn_DownloadUpdate.Enabled = false;
            grb_DownloadUpdate.Enabled = false;
            txt_UpdateDetails.Text = LegacyNetworkDisabledMessage;
            grb_MahouReleaseTitle.Text = "Manual verified updates only";

            chk_AppDataConfigs.Checked = true;
            chk_AppDataConfigs.Enabled = false;
            ClipBackOnlyText = false;
            MMain.MyConfs.Write("Hidden", "ClipBackOnlyText", "false");
            Hchk_ClipBackOnlyText.Checked = false;
            Hchk_ClipBackOnlyText.Enabled = false;
            Hchk_ClipBackOnlyText.Visible = false;
            txt_ProxyPassword.UseSystemPasswordChar = true;
            HelpMeUnderstand.SetToolTip(txt_ProxyPassword,
                "Stored for the current Windows user with DPAPI; hidden on screen.");
            cbb_AutostartType.SelectedIndex = 0;
            cbb_AutostartType.Enabled = false;

            btn_backup.Enabled = false;
            btn_restore.Enabled = false;
            tab_sync.Text = tab_sync.Text + " (disabled)";
            txt_backupStatus.Text = LegacyNetworkDisabledMessage;
            txt_restoreStatus.Text = LegacyNetworkDisabledMessage;
            txt_backupStatus.Visible = true;
            txt_restoreStatus.Visible = true;

            chk_DownloadASD_InZip.Checked = false;
            chk_DownloadASD_InZip.Enabled = false;
            HelpMeUnderstand.SetToolTip(chk_TrEnable,
                "When enabled, selected text is sent to the configured online translation service.");
            HelpMeUnderstand.SetToolTip(chk_TrOnDoubleClick,
                "When enabled, double-clicked text may be sent to the configured online translation service.");
            btn_UpdateAutoSwitchDictionary.Text = "Restore bundled dictionary";
            HelpMeUnderstand.SetToolTip(btn_UpdateAutoSwitchDictionary,
                "Restores the dictionary shipped with this verified build. No network request is made.");
        }

        void ShowLegacyNetworkDisabled() {
            MessageBox.Show(this, LegacyNetworkDisabledMessage, "MIXANIZM Mahou",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        bool RestoreBundledAutoSwitchDictionary() {
            if (!UserDataPaths.RestoreBundledDictionary(AS_dictfile)) return false;
            AutoSwitchDictionaryRaw = File.ReadAllText(AS_dictfile, Encoding.UTF8);
            ChangeAutoSwitchDictionaryTextBox();
            UpdateSnippetCountLabel(AutoSwitchDictionaryRaw, lbl_AutoSwitchWordsCount, false);
            return true;
        }
    }
}
