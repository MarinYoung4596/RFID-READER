using System;
using System.Windows.Forms;
using TagReader.RFIDReader;

namespace TagReader
{
    public partial class SettingsWindow : Form
    {
        public static bool IsSaveSettingsButtonClicked = false;

        public SettingsWindow()
        {
            InitializeComponent();
        }



        private void checkBox_TimerMode_CheckedChanged(object sender, System.EventArgs e)
        {
            if (checkBox_TimerMode.Checked)
            {
                label_Timer.Enabled = true;
                textBox_Timer.Enabled = true;
                checkBox_AutoSave.Enabled = true;
            }
            else
            {
                label_Timer.Enabled = false;
                textBox_Timer.Enabled = false;
                checkBox_AutoSave.Enabled = false;
            }
        }


        private void button_SaveSettings_Click(object sender, System.EventArgs e)
        {
            var ip = string.Empty;
            if (radioButton_IP.Checked)
                ip = textBox_IP.Text;
            else if (radioButton_MAC.Checked)
                ip = String.Format(@"speedwayr-{0}-{1}-{2}.local", textBox_MAC_1, textBox_MAC_2, textBox_MAC_3);

            if (ip == string.Empty)
                MessageBox.Show("IP address can not be empty!");

            ReaderWrapper.ReaderParameters.Ip = ip;
            ReaderWrapper.ReaderParameters.TransmitPower = Convert.ToDouble(textBox_Power.Text);
            ReaderWrapper.ReaderParameters.ChannelIndex = Convert.ToUInt16((Convert.ToDouble(comboBox_Frequency.Text) - 920.625) / 0.25);


            FormTagReader.IsSettingsButtonClicked = false;
            IsSaveSettingsButtonClicked = true;
            this.Close();
        }

        private void comboBox_ReaderType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_ReaderType.SelectedText == "R420")
            {
                this.comboBox_Antennas.Items.Add(new object[] {"3", "4"});
                
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
    } // end of settings window
}
