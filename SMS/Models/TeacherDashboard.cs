using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SMS.Models
{
    public class TeacherDashboard
    {
        public string SexRatio;
        private SMSEntities db = new SMSEntities();
        public string Username;
        public DateTime date;
        public int Pres { get; set; }
        public int Abs { get; set; }
        public int Lt { get; set; }
        public int TotalPres { get; set; }
        public int TotalLt { get; set; }
        public int TotalAbs { get; set; }
        public List<Student> Students { get; set; }

        public TeacherDashboard(string username)
        {
            Username = username;
            date = DateTime.Today;
            Students = db.Teachers.Find(Username).ClassGrades.FirstOrDefault().Students.ToList();


        }

        public int Boys()
        {

            return Students.Where(s => s.Gender.Equals("M")).Count();

        }

        public int Girls()
        {
            return Students.Where(s => s.Gender.Equals("F")).Count();

        }

        public int Present()
        {

            foreach (var item in Students)
            {
                if (item.Attendances.Where(a => a.AttendanceDate.Equals(date) && (a.AfternoonAttendance.Equals("Present") || a.MorningAttendance.Equals("Present"))).Count() == 1)
                    Pres++;
                else if (item.Attendances.Where(a => a.AttendanceDate.Equals(date) && (a.AfternoonAttendance.Equals("Late") || a.MorningAttendance.Equals("Late"))).Count() == 1)
                    Lt++;
                else if (item.Attendances.Where(a => a.AttendanceDate.Equals(date) && (a.AfternoonAttendance.Equals("Absent") && a.MorningAttendance.Equals("Absent"))).Count() == 1)
                    Abs++;
            }

            return Pres;
        }

        public int TotalPresent()
        {

            foreach (var item in Students)
            {
                TotalPres += item.Attendances.Where(x => x.AttendanceDate.Month.Equals(date.Month) && x.AttendanceDate.Year.Equals(date.Year) && x.MorningAttendance.Equals("Present")).Count() + item.Attendances.Where(x => x.AttendanceDate.Month.Equals(date.Month) && x.AttendanceDate.Year.Equals(date.Year) && x.AfternoonAttendance.Equals("Present")).Count();
                TotalLt += item.Attendances.Where(x => x.AttendanceDate.Month.Equals(date.Month) && x.AttendanceDate.Year.Equals(date.Year) && x.MorningAttendance.Equals("Late")).Count() + item.Attendances.Where(x => x.AttendanceDate.Month.Equals(date.Month) && x.AttendanceDate.Year.Equals(date.Year) && x.AfternoonAttendance.Equals("Late")).Count();
                TotalAbs += item.Attendances.Where(x => x.AttendanceDate.Month.Equals(date.Month) && x.AttendanceDate.Year.Equals(date.Year) && x.MorningAttendance.Equals("Absent")).Count() + item.Attendances.Where(x => x.AttendanceDate.Month.Equals(date.Month) && x.AttendanceDate.Year.Equals(date.Year) && x.AfternoonAttendance.Equals("Absent")).Count();
            }
            
            return TotalPres;

        }









    }
}