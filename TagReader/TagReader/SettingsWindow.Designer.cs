using TagReader.Properties;

namespace TagReader
{
    partial class SettingsWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SettingsWindow));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_MAC_3 = new System.Windows.Forms.TextBox();
            this.textBox_MAC_2 = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_MAC_1 = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_IP = new System.Windows.Forms.TextBox();
            this.radioButton_MAC = new System.Windows.Forms.RadioButton();
            this.radioButton_IP = new System.Windows.Forms.RadioButton();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBox_Antenna4 = new System.Windows.Forms.CheckBox();
            this.checkBox_Antenna3 = new System.Windows.Forms.CheckBox();
            this.checkBox_Antenna2 = new System.Windows.Forms.CheckBox();
            this.checkBox_Antenna1 = new System.Windows.Forms.CheckBox();
            this.checkBox_AutoSave = new System.Windows.Forms.CheckBox();
            this.textBox_Timer = new System.Windows.Forms.TextBox();
            this.label_Timer = new System.Windows.Forms.Label();
            this.checkBox_TimerMode = new System.Windows.Forms.CheckBox();
            this.comboBox_ReaderType = new System.Windows.Forms.ComboBox();
            this.label_ReaderType = new System.Windows.Forms.Label();
            this.textBox_Tari = new System.Windows.Forms.TextBox();
            this.label_Tari = new System.Windows.Forms.Label();
            this.textBox_Population = new System.Windows.Forms.TextBox();
            this.label_Population = new System.Windows.Forms.Label();
            this.comboBox_SearchMode = new System.Windows.Forms.ComboBox();
            this.label_SearchMode = new System.Windows.Forms.Label();
            this.label_ReaderMode = new System.Windows.Forms.Label();
            this.label_Antenna = new System.Windows.Forms.Label();
            this.comboBox_Frequency = new System.Windows.Forms.ComboBox();
            this.label_Frequency = new System.Windows.Forms.Label();
            this.textBox_Power = new System.Windows.Forms.TextBox();
            this.label_TxPower = new System.Windows.Forms.Label();
            this.comboBox_ReaderMode = new System.Windows.Forms.ComboBox();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.button_SaveSettings = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBox_MAC_3);
            this.groupBox1.Controls.Add(this.textBox_MAC_2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBox_MAC_1);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBox_IP);
            this.groupBox1.Controls.Add(this.radioButton_MAC);
            this.groupBox1.Controls.Add(this.radioButton_IP);
            this.groupBox1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox1.Location = new System.Drawing.Point(12, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(446, 66);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Address";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(260, 44);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(38, 15);
            this.label4.TabIndex = 9;
            this.label4.Text = ".local";
            // 
            // textBox_MAC_3
            // 
            this.textBox_MAC_3.Location = new System.Drawing.Point(228, 40);
            this.textBox_MAC_3.Name = "textBox_MAC_3";
            this.textBox_MAC_3.Size = new System.Drawing.Size(31, 23);
            this.textBox_MAC_3.TabIndex = 8;
            // 
            // textBox_MAC_2
            // 
            this.textBox_MAC_2.Location = new System.Drawing.Point(188, 40);
            this.textBox_MAC_2.Name = "textBox_MAC_2";
            this.textBox_MAC_2.Size = new System.Drawing.Size(31, 23);
            this.textBox_MAC_2.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(179, 43);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(11, 15);
            this.label3.TabIndex = 7;
            this.label3.Text = "-";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(219, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(11, 15);
            this.label2.TabIndex = 5;
            this.label2.Text = "-";
            // 
            // textBox_MAC_1
            // 
            this.textBox_MAC_1.Location = new System.Drawing.Point(148, 40);
            this.textBox_MAC_1.Name = "textBox_MAC_1";
            this.textBox_MAC_1.Size = new System.Drawing.Size(31, 23);
            this.textBox_MAC_1.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(83, 43);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(72, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "SpeedwayR-";
            // 
            // textBox_IP
            // 
            this.textBox_IP.Location = new System.Drawing.Point(87, 16);
            this.textBox_IP.Name = "textBox_IP";
            this.textBox_IP.Size = new System.Drawing.Size(205, 23);
            this.textBox_IP.TabIndex = 1;
            this.textBox_IP.Text = "192.168.1.222";
            // 
            // radioButton_MAC
            // 
            this.radioButton_MAC.AutoSize = true;
            this.radioButton_MAC.Location = new System.Drawing.Point(7, 41);
            this.radioButton_MAC.Name = "radioButton_MAC";
            this.radioButton_MAC.Size = new System.Drawing.Size(50, 19);
            this.radioButton_MAC.TabIndex = 2;
            this.radioButton_MAC.TabStop = true;
            this.radioButton_MAC.Text = "MAC";
            this.radioButton_MAC.UseVisualStyleBackColor = true;
            this.radioButton_MAC.CheckedChanged += new System.EventHandler(this.radioButton_MAC_CheckedChanged);
            // 
            // radioButton_IP
            // 
            this.radioButton_IP.AutoSize = true;
            this.radioButton_IP.Location = new System.Drawing.Point(7, 18);
            this.radioButton_IP.Name = "radioButton_IP";
            this.radioButton_IP.Size = new System.Drawing.Size(36, 19);
            this.radioButton_IP.TabIndex = 0;
            this.radioButton_IP.TabStop = true;
            this.radioButton_IP.Text = "IP";
            this.radioButton_IP.UseVisualStyleBackColor = true;
            this.radioButton_IP.CheckedChanged += new System.EventHandler(this.radioButton_IP_CheckedChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBox_Antenna4);
            this.groupBox2.Controls.Add(this.checkBox_Antenna3);
            this.groupBox2.Controls.Add(this.checkBox_Antenna2);
            this.groupBox2.Controls.Add(this.checkBox_Antenna1);
            this.groupBox2.Controls.Add(this.checkBox_AutoSave);
            this.groupBox2.Controls.Add(this.textBox_Timer);
            this.groupBox2.Controls.Add(this.label_Timer);
            this.groupBox2.Controls.Add(this.checkBox_TimerMode);
            this.groupBox2.Controls.Add(this.comboBox_ReaderType);
            this.groupBox2.Controls.Add(this.label_ReaderType);
            this.groupBox2.Controls.Add(this.textBox_Tari);
            this.groupBox2.Controls.Add(this.label_Tari);
            this.groupBox2.Controls.Add(this.textBox_Population);
            this.groupBox2.Controls.Add(this.label_Population);
            this.groupBox2.Controls.Add(this.comboBox_SearchMode);
            this.groupBox2.Controls.Add(this.label_SearchMode);
            this.groupBox2.Controls.Add(this.label_ReaderMode);
            this.groupBox2.Controls.Add(this.label_Antenna);
            this.groupBox2.Controls.Add(this.comboBox_Frequency);
            this.groupBox2.Controls.Add(this.label_Frequency);
            this.groupBox2.Controls.Add(this.textBox_Power);
            this.groupBox2.Controls.Add(this.label_TxPower);
            this.groupBox2.Controls.Add(this.comboBox_ReaderMode);
            this.groupBox2.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.groupBox2.Location = new System.Drawing.Point(12, 78);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(446, 151);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Settings";
            // 
            // checkBox_Antenna4
            // 
            this.checkBox_Antenna4.AutoSize = true;
            this.checkBox_Antenna4.Enabled = false;
            this.checkBox_Antenna4.Location = new System.Drawing.Point(404, 46);
            this.checkBox_Antenna4.Name = "checkBox_Antenna4";
            this.checkBox_Antenna4.Size = new System.Drawing.Size(33, 19);
            this.checkBox_Antenna4.TabIndex = 23;
            this.checkBox_Antenna4.Text = "4";
            this.checkBox_Antenna4.UseVisualStyleBackColor = true;
            // 
            // checkBox_Antenna3
            // 
            this.checkBox_Antenna3.AutoSize = true;
            this.checkBox_Antenna3.Enabled = false;
            this.checkBox_Antenna3.Location = new System.Drawing.Point(373, 46);
            this.checkBox_Antenna3.Name = "checkBox_Antenna3";
            this.checkBox_Antenna3.Size = new System.Drawing.Size(33, 19);
            this.checkBox_Antenna3.TabIndex = 22;
            this.checkBox_Antenna3.Text = "3";
            this.checkBox_Antenna3.UseVisualStyleBackColor = true;
            // 
            // checkBox_Antenna2
            // 
            this.checkBox_Antenna2.AutoSize = true;
            this.checkBox_Antenna2.Location = new System.Drawing.Point(340, 46);
            this.checkBox_Antenna2.Name = "checkBox_Antenna2";
            this.checkBox_Antenna2.Size = new System.Drawing.Size(33, 19);
            this.checkBox_Antenna2.TabIndex = 21;
            this.checkBox_Antenna2.Text = "2";
            this.checkBox_Antenna2.UseVisualStyleBackColor = true;
            // 
            // checkBox_Antenna1
            // 
            this.checkBox_Antenna1.AutoSize = true;
            this.checkBox_Antenna1.Checked = true;
            this.checkBox_Antenna1.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_Antenna1.Location = new System.Drawing.Point(310, 46);
            this.checkBox_Antenna1.Name = "checkBox_Antenna1";
            this.checkBox_Antenna1.Size = new System.Drawing.Size(33, 19);
            this.checkBox_Antenna1.TabIndex = 20;
            this.checkBox_Antenna1.Text = "1";
            this.checkBox_Antenna1.UseVisualStyleBackColor = true;
            // 
            // checkBox_AutoSave
            // 
            this.checkBox_AutoSave.AutoSize = true;
            this.checkBox_AutoSave.Enabled = false;
            this.checkBox_AutoSave.Location = new System.Drawing.Point(236, 127);
            this.checkBox_AutoSave.Name = "checkBox_AutoSave";
            this.checkBox_AutoSave.Size = new System.Drawing.Size(79, 19);
            this.checkBox_AutoSave.TabIndex = 19;
            this.checkBox_AutoSave.Text = "Auto Save";
            this.checkBox_AutoSave.UseVisualStyleBackColor = true;
            this.checkBox_AutoSave.CheckedChanged += new System.EventHandler(this.checkBox_AutoSave_CheckedChanged);
            // 
            // textBox_Timer
            // 
            this.textBox_Timer.Enabled = false;
            this.textBox_Timer.Location = new System.Drawing.Point(148, 126);
            this.textBox_Timer.Name = "textBox_Timer";
            this.textBox_Timer.Size = new System.Drawing.Size(59, 23);
            this.textBox_Timer.TabIndex = 18;
            // 
            // label_Timer
            // 
            this.label_Timer.AutoSize = true;
            this.label_Timer.Enabled = false;
            this.label_Timer.Location = new System.Drawing.Point(104, 129);
            this.label_Timer.Name = "label_Timer";
            this.label_Timer.Size = new System.Drawing.Size(38, 15);
            this.label_Timer.TabIndex = 17;
            this.label_Timer.Text = "Timer";
            // 
            // checkBox_TimerMode
            // 
            this.checkBox_TimerMode.AutoSize = true;
            this.checkBox_TimerMode.Location = new System.Drawing.Point(7, 128);
            this.checkBox_TimerMode.Name = "checkBox_TimerMode";
            this.checkBox_TimerMode.Size = new System.Drawing.Size(91, 19);
            this.checkBox_TimerMode.TabIndex = 16;
            this.checkBox_TimerMode.Text = "Timer Mode";
            this.checkBox_TimerMode.UseVisualStyleBackColor = true;
            this.checkBox_TimerMode.CheckedChanged += new System.EventHandler(this.checkBox_TimerMode_CheckedChanged);
            // 
            // comboBox_ReaderType
            // 
            this.comboBox_ReaderType.FormattingEnabled = true;
            this.comboBox_ReaderType.Items.AddRange(new object[] {
            "R220",
            "R420"});
            this.comboBox_ReaderType.Location = new System.Drawing.Point(87, 43);
            this.comboBox_ReaderType.Name = "comboBox_ReaderType";
            this.comboBox_ReaderType.Size = new System.Drawing.Size(120, 23);
            this.comboBox_ReaderType.TabIndex = 5;
            this.comboBox_ReaderType.Text = "R220";
            this.comboBox_ReaderType.SelectedIndexChanged += new System.EventHandler(this.comboBox_ReaderType_SelectedIndexChanged);
            // 
            // label_ReaderType
            // 
            this.label_ReaderType.AutoSize = true;
            this.label_ReaderType.Location = new System.Drawing.Point(6, 46);
            this.label_ReaderType.Name = "label_ReaderType";
            this.label_ReaderType.Size = new System.Drawing.Size(72, 15);
            this.label_ReaderType.TabIndex = 4;
            this.label_ReaderType.Text = "Reader Type";
            // 
            // textBox_Tari
            // 
            this.textBox_Tari.Location = new System.Drawing.Point(310, 91);
            this.textBox_Tari.Name = "textBox_Tari";
            this.textBox_Tari.Size = new System.Drawing.Size(121, 23);
            this.textBox_Tari.TabIndex = 15;
            this.textBox_Tari.Text = "10";
            // 
            // label_Tari
            // 
            this.label_Tari.AutoSize = true;
            this.label_Tari.Location = new System.Drawing.Point(233, 94);
            this.label_Tari.Name = "label_Tari";
            this.label_Tari.Size = new System.Drawing.Size(28, 15);
            this.label_Tari.TabIndex = 14;
            this.label_Tari.Text = "Tari";
            // 
            // textBox_Population
            // 
            this.textBox_Population.Location = new System.Drawing.Point(87, 91);
            this.textBox_Population.Name = "textBox_Population";
            this.textBox_Population.Size = new System.Drawing.Size(120, 23);
            this.textBox_Population.TabIndex = 13;
            this.textBox_Population.Text = "32";
            // 
            // label_Population
            // 
            this.label_Population.AutoSize = true;
            this.label_Population.Location = new System.Drawing.Point(7, 94);
            this.label_Population.Name = "label_Population";
            this.label_Population.Size = new System.Drawing.Size(67, 15);
            this.label_Population.TabIndex = 12;
            this.label_Population.Text = "Population";
            // 
            // comboBox_SearchMode
            // 
            this.comboBox_SearchMode.FormattingEnabled = true;
            this.comboBox_SearchMode.Items.AddRange(new object[] {
            "No_Target",
            "Reader_Selected",
            "Dual_Target",
            "Dual_Target_with_BtoASelect",
            "Single_Target",
            "Single_Target_BtoA",
            "Single_Target_With_Suppression"});
            this.comboBox_SearchMode.Location = new System.Drawing.Point(310, 67);
            this.comboBox_SearchMode.Name = "comboBox_SearchMode";
            this.comboBox_SearchMode.Size = new System.Drawing.Size(121, 23);
            this.comboBox_SearchMode.TabIndex = 11;
            this.comboBox_SearchMode.Text = "Dual_Target";
            this.comboBox_SearchMode.SelectedIndexChanged += new System.EventHandler(this.comboBox_SearchMode_SelectedIndexChanged);
            // 
            // label_SearchMode
            // 
            this.label_SearchMode.AutoSize = true;
            this.label_SearchMode.Location = new System.Drawing.Point(233, 71);
            this.label_SearchMode.Name = "label_SearchMode";
            this.label_SearchMode.Size = new System.Drawing.Size(78, 15);
            this.label_SearchMode.TabIndex = 10;
            this.label_SearchMode.Text = "Search Mode";
            // 
            // label_ReaderMode
            // 
            this.label_ReaderMode.AutoSize = true;
            this.label_ReaderMode.Location = new System.Drawing.Point(7, 71);
            this.label_ReaderMode.Name = "label_ReaderMode";
            this.label_ReaderMode.Size = new System.Drawing.Size(79, 15);
            this.label_ReaderMode.TabIndex = 8;
            this.label_ReaderMode.Text = "Reader Mode";
            // 
            // label_Antenna
            // 
            this.label_Antenna.AutoSize = true;
            this.label_Antenna.Location = new System.Drawing.Point(234, 46);
            this.label_Antenna.Name = "label_Antenna";
            this.label_Antenna.Size = new System.Drawing.Size(58, 15);
            this.label_Antenna.TabIndex = 6;
            this.label_Antenna.Text = "Antennas";
            // 
            // comboBox_Frequency
            // 
            this.comboBox_Frequency.FormattingEnabled = true;
            this.comboBox_Frequency.Items.AddRange(new object[] {
            "920.625",
            "920.875",
            "921.125",
            "921.375",
            "921.625",
            "921.875",
            "922.125",
            "922.375",
            "922.625",
            "922.875",
            "923.125",
            "923.375",
            "923.625",
            "923.875",
            "924.125",
            "924.375"});
            this.comboBox_Frequency.Location = new System.Drawing.Point(310, 19);
            this.comboBox_Frequency.Name = "comboBox_Frequency";
            this.comboBox_Frequency.Size = new System.Drawing.Size(121, 23);
            this.comboBox_Frequency.TabIndex = 3;
            this.comboBox_Frequency.Text = "924.375";
            // 
            // label_Frequency
            // 
            this.label_Frequency.AutoSize = true;
            this.label_Frequency.Location = new System.Drawing.Point(233, 23);
            this.label_Frequency.Name = "label_Frequency";
            this.label_Frequency.Size = new System.Drawing.Size(63, 15);
            this.label_Frequency.TabIndex = 2;
            this.label_Frequency.Text = "Frequency";
            // 
            // textBox_Power
            // 
            this.textBox_Power.Location = new System.Drawing.Point(87, 19);
            this.textBox_Power.Name = "textBox_Power";
            this.textBox_Power.Size = new System.Drawing.Size(120, 23);
            this.textBox_Power.TabIndex = 1;
            this.textBox_Power.Text = "32.5";
            // 
            // label_TxPower
            // 
            this.label_TxPower.AutoSize = true;
            this.label_TxPower.Location = new System.Drawing.Point(7, 22);
            this.label_TxPower.Name = "label_TxPower";
            this.label_TxPower.Size = new System.Drawing.Size(41, 15);
            this.label_TxPower.TabIndex = 0;
            this.label_TxPower.Text = "Power";
            // 
            // comboBox_ReaderMode
            // 
            this.comboBox_ReaderMode.FormattingEnabled = true;
            this.comboBox_ReaderMode.Items.AddRange(new object[] {
            "0 (Max Throughput)",
            "1 (Hybrid)",
            "2 (Dense Reader M4)",
            "3 (Dense Reader M8)",
            "4 (Max Miller)",
            "1000 (Auto set Dense Reader)",
            "1001 (Auto set Single Reader)"});
            this.comboBox_ReaderMode.Location = new System.Drawing.Point(87, 67);
            this.comboBox_ReaderMode.Name = "comboBox_ReaderMode";
            this.comboBox_ReaderMode.Size = new System.Drawing.Size(120, 23);
            this.comboBox_ReaderMode.TabIndex = 9;
            this.comboBox_ReaderMode.Text = "2 (Dense Reader M4)";
            this.comboBox_ReaderMode.SelectedIndexChanged += new System.EventHandler(this.comboBox_ReaderMode_SelectedIndexChanged);
            // 
            // button_SaveSettings
            // 
            this.button_SaveSettings.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button_SaveSettings.Location = new System.Drawing.Point(322, 233);
            this.button_SaveSettings.Name = "button_SaveSettings";
            this.button_SaveSettings.Size = new System.Drawing.Size(121, 26);
            this.button_SaveSettings.TabIndex = 2;
            this.button_SaveSettings.Text = "Save Settings";
            this.button_SaveSettings.UseVisualStyleBackColor = true;
            this.button_SaveSettings.Click += new System.EventHandler(this.button_SaveSettings_Click);
            // 
            // SettingsWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(467, 262);
            this.Controls.Add(this.button_SaveSettings);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SettingsWindow";
            this.Text = "Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton_IP;
        private System.Windows.Forms.TextBox textBox_IP;
        private System.Windows.Forms.RadioButton radioButton_MAC;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_MAC_1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_MAC_2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_MAC_3;
        private System.Windows.Forms.Label label4;

        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox textBox_Power;
        private System.Windows.Forms.Label label_TxPower;
        private System.Windows.Forms.ComboBox comboBox_Frequency;
        private System.Windows.Forms.Label label_Frequency;
        private System.Windows.Forms.ComboBox comboBox_ReaderType;
        private System.Windows.Forms.Label label_ReaderType;
        private System.Windows.Forms.Label label_Antenna;
        private System.Windows.Forms.ComboBox comboBox_ReaderMode;
        private System.Windows.Forms.Label label_ReaderMode;    
        private System.Windows.Forms.ComboBox comboBox_SearchMode;
        private System.Windows.Forms.Label label_SearchMode;
        private System.Windows.Forms.TextBox textBox_Population;
        private System.Windows.Forms.Label label_Population;
        private System.Windows.Forms.TextBox textBox_Tari;
        private System.Windows.Forms.Label label_Tari;
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.CheckBox checkBox_TimerMode;
        private System.Windows.Forms.CheckBox checkBox_AutoSave;
        private System.Windows.Forms.TextBox textBox_Timer;
        private System.Windows.Forms.Label label_Timer;
        private System.Windows.Forms.Button button_SaveSettings;
        private System.Windows.Forms.CheckBox checkBox_Antenna4;
        private System.Windows.Forms.CheckBox checkBox_Antenna3;
        private System.Windows.Forms.CheckBox checkBox_Antenna2;
        private System.Windows.Forms.CheckBox checkBox_Antenna1;
    }
}