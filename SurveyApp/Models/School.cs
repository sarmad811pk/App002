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
        [Required(ErrorMessage = "Please provide school name", AllowEmptyStrings = false)]
        public string Name { get; set; }
        public bool IsDeleted { get; set; }

        public static List<School> SchoolGetAll()
        {
            List<School> lstSchools = new List<School>();
            using (var context = new SchoolContext())
            {
                foreach (School objSchool in context.Schools.ToList())
                {
                    if (objSchool.IsDeleted == false)
                    {
                        lstSchools.Add(objSchool);
                    }
                }
            }

            return lstSchools;
        }
    }
}
