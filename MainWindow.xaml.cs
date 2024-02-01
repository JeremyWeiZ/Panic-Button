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
            if (string.IsNullOrWhiteSpace(userLocation))
            {
                LocationError.Text = "Please enter a location.";
                LocationError.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                // Hide the error message and proceed with your logic
                LocationError.Visibility = Visibility.Collapsed;
                UpdateUsers(userName, userLocation, userPhone, userEmail);
                BlueDot blueDotWindow = new BlueDot();
                blueDotWindow.Show();
                this.Hide();
                // Proceed with your submit logic here
            }

        




            //if (IsUniqueUser(users, userName, userLocation))
            //    {
            ////        // Add new user
            ////        User newUser = new User
            ////        {
            ////            Name = userName,
            ////            Location = userLocation,
            ////            Phone = userPhone,
            ////            Email = userEmail
            ////        };
            ////        users.Add(newUser);
            ////        SaveUsers(users, "users.ini");
            ////        SaveUsers(new List<User> { newUser }, "currentUser.ini");
            //}
            //    else
            //    {
            //        // Save to currentUser.json
            //        User currentUser = new User
            //        {
            //            Name = userName,
            //            Location = userLocation,
            //            Phone = userPhone,
            //            Email = userEmail
            //        };
            //        SaveUsers(new List<User> { currentUser }, "currentUser.ini");
            //    }



        }
        private void DeleteUser_Click(object sender, RoutedEventArgs e)
            {
            string userName = NameInput.Text;
            string userLocation = LocationInput.Text;
            if (IsUniqueUser(users, userName, userLocation))
            {
                System.Windows.MessageBox.Show("Can't find the user.");
            }
            else
            {
                var user = users.First(u => u.Name == userName && u.Location == userLocation);
                users.Remove(user);
                SaveUsers(users, "users.ini");
                System.Windows.MessageBox.Show($"This user is deleted.");
            }
        }

        private void UpdateUsers(string userName, string userLocation, string userPhone, string userEmail) 
        {
            User newUser = new User
            {
                Name = userName,
                Location = userLocation,
                Phone = userPhone,
                Email = userEmail
            };
            if (IsUniqueUser(users, userName, userLocation))
            {
                // Add new user
                users.Add(newUser);
                
            }
            else 
            {
                var user = users.First(u => u.Name == userName && u.Location == userLocation);
                user.Phone = userPhone;
                user.Email = userEmail;
            }
            SaveUsers(users, "users.ini");
            SaveUsers(new List<User> { newUser }, "currentUser.ini");

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
                
                string json = JsonConvert.SerializeObject(users, Formatting.Indented);

                
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error saving user name: {ex.Message}");
            }
        }
    }
}
