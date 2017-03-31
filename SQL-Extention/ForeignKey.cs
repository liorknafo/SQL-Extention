using System;
using System.Collections.Generic;
using System.Text;

namespace SQL_Extention.Attributes.Key
{
    public class ForeignKey : AttributeKeys
    {
        private Type type;
        public ForeignKey(Type type)
        {
            this.type = type;
        }

        public override string ToSql(string attributeName)
        {
            return "FOREIGN KEY(" + attributeName + ") REFERENCES " + typeof(T).Name + "(" + attributeName + ")";
        }
    }
}
