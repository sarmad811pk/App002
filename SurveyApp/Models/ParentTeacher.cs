using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SurveyApp.Models
{
    public class ParentTeacher
    {
        public int Id { get; set; }
        public int? SchoolId { get; set; }
        public string SchoolName { get; set; }

        [Required(ErrorMessage = "Please select a role", AllowEmptyStrings = false)]
        public int Role { get; set; }
        [Required(ErrorMessage = "Please provide name")]        
        public string Name { get; set; }
    }

    public class ParentTeacherContext : DbContext
    {
        public ParentTeacherContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<ParentTeacher> ParentTeachers { get; set; }
    }

    [Table("ParentTeacher_Study")]
    public class ParentTeacher_Study
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }       
        public int ParentTeacherId { get; set; }
        public int StudyId { get; set; }        
    }

    public class ParentTeacher_StudyContext : DbContext
    {
        public ParentTeacher_StudyContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<ParentTeacher_Study> ParentTeacher_Studys { get; set; }
    }

    [Table("ParentTeacher_School")]
    public class ParentTeacher_School
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ParentTeacherId { get; set; }
        public int SchoolId { get; set; }
    }

    public class PParentTeacher_SchoolContext : DbContext
    {
        public PParentTeacher_SchoolContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<ParentTeacher_School> ParentTeacher_Schools { get; set; }
    }

    public enum SurveyAppRoles
    {
        Parent = 1,
        Teacher = 2
    }

    public class ParentTeacher_Register
    {
        public ParentTeacher vw_ParentTeacher { get; set; }
        public RegisterModel vw_Register { get; set; }
    }
}