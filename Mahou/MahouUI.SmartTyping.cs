using System;
using System.Drawing;
using System.Windows.Forms;

namespace Mahou {
    public partial class MahouUI {
        TabPage tab_smartTyping;
        CheckBox chk_SmartCaps;
        Label lbl_SmartCapsDescription;
        Label lbl_SmartCapsExceptions;
        TextBox txt_SmartCapsExceptions;
        Button btn_ClearSmartCapsExceptions;
        bool smartTypingUiUpdating;

        void InitializeSmartTypingUi() {
            tab_smartTyping = new TabPage();
            chk_SmartCaps = new CheckBox();
            lbl_SmartCapsDescription = new Label();
            lbl_SmartCapsExceptions = new Label();
            txt_SmartCapsExceptions = new TextBox();
            btn_ClearSmartCapsExceptions = new Button();

            tab_smartTyping.Name = "tab_smartTyping";
            tab_smartTyping.Padding = new Padding(3);
            tab_smartTyping.Size = new Size(559, 268);
            tab_smartTyping.UseVisualStyleBackColor = true;

            chk_SmartCaps.AutoSize = true;
            chk_SmartCaps.Location = new Point(12, 12);
            chk_SmartCaps.Name = "chk_SmartCaps";
            chk_SmartCaps.Size = new Size(260, 19);
            chk_SmartCaps.UseVisualStyleBackColor = true;
            chk_SmartCaps.CheckedChanged += SmartCapsCheckedChanged;

            lbl_SmartCapsDescription.Location = new Point(12, 39);
            lbl_SmartCapsDescription.Name = "lbl_SmartCapsDescription";
            lbl_SmartCapsDescription.Size = new Size(530, 58);

            lbl_SmartCapsExceptions.AutoSize = true;
            lbl_SmartCapsExceptions.Location = new Point(12, 101);
            lbl_SmartCapsExceptions.Name = "lbl_SmartCapsExceptions";

            txt_SmartCapsExceptions.Location = new Point(12, 122);
            txt_SmartCapsExceptions.Multiline = true;
            txt_SmartCapsExceptions.Name = "txt_SmartCapsExceptions";
            txt_SmartCapsExceptions.ScrollBars = ScrollBars.Vertical;
            txt_SmartCapsExceptions.Size = new Size(530, 98);
            txt_SmartCapsExceptions.TextChanged += SmartCapsExceptionsTextChanged;

            btn_ClearSmartCapsExceptions.Location = new Point(387, 228);
            btn_ClearSmartCapsExceptions.Name = "btn_ClearSmartCapsExceptions";
            btn_ClearSmartCapsExceptions.Size = new Size(155, 28);
            btn_ClearSmartCapsExceptions.UseVisualStyleBackColor = true;
            btn_ClearSmartCapsExceptions.Click += ClearSmartCapsExceptionsClicked;

            tab_smartTyping.Controls.Add(chk_SmartCaps);
            tab_smartTyping.Controls.Add(lbl_SmartCapsDescription);
            tab_smartTyping.Controls.Add(lbl_SmartCapsExceptions);
            tab_smartTyping.Controls.Add(txt_SmartCapsExceptions);
            tab_smartTyping.Controls.Add(btn_ClearSmartCapsExceptions);
            tabs.Controls.Add(tab_smartTyping);
            RefreshSmartTypingLanguage();
        }

        void LoadSmartTypingSettings() {
            smartTypingUiUpdating = true;
            try {
                SmartCapsEnabled = MMain.MyConfs.ReadBool("SmartTyping", "SmartCapsEnabled");
                chk_SmartCaps.Checked = SmartCapsEnabled;
                SmartCaps.Configure(SmartCapsEnabled, MMain.MyConfs.Read("SmartTyping", "SmartCapsExceptions"));
                txt_SmartCapsExceptions.Text = SmartCaps.GetExceptionsForDisplay();
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
            if (tab_smartTyping == null) return;
            var russian = String.Equals(MMain.MyConfs.Read("Appearence", "Language"), "Russian", StringComparison.OrdinalIgnoreCase);
            if (russian) {
                tab_smartTyping.Text = "Умный ввод";
                chk_SmartCaps.Text = "Исправлять две случайные заглавные в начале слова";
                lbl_SmartCapsDescription.Text = "Исправление выполняется после завершения слова и только в редакторах с безопасной прямой заменой. " +
                    "Мгновенный Backspace возвращает исходный регистр; после двух отмен слово добавляется в личные исключения. " +
                    "Парольные поля и программы из списка исключений не обрабатываются.";
                lbl_SmartCapsExceptions.Text = "Личные исключения — по одному слову в строке:";
                btn_ClearSmartCapsExceptions.Text = "Очистить исключения";
            } else {
                tab_smartTyping.Text = "Smart typing";
                chk_SmartCaps.Text = "Fix two accidental initial capitals";
                lbl_SmartCapsDescription.Text = "Correction runs after the word is completed and only in editors with a verified direct replacement path. " +
                    "Immediate Backspace restores the original casing; after two rejections the word becomes a personal exception. " +
                    "Password fields and excluded applications are never processed.";
                lbl_SmartCapsExceptions.Text = "Personal exceptions — one word per line:";
                btn_ClearSmartCapsExceptions.Text = "Clear exceptions";
            }
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
    }
}
