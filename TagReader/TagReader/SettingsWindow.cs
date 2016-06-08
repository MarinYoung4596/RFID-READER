using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;
using System.Threading;
using Org.LLRP.LTK.LLRPV1.Impinj;
using TagReader.RFIDReader;


namespace TagReader
{
    public partial class SettingsWindow : Form
    {
        public static bool IsTimerModeActied = false;
        public static bool IsAutoSaveChecked = false;
        public static bool IsSaveSettingsButtonClicked = false;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void button_SaveSettings_Click(object sender, System.EventArgs e)
        {
            var ip = String.Empty;
            if (radioButton_IP.Checked)
            {
                ip = textBox_IP.Text;
            }
            else if (radioButton_MAC.Checked)
            {
                ip = String.Format(@"speedwayr-{0}-{1}-{2}.local", textBox_MAC_1, textBox_MAC_2, textBox_MAC_3);
            }
            if (ip == String.Empty)
            {
                MessageBox.Show("IP address can not be String.Empty!");
                return;
            }
            ReaderWrapper.ReaderParameters.Ip = ip;

            if (textBox_Power.Text == String.Empty)
            {
                MessageBox.Show("Please assign Transmission Power!");
                return;
            }
            ReaderWrapper.ReaderParameters.TransmitPower = Convert.ToDouble(textBox_Power.Text);
            
            ReaderWrapper.ReaderParameters.ChannelIndex = Convert.ToUInt16((Convert.ToDouble(comboBox_Frequency.Text) - 920.625) / 0.25);

            var activedAntennas = 0;
            if (checkBox_Antenna1.Checked) ++activedAntennas;
            if (checkBox_Antenna2.Checked) ++activedAntennas;
            if (checkBox_Antenna3.Checked) ++activedAntennas;
            if (checkBox_Antenna4.Checked) ++activedAntennas;
            if (0 == activedAntennas)
            {
                MessageBox.Show("You should assign at least 1 antenna!");
                return;
            }
            ReaderWrapper.ReaderParameters.AntennaId[0] = checkBox_Antenna1.Checked;
            ReaderWrapper.ReaderParameters.AntennaId[1] = checkBox_Antenna2.Checked;
            if (comboBox_ReaderType.SelectedIndex == 1)
            {
                ReaderWrapper.ReaderParameters.AntennaId[2] = checkBox_Antenna3.Checked;
                ReaderWrapper.ReaderParameters.AntennaId[3] = checkBox_Antenna4.Checked;
            }

            if (textBox_Population.Text == String.Empty)
            {
                MessageBox.Show("Pls assign population!");
                return;
            }
            ReaderWrapper.ReaderParameters.TagPopulation = Convert.ToUInt16(textBox_Population.Text);

            if (textBox_Tari.Text == String.Empty)
            {
                MessageBox.Show("Pls assign Tari!");
                return;
            }
            ReaderWrapper.ReaderParameters.Tari = Convert.ToUInt16(textBox_Tari.Text);

            if (IsTimerModeActied)
            {
                if (textBox_Timer.Text == String.Empty)
                {
                    MessageBox.Show("Pls assign transmission timer!");
                    return;
                } 
                ReaderWrapper.ReaderParameters.Duration = Convert.ToUInt16(textBox_Timer.Text);
            }
            IsSaveSettingsButtonClicked = true;

            this.Close();
            FormTagReader.IsSettigsWindowShowing = false;
            FormTagReader.IsSettigsWindowShowing = false;
        }

        private void comboBox_ReaderType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_ReaderType.SelectedIndex == 1)
            {
                checkBox_Antenna3.Enabled = true;
                checkBox_Antenna4.Enabled = true;
            }
            else
            {
                checkBox_Antenna3.Enabled = false;
                checkBox_Antenna4.Enabled = false;
            }
        }

        private void radioButton_IP_CheckedChanged(object sender, EventArgs e)
        {
            textBox_IP.Enabled = radioButton_IP.Checked;

            var isEnable = !radioButton_IP.Checked;
            label1.Enabled = isEnable;
            textBox_MAC_1.Enabled = isEnable;
            label2.Enabled = isEnable;
            textBox_MAC_2.Enabled = isEnable;
            label3.Enabled = isEnable;
            textBox_MAC_3.Enabled = isEnable;
            label4.Enabled = isEnable;
        }

        private void radioButton_MAC_CheckedChanged(object sender, EventArgs e)
        {
            var isEnable = radioButton_MAC.Checked;

            label1.Enabled = isEnable;
            textBox_MAC_1.Enabled = isEnable;
            label2.Enabled = isEnable;
            textBox_MAC_2.Enabled = isEnable;
            label3.Enabled = isEnable;
            textBox_MAC_3.Enabled = isEnable;
            label4.Enabled = isEnable;

            textBox_IP.Enabled = !isEnable;
        }

        private void checkBox_TimerMode_CheckedChanged(object sender, System.EventArgs e)
        {
            label_Timer.Enabled = checkBox_TimerMode.Checked;
            textBox_Timer.Enabled = checkBox_TimerMode.Checked;
            checkBox_AutoSave.Enabled = checkBox_TimerMode.Checked;

            IsTimerModeActied = checkBox_TimerMode.Checked;
        }

        private void checkBox_AutoSave_CheckedChanged(object sender, EventArgs e)
        {
            IsAutoSaveChecked = checkBox_AutoSave.Checked;
        }

        private void comboBox_ReaderMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            ushort mode = 0;
            switch (comboBox_ReaderMode.SelectedIndex)
            {
                case 0:
                    mode = 0;
                    break;
                case 1:
                    mode = 1;
                    break;
                case 2:
                    mode = 2;
                    break;
                case 3:
                    mode = 3;
                    break;
                case 4:
                    mode = 4;
                    break;
                case 5:
                    mode = 1000;
                    break;
                case 6:
                    mode = 1001;
                    break;
            }
            ReaderWrapper.ReaderParameters.ModeIndex = mode;
        }

        private void comboBox_SearchMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            var type = ENUM_ImpinjInventorySearchType.Dual_Target;
            switch (comboBox_SearchMode.SelectedIndex)
            {
                case 0:
                    type = ENUM_ImpinjInventorySearchType.No_Target;
                    break;
                case 1:
                    type = ENUM_ImpinjInventorySearchType.Reader_Selected;
                    break;
                case 2:
                    type = ENUM_ImpinjInventorySearchType.Dual_Target;
                    break;
                case 3:
                    type = ENUM_ImpinjInventorySearchType.Dual_Target_with_BtoASelect;
                    break;
                case 4:
                    type = ENUM_ImpinjInventorySearchType.Single_Target;
                    break;
                case 5:
                    type = ENUM_ImpinjInventorySearchType.Single_Target_BtoA;
                    break;
                case 6:
                    type = ENUM_ImpinjInventorySearchType.Single_Target_With_Suppression;
                    break;
            }
            ReaderWrapper.ReaderParameters.SearchType = type;
        }
    } // end of settings window
}
