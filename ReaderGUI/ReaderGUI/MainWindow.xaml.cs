using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SimpleLLRPSample;

namespace ReaderGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static CsvStreamWriter CsvWritter;
        public string IpAddress = "169.254.1.1";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ButtonSaveClicked(object sender, RoutedEventArgs e)
        {
            // invoke system window
            Program.SaveData(CsvWritter);
            throw new NotImplementedException();
        }

        private void ButtonConnect2ReaderClicked(object sender, RoutedEventArgs e)
        {
            Program.ConnectTo(IpAddress);
            // invoke system window
            throw new NotImplementedException();
        }

        private void ButtonStartReadingClicked(object sender, RoutedEventArgs e)
        {
            
            throw new NotImplementedException();
        }

        private void ButtonSettingsClicked(object sender, RoutedEventArgs e)
        {
            // new settings window
            throw new NotImplementedException();
        }

        private void ButtonClearDataGridClicked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ButtonStopReadingClicked(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void ListBoxEpcList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
