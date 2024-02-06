using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Media;
using System.Timers;
using System.Windows.Media;
using System.Windows;

namespace WpfApp1
{
    public class SoundPlayerHelper
    {
        private static MediaPlayer player = new MediaPlayer();
        private static Uri uri = new Uri("Resources/converted_Siren.wav",UriKind.Relative);
        //private static Timer stopTimer = new Timer(10000); // 10 seconds

        public static void PlaySiren()
        {
            player.MediaFailed += (s, e) =>
            {
                MessageBox.Show($"Media Failed: {e.ErrorException.Message}");
            };

            player.Volume = 1;
            player.Open(uri);
            player.Play();

            //stopTimer.Elapsed += StopTimer_Elapsed;
            //stopTimer.Start();
        }

        //private static void StopTimer_Elapsed(object sender, ElapsedEventArgs e)
        //{
        //    player.Stop();
        //    stopTimer.Stop();
        //}
    }
}
