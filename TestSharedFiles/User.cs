using System;
using System.Collections.Generic;
using System.Text;
using SQL_Extention.Attributes;
using SQL_Extention.Attributes.Key;
using SQL_Extention.Attributes.Constraint;
namespace TestSharedFiles
{
    public class User
    {
        [PrimaryKey]
        public long Id { get; set; }
        [MaxLength(20)]
        public string FirstName { get; set; }
        [MaxLength(20)]
        public string LastName { get; set; }
        [MaxLength(10)]
        public string Password { get; set; }
        public DateTime Birthday { get; set; }
        [Ignore]
        public int Age
        {
            get
            {
                return (int)((DateTime.Now - Birthday).TotalDays / 365.25);
            }
        }
    }
}
