﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Core.Objects;
    using System.Linq;
    
    public partial class SMSEntities : DbContext
    {
        public SMSEntities()
            : base("name=SMSEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Attendance> Attendances { get; set; }
        public virtual DbSet<ClassGrade> ClassGrades { get; set; }
        public virtual DbSet<Guardian> Guardians { get; set; }
        public virtual DbSet<Guardian_Password> Guardian_Password { get; set; }
        public virtual DbSet<Mark> Marks { get; set; }
        public virtual DbSet<MedicalInformation> MedicalInformations { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<School> Schools { get; set; }
        public virtual DbSet<Student> Students { get; set; }
        public virtual DbSet<Student_Password> Student_Password { get; set; }
        public virtual DbSet<Subject> Subjects { get; set; }
        public virtual DbSet<Teacher> Teachers { get; set; }
        public virtual DbSet<Teacher_Password> Teacher_Password { get; set; }
    
        public virtual ObjectResult<Nullable<bool>> GuardianLogin(Nullable<int> username, string password)
        {
            var usernameParameter = username.HasValue ?
                new ObjectParameter("username", username) :
                new ObjectParameter("username", typeof(int));
    
            var passwordParameter = password != null ?
                new ObjectParameter("password", password) :
                new ObjectParameter("password", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<bool>>("GuardianLogin", usernameParameter, passwordParameter);
        }
    
        public virtual ObjectResult<Nullable<bool>> StaffLogin(string username, string password)
        {
            var usernameParameter = username != null ?
                new ObjectParameter("username", username) :
                new ObjectParameter("username", typeof(string));
    
            var passwordParameter = password != null ?
                new ObjectParameter("password", password) :
                new ObjectParameter("password", typeof(string));
    
            return ((IObjectContextAdapter)this).ObjectContext.ExecuteFunction<Nullable<bool>>("StaffLogin", usernameParameter, passwordParameter);
        }
    }
}