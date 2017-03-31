using System;
using System.Collections.Generic;
using System.Text;

namespace SQL_Extention.Attributes.Constraint
{
    public abstract class AttributeConstraint : SqlAttribute
    {
        public virtual string Constraint()
        {
            return "";
        }
    }
}
