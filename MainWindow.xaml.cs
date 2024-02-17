using System;
using System.IO;
using System.Windows;
using System.Windows.Forms; // Add this namespace
using System.Drawing; // Add this namespace
using Path = System.IO.Path;
using MessageBox = System.Windows.MessageBox;
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
using Cryptlex;
using TextBox = System.Windows.Controls.TextBox;

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
            LocationInput.TextChanged += ValidateInput;
            NameInput.TextChanged += ValidateInput;
            PhoneInput.TextChanged += ValidateInput;
            EmailInput.TextChanged += ValidateInput;


        }



        private void ValidateInput(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                // Check if the text contains unsupported characters
                if (textBox.Text.Contains("#") || textBox.Text.Contains(","))
                {
                    // Display an error message or handle as needed
                    MessageBox.Show("Unsupported format: Input cannot contain '#' or ','.", "Input Error", MessageBoxButton.OK, MessageBoxImage.Error);

                    // Optionally, remove the unsupported characters from the input
                    textBox.Text = textBox.Text.Replace("#", string.Empty).Replace(",", string.Empty);

                    // Set the cursor back to the end of the text
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }
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
                try
                {
                    LexActivator.SetProductData("RDQ3RUNGRDk1QTU1Rjc2MkU4REUyNDVBNjc1NjNCMjc=.NQB0kcM7O8rBy+XjbyaEN6sv3ObwIQWFBvRx8L0blRc1wPi9Cbk6zHOjFzPVQndXFufUYOmr7TH2uYAlGeL1CZu0uqKMFut2XxyFuR2GYo1PMCITv9ZCySI64eo7CYOfYEA7PXZGSZFLhnZxAKx5v4/m/i1GsSrodRCx/j4sxMqibjDalW+z09MJcW0L2op4VP1kh9I9QNyB/8RhGHFTs23jcE7NmnjSJ9+1zGnHCXEulN+ONSoFy5zkWs+wrgBZG6QWjTQXhMYdESscV8Igykz/YIaNV+hqF7peGX1K3zZ6uyjUb16T2Keir2zjqJrDPHo8QqEgsKC8/Hu13QtU6F700/BBd1L7NJTW+2q0dWqVDN/xC3CW9V2/AZInlw1pAi+8N6a0bZfS76l1P7YEvPDA1TKViyMHnlVby+VcXLAjgWrGgrqpcvHCmDe+brpeLYi2FnyonS4oyi5XaB8tpb+/hRu2lGBPaEwfC2eHIGd6QjL1RvQMUlab4gCPLQvYS1Co87NAiweYcwbhTSEyL73owUJg0i5mhcybTZm6LIZlCMIAq/AKFVROxOgdpArz7gvl+f+SGOOFxKsik64xbGE8Op+40RGHm6eHnSpQc0dcWciAHb/S0MPllBwKmIz0H84o4CFtfsVjBiVvxAEw6xd6fSgGwhxo5JUgGmJuR5+d2236mFJaGphMfNTxYNX9l2fEg28soVaoaZ7dniqkgozcoKcDA+mHNlkaxxDbZ/VigMXMmiZPZrHo7ft+JOPpQ35teQHwu9tUHxZz/Bp9Zu4S3ZQDJZ9Iublsf9T2qY0CqbaQHukawY06Jz7RHSqq");
                    LexActivator.SetProductId("10e60476-ec82-4cc7-8767-f5a1d01d2001", LexActivator.PermissionFlags.LA_USER);
                    int status;
                    String license = File.ReadAllText("license.txt");
                    LexActivator.SetLicenseKey(license);
                    status = LexActivator.ActivateLicense();
                    if (status == LexStatusCodes.LA_OK || status == LexStatusCodes.LA_EXPIRED || status == LexStatusCodes.LA_SUSPENDED)
                    {
                        Logger.Log("Activation successful");
                       
                    }
                    else
                    {
                        // Activation failed
                        Logger.Log("Activation failed");
                        MessageBox.Show("Activation failed");

                    }
                }
                catch (LexActivatorException ex)
                {
                    // handle error


                    if (ex.Message is "Invalid license key.")
                    {
                        Logger.Log("Invalied license key, prompt to enter new key");
                        LicenseWindow licenseWindow = new LicenseWindow();
                        licenseWindow.Show();
                    }
                    else if (ex.Code is 58)
                    {
                        MessageBox.Show(ex.Message+" Please upgrade your plan or try again after 3 minutes");
                        Logger.Log(ex.Message+ " Please upgrade your plan or try again after 3 minutes");
                    }
                    else
                    {
                        MessageBox.Show("License check Error code: " + ex.Code.ToString() + " Error message: " + ex.Message + " Please contact us at support@cybersquad.com.au");
                        Logger.Log("License check Error code: " + ex.Code.ToString() + " Error message: " + ex.Message + " Please contact us at support@cybersquad.com.au");
                    }
                    return;


                }
                try
                {

                    int status = LexActivator.IsLicenseGenuine();
                    if (status == LexStatusCodes.LA_OK || status == LexStatusCodes.LA_EXPIRED || status == LexStatusCodes.LA_SUSPENDED || status == LexStatusCodes.LA_GRACE_PERIOD_OVER)
                    {
                        Logger.Log("License is activated: " + status.ToString());
                    }
                    else
                    {
                        Logger.Log("License is not activated: " + status.ToString());
                        MessageBox.Show("License is not activated: " + status.ToString());
                        return;
                    }
                }
                catch (LexActivatorException ex)
                {
                    Logger.Log("Activation check Error code: " + ex.Code.ToString() + " Error message: " + ex.Message);
                    MessageBox.Show("Activation check Error code: " + ex.Code.ToString() + " Error message: " + ex.Message);
                    return;

                }
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
                LoadUsers();
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
                users.Insert(0, newUser);

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
                UserSelection.Items.Clear();
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
