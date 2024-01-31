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
using System.Collections.ObjectModel;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        
        private List<User> users = new List<User>();
        private const string JsonFilePath = "users.ini";
        public MainWindow()
        {
            InitializeComponent();
            LoadUsers();
            
           
        }

        

        
        


        private void SubmitButton_Click(object sender, RoutedEventArgs e)
        {
            
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
                    SaveUsers(users, "users.ini");
                    SaveUsers(new List<User> { newUser }, "currentUser.ini");
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
                    SaveUsers(new List<User> { currentUser }, "currentUser.ini");
                }

            // 启动蓝点窗口
            BlueDot blueDotWindow = new BlueDot();
            blueDotWindow.Show();

            // 隐藏主窗口
            this.Hide();
            
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
