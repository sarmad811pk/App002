using SurveyApp.Filters;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace SurveyApp.Models
{   
    public class SchoolContext : DbContext
    {
        public SchoolContext()
            : base("DefaultConnection")
        {
        }
        
        public DbSet<School> Schools { get; set; }
    }
    [Table("School")]
    public class School
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int SchoolId { get; set; }
        [Required(ErrorMessage = "Please provide school name")]
        public string Name { get; set; }

        public static List<School> SchoolGetAll()
        {
            using (var context = new SchoolContext())
            {
                return context.Schools.ToList();
            }
        }
    }
}
