using System;
using System.Collections.Generic;
using System.Text;

namespace SQL_Extention.Attributes.Constraint
{
    public class NotNull : AttributeConstraint
    {
        public override string Constraint()
        {
            return "NOT NULL";
        }
    }
}
