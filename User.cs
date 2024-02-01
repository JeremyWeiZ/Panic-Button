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
}
