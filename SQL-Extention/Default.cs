using System;
using System.Collections.Generic;
using System.Text;

namespace SQL_Extention.Attributes.Constraint
{
    public class Default : AttributeConstraint
    {
        public object Value { get; }
        public Default(object value)
        {
            Value = value;
        }
    }
}
