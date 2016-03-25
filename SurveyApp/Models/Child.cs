using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SurveyApp.Models
{
    [Table("Child")]
    public class Child
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please provide name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please select date of birth", AllowEmptyStrings = false)]
        public DateTime dob { get; set; }

        [Required(ErrorMessage = "Please select gender", AllowEmptyStrings = false)]
        public int Gender { get; set; }

        public int? SchoolId { get; set; }
        public int? ParentId { get; set; }
    }

    public class ChildContext : DbContext
    {
        public ChildContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Child> Children { get; set; }
    }

    [Table("Child_Study")]
    public class Child_Study
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ChildId { get; set; }
        public int StudyId { get; set; }
    }

    public class Child_StudyContext : DbContext
    {
        public Child_StudyContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Child_Study> Child_Studies { get; set; }

        public static List<Child_Study> Child_StudyGetAll(int childId)
        {
            using (var context = new Child_StudyContext())
            {
                //return context.Child_Studies.Find(new Child_Study() { ChildId = childId }).ToList();
                //List<Child_Study> lstChildStudies = new List<Child_Study>();
                return context.Child_Studies.Where(m => m.ChildId == childId).ToList();
            }
        }
    }

    [Table("Child_Teacher")]
    public class Child_Teacher
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ChildId { get; set; }
        public int TeacherId { get; set; }

        public static DataSet Child_TeacherGetAll(int? childId = 0)
        {
            return DataHelper.Child_TeacherGetAll(childId);
        }
    }

    public class Child_TeacherContext : DbContext
    {
        public Child_TeacherContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Child_Teacher> Child_Teachers { get; set; }
    }
}