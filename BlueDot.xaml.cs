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
using Microsoft.VisualBasic.ApplicationServices;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Drawing;

using MessageBox = System.Windows.MessageBox;
using Application = System.Windows.Application;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    /// 

    public partial class BlueDot : Window
    {
        public NotifyIcon trayIcon;


        public BlueDot()
            {
                InitializeComponent();
                StartListening();
            
                trayIcon = new NotifyIcon
                {
                    Icon = new Icon("Resources/bluedot.ico"), // Specify the path to your icon file
                    Visible = false,
                    ContextMenuStrip = new ContextMenuStrip()
                };
                trayIcon.Visible = true;


            // Add an Exit menu item
                trayIcon.ContextMenuStrip.Items.Add("Show Users", null, ShowUsers_Click);
                trayIcon.ContextMenuStrip.Items.Add("Exit", null, (sender, e) => Application.Current.Shutdown());

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
           
                if (e.ChangedButton == MouseButton.Left)
                {
                    Dot.Fill = new SolidColorBrush(Colors.Red);
                    BroadcastDangerAlert();
                    
                }

           
        }

        private bool keepListening = true;

        private List<string> receivedUserNames = new List<string>();

        private ManualResetEvent receivedUserNamesEvent = new ManualResetEvent(false);

        private ObservableCollection<User> users = new ObservableCollection<User>();

        private Thread listenerThread = null;


        private void BroadcastDangerAlert()
        {
            UdpClient udpClient = new UdpClient();
            udpClient.EnableBroadcast = true;
            IPEndPoint ip = new IPEndPoint(IPAddress.Broadcast, 12345);

            try
            {
                User currentUser  = ReadUserNameFromIniFile();
                string dangerMessage = $"ALERT_DANGER_Name: {currentUser.Name}, Location: {currentUser.Location}, Phone: {currentUser.Phone}, Email: {currentUser.Email}";
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

            if (listenerThread == null || !listenerThread.IsAlive)
            {
                listenerThread = new Thread(() =>
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
                                User currentUser = ReadUserNameFromIniFile();
                                string dangerMessage = message.Substring("ALERT_DANGER_".Length);
                                if (dangerMessage != $"Name: {currentUser.Name}, Location: {currentUser.Location}, Phone: {currentUser.Phone}, Email: {currentUser.Email}")
                                    MessageBox.Show($"{dangerMessage} is in danger!", "Danger Alert");

                            }
                            else if (message == "REQUEST_USERNAMES")
                            {
                                receivedUserNamesEvent.Reset();
                                SendUserNameBack(remoteEP.Address.ToString());
                            }
                            else
                            {
                                try
                                {
                                    User receivedUser = JsonConvert.DeserializeObject<User>(message);


                                    if (!users.Any(u => u.Name == receivedUser.Name && u.Location == receivedUser.Location))
                                    {

                                        Application.Current.Dispatcher.Invoke(() => users.Add(receivedUser));

                                    }
                                }
                                catch (JsonException jsonEx)
                                {
                                    // Handle JSON parsing exception
                                    MessageBox.Show("Json Error" + jsonEx);
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
            
        }

        private void SendUserNameBack(string requesterIPAddress)
        {
            UdpClient udpClient = new UdpClient();
            IPEndPoint requesterEP = new IPEndPoint(IPAddress.Parse(requesterIPAddress), 12345);

            try
            {
                User currentUser = ReadUserNameFromIniFile();

                string jsonUserInfo = JsonConvert.SerializeObject(new
                {
                    Name = currentUser.Name,
                    Location = currentUser.Location,
                    Phone = currentUser.Phone,
                    Email = currentUser.Email
                });

                byte[] bytesToSend = Encoding.ASCII.GetBytes(jsonUserInfo);
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




        public static void BroadcastRequestForUsernames()
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

        private void Ellipse_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
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

        private void ShowUsers_Click(object sender, EventArgs e)
        {
            receivedUserNamesEvent = new ManualResetEvent(false);
            ShowUsers();

            // Wait for the receivedUserNamesEvent to be set
            //bool isReceived = receivedUserNamesEvent.WaitOne(TimeSpan.FromSeconds(0.5)); // 0.5 seconds timeout

            //if (isReceived)
            //{

            //    userListWindow.UpdateUserList("test");
            //    userListWindow.Show();
            //}
            //else { MessageBox.Show("No users in your local network, please "); }
        }

        public void ShowUsers() 
        {
            var userListWindow = new UserListWindow(users);

            BroadcastRequestForUsernames();
            userListWindow.Show();
        }


        private void ShowAlert()
            {
                string userName = ReadUserNameFromIniFile().Name;
                string userLocation = ReadUserNameFromIniFile().Location;
                string userPhone = ReadUserNameFromIniFile().Phone;
                string userEmail = ReadUserNameFromIniFile().Email;
                MessageBox.Show($"Attention! {userName} at {userLocation} pressed the button, call {userName} at {userPhone} or email at {userEmail}", "Alert");
                
        }

        private User ReadUserNameFromIniFile()
        {
            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string filePath = Path.Combine(appDirectory, "currentUser.ini");
                string jsonContent = File.ReadAllText(filePath);

                List<User> users = JsonConvert.DeserializeObject<List<User>>(jsonContent);

                return users!=null && users.Count >0 ? users[0] : new User();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error reading user name");
                return new User { Name = "Unknown User" };
            }
        }
    }
 }

