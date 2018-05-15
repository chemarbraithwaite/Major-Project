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
    using System.ComponentModel.DataAnnotations;

    public partial class Student
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Student()
        {
            this.Attendances = new HashSet<Attendance>();
            this.Marks = new HashSet<Mark>();
            this.Teachers = new HashSet<Teacher>();
            this.Guardians = new HashSet<Guardian>();
            this.MAttend = "Present";
            this.AAttend = "Present";
            Scores = new List<double>();
        }

        [Display(Name = "Student ID")]
        public int StudentID { get; set; }
        [Display(Name = "School ID")]
        public string SchoolID { get; set; }
        [Display(Name = "Grade ID")]
        public string GradeID { get; set; }
        [Required(ErrorMessage = "First name is required")]
        [MaxLength(50)]
        [Display(Name = "First name")]
        public string FirstName { get; set; }
        [MaxLength(50)]
        [Display(Name = "Middle name")]
        public string MiddleName { get; set; }
        [Required(ErrorMessage = "Last name is required")]
        [MaxLength(50)]
        [Display(Name = "Last name")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Gender is required")]
        [MaxLength(1)]
        [Display(Name = "Gender")]
        public string Gender { get; set; }
        [Required(ErrorMessage = "Date of birth is required")]
        [Display(Name = "Date of birth")]
        public System.DateTime DOB { get; set; }

        public string ListTitle { get; set; }
        public string MAttend { get; set; }
        public string AAttend { get; set; }
        public List<double> Scores { get; set; }
        public DateTime Date { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Attendance> Attendances { get; set; }
        public virtual ClassGrade ClassGrade { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Mark> Marks { get; set; }
        public virtual MedicalInformation MedicalInformation { get; set; }
        public virtual School School { get; set; }
        public virtual Student_Password Student_Password { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Teacher> Teachers { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Guardian> Guardians { get; set; }
    }
}
