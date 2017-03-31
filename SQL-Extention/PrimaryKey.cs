using System;
using System.Collections.Generic;
using System.Text;

namespace SQL_Extention.Attributes.Key
{
    public class PrimaryKey : AttributeKeys
    {
        public override string ToSql(string attributeName)
        {
            return "PRIMARY KEY (" + attributeName + ")";
        }
    }
}
