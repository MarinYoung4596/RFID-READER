using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Forms;
using TagReader.RFIDReader;
using System.Windows.Forms.DataVisualization.Charting;

namespace TagReader
{
    public partial class FormTagReader : Form
    {
        private static bool _isConnected2Reader;
        private static bool _isStartButtonClicked;
        public  static bool IsStopButtonClicked;
        private static bool _isClearButtonClicked;
        public  static bool IsSettingsButtonClicked;
        public  static bool IsSettigsWindowShowing;
        private static bool _showQuickAccessToolStrip = true;
        private static bool _showReaderSettingsToolStrip = true;


        public FormTagReader()
        {
            InitializeComponent();
            ReaderWrapper.MainForm = this;

            ReaderWrapper.Initialize_Configuration();

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
        private DateTime _startTimeDateTime;
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
                    var dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
                    _startTimeDateTime = dt.AddSeconds(Convert.ToDouble(_startTime / 1000000)).ToLocalTime();
                }

                var s = new Series
                {
                    ChartType = SeriesChartType.FastLine,
                    Name = epc + "_" + antenna
                };
                chart_Rssi.Series.Add(s);

                var s1 = new Series
                {
                    ChartType = SeriesChartType.FastLine,
                    Name = epc + "_" + antenna
                };
                chart_Phase.Series.Add(s1);

                var s2 = new Series
                {
                    ChartType = SeriesChartType.FastLine,
                    Name = epc + "_" + antenna
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

            if (SettingsWindow.IsTimerModeActied)
            {
                int t = ConvertTime(time);
                Invoke(new Action(() =>
                {
                    UpdateStatusBar_ProgressBar(ref t);
                }));
                
                if (t >= ReaderWrapper.ReaderParameters.Duration)
                {
                    StopReceive();

                    if (SettingsWindow.IsAutoSaveChecked)
                    {
                        var fpath = @"C:\Users\Marin\Desktop\";
                        if (!Directory.Exists(fpath))
                            Directory.CreateDirectory(fpath);

                        var dt = DateTime.Now;
                        var strCurrentTime = dt.ToString("yyyyMMdd_HHmmss");
                        var fname = strCurrentTime + ".csv";
                        var csvWriter = new CsvStreamWriter(fpath + fname);
                        ReaderWrapper.SaveData(csvWriter);
                    }
                }
            }
        }


        public void UpdateStatusBar_ProgressBar(ref int val)
        {
            toolStripProgressBar.Value = val;
            if (toolStripProgressBar.Value < toolStripProgressBar.Maximum)
                toolStripProgressBar.PerformStep();
        }

        public void UpdateStatusBar_Message(ref string str)
        {
            toolStripStatusLabel_Message.Text = str;
        }

        public void UpdateStatusBar_Event()
        {
            toolStripStatusLabel_TotalEvent.Text = ReaderWrapper.TotalEvent.ToString();
        }

        public void UpdateStatusBar_Report()
        {
            toolStripStatusLabel_TotalReport.Text = ReaderWrapper.TotalReport.ToString();
        }

        public void UpdateStatusBar_Time(ulong timestamp)
        {
            System.DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dt = dt.AddSeconds(Convert.ToDouble(timestamp / 1000000)).ToLocalTime();
            
            TimeSpan time = dt - _startTimeDateTime;
            toolStripStatusLabel_RunTime.Text = time.Hours + @":" + time.Minutes + @":" + time.Seconds;
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

            //MessageBox.Show(_isConnected2Reader ? "Successfully Connected!" : "Connect Failed!");

            if (_isConnected2Reader)
            {
                toolStripButton_Settings.Enabled = false;
                ToolStripMenuItem_Settings.Enabled = false;

                toolStripButton_Connect.Enabled = false;
                ToolStripMenuItem_Connect.Enabled = false;

                toolStripButton_Start.Enabled = true;
                ToolStripMenuItem_Start.Enabled = true;
            }
        }

        private void StartReceive()
        {
            ReaderWrapper.Start();

            _startTime = 0;
            _isStartButtonClicked = true;
            IsStopButtonClicked = false;

            toolStripButton_Start.Enabled = false;
            ToolStripMenuItem_Start.Enabled = false;

            toolStripButton_Stop.Enabled = true;
            ToolStripMenuItem_Stop.Enabled = true;

            //toolStripButton_Clear.Enabled = true;

            toolStripButton_Settings.Enabled = false;
            ToolStripMenuItem_Settings.Enabled = false;
        }

        private void button_Start_Click(object sender, EventArgs e)
        {
            if (_isConnected2Reader)
            {
                StartReceive();

                if (SettingsWindow.IsTimerModeActied)
                {
                    toolStripProgressBar.Maximum = ReaderWrapper.ReaderParameters.Duration;
                    toolStripProgressBar.Step = 1;
                }
            }
        }

        private void StopReceive()
        {
            IsStopButtonClicked = true;
            IsSettingsButtonClicked = false;
            _isClearButtonClicked = false;

            ReaderWrapper.Stop();

            chart_Rssi.EndInit();
            chart_Phase.EndInit();
            chart_Doppler.EndInit();

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

        private void button_Stop_Click(object sender, EventArgs e)
        {
            if (_isConnected2Reader && _isStartButtonClicked)
            {
                StopReceive();
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
            if (IsStopButtonClicked)
            {
                listView_Data.Items.Clear(); // clean up the Reader
                chart_Rssi.Dispose();
                chart_Phase.Dispose();
                chart_Doppler.Dispose();

                var a = 0;
                UpdateStatusBar_ProgressBar(ref a);
                _isClearButtonClicked = true;
            }
        }

        private void toolStripButton_Refresh_Click(object sender, EventArgs e)
        {

        }

        private void button_Settings_Click(object sender, EventArgs e)
        {
            if (!IsSettigsWindowShowing)
            {
                var x = new SettingsWindow();
                x.Show();

                IsSettingsButtonClicked = true;
                IsSettigsWindowShowing = true;
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
