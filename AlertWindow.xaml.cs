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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for AlertWindow.xaml
    /// </summary>
    /// 
   
    public partial class AlertWindow : Window
    {
        private DispatcherTimer timer;
        private bool isRed = false;
        public AlertWindow()
        {
            InitializeComponent();
            StartFlashingText();
            
        }

        public void ShowAlert(string message)
        {
            TriggerInfo.Text = message;
            this.Show();
        }

        private void StartFlashingText()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1); // Set the flashing interval (1 second in this example)
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Toggle the color
            isRed = !isRed;
            AlertTextBlock.Foreground = isRed ? new SolidColorBrush(Colors.White) : new SolidColorBrush(Colors.Black);
        }
    }
}
