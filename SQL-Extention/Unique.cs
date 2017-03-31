using System;
using System.Collections.Generic;
using System.Text;

namespace SQL_Extention.Attributes.Constraint
{
    public class Unique : AttributeConstraint
    {
        public override string Constraint()
        {
            return "UNIQUE";
        }
    }
}
