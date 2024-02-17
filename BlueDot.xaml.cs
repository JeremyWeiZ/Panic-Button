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
using System.Media;
using System.Windows.Threading;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    /// 

    public partial class BlueDot : Window
    {
        public NotifyIcon trayIcon;
        public AlertWindow alertWindow2 = new AlertWindow();



        public BlueDot()
            {
                


                InitializeComponent();
                
                //testing(alertWindow2);
                StartListening(alertWindow2);
                originalFill = Dot.Fill as ImageBrush; // Store the original ImageBrush fill














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




        private DateTime lastClickTime = DateTime.MinValue;
        private ImageBrush originalFill;

        private bool keepListening = true;

        private List<string> receivedUserNames = new List<string>();

        private ManualResetEvent receivedUserNamesEvent = new ManualResetEvent(false);

        private ObservableCollection<User> users = new ObservableCollection<User>();

        private Thread listenerThread = null;

        private void Ellipse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TimeSpan clickSpan = DateTime.Now - lastClickTime;
            if (clickSpan.TotalMilliseconds < System.Windows.Forms.SystemInformation.DoubleClickTime) // Adjust the threshold as needed
            {
                if (e.ChangedButton == MouseButton.Left)
                {
                    // Change the Dot's Fill color to red temporarily
                    Dot.Fill = new SolidColorBrush(Colors.Red);

                    // Broadcast the danger alert
                    BroadcastDangerAlert();

                    // Create a DispatcherTimer to revert the fill back after 1 second
                    DispatcherTimer timer = new DispatcherTimer();
                    timer.Interval = TimeSpan.FromSeconds(1);
                    timer.Tick += (s, args) =>
                    {
                        // Revert the Dot's Fill back to the original ImageBrush
                        Dot.Fill = originalFill;

                        // Stop and dispose the timer
                        timer.Stop();
                    };
                    timer.Start();
                }
            }
            lastClickTime = DateTime.Now;
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized || this.WindowState == WindowState.Maximized)
            {
                this.WindowState = WindowState.Normal;
            }
        }
        private void Size_Click(object sender, RoutedEventArgs e)
        {
            MenuItem clickedItem = (MenuItem)sender;
            switch (clickedItem.Header.ToString().ToLower())
            {
                case "small":
                    this.Width = 50;
                    this.Height = 50;
                    Dot.Width = 50;
                    Dot.Height = 50;
                    break;
                case "medium":
                    this.Width = 80;
                    this.Height = 80;
                    Dot.Width = 80;
                    Dot.Height = 80;
                    break;
                case "large":
                    this.Width = 120;
                    this.Height = 120;
                    Dot.Width = 120;
                    Dot.Height = 120;
                    break;
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
                string alertType = "Assistance is Required";
                

                if (GetCheckedAssistanceTypeItem().Header.ToString() != null)
                {
                    alertType = GetCheckedAssistanceTypeItem().Header.ToString();
                }
                
                string dangerMessage = $"ALERT_DANGER_Name: {currentUser.Name}, Location: {currentUser.Location}, Phone: {currentUser.Phone}, Email: {currentUser.Email}#8AlertType:{alertType}";
                
                byte[] bytesToSend = Encoding.ASCII.GetBytes(dangerMessage);
                udpClient.Send(bytesToSend, bytesToSend.Length, ip);
                
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
        private void StartListening(AlertWindow alertWindow)
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
                            //    SendUserNameBack(remoteEP.Address.ToString());3w2eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee
                            //}
                            //else
                            //{
                            //    if (!receivedUserNames.Contains(message))
                            //    receivedUserNames.Add(message);

                            //}
                            

                            if (message.StartsWith("ALERT_DANGER_"))
                            {
                                User currentUser = ReadUserNameFromIniFile();
                                string[] parts = message.Split(new string[] { "#8" }, StringSplitOptions.None);
                                string dangerMessage = parts[0].Replace("ALERT_DANGER_", "");
                                var fields = dangerMessage.Split(new string[] { ", " }, StringSplitOptions.None);
                                string alertType = parts[1];
                                if (dangerMessage != $"Name: {currentUser.Name}, Location: {currentUser.Location}, Phone: {currentUser.Phone}, Email: {currentUser.Email}")
                                {
                                    Application.Current.Dispatcher.Invoke(() => alertWindow.ShowAlert(fields[0], fields[1], fields[2], fields[3], alertType));

                                }

                                




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
                                    MessageBox.Show(message);


                                    if (!users.Any(u => u.Name == receivedUser.Name && u.Location == receivedUser.Location))
                                    {

                                        
                                        Application.Current.Dispatcher.Invoke(() => users.Add(receivedUser));
                                        Thread.Sleep(200);
                                        

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
                MessageBox.Show(requesterIPAddress + bytesToSend);
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
            //receivedUserNamesEvent = new ManualResetEvent(false);
            

            //// Wait for the receivedUserNamesEvent to be set
            //bool isReceived = receivedUserNamesEvent.WaitOne(TimeSpan.FromSeconds(1)); // 0.5 seconds timeout

            //if (isReceived)
            //{
            //    ShowUsers();
            //}
            ShowUsers();

        }

        public void ShowUsers() 
        {
            users = new ObservableCollection<User>();
            BroadcastRequestForUsernames();
            var userListWindow = new UserListWindow(users);

            
            userListWindow.Show();
        }


        private void ShowAlert(string userInfo)
            {

                
                
                MessageBox.Show(userInfo);
                
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
            UpdateSelectedAssistanceType(GetCheckedAssistanceTypeItem().Header.ToString());

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

        private MenuItem GetCheckedAssistanceTypeItem()
        {
            // Assuming 'AssistanceTypeMenu' is the name of the MenuItem that contains the assistance type options.
            // If it is not named, you will need to find it dynamically or reference it directly if you have a direct reference.
            var assistanceTypeMenu = this.AssistanceTypeMenu; // Replace with actual reference if named differently

            if (assistanceTypeMenu != null)
            {
                foreach (MenuItem item in assistanceTypeMenu.Items)
                {
                    if (item.IsChecked)
                    {
                        return item;
                    }
                }
            }

            return null; // No item is checked
        }

        private void AssistanceType_Click(object sender, RoutedEventArgs e)
        {
            // Cast the sender back to a MenuItem
            var clickedItem = sender as MenuItem;

            // Access the parent MenuItem to iterate over its items
            var parentItem = clickedItem.Parent as MenuItem;

            if (parentItem != null)
            {
                foreach (MenuItem item in parentItem.Items)
                {
                    // Ensure that we uncheck all items except the one that was just clicked
                    if (item != clickedItem)
                    {
                        item.IsChecked = false;
                    }
                }
            }

            // The clicked item's IsChecked property is automatically toggled by WPF
            if (clickedItem != null) 
            {
                UpdateSelectedAssistanceType(clickedItem.Header.ToString());
                
            }
        }

        private void UpdateSelectedAssistanceType(string assistanceType)
        {
            var topMenuItem = this.SelectedAssistanceTypeItem; // Assuming 'this' is the context of your window/control
            if (topMenuItem != null)
            {
                topMenuItem.Header = assistanceType;
                topMenuItem.Visibility = Visibility.Visible;
                topMenuItem.FontWeight = FontWeights.Bold;

                // Optionally, reset the check state if the menu is dynamic and can change based on other conditions
                //var assistanceMenuItem = FindAssistanceMenuItem(assistanceType);
                //if (assistanceMenuItem != null)
                //{
                //    assistanceMenuItem.IsChecked = true;
                //}
            }
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

