using System;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using SQL_Extention;
using TestSharedFiles;

namespace sqlserverTestProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SqlConnection con = new SqlConnection("databaseTestServer.db");
            Connection Connection = new Connection(con);
            Connection.CreateTable<User>();
            User user = new User() { Id = 10, Birthday = DateTime.Now, FirstName = "lior", LastName = "knafo", Password = "3rfsdfwe3" };
            Connection.Insert(user);
        }
    }
}
