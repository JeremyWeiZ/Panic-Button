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

        public void ShowAlert(string name, string location,string email, string phone, string alertMessage)
        {
            NameTextBlock.Text = name;
            LocationTextBlock.Text = location;
            EmailTextBlock.Text = email;
            PhoneTextBlock.Text = phone;
            AlertType.Text = alertMessage;
            SoundPlayerHelper.PlaySoundRepeatedly();
            this.Show();
        }
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // Prevents the window from closing
            SoundPlayerHelper.StopSound();
            this.Hide(); // Hides the window instead
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
