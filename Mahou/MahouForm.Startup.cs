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
            ApplySecurityPolicyUi();
            WireStartupRegistryBridge();
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

        private void ApplySecurityPolicyUi()
        {
            cbCSActive.Checked = false;
            cbCSActive.Enabled = false;
            cbCSActive.AccessibleDescription = "Disabled until clipboard preservation is safe for all formats.";
            tbCSHK.Enabled = false;
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

            try { cbAutorun.Checked = StartupManager.IsEnabled(); }
            catch { }
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

                // Keep the old Apply() path from creating a COM/WScript shortcut.
                cbAutorun.Checked = false;

                BeginInvoke(new Action(delegate
                {
                    cbAutorun.Checked = StartupManager.IsEnabled();
                    autorunBridgeActive = false;
                    ApplySecurityPolicyUi();
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
