using Table = SQL_Extention.TableInfo.TableInfo;
using SQL_Extention.TableInfo;
using System;
using System.Reflection;
using SQL_Extention.Attributes.Key;
using SQL_Extention.Attributes.Constraint;
using SQL_Extention.Attributes;
using System.Collections.Generic;
using System.Data;
namespace SQL_Extention
{
    public abstract class SQLCommandAdapter
    {
        protected IDbConnection Connction;
        public SQLCommandAdapter(IDbConnection connection)
        {
            Connction = connection;
        }

        public abstract IDbCommand CreateTable(Table tableInfo);
        public IDbCommand Insert<T>()
        {
            Type type = typeof(T);
            string sql = $"INSERT INTO '{type.Name}' VALUES(";
            IDbCommand Command = Connction.CreateCommand();
            bool isFirst = true;
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (IsValidProperty(property))
                {
                    IDbDataParameter param = Command.CreateParameter();
                    param.ParameterName = $"@{property.Name}";
                    Command.Parameters.Add(param);
                    if (!isFirst)
                        sql += " , ";
                    sql += $"@{property.Name}";
                    isFirst = false;
                }
            }
            sql += ");";
            Command.CommandText = sql;
            return Command;
        }
        public IDbCommand Delete<T>()
        {
            Type type = typeof(T);
            string sql = $"DELETE FROM '{type.Name}' WHERE ";
            IDbCommand Command = Connction.CreateCommand();

            bool isFirst = true;
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.IsPrimaryKey())
                {
                    IDbDataParameter param = Command.CreateParameter();
                    param.ParameterName = $"@{property.Name}";
                    Command.Parameters.Add(param);
                    if (!isFirst)
                        sql += " AND ";
                    sql += $"'{property.Name}' = @{property.Name}";
                    isFirst = false;
                }
            }
            sql += ";";
            Command.CommandText = sql;
            return Command;
        }
        public IDbCommand Update<T>()
        {
            Type type = typeof(T);
            string sql = $"UPDATE '{type.Name}' SET ";
            IDbCommand Command = Connction.CreateCommand();

            bool isFirst = true;
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (!property.IsPrimaryKey())
                {
                    IDbDataParameter param = Command.CreateParameter();
                    param.ParameterName = $"@{property.Name}";
                    Command.Parameters.Add(param);
                    if (!isFirst)
                        sql += " , ";
                    sql += $"'{property.Name}' = @{property.Name}";
                    isFirst = false;
                }
            }
            sql += "WHERE ";
            isFirst = true;
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.IsPrimaryKey())
                {
                    IDbDataParameter param = Command.CreateParameter();
                    param.ParameterName = $"@{property.Name}";
                    Command.Parameters.Add(param);
                    if (!isFirst)
                        sql += " AND ";
                    sql += $"'{property.Name}' = @{property.Name}";
                    isFirst = false;
                }
            }

            sql += ";";
            Command.CommandText = sql;
            return Command;
        }
        private bool IsValidProperty(PropertyInfo property)
        {
            foreach (object attribute in property.GetCustomAttributes(true))
            {
                if (attribute is Ignore)
                    return false;
            }
            return true;
        }

        public IDbCommand Get<T>(System.Linq.Expressions.Expression<Func<T, bool>> filter = null)
        {
            //if(filter == null)
            //{
            //    return $"SELECT * FROM '{typeof(T).Name}';";
            //}
            //string sql = $"SELECT * FROM '{typeof(T).Name}' WHERE ";
            //System.Linq.Expressions.Expression filterWork = filter;
            //if (filter.CanReduce)
            //    filterWork = filterWork.ReduceExtensions();
            return null;
        }

        public string ExpretionToString<T>(System.Linq.Expressions.Expression<Func<T, bool>> filter)
        {
            return null;
        }
    }

    internal static class Extentions
    {
        public static bool IsUnique(this ColumnInfo column)
        {
            foreach (SqlAttribute attribute in column.Attributes)
            {
                if (attribute is Unique)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool IsLengthLimit(this ColumnInfo column, ref int limit)
        {
            foreach (SqlAttribute attribute in column.Attributes)
            {
                if (attribute is MaxLength)
                {
                    limit = (attribute as MaxLength).Length;
                    return true;
                }
            }
            return false;
        }

        public static bool isForeignKey(this ColumnInfo column, out ForeignKey foreignKey)
        {
            foreach (SqlAttribute attribute in column.Attributes)
            {
                if (attribute is ForeignKey)
                {
                    foreignKey = attribute as ForeignKey;
                    return true;
                }
            }
            foreignKey = null;
            return false;
        }

        public static bool IsPrimaryKey(this PropertyInfo propery)
        {
            foreach (object attribute in propery.GetCustomAttributes(true))
            {
                if (attribute is PrimaryKey)
                    return true;
            }
            return false;
        }
    }

    internal class SQLiteCommandAdapter : SQLCommandAdapter
    {

        public SQLiteCommandAdapter(IDbConnection connection) : base(connection)
        {
        }

        public override IDbCommand CreateTable(Table tableInfo)
        {
            string sql = $"CREATE TABLE IF NOT EXISTS {tableInfo.Name}(";
            Dictionary<Type, List<Tuple<string, string>>> foreignKeys = new Dictionary<Type, List<Tuple<string, string>>>();
            bool isFirst = true;
            foreach (ColumnInfo column in tableInfo.Columns)
            {
                Type type = column.Type;
                if (!isFirst)
                    sql += ", ";
                isFirst = false;

                int limit = -1;
                column.IsLengthLimit(ref limit);

                sql += $" {column.Name} ";
                sql += typeToSqlType(type, limit);

                if (column.IsNotNull)
                {
                    sql += "NOT NULL ";
                }
                if (column.IsPrimaryKey)
                {
                    sql += "PRIMARY KEY ";
                }
                if (column.IsUnique())
                {
                    sql += "UNIQUE ";
                }
                ForeignKey foreignKey;
                if (column.isForeignKey(out foreignKey))
                {
                    if (!foreignKeys.ContainsKey(foreignKey.Type))
                    {
                        foreignKeys.Add(foreignKey.Type, new List<Tuple<string, string>>());
                    }
                    foreignKeys[foreignKey.Type].Add(new Tuple<string, string>(column.Name, foreignKey.Name));
                }
            }
            foreach (var foreignKey in foreignKeys)
            {
                sql += ", FOREIGN KEY(";
                isFirst = true;
                foreach (var foreignKeyName in foreignKey.Value)
                {
                    if (!isFirst)
                        sql += ", ";
                    isFirst = false;
                    sql += foreignKeyName.Item1;
                }
                sql += $") REFERENCES {foreignKey.Key.Name}(";
                isFirst = true;
                foreach (var foreignKeyName in foreignKey.Value)
                {
                    if (!isFirst)
                        sql += ", ";
                    else
                        isFirst = false;
                    sql += foreignKeyName.Item2;
                }
                sql += ")";
            }
            sql += ");";
            var command = Connction.CreateCommand();
            command.CommandText = sql;
            return command;
        }

        private string typeToSqlType(Type type, int maxLength = -1)
        {
            if (type == typeof(string))
            {
                if (maxLength == -1)
                    return "TEXT";
                else
                {
                    return $"VARCHAR({maxLength})";
                }
            }
            else if (type == typeof(long) || type == typeof(int))
            {
                return "INTEGER";
            }
            else if (type == typeof(float) || type == typeof(double))
            {
                return "REAL";
            }
            else if (type == typeof(ulong) || type == typeof(uint))
            {
                return "UNSIGNED BIG INT";
            }
            else if(type == typeof(DateTime))
            {
                return "DATE";
            }
            else
            {
                return type.Name.ToUpper();
            }
        }
    }

    internal class MYSQLCommandAdapter : SQLCommandAdapter
    {
        public MYSQLCommandAdapter(IDbConnection connection) : base(connection)
        {
        }

        public override IDbCommand CreateTable(Table tableInfo)
        {
            throw new NotImplementedException();
        }
    }

    internal class SQLServerCommandAdapter : SQLCommandAdapter
    {
        public SQLServerCommandAdapter(IDbConnection connection) : base(connection)
        {
        }

        public override IDbCommand CreateTable(Table tableInfo)
        {
            throw new NotImplementedException();
        }
    }
}