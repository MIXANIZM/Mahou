using System;

namespace Mahou
{
    public partial class Update
    {
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            DisableLegacyUpdaterUi();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (Visible)
                DisableLegacyUpdaterUi();
        }

        private void DisableLegacyUpdaterUi()
        {
            btnCheck.Enabled = false;
            btDMahou.Enabled = false;
            pbStatus.Enabled = false;
            lbChecking.Visible = true;
            lbChecking.Text = "Updates are temporarily disabled in MIXANIZM Mahou.";
        }
    }
}
