using System;
using System.Windows.Forms;

namespace Mahou
{
    public partial class Update : Form
    {
        // Kept for legacy path migration only. This class contains no network or installation code.
        public static readonly string nPath = AppDomain.CurrentDomain.BaseDirectory;

        public Update()
        {
            InitializeComponent();
            ConfigureDisabledUpdaterUi();
        }

        private void Update_Load(object sender, EventArgs e)
        {
            ConfigureDisabledUpdaterUi();
        }

        private void Update_VisibleChanged(object sender, EventArgs e)
        {
            if (Visible)
                ConfigureDisabledUpdaterUi();
        }

        private void btnCheck_Click(object sender, EventArgs e)
        {
            ShowDisabledMessage();
        }

        private void btDMahou_Click(object sender, EventArgs e)
        {
            ShowDisabledMessage();
        }

        private void BtShowProxyClick(object sender, EventArgs e)
        {
            ShowDisabledMessage();
        }

        public void StartupCheck()
        {
            // Intentionally disabled. MIXANIZM Mahou updates are installed manually
            // until a signed manifest, verified package and rollback mechanism exist.
        }

        private void ConfigureDisabledUpdaterUi()
        {
            Text = "MIXANIZM Mahou";
            btnCheck.Enabled = false;
            btDMahou.Enabled = false;
            pbStatus.Enabled = false;
            btShowProxy.Enabled = false;
            gbProxy.Enabled = false;
            tbPass.UseSystemPasswordChar = true;
            lbDownloading.Visible = false;
            lbChecking.Visible = true;
            lbChecking.Text = "Updates are disabled. Install verified releases manually.";
            lbVer.Text = "Automatic updates unavailable";
            gpRTitle.Text = "Security";
            lbRDesc.Text = "The legacy updater was removed because it did not verify downloaded packages.";
        }

        private void ShowDisabledMessage()
        {
            MessageBox.Show(
                "Automatic updates are disabled. Install only verified MIXANIZM Mahou releases manually.",
                "MIXANIZM Mahou",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }
}
