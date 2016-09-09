using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace SurveyApp.Models
{
    [Table("Errorlog")]
    public class ErrorLog
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string ErrorMessage { get; set; }
        public string StackTrace { get; set; }
        public int UserId { get; set; }
        public int UserName { get; set; }
        public string UserAgent { get; set; }
        public string UserHostAddress { get; set; }
        public string SessionId { get; set; }
    }

    public class ErrorLogContext : DbContext
    {
        public ErrorLogContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<ErrorLog> ErrorLogs { get; set; }
    }
}