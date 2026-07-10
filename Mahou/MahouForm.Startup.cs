using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Mahou
{
    public partial class MahouForm
    {
        private bool safeApplyActive;
        private bool legacyLayoutNormalized;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            NormalizeLegacyWindowLayout();
            DpiAccessibility.Apply(this, "MIXANIZM Mahou settings", "btnOK", "btnCancel");
            ApplySecurityPolicyUi();
            ReplaceLegacyApplyHandlers();
            ReplaceLegacyRepositoryLink();
            RefreshStartupCheckboxFromRegistry();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
            {
                ApplySecurityPolicyUi();
                RefreshStartupCheckboxFromRegistry();
            }
        }

        private void NormalizeLegacyWindowLayout()
        {
            if (legacyLayoutNormalized)
                return;

            legacyLayoutNormalized = true;
            AutoScaleMode = AutoScaleMode.None;
            StartPosition = FormStartPosition.CenterScreen;

            const int targetWidth = 460;
            const int targetHeight = 380;
            float widthScale = ClientSize.Width > 0 ? targetWidth / (float)ClientSize.Width : 1.0f;
            float heightScale = ClientSize.Height > 0 ? targetHeight / (float)ClientSize.Height : 1.0f;
            float scale = Math.Max(1.0f, Math.Min(1.45f, Math.Max(widthScale, heightScale)));

            if (scale > 1.01f)
            {
                SuspendLayout();
                Scale(new SizeF(scale, scale));
                ResumeLayout(true);
            }

            MinimumSize = Size;
            MaximumSize = Size;
            AutoScroll = true;
        }

        private void ReplaceLegacyApplyHandlers()
        {
            btnApply.Click -= btnApply_Click;
            btnOK.Click -= btnOK_Click;
            btnApply.Click -= SafeApply_Click;
            btnOK.Click -= SafeOk_Click;
            btnApply.Click += SafeApply_Click;
            btnOK.Click += SafeOk_Click;
        }

        private void ReplaceLegacyRepositoryLink()
        {
            GitHubLink.LinkClicked -= GitHubLink_LinkClicked;
            GitHubLink.LinkClicked -= SafeGitHubLink_LinkClicked;
            GitHubLink.LinkClicked += SafeGitHubLink_LinkClicked;
        }

        private void SafeGitHubLink_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try { Process.Start("https://github.com/MIXANIZM/Mahou"); }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MIXANIZM Mahou", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void SafeApply_Click(object sender, EventArgs e)
        {
            ApplyWithRegistryStartup();
        }

        private void SafeOk_Click(object sender, EventArgs e)
        {
            if (ApplyWithRegistryStartup())
                ToggleVisibility();
        }

        private bool ApplyWithRegistryStartup()
        {
            if (safeApplyActive)
                return false;

            bool requestedAutorun = cbAutorun.Checked;
            try
            {
                safeApplyActive = true;
                if (requestedAutorun)
                    StartupManager.Enable();
                else
                    StartupManager.Disable();

                cbAutorun.Checked = false;
                Apply();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MIXANIZM Mahou", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            finally
            {
                cbAutorun.Checked = StartupManager.IsEnabled();
                safeApplyActive = false;
                ApplySecurityPolicyUi();
            }
        }

        private void ApplySecurityPolicyUi()
        {
            bool russian = String.Equals(MMain.MyConfs.Read("Locales", "LANGUAGE"), "RU", StringComparison.OrdinalIgnoreCase);

            cbCLActive.Text = russian ? "Слово или выделение:" : "Word or selection:";
            cbCLActive.AccessibleName = russian ? "Изменить раскладку слова или выделения" : "Convert word or selection";
            tbCLHK.AccessibleName = russian ? "Горячая клавиша слова или выделения" : "Word or selection hotkey";

            cbCSActive.Checked = false;
            cbCSActive.Visible = false;
            tbCSHK.Visible = false;

            cbSwitchLayoutKeys.Text = "None";
            cbSwitchLayoutKeys.Visible = false;
            lbswithlayout.Visible = false;

            btnDDD.AccessibleName = russian ? "Дополнительные настройки" : "Advanced settings";
        }

        private void RefreshStartupCheckboxFromRegistry()
        {
            if (safeApplyActive)
                return;

            try { cbAutorun.Checked = StartupManager.IsEnabled(); }
            catch { }
        }
    }
}
