using System;
using System.Windows.Forms;

namespace Mahou
{
    public partial class MahouForm
    {
        private bool autorunStateBeforeLegacyApply;
        private bool autorunBridgeActive;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            WireStartupRegistryBridge();
            RefreshStartupCheckboxFromRegistry();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
                RefreshStartupCheckboxFromRegistry();
        }

        private void WireStartupRegistryBridge()
        {
            btnApply.MouseDown -= StartupRegistryBridgeBeforeLegacyApply;
            btnOK.MouseDown -= StartupRegistryBridgeBeforeLegacyApply;
            btnApply.MouseDown += StartupRegistryBridgeBeforeLegacyApply;
            btnOK.MouseDown += StartupRegistryBridgeBeforeLegacyApply;
        }

        private void StartupRegistryBridgeBeforeLegacyApply(object sender, MouseEventArgs e)
        {
            ApplyStartupRegistryStateBeforeLegacyShortcutCode();
        }

        private void RefreshStartupCheckboxFromRegistry()
        {
            if (autorunBridgeActive)
                return;

            try
            {
                cbAutorun.Checked = StartupManager.IsEnabled();
            }
            catch
            {
            }
        }

        private void ApplyStartupRegistryStateBeforeLegacyShortcutCode()
        {
            try
            {
                autorunBridgeActive = true;
                autorunStateBeforeLegacyApply = cbAutorun.Checked;

                if (autorunStateBeforeLegacyApply)
                    StartupManager.Enable();
                else
                    StartupManager.Disable();

                // The original Apply() method still contains old Startup-folder .lnk code.
                // Force that legacy branch to DeleteShortcut() so Windows Script Host / COM is not required.
                cbAutorun.Checked = false;

                BeginInvoke(new Action(delegate
                {
                    cbAutorun.Checked = StartupManager.IsEnabled() || autorunStateBeforeLegacyApply;
                    autorunBridgeActive = false;
                }));
            }
            catch (Exception ex)
            {
                autorunBridgeActive = false;
                MessageBox.Show(ex.Message, "MIXANIZM Mahou", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
