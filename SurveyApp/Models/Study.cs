using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace SurveyApp.Models
{
    public class StudyContext : DbContext
    {
        public StudyContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Study> Studies { get; set; }        
    }

    public class Study_Survey_ScheduleContext : DbContext
    {
        public Study_Survey_ScheduleContext()
            : base("DefaultConnection")
        {
        }
        
        public DbSet<Study_Survey_Schedule> SSSs { get; set; }
    }

    [Table("Study")]
    public class Study
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "Please provide name for study")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Please select status of study")]
        public int Status { get; set; }

        public static List<Study> StudyGetAll()
        {
            using (var context = new StudyContext())
            {
                return context.Studies.ToList();
            }
        }
    }

    [Table("Study_Survey_Schedule")]
    public class Study_Survey_Schedule
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int StudyId { get; set; }
        public int SurveyId { get; set; }
        public int ScheduleIdTeacher { get; set; }
        public int ScheduleIdParent { get; set; }
    }

    public class StudyStatus{
        public int StatusId{ get; set; }
        public string StatusOption { get; set; }
        
        public static List<StudyStatus> GetAllStatus()
        {
            return new List<StudyStatus>() { new StudyStatus() { StatusId = 1, StatusOption = "Active" }, new StudyStatus() { StatusId = 2, StatusOption = "Inactive" }, new StudyStatus() { StatusId = 3, StatusOption = "Incomplete" } };
        }
    }

    
}