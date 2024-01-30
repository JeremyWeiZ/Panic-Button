using System;
using System.IO;
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
using Path = System.IO.Path;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using Newtonsoft.Json;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class BlueDot : Window
    {

            public BlueDot()
            {
                InitializeComponent();
                StartListening();

            }



        //private void Window_LocationChanged(object sender, EventArgs e)
        //{
        //    const int threshold = 30; // Pixels from the top screen edge to trigger full screen
        //    if (this.Top <= threshold)
        //    {
        //        // Prevent the window from maximizing or moving off-screen
        //        this.Top = threshold;
        //    }

        //    if (this.Left <= threshold)
        //    {
        //        // Prevent the window from maximizing or moving off-screen
        //        this.Left = 50;
        //    }


        //}




        private void Ellipse_MouseUp(object sender, MouseButtonEventArgs e)
            {
            // 改变颜色并显示警报
                if (e.ChangedButton == MouseButton.Left)
                {
                    Dot.Fill = new SolidColorBrush(Colors.Red);
                    BroadcastDangerAlert();
                    
                }

            // 如果需要，可以在这里添加警报声音的代码
        }

        private bool keepListening = true;

        private List<string> receivedUserNames = new List<string>();

        private ManualResetEvent receivedUserNamesEvent = new ManualResetEvent(false);

        private void BroadcastDangerAlert()
        {
            UdpClient udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 12345);

            try
            {
                string userName = ReadUserNameFromIniFile();
                string dangerMessage = $"ALERT_DANGER_{userName}";
                byte[] bytesToSend = Encoding.ASCII.GetBytes(dangerMessage);
                udpClient.Send(bytesToSend, bytesToSend.Length, ip);
                ShowAlert();
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine("Error broadcasting danger alert: " + ex.Message);
            }
            finally
            {
                udpClient.Close();
            }
        }
        private void StartListening()
        {
            Thread listenerThread = new Thread(() =>
            {
                UdpClient udpClient = new UdpClient(12345);
                IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 12345);

                try
                {
                    while (keepListening)
                    {
                        byte[] bytesReceived = udpClient.Receive(ref remoteEP);
                        string message = Encoding.ASCII.GetString(bytesReceived);

                        //if (message == "REQUEST_USERNAMES")
                        //{
                        //    SendUserNameBack(remoteEP.Address.ToString());
                        //}
                        //else
                        //{
                        //    if (!receivedUserNames.Contains(message))
                        //    receivedUserNames.Add(message);

                        //}
                        if (message.StartsWith("ALERT_DANGER_"))
                        {
                            
                            string dangerUserName = message.Substring("ALERT_DANGER_".Length);
                            if(dangerUserName!= ReadUserNameFromIniFile())
                            MessageBox.Show($"{dangerUserName} is in danger!", "Danger Alert");

                        }
                        else if (message == "REQUEST_USERNAMES")
                        {
                            receivedUserNamesEvent.Reset();
                            SendUserNameBack(remoteEP.Address.ToString());
                        }
                        else
                        {
                            if (!receivedUserNames.Contains(message))
                            {
                                receivedUserNames.Add(message);
                                receivedUserNamesEvent.Set();
                            }
                            
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle exceptions
                }
                finally
                {
                    udpClient.Close();
                }
            });

            listenerThread.IsBackground = true;
            listenerThread.Start();
        }

        private void SendUserNameBack(string requesterIPAddress)
        {
            UdpClient udpClient = new UdpClient();
            IPEndPoint requesterEP = new IPEndPoint(IPAddress.Parse(requesterIPAddress), 12345);

            try
            {
                string userName = ReadUserNameFromIniFile(); // Retrieves the user name from user.ini
                byte[] bytesToSend = Encoding.ASCII.GetBytes(userName);
                udpClient.Send(bytesToSend, bytesToSend.Length, requesterEP);
            }
            catch (Exception ex)
            {
                // Handle any potential exceptions
                Console.WriteLine("Error sending username back: " + ex.Message);
            }
            finally
            {
                udpClient.Close();
            }
        }


        private void BroadcastRequestForUsernames()
        {
           
           UdpClient udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 12345);

            try
            {
                byte[] bytesToSend = Encoding.ASCII.GetBytes("REQUEST_USERNAMES");
                udpClient.Send(bytesToSend, bytesToSend.Length, ip);
            }
            catch (Exception ex)
            {
                // Handle exceptions
            }
            finally
            {
                udpClient.Close();
            }
        }

        private void Ellipse_MouseMove(object sender, MouseEventArgs e)
            {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                // this prevents win7 aerosnap
                if (this.ResizeMode != System.Windows.ResizeMode.NoResize)
                {
                    this.ResizeMode = System.Windows.ResizeMode.NoResize;
                    this.UpdateLayout();
                }

                DragMove();
            }
        }
            private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
            {
                Application.Current.Shutdown();
            }

        //private void ShowUsers_Click(object sender, RoutedEventArgs e)
        //{
        //    var userListWindow = new UserListWindow();
        //    receivedUserNames = new List<string> { ReadUserNameFromIniFile() };
        //    BroadcastRequestForUsernames();

        //    // Here you would normally fetch the list of user names from your networking logic
        //    List<string> userNames = FetchUserNamesFromNetwork();

        //    userListWindow.UpdateUserList(userNames);
        //    userListWindow.Show();
        //}

        private void ShowUsers_Click(object sender, RoutedEventArgs e)
        {

            receivedUserNamesEvent = new ManualResetEvent(false);
            var userListWindow = new UserListWindow();
            receivedUserNames = new List<string> { ReadUserNameFromIniFile() };
            BroadcastRequestForUsernames();

            // Wait for the receivedUserNamesEvent to be set
            bool isReceived = receivedUserNamesEvent.WaitOne(TimeSpan.FromSeconds(0.5)); // 0.5 seconds timeout

            if (isReceived)
            {

                List<string> userNames = FetchUserNamesFromNetwork();
                userListWindow.UpdateUserList(userNames);
                userListWindow.Show();
            }
            else { MessageBox.Show("No users in your local network, please "); }
        }

        private List<string> FetchUserNamesFromNetwork()
        {
            // Implement your logic to fetch user names from the network
            return receivedUserNames; 
        }

        private void ShowAlert()
            {
                string userName = ReadUserNameFromIniFile();
                MessageBox.Show($"Attention! {userName} pressed the button", "Alert");
                Dot.Fill = new SolidColorBrush(Colors.Blue);
        }

        private string ReadUserNameFromIniFile()
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(appDirectory, "user.ini");
                return File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading user name");
                return "Unknown User";
            }
        }
    }
    }

