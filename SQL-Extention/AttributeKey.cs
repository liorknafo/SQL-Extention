using System;
using System.Collections.Generic;
using System.Text;

namespace SQL_Extention.Attributes.Key
{
    public abstract class AttributeKeys : SqlAttribute
    {
        public abstract string ToSql(string attributeName);
    }
}
