using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using SMS.Models;

namespace SMS.Controllers
{
    [Authorize(Roles = "T")]
    public class TeachersController : Controller
    {
        private SMSEntities db = new SMSEntities();
        private List<string> gender = new List<string> { "M", "F" };
        private List<string> status = new List<string> { "Present", "Absent", "Late" };
        private List<string> months = new List<string> { "Sep", "Oct", "Nov", "Dec", "Jan", "Feb", "Mar", "Apr", "May", "June" };
        private List<string> gradeOrder = new List<string> {"9", "10", "11", "12", "1", "2", "3", "4", "5", "6" };

        // GET: Teachers
        public ActionResult Index()
        {

            return View(new TeacherDashboard(User.Identity.Name));
        }


        public ActionResult AttendanceSummary(List<Student> students)
        {

            return View(students);
        }

        


        public ActionResult TakeRegister(List<Student> student)
        {


            return View(student);

        }

        public ActionResult EnterGrades(List<Student> student)
        {

            return View(student);

        }



        public ActionResult Attendance(DateTime? date)
        {

            if (date == null)
                date = DateTime.Today;

            string ID = db.Teachers.Find(User.Identity.Name).ClassGrades.FirstOrDefault().GradeID;
            var students = db.Students.Include(s => s.Attendances).Where(s => s.GradeID.Equals(ID)).ToList();
            ViewBag.Date = date;

            return View("AttendanceSummary", students);


        }

        public ActionResult Register(DateTime? date)
        {
            if (date == null)
                date = DateTime.Today;

            string ID = db.Teachers.Find(User.Identity.Name).ClassGrades.FirstOrDefault().GradeID;
            var students = db.Students.Where(s => s.GradeID.Equals(ID)).ToList();
            SetAttendance(students, (DateTime)date);
            ViewBag.Attendance = new List<string>(status);

            ViewBag.Date = date.ToString().Replace(" 12:00:00 AM", "");

            return View("TakeRegister", students);

        }

        public ActionResult Grades(DateTime? date)
        {
            if (date == null)
                date = DateTime.Today;

            string ID = db.Teachers.Find(User.Identity.Name).ClassGrades.FirstOrDefault().GradeID;
            var subjects = db.ClassGrades.Find(ID).Subjects.ToList();
            var students = db.Students.Where(s => s.GradeID.Equals(ID)).ToList();
            SetSubjects(students, (DateTime)date, subjects);

            ViewBag.Date = date.Value.Month + "/" + date.Value.Year;
            ViewBag.Subjects = subjects;

            return View("EnterGrades", students);

        }

        private void SetSubjects(List<Student> students, DateTime date, List<Subject> subjects)
        {
            int count = 1;

            foreach (var item in students)
            {

                using (var data = new SMSEntities())
                {
                    
                        var stud = data.Students.Where(c => c.StudentID.Equals(item.StudentID)).Include(x => x.Marks).FirstOrDefault();
                        item.Scores.Clear();
                        foreach (var subject in subjects)
                        {
                            count++;
                            if (stud.Marks.Where(m => m.SubjectID.Equals(subject.SubjectID) && m.Month.Equals(date.Month.ToString()) && m.Year.Equals(date.Year)).FirstOrDefault() == null)
                            {
                                stud.Marks.Add(new Mark(item, subject, date, User.Identity.Name));
                                item.Scores.Add(0);

                            }
                            else
                            {

                                item.Scores.Add(stud.Marks.Where(m => m.StudentID.Equals(item.StudentID) && m.SubjectID.Equals(subject.SubjectID) && m.Month.Equals(date.Month.ToString()) && m.Year.Equals(date.Year)).FirstOrDefault().Score);
                            }

                            if (count % 100 == 0)
                                data.SaveChanges();



                        }

                        data.SaveChanges();
                    

                }

                item.Date = date;




            }


        }

        private void SetAttendance(List<Student> students, DateTime date)
        {
            int count = 1;
            using (var data = new SMSEntities())
            {
                foreach (var item in students)
                {
                    var stud = data.Students.Where(c => c.StudentID.Equals(item.StudentID)).FirstOrDefault();
                    

                    if (item.Attendances.Where(a => a.AttendanceDate.Equals(date)).Count() == 0)
                    {
                        stud.Attendances.Add(new Attendance(item.StudentID, date));
                    }
                    else
                    {
                        item.MAttend = stud.Attendances.Where(a => a.AttendanceDate.Equals(date)).FirstOrDefault().MorningAttendance;
                        item.AAttend = stud.Attendances.Where(a => a.AttendanceDate.Equals(date)).FirstOrDefault().AfternoonAttendance;
                    }

                    item.Date = date;

                    if (count % 50 == 0)
                        data.SaveChanges();

                }


                data.SaveChanges();

            }


        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RegisterUpdate(List<Student> student)
        {

            try
            {
                using (var sms = new SMSEntities())
                {
                    foreach (var item in student)
                    {
                        var pupil = sms.Students.Where(s => s.StudentID.Equals(item.StudentID)).FirstOrDefault();

                        var attend = sms.Attendances.Where(a => a.AttendanceDate.Equals(item.Date) && a.StudentID.Equals(pupil.StudentID)).FirstOrDefault();

                        if (attend == null)
                            pupil.Attendances.Add(new Attendance(pupil.StudentID, item.Date, item.MAttend, item.AAttend));
                        else
                        {
                            attend.MorningAttendance = item.MAttend;
                            attend.AfternoonAttendance = item.AAttend;
                        }
                    }

                    sms.SaveChanges();
                }
            }
            catch (DbEntityValidationException e)
            {
                foreach (var ve in e.EntityValidationErrors)
                {
                    foreach (var item in ve.ValidationErrors)
                    {
                        System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                item.ErrorMessage);

                    }

                }

            }


            TempData["Success"] = "Updated Successfully!";
            
            ViewBag.Date = student[0].Date.ToString().Replace(" 12:00:00 AM", "");


            ViewBag.Attendance = new List<string>(status);
            return View("TakeRegister", student);

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GradesUpdate(List<Student> student)
        {
            
            var teacher = db.Teachers.Find(User.Identity.Name);
            var ID = teacher.ClassGrades.FirstOrDefault().GradeID;
            var subjects = db.ClassGrades.Find(ID).Subjects.ToList();
            try
             {
                 int count = 0;

                 using (var sms = new SMSEntities())
                 {
                     foreach (var item in student)
                     {
                         var pupil = sms.Students.Where(s => s.StudentID.Equals(item.StudentID)).FirstOrDefault();

                         for (int x = 0; x < subjects.Count; x++)
                         {
                             count++;

                            var sub = pupil.Marks.Where(m => m.StudentID.Equals(item.StudentID) && m.SubjectID.Equals(subjects.ElementAt(x).SubjectID) && m.Month.Equals(item.Date.Month.ToString()) && m.Year.Equals(item.Date.Year)).FirstOrDefault();
                             sub.Score = item.Scores[x];

                         }

                         if (count % 100 == 0)
                             sms.SaveChanges();

                     }

                     sms.SaveChanges();
                 }
             }
             catch (DbEntityValidationException e)
             {
                 foreach (var ve in e.EntityValidationErrors)
                 {
                     foreach (var item in ve.ValidationErrors)
                     {
                         System.Diagnostics.Debug.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                 item.ErrorMessage);

                     }

                 }

             }


            TempData["Success"] = "Updated Successfully!";

             ViewBag.Date = student[0].Date.Month + "/" + student[0].Date.Year;

            
            ViewBag.Subjects = subjects;
            return View("EnterGrades", student);

        }



        // GET: Teachers/Details/5
        public ActionResult Details()
        {

            Teacher teacher = db.Teachers.Find(User.Identity.Name);
            return View(teacher);
        }


        // GET: Students
        public ActionResult Students()
        {

            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            var ID = db.Teachers.Find(User.Identity.Name).ClassGrades.FirstOrDefault().GradeID;
            var students = db.Students.Include(s => s.ClassGrade).Where(s => s.GradeID.Equals(ID));
            
            return View(students.ToList());
        }

        public ActionResult Details_Student(int? id)
        {


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var student = db.Students.Include(s => s.MedicalInformation).Include(s => s.Marks).Include(s => s.Guardians).Include(s => s.Attendances).Include(s => s.ClassGrade).Where(s => s.StudentID == id).FirstOrDefault();
            if (student == null)
            {
                return HttpNotFound();
            }
            
            ViewBag.Months = months;
            ViewBag.Order = gradeOrder;
            return View(student);
        }

        public ActionResult GradesSummary()
        {
            DateTime date = DateTime.Today;
            List<Mark> marks;
            var subjects = db.Teachers.Find(User.Identity.Name).ClassGrades.FirstOrDefault().Subjects.ToList();
            string username = User.Identity.Name;
            if (date.Month >= 9)
                marks = db.Marks.Include(m => m.Student).Where(m => m.Teacher_username.Equals(username) && m.Year.Equals(date.Year) && new[] {"9","10","11","12" }.Contains(m.Month)).ToList();
            else
                marks = db.Marks.Include(m => m.Student).Where(m => m.Teacher_username.Equals(username) && (m.Year.Equals(date.Year) || m.Year.Equals(date.Year - 1)) && gradeOrder.Contains(m.Month)).ToList();

            ViewBag.Months = months;
            ViewBag.Order = gradeOrder;
            ViewBag.Subjects = subjects;

            return View(marks);
        }

        public ActionResult GenerateReport(string error)
        {
            string ID = db.Teachers.Find(User.Identity.Name).ClassGrades.FirstOrDefault().GradeID;
            var students = db.Students.Include(s => s.ClassGrade).Where(s => s.GradeID.Equals(ID)).ToList();

            if(!string.IsNullOrEmpty(error))
                ViewBag.Error = error;

            return View(students);
        }

        public ActionResult StudentReport(int? id,string period)
        {


            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var student = db.Students.Include(s => s.Marks).Include(s => s.ClassGrade).Where(s => s.StudentID == id).FirstOrDefault();
            if (student == null)
            {
                return HttpNotFound();
            }

            IQueryable<Mark> marks = null;
            List<Mark> studentMarks = null;
            List<Attendance> attendance = null;
            var today = DateTime.Today;
            string ID = db.Teachers.Find(User.Identity.Name).ClassGrades.FirstOrDefault().GradeID;
            var students = db.Students.Include(s => s.Marks).Where(s => s.GradeID.Equals(ID));
            var studentIDs = students.Select(s => s.StudentID);
            List<StudentRank> MonthRank = new List<StudentRank>();
            List<string> months;
            List<string> gradeOrder;
            int year = 0;

            if (period.Equals("easter"))
            {
                months = new List<string> {"Jan","Feb","Exam" };
                gradeOrder = new List<string> { "1", "2", "3" };
                ViewBag.Term = "Easter Term";
                marks = db.Marks.Where(m => m.Year.Equals(today.Year) && studentIDs.Contains(m.StudentID) && m.Month.Equals("3"));
                studentMarks = db.Marks.Where(m => m.Year.Equals(today.Year) && m.StudentID.Equals(student.StudentID) && gradeOrder.Contains(m.Month.ToString())).ToList();

                if (!marks.Any())
                {
                    string error = "Please enter all grades for the easter semester before generating a easter semester report";
                    return RedirectToAction("GenerateReport", new { error });
                }

                attendance = db.Attendances.Where(a => a.StudentID.Equals(student.StudentID) && gradeOrder.Contains(a.AttendanceDate.Month.ToString())).ToList();
              
            }
            else if (period.Equals("christmas"))
            {
                months = new List<string> { "Sep", "Oct", "Nov", "Exam" };
                gradeOrder = new List<string> { "9", "10", "11", "12" };
                ViewBag.Term = "Christmas Term";

                if (today.Month >= 9)
                    year = today.Year;
                else
                    year = today.Year - 1;

                marks = db.Marks.Where(m => m.Year.Equals(year) && studentIDs.Contains(m.StudentID) && m.Month.Equals("12"));
                studentMarks = db.Marks.Where(m => m.Year.Equals(year) && m.StudentID.Equals(student.StudentID) && gradeOrder.Contains(m.Month.ToString())).ToList();

                if (!marks.Any())
                {
                    string error = "Please enter all grades for the christmas semester before generating a chirstmas semester report";
                    return RedirectToAction("GenerateReport", new {error });
                }

                attendance = db.Attendances.Where(a => a.StudentID.Equals(student.StudentID) && gradeOrder.Contains(a.AttendanceDate.Month.ToString())).ToList();
             

            }
            else if (period.Equals("summer"))
            {
                months = new List<string> { "Apr", "May", " Final Exam" };
                gradeOrder = new List<string> { "4", "5", "6" };
                ViewBag.Term = "Summer Term";
                marks = db.Marks.Where(m => m.Year.Equals(today.Year) && studentIDs.Contains(m.StudentID) && m.Month.Equals("6"));
                studentMarks = db.Marks.Where(m => m.Year.Equals(today.Year) && m.StudentID.Equals(student.StudentID) && gradeOrder.Contains(m.Month.ToString())).ToList();

                attendance = db.Attendances.Where(a => a.StudentID.Equals(student.StudentID) && gradeOrder.Contains(a.AttendanceDate.Month.ToString())).ToList();

                if (!marks.Any())
                {
                    string error = "Please enter all grades for the summer semester before generating a summer semester report";
                    return RedirectToAction("GenerateReport", new { error });
                }

            }
            else
            {
                string error = "Error in academic terms selected";
                return RedirectToAction("GenerateReport", new { error });
            }

            foreach(var stud in students)
            {
                var monthMark = marks.Where(m => m.StudentID.Equals(stud.StudentID));
                var monthSum = monthMark.Select(m => m.Score).ToList().Sum();
                var monthAverage = monthSum / monthMark.Count();
                MonthRank.Add(new StudentRank(stud.StudentID, stud.FirstName, stud.LastName, monthAverage));

            }

            ViewBag.Attendances = attendance;

            int position = MonthRank.OrderByDescending(s => s.Average).ToList().FindIndex(s => s.ID.Equals(id));
            ViewBag.Average = MonthRank.OrderByDescending(s => s.Average).ToList()[position].Average;
            ViewBag.Position = position + 1;
            ViewBag.Marks = studentMarks;
            ViewBag.Order = gradeOrder;
            ViewBag.Months = months;
            ViewBag.Rank = MonthRank.OrderByDescending(s => s.Average).ToList();
            ViewBag.Absent = attendance.Where(a => a.MorningAttendance.Equals("Absent")).Count() + attendance.Where(a => a.AfternoonAttendance.Equals("Absent")).Count();
            ViewBag.Late = attendance.Where(a => a.MorningAttendance.Equals("Late")).Count() + attendance.Where(a => a.AfternoonAttendance.Equals("Late")).Count();


            var age = today.Year - student.DOB.Year;
            if (student.DOB > today.AddYears(-age)) age--;
            ViewBag.Age = age;

            if (today.Month >= 9)
                ViewBag.AcademicYear = today.Year + "-" + (today.Year + 1);
            else
                ViewBag.AcademicYear = (today.Year - 1) + "-" + today.Year;

            var average = marks.Select(m => m.Score).Sum() / marks.Count();
            ViewBag.ClassAverage = average;

            ViewBag.Months = months;
            ViewBag.Order = gradeOrder;
            return View(student);
        }

        // GET: Teachers/Edit/5
        public ActionResult Edit()
        {


            Teacher teacher = db.Teachers.Find(User.Identity.Name);

            ViewBag.Gender = new SelectList(gender, teacher.Gender);
            return View(teacher);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Teacher_username,SchoolID,RoleID,Password,Firstname,MiddleName,LastName,Gender,DOB,StartDate,Email,CellContact,HomeContact, ClassID")] Teacher teacher)
        {


            teacher.SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;



            if (ModelState.IsValid)
            {

                UpdateTeacher(teacher);

                return RedirectToAction("Details");
            }

            ViewBag.Gender = new SelectList(gender, teacher.Gender);
            return View(teacher);
        }

        public bool UpdateTeacher(Teacher teacher)
        {
            var entity = db.Teachers.Where(c => c.Teacher_username.Equals(teacher.Teacher_username)).AsQueryable().FirstOrDefault();
            if (entity == null)
            {
                db.Teachers.Add(teacher);
            }
            else
            {
                db.Entry(entity).CurrentValues.SetValues(teacher);
            }

            return db.SaveChanges() > 0;

        }


        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
