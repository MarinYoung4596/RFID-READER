using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Forms;
using TagReader.RFIDReader;
using System.Windows.Forms.DataVisualization.Charting;

namespace TagReader
{
    public partial class FormTagReader : Form
    {
        private static bool _isConnected2Reader;
        private static bool _isStartButtonClicked;
        private static bool _isStopButtonClicked;
        private static bool _isClearButtonClicked;
        public  static bool IsSettingsButtonClicked;
        private static bool _showQuickAccessToolStrip = true;
        private static bool _showReaderSettingsToolStrip = true;


        public FormTagReader()
        {
            InitializeComponent();
            ReaderWrapper.MainForm = this;

            toolStripButton_Save.Enabled = false;
            toolStripButton_Connect.Enabled = true;
            toolStripButton_Start.Enabled = false;
            toolStripButton_Stop.Enabled = false;
            toolStripButton_Clear.Enabled = false;
            toolStripButton_Refresh.Enabled = false;
            toolStripButton_Settings.Enabled = true;

            ToolStripMenuItem_Save.Enabled = false;
            ToolStripMenuItem_Connect.Enabled = true;
            ToolStripMenuItem_Start.Enabled = false;
            ToolStripMenuItem_Stop.Enabled = false;
            ToolStripMenuItem_Settings.Enabled = true;
        }

        #region Update Component
        private ulong _startTime;
        private int ConvertTime(ulong time)
        {
            var ts = time - _startTime;
            return (int) (ts / 1000000);
        }


        private static HashSet<TagInfos.Key> _keySet = new HashSet<TagInfos.Key>();
        public void AddListItem(ref TagStatus tagStatus)
        {
            var epc = tagStatus.Epc;
            var antenna = tagStatus.Antenna;
            var channel = tagStatus.ChannelIndex;
            var key = new TagInfos.Key(epc, antenna, channel);
            
            if (_keySet.Contains(key))
            {
                foreach (ListViewItem item in listView_Data.Items)
                {
                    if (item.SubItems[0].Text != epc || Convert.ToUInt16(item.SubItems[1].Text) != antenna)
                        continue;
                    item.SubItems[2].Text = tagStatus.TimeStamp.ToString(); // time
                    item.SubItems[3].Text = tagStatus.Rssi.ToString(CultureInfo.InvariantCulture); // rssi
                    item.SubItems[4].Text = tagStatus.PhaseRadian.ToString(CultureInfo.InvariantCulture);// phase
                    item.SubItems[5].Text = tagStatus.DopplerShift.ToString();// Doppler
                    item.SubItems[6].Text = tagStatus.Velocity.ToString(CultureInfo.InvariantCulture);// velocity
                    item.SubItems[7].Text = tagStatus.TagSeenCount.ToString();// count
                }
            }
            else
            {
                var lvi = new ListViewItem(epc);
                lvi.SubItems.Add(antenna.ToString());
                lvi.SubItems.Add(ConvertTime(tagStatus.TimeStamp).ToString());
                lvi.SubItems.Add(tagStatus.Rssi.ToString(CultureInfo.InvariantCulture));
                lvi.SubItems.Add(tagStatus.PhaseRadian.ToString(CultureInfo.InvariantCulture));
                lvi.SubItems.Add(tagStatus.DopplerShift.ToString());
                lvi.SubItems.Add(tagStatus.Velocity.ToString(CultureInfo.InvariantCulture));
                lvi.SubItems.Add(tagStatus.TagSeenCount.ToString());

                listView_Data.Items.Add(lvi);
                _keySet.Add(key);
            }
        }


        private static Dictionary<TagInfos.Key, int> _map = new Dictionary<TagInfos.Key, int>(); 
        public void UpdateChart(ref TagStatus tagStatus)
        {
            var epc = tagStatus.Epc;
            var antenna = tagStatus.Antenna;
            var channel = tagStatus.ChannelIndex;
            var key = new TagInfos.Key(epc, antenna, channel);
            var time = tagStatus.TimeStamp;

            if (!_map.ContainsKey(key))
            {
                _isClearButtonClicked = false;
                if (_startTime == 0)
                {
                    _startTime = time;
                }
                if (ConvertTime(time) < 0)
                {
                    _startTime = time;
                }

                var s = new Series
                {
                    ChartType = SeriesChartType.FastLine,
                    Name = epc
                };
                chart_Rssi.Series.Add(s);

                var s1 = new Series
                {
                    ChartType = SeriesChartType.FastLine,
                    Name = epc
                };
                chart_Phase.Series.Add(s1);

                var s2 = new Series
                {
                    ChartType = SeriesChartType.FastLine,
                    Name = epc
                };
                chart_Doppler.Series.Add(s2);

                _map.Add(key, _map.Count); // save index
            }
            var seriesId = _map[key];

            chart_Rssi.Series[seriesId].Points.AddXY(ConvertTime(time), tagStatus.Rssi);
            chart_Rssi.Series[seriesId].LegendText = epc.Substring(epc.Length - 4, 4) + "_" + antenna;

            chart_Phase.Series[seriesId].Points.AddXY(ConvertTime(time), tagStatus.PhaseRadian);
            chart_Phase.Series[seriesId].LegendText = epc.Substring(epc.Length - 4, 4) + "_" + antenna;     

            chart_Doppler.Series[seriesId].Points.AddXY(ConvertTime(time), tagStatus.DopplerShift);
            chart_Doppler.Series[seriesId].LegendText = epc.Substring(epc.Length - 4, 4) + "_" + antenna;

            /*
            if (ConvertTime(time) >= 20)
            {
                this.Invoke(new Action(() => { StopReceive(); }));
            }
            */
        }


        public void UpdateStatusBar_ProgressBar()
        {
            
        }

        public void UpdateStatusBar_Message(ref string str)
        {
            toolStripStatusLabel_Message.Text = str;
        }

        public void UpdateStatusBar_Event()
        {
            toolStripStatusLabel_NameEvent.Text = ReaderWrapper.TotalEvent.ToString();
        }

        public void UpdateStatusBar_Report()
        {
            toolStripStatusLabel_TotalReport.Text = ReaderWrapper.TotalReport.ToString();
        }

        public void UpdateStatusBar_Time()
        {
            //toolStripStatusLabel_RunTime.Text = 
        }


        #endregion

        private void Form_TagReader_Resize(object sender, EventArgs e)
        {
            var h = Height/2;
            chart_Rssi.Height = h;
            chart_Phase.Height = h;
            chart_Doppler.Height = h;
            
            var w = Width/3;
            chart_Rssi.Left = 0;
            chart_Phase.Left = w*1;
            chart_Doppler.Left = w*2;

            chart_Rssi.Width = w;
            chart_Phase.Width = w;
            chart_Doppler.Width = w;
        }


        #region Button Clicked Event Response

        private void button_Connect_Click(object sender, EventArgs e)
        {
            ReaderWrapper.Initialize_Configuration();
            ReaderWrapper.setReader_PARM(); // default
            if (IsSettingsButtonClicked)
            {
                toolStripTextBox_Address.Text = ReaderWrapper.ReaderParameters.Ip;
                toolStripTextBox_Power.Text =
                    ReaderWrapper.ReaderParameters.TransmitPower.ToString(CultureInfo.InvariantCulture);
                toolStripComboBox_Frequency.Text =
                    Convert.ToString(920.625 + ReaderWrapper.ReaderParameters.ChannelIndex*0.25,
                        CultureInfo.InvariantCulture);
            }
            else
            {
                var ipAddress = toolStripTextBox_Address.Text;
                var txPower = Convert.ToDouble(toolStripTextBox_Power.Text);
                var frequency = toolStripComboBox_Frequency.SelectedItem.ToString();

                if (ipAddress == string.Empty)
                {
                    MessageBox.Show("IP Address Cannot be Empty");
                }
                if (txPower < 10 || txPower > 32.5)
                {
                    MessageBox.Show("Invalid Power!");
                }

                ReaderWrapper.ReaderParameters.Ip = ipAddress;
                ReaderWrapper.ReaderParameters.TransmitPower = txPower;
                ReaderWrapper.ReaderParameters.ChannelIndex =
                    Convert.ToUInt16((Convert.ToDouble(frequency) - 920.625)/0.25);
            }

            _isConnected2Reader = ReaderWrapper.ConnectToReader();

            MessageBox.Show(_isConnected2Reader ? "Successfully Connected!" : "Connect Failed!");

            if (_isConnected2Reader)
            {
                toolStrip_ReaderSettings.Enabled = false;
                toolStripButton_Connect.Enabled = false;
                ToolStripMenuItem_Connect.Enabled = false;

                toolStripButton_Start.Enabled = true;
                ToolStripMenuItem_Start.Enabled = true;
            }
        }

        private void button_Start_Click(object sender, EventArgs e)
        {
            if (_isConnected2Reader)
            {

                if (!_isStartButtonClicked)
                {
                    ReaderWrapper.ResetReaderToFactoryDefault();
                    ReaderWrapper.GetReaderCapabilities();
                }
                ReaderWrapper.Enable_Impinj_Extensions();
                ReaderWrapper.SetReaderConfig(); //SetReaderConfig_WithXML();
                ReaderWrapper.reader_AddSubscription();
                ReaderWrapper.Add_RoSpec(); //Add_RoSpec_WithXML();
                ReaderWrapper.Enable_RoSpec();
                
                _startTime = 0;
                _isStartButtonClicked = true;
                _isStopButtonClicked = false;

                toolStripButton_Start.Enabled = false;
                ToolStripMenuItem_Start.Enabled = false;

                toolStripButton_Stop.Enabled = true;
                ToolStripMenuItem_Stop.Enabled = true;

                //toolStripButton_Clear.Enabled = true;

                toolStripButton_Settings.Enabled = false;
                ToolStripMenuItem_Settings.Enabled = false;
            }
        }

        private void button_Stop_Click(object sender, EventArgs e)
        {
            if (_isConnected2Reader && _isStartButtonClicked)
            {
                ReaderWrapper.Stop();

                chart_Rssi.EndInit();
                chart_Phase.EndInit();
                chart_Doppler.EndInit();

                //_isStartButtonClicked = false;
                _isStopButtonClicked = true;
                IsSettingsButtonClicked = false;

                toolStripButton_Stop.Enabled = false;
                ToolStripMenuItem_Stop.Enabled = false;

                // 
                //toolStripButton_Start.Enabled = true;
                //ToolStripMenuItem_Start.Enabled = true;

                toolStripButton_Save.Enabled = true;
                ToolStripMenuItem_Save.Enabled = true;
                toolStripButton_Settings.Enabled = true;
                ToolStripMenuItem_Settings.Enabled = true;

                toolStripButton_Clear.Enabled = true;
            }
        }

        private void button_Export_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "csv file|*.csv",
                FilterIndex = 1,
                RestoreDirectory = true,
                DefaultExt = ".csv"
            };


            if (sfd.ShowDialog() == DialogResult.OK)
            {
                var fpath = sfd.FileName;
                CsvStreamWriter csvWriter = new CsvStreamWriter(fpath);
                ReaderWrapper.SaveData(csvWriter);
            }
        }

        private void button_Clear_Click(object sender, EventArgs e)
        {
            if (_isStopButtonClicked)
            {
                listView_Data.Items.Clear(); // clean up the Reader
                chart_Rssi.Dispose();
                chart_Phase.Dispose();
                chart_Doppler.Dispose();

                _isClearButtonClicked = true;
            }
        }

        private void toolStripButton_Refresh_Click(object sender, EventArgs e)
        {

        }

        private void button_Settings_Click(object sender, EventArgs e)
        {
            if (!IsSettingsButtonClicked)
            {
                var x = new SettingsWindow();
                x.Show();

                IsSettingsButtonClicked = true;
            }
        }

        private void ToolStripMenuItem_About_Click(object sender, EventArgs e)
        {
            var x = new AboutBox();
            x.Show();
        }
        
        private void quickAccessToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_showQuickAccessToolStrip)
            {
                _showQuickAccessToolStrip = false;
                quickAccessToolStripMenuItem.Checked = false;
                toolStrip_QuickAccess.Visible = false;

                if (_showReaderSettingsToolStrip)
                {
                    toolStrip_ReaderSettings.Left = toolStrip_QuickAccess.Left;
                }
                else
                {
                    listView_Data.Top = toolStrip_QuickAccess.Top;
                    listView_Data.Height += toolStrip_QuickAccess.Height;
                }
            }
            else
            {
                _showQuickAccessToolStrip = true;
                quickAccessToolStripMenuItem.Checked = true;
                toolStrip_QuickAccess.Visible = true;
                if (_showReaderSettingsToolStrip)
                {
                    toolStrip_ReaderSettings.Left = toolStrip_QuickAccess.Right;
                }
                else
                {
                    listView_Data.Top += toolStrip_QuickAccess.Height;
                    listView_Data.Height -= toolStrip_QuickAccess.Height;
                }
            }
        }

        private void readerSettingsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_showReaderSettingsToolStrip)
            {
                _showReaderSettingsToolStrip = false;
                readerSettingsToolStripMenuItem.Checked = false;
                toolStrip_ReaderSettings.Visible = false;
                if (!_showQuickAccessToolStrip)
                {
                    listView_Data.Top = toolStrip_QuickAccess.Top;
                    listView_Data.Height += toolStrip_QuickAccess.Height;
                }
            }
            else
            {
                _showReaderSettingsToolStrip = true;
                readerSettingsToolStripMenuItem.Checked = true;
                toolStrip_ReaderSettings.Visible = true;
                if (_showQuickAccessToolStrip)
                {
                    toolStrip_ReaderSettings.Left = toolStrip_QuickAccess.Right;
                }
                else
                {
                    listView_Data.Top = toolStrip_QuickAccess.Bottom;
                    listView_Data.Height -= toolStrip_QuickAccess.Height;
                }
            }
        }

        #endregion

    } // end FormTagReader
} // end namespace
