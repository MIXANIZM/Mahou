using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mahou {
    public partial class MahouUI {
        TabPage tab_smartTyping;
        CheckBox chk_SmartCaps;
        Label lbl_SmartCapsDescription;
        Label lbl_SmartCapsStatus;
        Label lbl_SmartCapsExceptions;
        TextBox txt_SmartCapsExceptions;
        Button btn_ClearSmartCapsExceptions;
        bool smartTypingUiUpdating;

        void InitializeSmartTypingUi() {
            tab_smartTyping = new TabPage();
            chk_SmartCaps = new CheckBox();
            lbl_SmartCapsDescription = new Label();
            lbl_SmartCapsStatus = new Label();
            lbl_SmartCapsExceptions = new Label();
            txt_SmartCapsExceptions = new TextBox();
            btn_ClearSmartCapsExceptions = new Button();

            tab_smartTyping.Name = "tab_smartTyping";
            tab_smartTyping.Padding = new Padding(3);
            tab_smartTyping.Size = new Size(559, 268);
            tab_smartTyping.UseVisualStyleBackColor = true;
            tab_smartTyping.SizeChanged += SmartTypingSizeChanged;

            chk_SmartCaps.AutoSize = true;
            chk_SmartCaps.Name = "chk_SmartCaps";
            chk_SmartCaps.UseVisualStyleBackColor = true;
            chk_SmartCaps.CheckedChanged += SmartCapsCheckedChanged;

            lbl_SmartCapsDescription.AutoSize = false;
            lbl_SmartCapsDescription.Name = "lbl_SmartCapsDescription";
            lbl_SmartCapsDescription.UseMnemonic = false;

            lbl_SmartCapsStatus.AutoSize = false;
            lbl_SmartCapsStatus.Name = "lbl_SmartCapsStatus";
            lbl_SmartCapsStatus.UseMnemonic = false;

            lbl_SmartCapsExceptions.AutoSize = true;
            lbl_SmartCapsExceptions.Name = "lbl_SmartCapsExceptions";
            lbl_SmartCapsExceptions.UseMnemonic = false;

            txt_SmartCapsExceptions.Multiline = true;
            txt_SmartCapsExceptions.Name = "txt_SmartCapsExceptions";
            txt_SmartCapsExceptions.ScrollBars = ScrollBars.Vertical;
            txt_SmartCapsExceptions.TextChanged += SmartCapsExceptionsTextChanged;

            btn_ClearSmartCapsExceptions.Name = "btn_ClearSmartCapsExceptions";
            btn_ClearSmartCapsExceptions.Size = new Size(185, 28);
            btn_ClearSmartCapsExceptions.UseVisualStyleBackColor = true;
            btn_ClearSmartCapsExceptions.Click += ClearSmartCapsExceptionsClicked;

            tab_smartTyping.Controls.Add(chk_SmartCaps);
            tab_smartTyping.Controls.Add(lbl_SmartCapsDescription);
            tab_smartTyping.Controls.Add(lbl_SmartCapsStatus);
            tab_smartTyping.Controls.Add(lbl_SmartCapsExceptions);
            tab_smartTyping.Controls.Add(txt_SmartCapsExceptions);
            tab_smartTyping.Controls.Add(btn_ClearSmartCapsExceptions);
            tabs.Controls.Add(tab_smartTyping);
            RefreshSmartTypingLanguage();
            LayoutSmartTypingUi();
        }

        void SmartTypingSizeChanged(object sender, EventArgs e) {
            LayoutSmartTypingUi();
        }

        void LayoutSmartTypingUi() {
            if (tab_smartTyping == null || tab_smartTyping.IsDisposed) return;

            const int margin = 12;
            const int gap = 7;
            var width = Math.Max(120, tab_smartTyping.ClientSize.Width - margin * 2);

            chk_SmartCaps.Location = new Point(margin, margin);
            chk_SmartCaps.MaximumSize = new Size(width, 0);

            var descriptionTop = chk_SmartCaps.Bottom + gap;
            var descriptionHeight = Math.Max(52, lbl_SmartCapsDescription.GetPreferredSize(new Size(width, 0)).Height);
            lbl_SmartCapsDescription.Location = new Point(margin, descriptionTop);
            lbl_SmartCapsDescription.Size = new Size(width, descriptionHeight);

            lbl_SmartCapsStatus.Location = new Point(margin, lbl_SmartCapsDescription.Bottom + gap);
            lbl_SmartCapsStatus.Size = new Size(width, Math.Max(20, lbl_SmartCapsStatus.GetPreferredSize(new Size(width, 0)).Height));

            lbl_SmartCapsExceptions.Location = new Point(margin, lbl_SmartCapsStatus.Bottom + gap);

            var buttonTop = Math.Max(lbl_SmartCapsExceptions.Bottom + 76,
                                     tab_smartTyping.ClientSize.Height - margin - btn_ClearSmartCapsExceptions.Height);
            btn_ClearSmartCapsExceptions.Location = new Point(
                Math.Max(margin, tab_smartTyping.ClientSize.Width - margin - btn_ClearSmartCapsExceptions.Width),
                buttonTop);

            var textTop = lbl_SmartCapsExceptions.Bottom + 5;
            var textBottom = btn_ClearSmartCapsExceptions.Top - gap;
            txt_SmartCapsExceptions.Location = new Point(margin, textTop);
            txt_SmartCapsExceptions.Size = new Size(width, Math.Max(60, textBottom - textTop));
        }

        void LoadSmartTypingSettings() {
            smartTypingUiUpdating = true;
            try {
                SmartCapsEnabled = MMain.MyConfs.ReadBool("SmartTyping", "SmartCapsEnabled");
                chk_SmartCaps.Checked = SmartCapsEnabled;
                SmartCaps.Configure(SmartCapsEnabled, MMain.MyConfs.Read("SmartTyping", "SmartCapsExceptions"));
                txt_SmartCapsExceptions.Text = SmartCaps.GetExceptionsForDisplay();
                UpdateSmartCapsStatusFromService(SmartCaps.CorrectionsThisSession, SmartCaps.ReversionsThisSession);
            } finally {
                smartTypingUiUpdating = false;
            }
        }

        void SaveSmartTypingSettings() {
            SmartCapsEnabled = chk_SmartCaps.Checked;
            SmartCaps.SetEnabled(SmartCapsEnabled);
            SmartCaps.SetExceptions(txt_SmartCapsExceptions.Text);
            MMain.MyConfs.Write("SmartTyping", "SmartCapsEnabled", SmartCapsEnabled.ToString());
            MMain.MyConfs.Write("SmartTyping", "SmartCapsExceptions", SmartCaps.GetExceptionsForConfig());
        }

        void RefreshSmartTypingLanguage() {
            if (tab_smartTyping == null || MMain.Lang == null) return;
            tab_smartTyping.Text = MMain.Lang[Languages.Element.tab_SmartTyping];
            chk_SmartCaps.Text = MMain.Lang[Languages.Element.SmartCapsEnabled];
            lbl_SmartCapsDescription.Text = MMain.Lang[Languages.Element.SmartCapsDescription];
            lbl_SmartCapsExceptions.Text = MMain.Lang[Languages.Element.SmartCapsExceptions];
            btn_ClearSmartCapsExceptions.Text = MMain.Lang[Languages.Element.SmartCapsClear];
            UpdateSmartCapsStatusFromService(SmartCaps.CorrectionsThisSession, SmartCaps.ReversionsThisSession);
            LayoutSmartTypingUi();
        }

        void SmartCapsCheckedChanged(object sender, EventArgs e) {
            if (smartTypingUiUpdating) return;
            SmartCapsEnabled = chk_SmartCaps.Checked;
            SmartCaps.SetEnabled(SmartCapsEnabled);
        }

        void SmartCapsExceptionsTextChanged(object sender, EventArgs e) {
            if (smartTypingUiUpdating) return;
            SmartCaps.SetExceptions(txt_SmartCapsExceptions.Text);
        }

        void ClearSmartCapsExceptionsClicked(object sender, EventArgs e) {
            smartTypingUiUpdating = true;
            try {
                txt_SmartCapsExceptions.Clear();
                SmartCaps.SetExceptions(String.Empty);
                MMain.MyConfs.WriteSave("SmartTyping", "SmartCapsExceptions", String.Empty);
            } finally {
                smartTypingUiUpdating = false;
            }
        }

        internal void UpdateSmartCapsExceptionsFromService(string display) {
            if (txt_SmartCapsExceptions == null || txt_SmartCapsExceptions.IsDisposed) return;
            smartTypingUiUpdating = true;
            try {
                txt_SmartCapsExceptions.Text = display ?? String.Empty;
            } finally {
                smartTypingUiUpdating = false;
            }
        }

        internal void UpdateSmartCapsStatusFromService(int corrections, int reversions) {
            if (lbl_SmartCapsStatus == null || lbl_SmartCapsStatus.IsDisposed || MMain.Lang == null) return;
            lbl_SmartCapsStatus.Text = String.Format(
                MMain.Lang[Languages.Element.SmartCapsStatus], corrections, reversions);
        }
    }
}
