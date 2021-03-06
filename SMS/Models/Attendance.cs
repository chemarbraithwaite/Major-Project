//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SMS.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Attendance
    {
        public Attendance()
        {

        }
        public Attendance(int id, DateTime date)
        {
            StudentID = id;
            AttendanceDate = date;
            MorningAttendance = "Present";
            AfternoonAttendance = "Present";
        }

        public Attendance(int id, DateTime date, string mAttend, string aAttend)
        {
            StudentID = id;
            AttendanceDate = date;
            MorningAttendance = mAttend;
            AfternoonAttendance = aAttend;
        }

        public int StudentID { get; set; }
        public string MorningAttendance { get; set; }
        public string AfternoonAttendance { get; set; }
        public System.DateTime AttendanceDate { get; set; }
    
        public virtual Student Student { get; set; }
    }
}
