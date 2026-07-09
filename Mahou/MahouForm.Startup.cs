using System;
using System.Windows.Forms;

namespace Mahou
{
    public partial class MahouForm
    {
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
            btnApply.Click -= StartupRegistryBridge_Click;
            btnOK.Click -= StartupRegistryBridge_Click;
            btnApply.Click += StartupRegistryBridge_Click;
            btnOK.Click += StartupRegistryBridge_Click;
        }

        private void StartupRegistryBridge_Click(object sender, EventArgs e)
        {
            ApplyStartupRegistryState();
        }

        private void RefreshStartupCheckboxFromRegistry()
        {
            try
            {
                cbAutorun.Checked = StartupManager.IsEnabled();
            }
            catch
            {
            }
        }

        private void ApplyStartupRegistryState()
        {
            try
            {
                if (cbAutorun.Checked)
                    StartupManager.Enable();
                else
                    StartupManager.Disable();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "MIXANIZM Mahou", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
