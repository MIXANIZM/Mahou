using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace Mahou
{
    public partial class MahouForm
    {
        private bool safeApplyActive;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
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

                // The legacy Apply() method still has old .lnk logic. Keep its branch on delete-only;
                // the single source of truth is StartupManager/HKCU Run.
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
            cbCSActive.Checked = false;
            cbCSActive.Enabled = false;
            cbCSActive.AccessibleName = "Convert selected text";
            cbCSActive.AccessibleDescription = "Disabled until clipboard preservation is safe for all formats.";
            tbCSHK.Enabled = false;
            tbCSHK.AccessibleName = "Convert selected text hotkey";
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
