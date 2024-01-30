using System;
using System.IO;
using System.Windows;
using System.Windows.Forms; // Add this namespace
using System.Drawing; // Add this namespace
using Path = System.IO.Path;
using Application = System.Windows.Application;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using Microsoft.VisualBasic.ApplicationServices;
using System.Windows.Controls;
using System.Linq;

namespace WpfApp1
{
    public class User
    {
        public string Name { get; set; }
        public string Location { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }

        public string NameAndLocation
        {
            get
            {
                return $"{Location} ({Name})";
            }
        }
    }
    public partial class MainWindow : Window
    {
        public NotifyIcon trayIcon;
        private List<User> users = new List<User>();
        private const string JsonFilePath = "users.json";
        public MainWindow()
        {
            InitializeComponent();
            LoadUsers();
            
            trayIcon = new NotifyIcon
            {
                Icon = new Icon("Resources/bluedot.ico"), // Specify the path to your icon file
                Visible = false,
                ContextMenuStrip = new ContextMenuStrip()
            };
         

            // Add an Exit menu item
            trayIcon.ContextMenuStrip.Items.Add("Show Users", null, ShowUsers_Click);
            trayIcon.ContextMenuStrip.Items.Add("Exit", null, (sender, e) => Application.Current.Shutdown());
        }

        

        private void ShowUsers_Click(object sender, EventArgs e)
        {
            // Logic to show the user list window
            var userListWindow = new UserListWindow();

            List<string> userNames = FetchUserNamesFromNetwork(); // Replace with actual method to fetch user names
            userListWindow.UpdateUserList(userNames);
            userListWindow.Show();
        }

        private List<string> FetchUserNamesFromNetwork()
        {
            // Implement your logic to fetch user names from the network
            return new List<string>(); // Placeholder
        }


        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            // 获取用户输入的姓名
            string userName = NameInput.Text;
            string userLocation = LocationInput.Text;
            string userPhone = PhoneInput.Text;
            string userEmail = EmailInput.Text;
        

           
            
            if (IsUniqueUser(users, userName, userLocation))
                {
                    // Add new user
                    User newUser = new User
                    {
                        Name = userName,
                        Location = userLocation,
                        Phone = userPhone,
                        Email = userEmail
                    };
                    users.Add(newUser);
                    SaveUsers(users, "users.json");
                    SaveUsers(new List<User> { newUser }, "currentUser.json");
            }
                else
                {
                    // Save to currentUser.json
                    User currentUser = new User
                    {
                        Name = userName,
                        Location = userLocation,
                        Phone = userPhone,
                        Email = userEmail
                    };
                    SaveUsers(new List<User> { currentUser }, "currentUser.json");
                }

            // 启动蓝点窗口
            BlueDot blueDotWindow = new BlueDot();
            blueDotWindow.Show();

            // 隐藏主窗口
            this.Hide();
            trayIcon.Visible = true;
        }

        private bool IsUniqueUser(List<User> users, string name, string location)
        {
            return !users.Any(u => u.Name == name && u.Location == location);
        }

        private void LoadUsers()
        {
            if (File.Exists(JsonFilePath))
            {
                string json = File.ReadAllText(JsonFilePath);
                users = JsonConvert.DeserializeObject<List<User>>(json);
                foreach (var user in users)
                {
                    UserSelection.Items.Add(user.NameAndLocation);

                }
            }
            if (UserSelection.Items.Count > 0)
            {
                UserSelection.SelectedIndex = 0;
            }
        }

        private void UserSelection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (UserSelection.SelectedIndex != -1)
            {
                string selectedUserInfo = UserSelection.SelectedItem.ToString();
                // Assuming the format in the ComboBox is "Name (Location)"
                //var parts = selectedUserName.Split(new[] { " (" }, StringSplitOptions.None);
                //string name = parts[0];
                //string location = parts.Length > 1 ? parts[1].TrimEnd(')') : "";

                var selectedUser = users.FirstOrDefault(u => u.NameAndLocation == selectedUserInfo);
                if (selectedUser != null)
                {
                    NameInput.Text = selectedUser.Name;
                    LocationInput.Text = selectedUser.Location;
                    PhoneInput.Text = selectedUser.Phone;
                    EmailInput.Text = selectedUser.Email;
                }
            }
        }

        private void SaveUsers(List<User> users, string filePath)
        {
            try
            {
                // 获取应用程序的安装目录
                string json = JsonConvert.SerializeObject(users, Formatting.Indented);

                // 写入文件
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving user name: {ex.Message}");
            }
        }
    }
}
