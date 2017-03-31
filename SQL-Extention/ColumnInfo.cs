using System;
using System.Collections.Generic;
using System.Text;

namespace SQL_Extention.TableInfo
{
    public class ColumnInfo
    {
        public string Name { get; }
        public Type Type { get; }
        public bool IsPrimaryKey { get; }
        public IEnumerable<Attributes.SqlAttribute> Attributes { get; }

        public ColumnInfo(string name,Type type,IEnumerable<Attributes.SqlAttribute> attributes,bool isPrimaryKey = false)
        {
            Name = name;
            Type = type;
            Attributes = attributes;
            IsPrimaryKey = isPrimaryKey;
        }
    }
}
