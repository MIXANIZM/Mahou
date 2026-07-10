using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Mahou
{
    internal sealed class LayoutLearningSettingsForm : Form
    {
        private readonly CheckBox enabled = new CheckBox();
        private readonly CheckBox autoConvert = new CheckBox();
        private readonly CheckBox perAppRules = new CheckBox();
        private readonly NumericUpDown confirmations = new NumericUpDown();
        private readonly NumericUpDown minWordLength = new NumericUpDown();
        private readonly Label rulesInfo = new Label();
        private readonly bool russian;

        public LayoutLearningSettingsForm()
        {
            russian = String.Equals(MMain.MyConfs.Read("Locales", "LANGUAGE"), "RU", StringComparison.OrdinalIgnoreCase);
            InitializeUi();
            LoadSettings();
            DpiAccessibility.Apply(
                this,
                russian ? "Настройки адаптивного исправления раскладки" : "Adaptive layout correction settings",
                "btnSaveLayoutLearning",
                "btnCancelLayoutLearning");
        }

        private void InitializeUi()
        {
            Text = russian ? "Адаптивное исправление раскладки" : "Adaptive layout correction";
            Name = "LayoutLearningSettingsForm";
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox = true;
            MinimizeBox = false;
            ShowIcon = false;
            MinimumSize = new Size(580, 470);
            Size = new Size(640, 520);
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScroll = true;

            var layout = new TableLayoutPanel
            {
                Name = "layoutLearningTable",
                Dock = DockStyle.Fill,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Padding = new Padding(16),
                ColumnCount = 2,
                RowCount = 9
            };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            for (int i = 0; i < layout.RowCount; i++)
                layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            var title = new Label
            {
                Name = "lblLearningConsent",
                AutoSize = true,
                Dock = DockStyle.Fill,
                Font = new Font(Font, FontStyle.Bold),
                Margin = new Padding(0, 0, 0, 8),
                MaximumSize = new Size(570, 0),
                Text = russian
                    ? "Обучение выключено по умолчанию и включается только с вашего согласия."
                    : "Learning is off by default and starts only after your explicit consent."
            };
            layout.Controls.Add(title, 0, 0);
            layout.SetColumnSpan(title, 2);

            var privacy = new Label
            {
                Name = "lblLearningPrivacy",
                AutoSize = true,
                Dock = DockStyle.Fill,
                Margin = new Padding(0, 0, 0, 14),
                MaximumSize = new Size(570, 0),
                Text = russian
                    ? "Слова не сохраняются. На диске хранится только защищённый необратимый идентификатор правила. Браузеры, терминалы, RDP и известные менеджеры паролей исключены."
                    : "Words are not stored. Only a protected one-way rule identifier is written to disk. Browsers, terminals, RDP and known password managers are excluded."
            };
            layout.Controls.Add(privacy, 0, 1);
            layout.SetColumnSpan(privacy, 2);

            enabled.Name = "cbLayoutLearningEnabled";
            enabled.AutoSize = true;
            enabled.Dock = DockStyle.Fill;
            enabled.Margin = new Padding(0, 3, 0, 8);
            enabled.Text = russian ? "Я согласен включить обучение" : "I consent to enable learning";
            enabled.AccessibleDescription = russian
                ? "Включает локальное обучение только после явного согласия."
                : "Enables local learning only after explicit consent.";
            enabled.CheckedChanged += delegate { RefreshEnabledState(); };
            layout.Controls.Add(enabled, 0, 2);
            layout.SetColumnSpan(enabled, 2);

            autoConvert.Name = "cbLayoutLearningAutoConvert";
            autoConvert.AutoSize = true;
            autoConvert.Dock = DockStyle.Fill;
            autoConvert.Margin = new Padding(0, 3, 0, 8);
            autoConvert.Text = russian
                ? "Автоматически исправлять на пробеле или Enter"
                : "Auto-correct on Space or Enter";
            layout.Controls.Add(autoConvert, 0, 3);
            layout.SetColumnSpan(autoConvert, 2);

            perAppRules.Name = "cbLayoutLearningPerApp";
            perAppRules.AutoSize = true;
            perAppRules.Dock = DockStyle.Fill;
            perAppRules.Margin = new Padding(0, 3, 0, 14);
            perAppRules.Text = russian
                ? "Отдельные правила для каждой разрешённой программы"
                : "Use separate rules for each permitted application";
            layout.Controls.Add(perAppRules, 0, 4);
            layout.SetColumnSpan(perAppRules, 2);

            var confirmationsLabel = new Label
            {
                Name = "lblLayoutLearningConfirmations",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 6, 12, 6),
                Text = russian ? "Ручных исправлений до активации правила:" : "Manual corrections before a rule activates:"
            };
            confirmations.Name = "nudLayoutLearningConfirmations";
            confirmations.Anchor = AnchorStyles.Right;
            confirmations.Size = new Size(90, 24);
            confirmations.Minimum = 1;
            confirmations.Maximum = 20;
            confirmations.AccessibleName = confirmationsLabel.Text;
            layout.Controls.Add(confirmationsLabel, 0, 5);
            layout.Controls.Add(confirmations, 1, 5);

            var minLengthLabel = new Label
            {
                Name = "lblLayoutLearningMinLength",
                AutoSize = true,
                Anchor = AnchorStyles.Left,
                Margin = new Padding(0, 6, 12, 6),
                Text = russian ? "Минимальная длина слова:" : "Minimum word length:"
            };
            minWordLength.Name = "nudLayoutLearningMinLength";
            minWordLength.Anchor = AnchorStyles.Right;
            minWordLength.Size = new Size(90, 24);
            minWordLength.Minimum = 2;
            minWordLength.Maximum = 64;
            minWordLength.AccessibleName = minLengthLabel.Text;
            layout.Controls.Add(minLengthLabel, 0, 6);
            layout.Controls.Add(minWordLength, 1, 6);

            rulesInfo.Name = "lblLayoutLearningRulesInfo";
            rulesInfo.AutoSize = true;
            rulesInfo.Dock = DockStyle.Fill;
            rulesInfo.Margin = new Padding(0, 12, 0, 8);
            layout.Controls.Add(rulesInfo, 0, 7);
            layout.SetColumnSpan(rulesInfo, 2);

            var bottom = new TableLayoutPanel
            {
                Name = "layoutLearningBottomActions",
                Dock = DockStyle.Fill,
                AutoSize = true,
                ColumnCount = 2,
                RowCount = 1,
                Margin = new Padding(0, 10, 0, 0)
            };
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            bottom.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var dataActions = new FlowLayoutPanel
            {
                Name = "layoutLearningDataActions",
                AutoSize = true,
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = true,
                Margin = new Padding(0)
            };
            var clear = new Button
            {
                Name = "btnClearLayoutLearning",
                AutoSize = true,
                MinimumSize = new Size(170, 32),
                Text = russian ? "Удалить всю память" : "Delete all learned data",
                UseVisualStyleBackColor = true
            };
            clear.Click += ClearRules_Click;
            var openFolder = new Button
            {
                Name = "btnOpenLayoutLearningFolder",
                AutoSize = true,
                MinimumSize = new Size(160, 32),
                Text = russian ? "Открыть папку данных" : "Open data folder",
                UseVisualStyleBackColor = true
            };
            openFolder.Click += OpenFolder_Click;
            dataActions.Controls.Add(clear);
            dataActions.Controls.Add(openFolder);

            var dialogActions = new FlowLayoutPanel
            {
                Name = "layoutLearningDialogActions",
                AutoSize = true,
                Anchor = AnchorStyles.Right,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                Margin = new Padding(12, 0, 0, 0)
            };
            var save = new Button
            {
                Name = "btnSaveLayoutLearning",
                AutoSize = true,
                MinimumSize = new Size(92, 32),
                Text = russian ? "Сохранить" : "Save",
                DialogResult = DialogResult.None,
                UseVisualStyleBackColor = true
            };
            save.Click += Save_Click;
            var cancel = new Button
            {
                Name = "btnCancelLayoutLearning",
                AutoSize = true,
                MinimumSize = new Size(92, 32),
                Text = russian ? "Отмена" : "Cancel",
                DialogResult = DialogResult.Cancel,
                UseVisualStyleBackColor = true
            };
            dialogActions.Controls.Add(save);
            dialogActions.Controls.Add(cancel);

            bottom.Controls.Add(dataActions, 0, 0);
            bottom.Controls.Add(dialogActions, 1, 0);
            layout.Controls.Add(bottom, 0, 8);
            layout.SetColumnSpan(bottom, 2);

            Controls.Add(layout);
            AcceptButton = save;
            CancelButton = cancel;
        }

        private void LoadSettings()
        {
            enabled.Checked = ReadBool("Enabled", false);
            autoConvert.Checked = ReadBool("AutoConvertOnSpace", false);
            perAppRules.Checked = ReadBool("PerAppRules", false);
            confirmations.Value = Clamp(ReadInt("ConfirmationsToEnable", 2), confirmations.Minimum, confirmations.Maximum);
            minWordLength.Value = Clamp(ReadInt("MinWordLength", 4), minWordLength.Minimum, minWordLength.Maximum);
            RefreshEnabledState();
            RefreshRulesInfo();
        }

        private void RefreshEnabledState()
        {
            autoConvert.Enabled = enabled.Checked;
            perAppRules.Enabled = enabled.Checked;
            confirmations.Enabled = enabled.Checked;
            minWordLength.Enabled = enabled.Checked;
            if (!enabled.Checked)
                autoConvert.Checked = false;
        }

        private void Save_Click(object sender, EventArgs e)
        {
            MMain.MyConfs.Write("LayoutLearning", "Enabled", enabled.Checked.ToString());
            MMain.MyConfs.Write("LayoutLearning", "AutoConvertOnSpace", (enabled.Checked && autoConvert.Checked).ToString());
            MMain.MyConfs.Write("LayoutLearning", "PerAppRules", (enabled.Checked && perAppRules.Checked).ToString());
            MMain.MyConfs.Write("LayoutLearning", "ConfirmationsToEnable", confirmations.Value.ToString());
            MMain.MyConfs.Write("LayoutLearning", "MinWordLength", minWordLength.Value.ToString());

            AdaptiveLayoutLearning.Start();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void ClearRules_Click(object sender, EventArgs e)
        {
            var answer = MessageBox.Show(
                russian ? "Удалить правила, резервные файлы и локальный ключ обучения?" : "Delete rules, backups and the local learning key?",
                Text,
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (answer != DialogResult.Yes)
                return;

            AdaptiveLayoutLearning.ClearRules();
            RefreshRulesInfo();
        }

        private void OpenFolder_Click(object sender, EventArgs e)
        {
            try
            {
                Directory.CreateDirectory(Configs.dataPath);
                Process.Start("explorer.exe", Configs.dataPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void RefreshRulesInfo()
        {
            int count = 0;
            var file = Path.Combine(Configs.dataPath, "layout-learning.tsv");
            try
            {
                if (File.Exists(file))
                    foreach (var line in File.ReadLines(file))
                        if (!String.IsNullOrWhiteSpace(line) && !line.StartsWith("#")) count++;
            }
            catch { }

            rulesInfo.Text = russian ? "Сохранено обезличенных правил: " + count : "Stored privacy-preserving rules: " + count;
        }

        private static bool ReadBool(string key, bool defaultValue)
        {
            bool value;
            return Boolean.TryParse(MMain.MyConfs.Read("LayoutLearning", key), out value) ? value : defaultValue;
        }

        private static int ReadInt(string key, int defaultValue)
        {
            int value;
            return Int32.TryParse(MMain.MyConfs.Read("LayoutLearning", key), out value) ? value : defaultValue;
        }

        private static decimal Clamp(int value, decimal minimum, decimal maximum)
        {
            if (value < minimum) return minimum;
            if (value > maximum) return maximum;
            return value;
        }
    }
}
