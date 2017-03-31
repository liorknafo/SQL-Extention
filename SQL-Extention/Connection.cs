using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;
using RefPropertyAttributes = System.Reflection.PropertyAttributes;
using Table = SQL_Extention.TableInfo.TableInfo;

namespace SQL_Extention
{
    public class Connection
    {
        private Dictionary<Type, IDbCommand> InsertCommands = new Dictionary<Type, IDbCommand>();
        private Dictionary<Type, IDbCommand> DeleteCommands = new Dictionary<Type, IDbCommand>();
        private Dictionary<Type, IDbCommand> UpdateCommands = new Dictionary<Type, IDbCommand>();
        private Dictionary<Type, Table> Tables = new Dictionary<Type, Table>();
        private Dictionary<Table<Type, int>, IDbCommand> GetByPk = new Dictionary<Table<Type, int>, IDbCommand>();

        private IDbConnection Connectoin;
        private SQLCommandAdapter SQLCommandAdapter;
        public Connection(IDbConnection connction,SQLCommandAdapter adapter = null)
        {
            Connectoin = connction;
            if (adapter != null)
                SQLCommandAdapter = adapter;
            else
            {
                //to sqlite 
                if (connction.GetType().Name.ToLower().Contains("sqlite"))
                {
                    SQLCommandAdapter = new SQLiteCommandAdapter(connction);
                }
                // to mysql
                else if (connction.GetType().Name.ToLower().Contains("mysql"))
                {
                    SQLCommandAdapter = new MYSQLCommandAdapter(connction);
                }
                // to sql server
                else if (connction is System.Data.SqlClient.SqlConnection)
                {
                    SQLCommandAdapter = new SQLServerCommandAdapter(connction);
                }
            }
        }

        public void CreateTable<T>()
        {
            Type type = typeof(T);
            Table tableInfo = new Table(type, Connectoin);
            Tables.Add(type, tableInfo);
            IDbCommand createCommand = SQLCommandAdapter.CreateTable(tableInfo);
            createCommand.ExecuteNonQuery();
        }

        public T Get<T>(params object[] pks)
        {

        }

        public void Insert<T>(T obj)
        {
            Type type = typeof(T);
            IDbCommand Command;
            if (InsertCommands.ContainsKey(type))
                Command = InsertCommands[type];
            else
            {
                Command = SQLCommandAdapter.Insert<T>();
                InsertCommands.Add(type, Command);
            }
            PropertyInfo[] properties = type.GetProperties();
            foreach(PropertyInfo property in properties)
            {
                if (!CheckValidProperty(property))
                    continue;
                object value = property.GetValue(obj);
                (Command.Parameters[$"@{property.Name}"] as IDbDataParameter).Value = value;
            }
            Command.ExecuteNonQuery();
        }

        private bool CheckValidProperty(PropertyInfo propertyInfo)
        {
            foreach (object attribute in propertyInfo.GetCustomAttributes(true))
            {
                if (attribute is Attributes.Ignore)
                {
                    return false;
                }
            }
            return true;
        }
        public void Update<T>(T obj)
        {
            Type type = typeof(T);
            IDbCommand Command;
            if (UpdateCommands.ContainsKey(type))
                Command = InsertCommands[type];
            else
            {
                Command = SQLCommandAdapter.Update<T>();
                UpdateCommands.Add(type, Command);
            }
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                if (!CheckValidProperty(property))
                    continue;
                object value = property.GetValue(obj);
                Command.Parameters[$"@{property.Name}"] = value;
            }
            Command.ExecuteNonQuery();
        }

        public void Delete<T>(T obj)
        {
            Type type = typeof(T);
            Table table = Tables[type];
            IDbCommand Command;
            if (DeleteCommands.ContainsKey(type))
                Command = InsertCommands[type];
            else
            {
                Command = SQLCommandAdapter.Delete<T>();
                DeleteCommands.Add(type, Command);
            }
            foreach (TableInfo.ColumnInfo pk in table.PrimaryKeys)
            {
                PropertyInfo property = type.GetProperty(pk.Name);
                object value = property.GetValue(obj);
                Command.Parameters[$"@{property.Name}"] = value;
            }
            Command.ExecuteNonQuery();
        }
    }
}
