using System;
using System.Collections.Generic;
using System.ComponentModel;
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

        //[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
        [Required(ErrorMessage = "Please select date of birth", AllowEmptyStrings = false)]
        public DateTime dob { get; set; }

        [Required(ErrorMessage = "Please select gender", AllowEmptyStrings = false)]
        public int Gender { get; set; }

        [Required(ErrorMessage = "Please select school", AllowEmptyStrings = false)]
        public int SchoolId { get; set; }

        [Required(ErrorMessage = "Please select parent for the child", AllowEmptyStrings = false)]
        public int ParentId { get; set; }

        [Required(ErrorMessage = "Please select status of enrollment", AllowEmptyStrings = false)]
        public int StatusId { get; set; }

        public DateTime? EnrollmentDate { get; set; }

        [DefaultValue(false)]
        public bool Consent { get; set; }

        [DefaultValue(false)]
        public bool Agreed { get; set; }

        [DefaultValue(null)]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Email is not valid")]
        public string Email { get; set; }
               
        public DateTime? AgreeDate { get; set; }

        [DefaultValue(false)]
        public bool Account { get; set; }
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

    public class EnrollmentStatus
    {
        public int Id { get; set; }
        public string Status { get; set; }

        public static List<EnrollmentStatus> EnrollmentStatus_GetAll()
        {
            List<EnrollmentStatus> lstEnroll = new List<EnrollmentStatus>();

            lstEnroll.Add(new EnrollmentStatus() { Id = 1, Status = "Enrolled" });
            lstEnroll.Add(new EnrollmentStatus() { Id = 2, Status = "Lost Followup" });
            lstEnroll.Add(new EnrollmentStatus() { Id = 3, Status = "With Drew Consent" });
            lstEnroll.Add(new EnrollmentStatus() { Id = 4, Status = "No Longer at School" });

            return lstEnroll;
        }
    }

    [Table("Child_Study_Schedule")]
    public class Child_Study_Schedule
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ChildId { get; set; }
        public int StudyId { get; set; }
        public int ScheduleId { get; set; }
        public int ActiveOn { get; set; }
        public int Day { get; set; }
        public int Month { get; set; }
        public int Weekday { get; set; }
    }

    public class Child_Study_ScheduleContext : DbContext
    {
        public Child_Study_ScheduleContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Child_Study_Schedule> Children_Studies_Schedules { get; set; }
    }

    [Table("Child_Survey_Schedule")]
    public class Child_Survey_Schedule
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ChildId { get; set; }
        public int StudyId { get; set; }
        public int SurveyId { get; set; }
        public int? ScheduleIdParent { get; set; }
        public int? ScheduleIdTeacher { get; set; }
        public int? ScheduleIdChild { get; set; }
        public DateTime ScheuleStartDate { get; set; }
        public DateTime ScheuleEndDate { get; set; }
    }

    public class Child_Survey_ScheduleContext : DbContext
    {
        public Child_Survey_ScheduleContext()
            : base("DefaultConnection")
        {
        }
        public DbSet<Child_Survey_Schedule> Child_Survey_Schedules { get; set; }
    }

    public class Child_Study_Respondent
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int ChildId { get; set; }
        public int StudyId { get; set; }
        public int RespondentId { get; set; }    
        public bool IncludeParent { get; set; }    
        public int? ConsentId { get; set; }
        [DefaultValue(false)]
        public bool Agreed { get; set; }
        public DateTime? AgreeDate { get; set; }
    }

    public class Child_Study_RespondentContext : DbContext
    {
        public Child_Study_RespondentContext()
            : base("DefaultConnection")
        {
        }
        public DbSet<Child_Study_Respondent> Child_Study_Respondents { get; set; }

        public static List<int> Child_GetStudies(int childId)
        {
            Child_Study_Respondent[] studies = null;
            List<int> lstStudies = new List<int>();
            List<int> lstStudiesDistinct = new List<int>();
            using (var csContext = new Child_Study_RespondentContext())
            {
                studies = csContext.Child_Study_Respondents.Where(cs => cs.ChildId == childId).ToArray();
                foreach (Child_Study_Respondent objCSR in studies)
                {
                    lstStudies.Add(objCSR.StudyId);
                }
                lstStudiesDistinct = lstStudies.Distinct().ToList();
                csContext.Dispose();
            }
                        
            return lstStudiesDistinct;
        }
    }
}