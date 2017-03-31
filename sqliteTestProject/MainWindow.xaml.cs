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
            SQLiteConnection con = new SQLiteConnection(@"Data Source=C:\Users\lior\Source\Repos\SQL-Extention\sqliteTestProject\bin\Debug\databaseSqlite.db");
            con.Open();
            Connection connction = new Connection(con);
            connction.CreateTable<User>();
            User user = connction.Get<User>(10);

        }
    }
}
