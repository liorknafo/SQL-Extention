using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Reflection;
using SQL_Extention.Attributes.Constraint;
namespace SQL_Extention.TableInfo
{
    public class TableInfo
    {
        public Type Type { get; }
        public string Name { get { return Type.Name; } }
        public IEnumerable<ColumnInfo> Columns { get; }
        public IDbCommand FindByPK { get; }
        public IEnumerable<ColumnInfo> PrimaryKeys { get; }

        public TableInfo(Type type, IDbConnection connction)
        {
            Type = type;
            List<ColumnInfo> columns = new List<ColumnInfo>();
            List<ColumnInfo> primaryKeys = new List<ColumnInfo>();
            PropertyInfo[] properties = type.GetProperties();
            foreach (PropertyInfo property in properties)
            {
                object[] attributes = property.GetCustomAttributes(true);
                bool isNotNull = false;
                bool toContinue = false;
                List<Attributes.SqlAttribute> propAttributes = new List<Attributes.SqlAttribute>();
                bool isPrimaryKey = false;

                foreach (object attribute in attributes)
                {
                    if (attribute is Attributes.Ignore)
                    {
                        toContinue = true;
                        break;
                    }
                    if (attribute is Attributes.SqlAttribute)
                    {
                        propAttributes.Add(attribute as Attributes.SqlAttribute);
                        if (attribute is Attributes.Key.PrimaryKey)
                            isPrimaryKey = true;
                        if (attribute is NotNull)
                            isNotNull = true;
                    }
                   
                }
                if (toContinue)
                    continue;
                ColumnInfo column = new ColumnInfo(property.Name, property.PropertyType, propAttributes,, isPrimaryKey);
                columns.Add(column);
                if (isPrimaryKey)
                    primaryKeys.Add(column);
            }

            Columns = columns;
            PrimaryKeys = primaryKeys;
            if (primaryKeys.Count != 0)
            {
                FindByPK = connction.CreateCommand();
                FindByPK.CommandText = $"SELECT * FROM '{type.Name}' WHERE {primaryKeys[0].Name} = @PK;";
                IDbDataParameter parameter = FindByPK.CreateParameter();
                parameter.ParameterName = "@PK";
                FindByPK.Parameters.Add(parameter);
            }
        }
    }
}
