using Microsoft.VisualBasic.ApplicationServices;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for UserListWindows.xaml
    /// </summary>
    public partial class UserListWindow : Window
    {
        private ObservableCollection<User> users;

        public UserListWindow(ObservableCollection<User> users)
        {
            InitializeComponent();
            this.users = users;
            UserListBox.ItemsSource = this.users;

        }
        

        //public void UpdateUserList(string jsonUserInfo)
        //{
        //    try
        //    {
        //        ObservableCollection<User> users = new ObservableCollection<User>();

        //        // Bind this collection to your ListView in the constructor or a suitable place
        //        UserListBox.ItemsSource = users;
        //        User userInfo = JsonConvert.DeserializeObject<User>(jsonUserInfo);
        //        // Add the deserialized object to the ObservableCollection
        //        users.Add(userInfo);
        //    }
        //    catch (Exception ex)
        //    {
        //        // Handle any potential exceptions
        //        Console.WriteLine("Error parsing JSON data: " + ex.Message);
        //    }
        //}
    }
}
