using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SurveyApp.Models
{
    [Table("Consent")]
    public class Consent
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int StudyId { get; set; }
        public string Title { get; set; }        
        public string ParentConsent { get; set; }
        public string TeacherConsent { get; set; }
        public string ChildConsent { get; set; }
    }

    public class ConsentContext : DbContext
    {
        public ConsentContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<Consent> Consents { get; set; }
    }

    public class UserConsent
    {
        public int consentId { get; set; }
        public int studyId { get; set; }
        public string Title { get; set; }
        public string Consent { get; set; }
        public bool isAgreed { get; set; }
    }
}