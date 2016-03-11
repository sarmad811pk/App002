using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SurveyApp.Models
{
    public class ScheduleContext : DbContext
    {
        public ScheduleContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Schedule> Schedules { get; set; }
    }
    [Table("Schedule")]
    public class Schedule
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Title { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int AssignmentRemider { get; set; }
        public int CompletionRemider { get; set; }
        public int OccurenceId { get; set; }
    }

    [Table("Occurence")]
    public class Occurence
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string OccurenceType { get; set; }        
    }
}