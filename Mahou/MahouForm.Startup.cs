using System;
using System.Windows.Forms;

namespace Mahou
{
    public partial class MahouForm
    {
        private bool safeApplyActive;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            ApplySecurityPolicyUi();
            ReplaceLegacyApplyHandlers();
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

        private void SafeApply_Click(object sender, EventArgs e)
        {
            ApplyWithRegistryStartup(false);
        }

        private void SafeOk_Click(object sender, EventArgs e)
        {
            if (ApplyWithRegistryStartup(true))
                ToggleVisibility();
        }

        private bool ApplyWithRegistryStartup(bool closing)
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
            cbCSActive.AccessibleDescription = "Disabled until clipboard preservation is safe for all formats.";
            tbCSHK.Enabled = false;
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
