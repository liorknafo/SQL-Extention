using System;
using System.Collections.Generic;
using System.Text;

namespace SQL_Extention.Attributes.Key
{
    public class ForeignKey : AttributeKeys
    {
        public Type Type { get; }
        public string Name { get; }
        public ForeignKey(Type type,string name)
        {
            Name = name;
            Type = type;
        }
    }
}
