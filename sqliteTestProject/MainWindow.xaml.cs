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
using System.Data.SQLite;
using SQL_Extention;
using TestSharedFiles;

namespace sqliteTestProject
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SQLiteConnection con = new SQLiteConnection(@"Data Source=databaseSqlite.db");
            con.Open();
            Connection connction = new Connection(con);
            connction.CreateTable<User>();
            connction.Insert(new User() { Id = 10, Birthday = DateTime.Now, FirstName = "lior", LastName = "knafo", Password = "f4rdf32e" });
            connction.Get<User, int>(((t, i) => t.Id == i && t.FirstName == "test"), 10);
        }
    }
}
