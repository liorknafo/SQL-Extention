using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using Table = SQL_Extention.TableInfo.TableInfo;
using SQL_Extention.TableInfo;
using System.Linq.Expressions;

namespace SQL_Extention
{
    internal static class Ex
    {
        public static T GetObject<T>(this IDataReader dataReader) where T : class, new()
        {
            Type type = typeof(T);
            object obj = new T();
            if (obj == null)
                return null;
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.CheckValidProperty())
                {
                    property.SetValue(obj, dataReader[property.Name]);
                }
            }
            return (T)obj;
        }

        public static List<T> GetObjects<T>(this IDataReader dataReader) where T : class, new()
        {
            var o = new List<T>();
            while (dataReader.Read())
                o.Add(dataReader.GetObject<T>());
            return o;
        }

        public static bool CheckValidProperty(this PropertyInfo propertyInfo)
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
    }

    public class Connection
    {
        private Dictionary<Type, IDbCommand> InsertCommands = new Dictionary<Type, IDbCommand>();
        private Dictionary<Type, IDbCommand> DeleteCommands = new Dictionary<Type, IDbCommand>();
        private Dictionary<Type, IDbCommand> UpdateCommands = new Dictionary<Type, IDbCommand>();
        private Dictionary<Type, Table> Tables = new Dictionary<Type, Table>();
        private Dictionary<Tuple<Type, int>, IDbCommand> GetByPk = new Dictionary<Tuple<Type, int>, IDbCommand>();
        private Dictionary<Expression, IDbCommand> Gets = new Dictionary<Expression, IDbCommand>();

        private IDbConnection Connectoin;
        private SQLCommandAdapter SQLCommandAdapter;
        public Connection(IDbConnection connction, SQLCommandAdapter adapter = null)
        {
            Connectoin = connction;
            if (adapter != null)
                SQLCommandAdapter = adapter;
            else
            {
                //to sqlite 
                if (connction.GetType().Name.ToLower().Contains("sqlite"))
                    SQLCommandAdapter = new SQLiteCommandAdapter(connction);
                // to mysql
                else if (connction.GetType().Name.ToLower().Contains("mysql"))
                    SQLCommandAdapter = new MYSQLCommandAdapter(connction);
                // to sql server
                else if (connction is System.Data.SqlClient.SqlConnection)
                    SQLCommandAdapter = new SQLServerCommandAdapter(connction);
            }
        }

        public void CreateTable<T>()
        {
            Type type = typeof(T);
            Table tableInfo = new Table(type, Connectoin);
            Tables.Add(type, tableInfo);
            IDbCommand createCommand = SQLCommandAdapter.CreateTable(tableInfo);
            IDbTransaction trans = Connectoin.BeginTransaction();
            createCommand.ExecuteNonQuery();
            trans.Commit();
        }

        public T Get<T>(params object[] pks) where T : class, new()
        {
            int pkNum = pks.Length;
            Type type = typeof(T);
            Table table = Tables[type];
            if (table.PrimaryKeys.Count < pkNum)
                throw new IndexOutOfRangeException($"PK NUM CAN'T BE OVER {table.PrimaryKeys.Count}");
            Tuple<Type, int> tuple = new Tuple<Type, int>(type, pkNum);
            IDbCommand Command;
            if (GetByPk.ContainsKey(tuple))
            {
                Command = GetByPk[tuple];
            }
            else
            {
                Command = SQLCommandAdapter.Get<T>(pkNum, table);
                GetByPk.Add(tuple, Command);
            }
            int i = 0;
            foreach (ColumnInfo column in table.PrimaryKeys)
            {
                (Command.Parameters[$"@{column.Name}"] as IDbDataParameter).Value = pks[i];
                i++;
            }
            using (IDataReader dataReader = Command.ExecuteReader())
            {
                if (dataReader.Read())
                    return dataReader.GetObject<T>();
                return null;
            }
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
            foreach (PropertyInfo property in properties)
            {
                if (!property.CheckValidProperty())
                    continue;
                object value = property.GetValue(obj);
                (Command.Parameters[$"@{property.Name}"] as IDbDataParameter).Value = value;
            }
            IDbTransaction trans = Connectoin.BeginTransaction();
            Command.ExecuteNonQuery();
            trans.Commit();
        }

        public List<T> Get<T>(Expression<Func<T, bool>> exp) where T : class, new()
        {
            using (var reader = ExecuteGet<T>(exp, () => SQLCommandAdapter.Get(exp)).ExecuteReader())
                return reader.GetObjects<T>();
        }
        public List<T> Get<T, T2>(Expression<Func<T, T2, bool>> exp,T2 t2) where T : class, new()
        {
            var cmd=ExecuteGet<T>(exp, () => SQLCommandAdapter.Get(exp));
            (cmd.Parameters["@1"] as IDbDataParameter).Value = t2;
            using (var reader = cmd.ExecuteReader())
                return reader.GetObjects<T>();
        }
        public List<T> Get<T, T2, T3>(Expression<Func<T, T2, T3, bool>> exp,T2 t2,T3 t3) where T : class, new()
        {
            var cmd = ExecuteGet<T>(exp, () => SQLCommandAdapter.Get(exp));
            (cmd.Parameters["@1"] as IDbDataParameter).Value = t2;
            (cmd.Parameters["@2"] as IDbDataParameter).Value = t3;
            using (var reader = cmd.ExecuteReader())
                return reader.GetObjects<T>();
        }
        public List<T> Get<T, T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> exp,T2 t2,T3 t3,T4 t4) where T : class, new()
        {
            var cmd = ExecuteGet<T>(exp, () => SQLCommandAdapter.Get(exp));
            (cmd.Parameters["@1"] as IDbDataParameter).Value = t2;
            (cmd.Parameters["@2"] as IDbDataParameter).Value = t3;
            (cmd.Parameters["@3"] as IDbDataParameter).Value = t4;
            using (var reader = cmd.ExecuteReader())
                return reader.GetObjects<T>();
        }
        public List<T> Get<T, T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, bool>> exp, T2 t2, T3 t3, T4 t4,T5 t5) where T : class, new()
        {
            var cmd = ExecuteGet<T>(exp, () => SQLCommandAdapter.Get(exp));
            (cmd.Parameters["@1"] as IDbDataParameter).Value = t2;
            (cmd.Parameters["@2"] as IDbDataParameter).Value = t3;
            (cmd.Parameters["@3"] as IDbDataParameter).Value = t4;
            (cmd.Parameters["@4"] as IDbDataParameter).Value = t5;
            using (var reader = cmd.ExecuteReader())
                return reader.GetObjects<T>();
        }
        public List<T> Get<T, T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> exp, T2 t2, T3 t3, T4 t4, T5 t5,T6 t6) where T : class, new()
        {
            var cmd = ExecuteGet<T>(exp, () => SQLCommandAdapter.Get(exp));
            (cmd.Parameters["@1"] as IDbDataParameter).Value = t2;
            (cmd.Parameters["@2"] as IDbDataParameter).Value = t3;
            (cmd.Parameters["@3"] as IDbDataParameter).Value = t4;
            (cmd.Parameters["@4"] as IDbDataParameter).Value = t5;
            (cmd.Parameters["@5"] as IDbDataParameter).Value = t6;
            using (var reader = cmd.ExecuteReader())
                return reader.GetObjects<T>();
        }
        public List<T> Get<T, T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> exp, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6,T7 t7) where T : class, new()
        {
            var cmd = ExecuteGet<T>(exp, () => SQLCommandAdapter.Get(exp));
            (cmd.Parameters["@1"] as IDbDataParameter).Value = t2;
            (cmd.Parameters["@2"] as IDbDataParameter).Value = t3;
            (cmd.Parameters["@3"] as IDbDataParameter).Value = t4;
            (cmd.Parameters["@4"] as IDbDataParameter).Value = t5;
            (cmd.Parameters["@5"] as IDbDataParameter).Value = t6;
            (cmd.Parameters["@6"] as IDbDataParameter).Value = t7;
            using (var reader = cmd.ExecuteReader())
                return reader.GetObjects<T>();
        }
        public List<T> Get<T, T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> exp, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7,T8 t8) where T : class, new()
        {
            var cmd = ExecuteGet<T>(exp, () => SQLCommandAdapter.Get(exp));
            (cmd.Parameters["@1"] as IDbDataParameter).Value = t2;
            (cmd.Parameters["@2"] as IDbDataParameter).Value = t3;
            (cmd.Parameters["@3"] as IDbDataParameter).Value = t4;
            (cmd.Parameters["@4"] as IDbDataParameter).Value = t5;
            (cmd.Parameters["@5"] as IDbDataParameter).Value = t6;
            (cmd.Parameters["@6"] as IDbDataParameter).Value = t7;
            (cmd.Parameters["@7"] as IDbDataParameter).Value = t8;
            using (var reader = cmd.ExecuteReader())
                return reader.GetObjects<T>();
        }
        public List<T> Get<T, T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> exp, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8,T9 t9) where T : class, new()
        {
            var cmd = ExecuteGet<T>(exp, () => SQLCommandAdapter.Get(exp));
            (cmd.Parameters["@1"] as IDbDataParameter).Value = t2;
            (cmd.Parameters["@2"] as IDbDataParameter).Value = t3;
            (cmd.Parameters["@3"] as IDbDataParameter).Value = t4;
            (cmd.Parameters["@4"] as IDbDataParameter).Value = t5;
            (cmd.Parameters["@5"] as IDbDataParameter).Value = t6;
            (cmd.Parameters["@6"] as IDbDataParameter).Value = t7;
            (cmd.Parameters["@7"] as IDbDataParameter).Value = t8;
            (cmd.Parameters["@8"] as IDbDataParameter).Value = t9;
            using (var reader = cmd.ExecuteReader())
                return reader.GetObjects<T>();
        }
        public List<T> Get<T, T2, T3, T4, T5, T6, T7, T8, T9,T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9,T10, bool>> exp, T2 t2, T3 t3, T4 t4, T5 t5, T6 t6, T7 t7, T8 t8, T9 t9,T10 t10) where T : class, new()
        {
            var cmd = ExecuteGet<T>(exp, () => SQLCommandAdapter.Get(exp));
            (cmd.Parameters["@1"] as IDbDataParameter).Value = t2;
            (cmd.Parameters["@2"] as IDbDataParameter).Value = t3;
            (cmd.Parameters["@3"] as IDbDataParameter).Value = t4;
            (cmd.Parameters["@4"] as IDbDataParameter).Value = t5;
            (cmd.Parameters["@5"] as IDbDataParameter).Value = t6;
            (cmd.Parameters["@6"] as IDbDataParameter).Value = t7;
            (cmd.Parameters["@7"] as IDbDataParameter).Value = t8;
            (cmd.Parameters["@8"] as IDbDataParameter).Value = t9;
            (cmd.Parameters["@9"] as IDbDataParameter).Value = t10;
            using (var reader = cmd.ExecuteReader())
                return reader.GetObjects<T>();
        }


        private IDbCommand ExecuteGet<T>(Expression exp, Func<IDbCommand> createIDbCommandOfGet) where T : class, new()
        {
            IDbCommand command;
            if (!Gets.ContainsKey(exp))
            {
                command = createIDbCommandOfGet();
                Gets.Add(exp, command);
            }
            else
                command = Gets[exp];
            return command;
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
                if (!property.CheckValidProperty())
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
