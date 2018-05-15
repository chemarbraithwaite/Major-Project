using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMS.Models
{
    public class StudentRank
    {
        public StudentRank(int id, string fname, string lname, double avg)
        {
            ID = id;
            FirstName = fname;
            LastName = lname;
            Average = avg;

        }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int ID { get; set; }
        public double Average { get; set; }

    }
}