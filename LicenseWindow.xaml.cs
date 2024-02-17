using System;
using System.Collections.Generic;
using System.IO;
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

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for LicenseWindow.xaml
    /// </summary>
    public partial class LicenseWindow : Window
    {
        public LicenseWindow()
        {
            InitializeComponent();
            LoadLicenseKey();
        }
        private void LoadLicenseKey()
        {
            try
            {
                // Assuming "license.txt" is in the application's directory
                var licenseFilePath = "license.txt";

                // Check if the file exists
                if (File.Exists(licenseFilePath))
                {
                    // Read the content of the file
                    var licenseKey = File.ReadAllText(licenseFilePath);

                    // Check if the license key is valid (you need to define what makes it valid)
                    // For demonstration, I'll just check if it's not empty or null
                    if (!string.IsNullOrWhiteSpace(licenseKey))
                    {
                        // If valid, update the label to show the license key is loaded
                        licenseKeyLabel.Content = $"The existing key {licenseKey} is invalid. please enter a new key:";
                        licenseKeyTextBox.Text = licenseKey;
                    }
                    else
                    {
                        // If the key is invalid (empty), prompt for a new key
                        licenseKeyLabel.Content = "Unable to read licensy key. Please enter License Key:";
                    }
                }
                else
                {
                    // If the file doesn't exist, prompt to enter a new key
                    licenseKeyLabel.Content = "Please Enter License Key:";
                }
            }
            catch (Exception ex)
            {
                // Handle potential exceptions, such as issues reading the file
                MessageBox.Show($"Error loading license key: {ex.Message}");
            }
        }


        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            // Close the window
            this.Close();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            // Get the license key from the TextBox
            var licenseKey = licenseKeyTextBox.Text;

            // Write the license key to a file named "license.txt"
            // Consider the path where you want to save this file. Here it's saved in the application's directory.
            File.WriteAllText("license.txt", licenseKey);

            // Optionally, provide feedback or close the window
            MessageBox.Show("License key saved successfully.");
            this.Close(); // Close the window if needed
        }
    }
}
