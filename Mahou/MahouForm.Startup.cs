using System;
using System.Windows.Forms;

namespace Mahou
{
    public partial class MahouForm
    {
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

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            var keyCode = keyData & Keys.KeyCode;
            bool enterWillApply = keyCode == Keys.Enter &&
                (btnApply.Focused || btnOK.Focused || AcceptButton == btnOK);
            bool spaceWillApply = keyCode == Keys.Space &&
                (btnApply.Focused || btnOK.Focused);

            if (!autorunBridgeActive && (enterWillApply || spaceWillApply))
                ApplyStartupRegistryStateBeforeLegacyShortcutCode();

            return base.ProcessCmdKey(ref msg, keyData);
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
            if (!autorunBridgeActive)
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
                bool requestedState = cbAutorun.Checked;

                if (requestedState)
                    StartupManager.Enable();
                else
                    StartupManager.Disable();

                // The original Apply() method still contains old Startup-folder .lnk code.
                // Force that legacy branch to DeleteShortcut() so Windows Script Host / COM is not required.
                cbAutorun.Checked = false;

                BeginInvoke(new Action(delegate
                {
                    cbAutorun.Checked = StartupManager.IsEnabled();
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
