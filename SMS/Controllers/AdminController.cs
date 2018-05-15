using SMS.Models;
using System.Data.Entity;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity.Core.Objects;

namespace SMS.Controllers
{
    [Authorize(Roles = "A")]
    public class AdminController : Controller
    {
        private SMSEntities db = new SMSEntities();
        private List<string> disabledStaff = new List<string> { "A", "S" };
        private List<string> disabledAdmin = new List<string> { "T", "S", "P", "GC" };
        private List<string> gender = new List<string> { "M", "F" };

        // GET: Admin
        public ActionResult Index()
        {
            return View();
        }

        // GET: Admin/Details/5
        public ActionResult Teachers()
        {

            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            var teachers = db.Teachers.Include(t => t.Role).Include(t => t.ClassGrades).Where(t => t.SchoolID.Equals(SchoolID) && !t.RoleID.Equals("A"));
            return View(teachers.ToList());
        }

        // GET: Students
        public ActionResult Students()
        {

            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            var students = db.Students.Include(s => s.ClassGrade).Include(s => s.School).Where(s => s.SchoolID.Equals(SchoolID));
            return View(students.ToList());
        }

        public ActionResult Classes()
        {


            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            var classGrades = db.ClassGrades.Include(c => c.Teachers).Where(s => s.SchoolID.Equals(SchoolID));
            return View(classGrades.ToList());
        }

        public ActionResult Details_Class(string id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClassGrade classGrade = db.ClassGrades.Find(id);
            if (classGrade == null)
            {
                return HttpNotFound();
            }
            return View(classGrade);
        }

        public ActionResult Create_Class()
        {

            ViewBag.SubjectIDs = new MultiSelectList(db.Subjects, "SubjectID", "Subject1");
            return View();
        }

        // POST: ClassGrades/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_Class([Bind(Include = "GradeID,SchoolID,Grade, SubjectIDs")] ClassGrade classGrade)
        {
            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            classGrade.SchoolID = SchoolID;

            if (db.ClassGrades.Find(classGrade.GradeID) != null)
                ModelState.AddModelError("GradeID", "Class ID already taken");

            if (ModelState.IsValid && db.ClassGrades.Find(classGrade.GradeID) == null)
            {
                if (classGrade.SubjectIDs != null)
                    foreach (string item in classGrade.SubjectIDs)
                        classGrade.Subjects.Add(db.Subjects.Find(item));

                db.ClassGrades.Add(classGrade);
                db.SaveChanges();
                return RedirectToAction("Classes");
            }

            ViewBag.SubjectIDs = new MultiSelectList(db.Subjects, "SubjectID", "Subject1");
            return View(classGrade);
        }

        // GET: ClassGrades/Edit/5
        public ActionResult Edit_Class(string id)
        {
            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ClassGrade classGrade = db.ClassGrades.Find(id);
            if (classGrade == null)
            {
                return HttpNotFound();
            }

            List<string> subjects = new List<string>();

            foreach (var item in classGrade.Subjects)
                subjects.Add(item.SubjectID);

            ViewBag.SubjectIDs = new MultiSelectList(db.Subjects, "SubjectID", "Subject1",subjects);
            return View(classGrade);
        }

        // POST: ClassGrades/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_Class([Bind(Include = "GradeID,SchoolID,Grade, SubjectIDs")] ClassGrade classGrade)
        {
            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            classGrade.SchoolID = SchoolID;

            if (ModelState.IsValid)
            {
                db.Entry(classGrade).State = EntityState.Modified;
                db.SaveChanges();

                using (var sms = new SMSEntities())
                {

                    var Class = sms.ClassGrades.Where(c => c.GradeID.Equals(classGrade.GradeID)).FirstOrDefault();
                    Class.Subjects.Clear();


                    if (classGrade.SubjectIDs != null)
                        foreach (var item in classGrade.SubjectIDs.ToArray())
                        {

                            Class.Subjects.Add(sms.Subjects.Where(c => c.SubjectID.Equals(item)).FirstOrDefault());
                        }

                    sms.SaveChanges();

                }
                

                return RedirectToAction("Classes");
            }
            ViewBag.SubjectIDs = new MultiSelectList(db.Subjects, "SubjectID", "Subject1");
            return View(classGrade);
        }



        public ActionResult Guardians()
        {
            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            return View(db.Guardians.Where(x => x.SchoolID.Equals(SchoolID)).ToList());
        }

        // GET: Guardians/Create
        public ActionResult Create_Guardian()
        {
            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            var students = db.Students.Where(s => s.SchoolID.Equals(SchoolID));
            ViewBag.Gender = new SelectList(gender, "M");
            foreach (var item in students)
            {
                item.ListTitle = item.StudentID + " " + item.FirstName;
            }
            ViewBag.StudentIDs = new SelectList(students, "StudentID", "ListTitle");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_Guardian([Bind(Include = "GuardianID,FirstName,LastName,Gender,Email,CellContact,HomeContact, StudentIDs")] Guardian guardian)
        {
            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            guardian.SchoolID = SchoolID;


            if (ModelState.IsValid)
            {
                if (guardian.StudentIDs != null)
                    foreach (int item in guardian.StudentIDs.ToArray())
                        guardian.Students.Add(db.Students.Find(item));
                db.Guardians.Add(guardian);
                db.Guardian_Password.Add(new Guardian_Password(guardian.GuardianID));
                db.SaveChanges();
                return RedirectToAction("Guardians");
            }
            
            var students = db.Students.Where(s => s.SchoolID.Equals(SchoolID));
            ViewBag.Gender = new SelectList(gender, guardian.Gender);
            foreach (var item in students)
            {
                item.ListTitle = item.StudentID + " " + item.FirstName;
            }
            ViewBag.StudentIDs = new MultiSelectList(students, "StudentID", "ListTitle");

            return View(guardian);
        }

        // GET: Guardians/Edit/5
        public ActionResult Edit_Guardian(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Guardian guardian = db.Guardians.Find(id);
            if (guardian == null)
            {
                return HttpNotFound();
            }
            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            var students = db.Students.Where(s => s.SchoolID.Equals(SchoolID));
            ViewBag.Gender = new SelectList(gender, guardian.Gender);
            foreach (var item in students)
            {
                item.ListTitle = item.StudentID + " " + item.FirstName;
            }
            
            ViewBag.StudentIDs = new MultiSelectList(students, "StudentID", "ListTitle");

            return View(guardian);
        }

        // POST: Guardians/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_Guardian([Bind(Include = "GuardianID,FirstName,LastName,Gender,Email,CellContact,HomeContact, StudentIDs")] Guardian guardian)
        {
            guardian.SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;

            if (ModelState.IsValid)
            {
                db.Entry(guardian).State = EntityState.Modified;
                db.SaveChanges();

                using (var sms = new SMSEntities())
                {

                    var Guardian = sms.Guardians.Where(g => g.GuardianID.Equals(guardian.GuardianID)).FirstOrDefault();
                    Guardian.Students.Clear();


                    if (guardian.StudentIDs != null)
                        foreach (var item in guardian.StudentIDs.ToArray())
                        {

                            Guardian.Students.Add(sms.Students.Where(s => s.StudentID.Equals(item)).FirstOrDefault()); 
                        }

                    sms.SaveChanges();

                }

                return RedirectToAction("Guardians");
            }

            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            var students = db.Students.Where(s => s.SchoolID.Equals(SchoolID));
            ViewBag.Gender = new SelectList(gender, guardian.Gender);
            foreach (var item in students)
            {
                item.ListTitle = item.StudentID + " " + item.FirstName;
            }

            ViewBag.StudentIDs = new MultiSelectList(students, "StudentID", "ListTitle");
            return View(guardian);
        }

      


        public ActionResult Create_Teacher()
        {
            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;

            ViewBag.RoleID = new SelectList(db.Roles, "RoleID", "Role1", (object)"T", disabledStaff);
            var classes = db.ClassGrades.Where(c => c.SchoolID.Equals(SchoolID));
            ViewBag.ClassID = new SelectList(classes, "GradeID", "Grade");
            ViewBag.Gender = new SelectList(gender, "M");

            return View();
        }

        // GET: Students/Create
        public ActionResult Create_Student()
        {
            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            var classes = db.ClassGrades.Where(c => c.SchoolID.Equals(SchoolID) && !c.Grade.Equals("General"));
            ViewBag.GradeID = new SelectList(classes, "GradeID", "Grade", classes.FirstOrDefault().GradeID);
            ViewBag.Gender = new SelectList(gender, "M");
            return View();
        }

        public ActionResult Details()
        {
            var teacher = db.Teachers.Find(User.Identity.Name);

            if (teacher == null)
                return HttpNotFound();
            else
                return View(teacher);
        }

        public ActionResult Details_Student(int? id)
        {
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }

            return View(student);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_Teacher([Bind(Include = "Teacher_username,SchoolID,RoleID,Password,Firstname,MiddleName,LastName,Gender,DOB,StartDate,Email,CellContact,HomeContact, ClassID")] Teacher teacher)
        {
            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            teacher.SchoolID = SchoolID;


            if (ModelState.IsValid && db.Teachers.Find(teacher.Teacher_username.ToLower()) == null)
            {

                if (teacher.ClassID != null)
                    teacher.ClassGrades.Add(db.ClassGrades.Find(teacher.ClassID));
                teacher.Teacher_username = teacher.Teacher_username.ToLower();
                db.Teachers.Add(teacher);
                db.Teacher_Password.Add(new Teacher_Password(teacher.Teacher_username.ToLower()));
                db.SaveChanges();

                return RedirectToAction("Teachers");
            }
            
            var classes = db.ClassGrades.Where(c => c.SchoolID.Equals(SchoolID));
            ViewBag.ClassID = new SelectList(classes, "GradeID", "Grade");

            ViewBag.RoleID = new SelectList(db.Roles, "RoleID", "Role1", (object)teacher.RoleID, disabledStaff);
            ViewBag.Gender = new SelectList(gender, teacher.Gender);

            if (teacher.Teacher_username != null && db.Teachers.Find(teacher.Teacher_username.ToLower()) != null)
                ModelState.AddModelError("Teacher_username", "Username already taken.");

            return View(teacher);
        }

        // POST: Students/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create_Student([Bind(Include = "StudentID,SchoolID,GradeID,FirstName,MiddleName,LastName,Gender,DOB")] Student student)
        {
            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;

            student.SchoolID = SchoolID;
            if (ModelState.IsValid)
            {
                db.Students.Add(student);
                db.Student_Password.Add(new Student_Password(student.StudentID));
                db.MedicalInformations.Add(new MedicalInformation(student.StudentID));
                db.SaveChanges();
                return RedirectToAction("Students");
            }


            
            var classes = db.ClassGrades.Where(c => c.SchoolID.Equals(SchoolID) && !c.Grade.Equals("General"));

            ViewBag.GradeID = new SelectList(classes, "GradeID", "Grade", student.GradeID);
            ViewBag.Gender = new SelectList(gender, student.Gender);

            return View(student);
        }



        // GET: Teachers/Edit/5
        public ActionResult Edit_Teacher(string id)
        {
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null || teacher.RoleID.Equals("A"))
            {
                return HttpNotFound();
            }

            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            var classes = db.ClassGrades.Where(c => c.SchoolID.Equals(SchoolID));

            ViewBag.ClassID = new SelectList(classes, "GradeID", "Grade", teacher.ClassGrades.FirstOrDefault().GradeID);
            ViewBag.RoleID = new SelectList(db.Roles, "RoleID", "Role1", (object)teacher.RoleID, disabledStaff);
            ViewBag.Gender = new SelectList(gender, teacher.Gender);
            return View(teacher);
        }

        public ActionResult Edit_Student(int? id)
        {
            
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Student student = db.Students.Find(id);
            if (student == null)
            {
                return HttpNotFound();
            }

            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            var classes = db.ClassGrades.Where(c => c.SchoolID.Equals(SchoolID) && !c.Grade.Equals("General"));

            ViewBag.GradeID = new SelectList(classes, "GradeID", "Grade", student.GradeID);
            ViewBag.Gender = new SelectList(gender, student.Gender);

            return View(student);
        }


        // POST: Students/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_Student([Bind(Include = "StudentID,SchoolID,GradeID,FirstName,MiddleName,LastName,Gender,DOB")] Student student)
        {

            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            student.SchoolID = SchoolID;
            if (ModelState.IsValid)
            {
                db.Entry(student).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Students");
            }
            
            var classes = db.ClassGrades.Where(c => c.SchoolID.Equals(SchoolID) && !c.Grade.Equals("General"));

            ViewBag.GradeID = new SelectList(classes, "GradeID", "Grade", student.GradeID);
            ViewBag.Gender = new SelectList(gender, student.Gender);

            return View(student);
        }

        public ActionResult Edit(string id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Teacher teacher = db.Teachers.Find(id);
            if (teacher == null || !teacher.RoleID.Equals("A"))
            {
                return HttpNotFound();
            }

            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            var classes = db.ClassGrades.Where(c => c.SchoolID.Equals(SchoolID));

            ViewBag.ClassID = new SelectList(classes, "GradeID", "Grade", teacher.ClassID);
            ViewBag.RoleID = new SelectList(db.Roles, "RoleID", "Role1", (object)teacher.RoleID, disabledAdmin);
            ViewBag.Gender = new SelectList(gender, teacher.Gender);
            return View(teacher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Teacher_username,SchoolID,RoleID,Password,Firstname,MiddleName,LastName,Gender,DOB,StartDate,Email,CellContact,HomeContact, ClassID")] Teacher teacher)
        {
            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;

            teacher.SchoolID = SchoolID;



            if (ModelState.IsValid)
            {

                UpdateTeacher(teacher);
                return RedirectToAction("Details");
            }
            
            var classes = db.ClassGrades.Where(c => c.SchoolID.Equals(SchoolID));
            ViewBag.RoleID = new SelectList(db.Roles, "RoleID", "Role1", (object)teacher.RoleID, disabledAdmin);
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


        // POST: Teachers/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_Teacher([Bind(Include = "Teacher_username,SchoolID,RoleID,Password,Firstname,MiddleName,LastName,Gender,DOB,StartDate,Email,CellContact,HomeContact, ClassID")] Teacher teacher)
        {
            string SchoolID = db.Teachers.Find(User.Identity.Name).SchoolID;
            teacher.SchoolID = SchoolID;
            

            if (ModelState.IsValid)
            {

                db.Entry(teacher).State = EntityState.Modified;
                db.SaveChanges();
                using (var sms = new SMSEntities())
                {
                    var existingClassGrade = sms.Teachers.Include("ClassGrades").Where(t => t.Teacher_username == teacher.Teacher_username).FirstOrDefault();
                    var deleteClassGrade = existingClassGrade.ClassGrades.Remove(existingClassGrade.ClassGrades.FirstOrDefault());

                    var classGrade = sms.ClassGrades.Find(teacher.ClassID);

                    existingClassGrade.ClassGrades.Add(classGrade);

                    sms.SaveChanges();

                }
                return RedirectToAction("Teachers");
            }
            
            var classes = db.ClassGrades.Where(c => c.SchoolID.Equals(SchoolID));
            ViewBag.ClassID = new SelectList(classes, "GradeID", "Grade", teacher.ClassID);
            ViewBag.RoleID = new SelectList(db.Roles, "RoleID", "Role1", (object)teacher.RoleID, disabledStaff);
            ViewBag.Gender = new SelectList(gender, teacher.Gender);
            return View(teacher);
        }



        public ActionResult Edit_Medical(int? id)
        {

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            MedicalInformation medicalInformation = db.MedicalInformations.Find(id);
            if (medicalInformation == null)
            {
                return HttpNotFound();
            }
            ViewBag.StudentID = new SelectList(db.Students.Where(s => s.StudentID == medicalInformation.StudentID), "StudentID", "StudentID");
            return View(medicalInformation);
        }

        // POST: MedicalInformations/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit_Medical([Bind(Include = "StudentID,MedSummary")] MedicalInformation medicalInformation)
        {

            if (ModelState.IsValid)
            {
                db.Entry(medicalInformation).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details_Student", new { id = medicalInformation.StudentID });
            }
            ViewBag.StudentID = new SelectList(db.Students.Where(s => s.StudentID == medicalInformation.StudentID), "StudentID", "StudentID");
            return View(medicalInformation);
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
