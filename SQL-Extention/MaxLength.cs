using System;
using System.Collections.Generic;
using System.Text;

namespace SQL_Extention.Attributes.Constraint
{
    public class MaxLength : AttributeConstraint
    {
        public int Length { get; set; }
        public MaxLength(int length)
        {
            Length = length;
        }
    }
}
