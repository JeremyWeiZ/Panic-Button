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
using System.Diagnostics;
using Cryptlex;

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
                try
                {
                    LexActivator.SetProductFile("C:\\Users\\Jeremy\\source\\repos\\WpfApp1_0909\\Resources\\product_v10e60476-ec82-4cc7-8767-f5a1d01d2001.dat");
                    LexActivator.SetProductId("10e60476-ec82-4cc7-8767-f5a1d01d2001", LexActivator.PermissionFlags.LA_USER);
                    int status;
                    LexActivator.SetLicenseKey("895CCE-4FAFD5-4647B5-EC4B4F-210DCF-A52BBA");
                    status = LexActivator.ActivateLicense();
                    if (status == LexStatusCodes.LA_OK || status == LexStatusCodes.LA_EXPIRED || status == LexStatusCodes.LA_SUSPENDED)
                    {
                        MessageBox.Show("Activation successful");
                    }
                    else
                    {
                        // Activation failed
                        MessageBox.Show("Activation failed");
                    }
                }
                catch (LexActivatorException ex)
                {
                    // handle error
                    MessageBox.Show("License check Error code: " + ex.Code.ToString() + " Error message: " + ex.Message);
                    return;

                }
                try
                {

                    int status = LexActivator.IsLicenseGenuine();
                    if (status == LexStatusCodes.LA_OK || status == LexStatusCodes.LA_EXPIRED || status == LexStatusCodes.LA_SUSPENDED || status == LexStatusCodes.LA_GRACE_PERIOD_OVER)
                    {
                        MessageBox.Show("License is activated: " + status.ToString());
                    }
                    else
                    {
                        MessageBox.Show("License is not activated: " + status.ToString());
                    }
                }
                catch (LexActivatorException ex)
                {
                    MessageBox.Show("Activation check Error code: " + ex.Code.ToString() + " Error message: " + ex.Message);
                }
                
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
                ToolStripMenuItem advancedToolsMenuItem = new ToolStripMenuItem("Advanced Tools");
                advancedToolsMenuItem.DropDownOpening += new EventHandler(TrayContextMenu_Opening);
                trayIcon.ContextMenuStrip.Items.Add(advancedToolsMenuItem);
                trayIcon.ContextMenuStrip.Items.Add("Exit", null, (sender, e) => Application.Current.Shutdown());

        }

      


        private bool keepListening = true;

        private List<string> receivedUserNames = new List<string>();

        private ManualResetEvent receivedUserNamesEvent = new ManualResetEvent(false);

        private ObservableCollection<User> users = new ObservableCollection<User>();

        private Thread listenerThread = null;

        private void Ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {

            if (e.ChangedButton == MouseButton.Left)
            {
                Dot.Fill = new SolidColorBrush(Colors.Red);
                BroadcastDangerAlert();

            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized || this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
        }




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

        private void TrayContextMenu_Opening(object sender, EventArgs e)
        {
            ToolStripMenuItem advancedToolsMenu = sender as ToolStripMenuItem;

            // Clear existing dynamically added items
            advancedToolsMenu.DropDownItems.Clear();

            // Path to the advanced_tool folder
            string folderPath = "advanced_tool";

            // Check if the directory exists
            if (Directory.Exists(folderPath))
            {
                // Get all items in the folder
                var files = Directory.GetFiles(folderPath);

                // Create a ToolStripMenuItem for each file
                foreach (var file in files)
                {
                    ToolStripMenuItem menuItem = new ToolStripMenuItem();
                    menuItem.Text = Path.GetFileName(file);
                    menuItem.Click += (s, args) => OpenTool(file); // Event handler for opening the file
                    advancedToolsMenu.DropDownItems.Add(menuItem);
                }
            }
        }


        private void ContextMenu_Opening(object sender, RoutedEventArgs e)
        {
            // Assuming 'AdvancedToolsMenu' is the x:Name of the 'Advanced tools' MenuItem in XAML
            MenuItem advancedToolsMenu = this.AdvancedToolsMenu;
            ToolStripMenuItem advancedToolsStripMenu = sender as ToolStripMenuItem;


            // Clear existing dynamically added items
            advancedToolsMenu.Items.Clear();

            // Path to the advanced_tool folder
            string folderPath = "advanced_tool";
            
            // Check if the directory exists
            if (Directory.Exists(folderPath))
            {
                // Get all items in the folder
                var files = Directory.GetFiles(folderPath);

                // Create a MenuItem for each file
                foreach (var file in files)
                {
                    MenuItem menuItem = new MenuItem();
                    menuItem.Header = Path.GetFileName(file);
                    menuItem.Click += (s, args) => OpenTool(file); // Event handler for opening the file
                    advancedToolsMenu.Items.Add(menuItem);
                }
            }
        }

        private void About_Click(object sender, RoutedEventArgs e) 
        {
            About aboutWindow = new About();
            aboutWindow.ShowDialog();
        }

        private void OpenTool(string filePath)
        {
            // Logic to open the file or perform the desired action
            try
            {
                // Use ProcessStartInfo to specify the file you want to open
                var psi = new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true // Important for opening files in their associated application
                };

                Process.Start(psi);
            }
            catch (Exception ex)
            {
                // Handle or log the exception as needed
                MessageBox.Show($"Error opening file: {ex.Message}");
            }
        }
    }
 }

