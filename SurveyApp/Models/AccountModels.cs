using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Globalization;
using System.Web.Security;
using WebMatrix.WebData;

namespace SurveyApp.Models
{
    public class UsersContext : DbContext
    {
        public UsersContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<UserProfile> UserProfiles { get; set; }
        //public DbSet<webpages_UsersInRoles> UsersInRoles { get; set; }
        //public (SimpleRoleProvider)Roles.Provider Roless { get; set; }
        //public DbSet<Roles> Roles { get; set; }
        //public DbSet<School> Schools { get; set; }
    }

    [Table("UserProfile")]
    public class UserProfile
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }        
    }

    public class RegisterExternalLoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        public string ExternalLoginData { get; set; }
    }

    public class LocalPasswordModel
    {
        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class LoginModel
    {
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }                
    }

    public class RegisterModel
    {
        [Required]
        [Display(Name = "User name")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "User Name is not valid")]        
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 0)]
        [DataType(DataType.Text)]
        [Display(Name = "Full name")]
        public string FullName { get; set; }
    }

    [Table("AccountRequest")]
    public class AccountRequest
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "Please provide email")]
        [Display(Name = "Email")]
        [RegularExpression("^[a-zA-Z0-9_\\.-]+@([a-zA-Z0-9-]+\\.)+[a-zA-Z]{2,6}$", ErrorMessage = "Email is not valid")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Please provide Full Name")]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }
        [Display(Name = "School Name")]
        public string SchoolName { get; set; }
        public string Comments { get; set; }
        //public bool? AccountCreated { get; set; }
    }

    public class AccountRequestContext : DbContext
    {
        public AccountRequestContext()
            : base("DefaultConnection")
        {

        }

        public DbSet<AccountRequest> AccountRequests { get; set; }
    }

    public class ExternalLogin
    {
        public string Provider { get; set; }
        public string ProviderDisplayName { get; set; }
        public string ProviderUserId { get; set; }
    }

    [Table("ActivityLog")]
    public class ActivityLog
    {
        [Key]
        [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Activity { get; set; }
        public DateTime Date { get; set; }
        public string Information { get; set; }
    }

    public class ActivityLogContext : DbContext
    {
        public ActivityLogContext()
            : base("DefaultConnection")
        {
        }

        public DbSet<ActivityLog> Activities { get; set; }
    }

    //[Table("webpages_UsersInRoles")]
    //public class webpages_UsersInRoles
    //{
    //    [Key]
    //    [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
    //    public int UserId { get; set; }
    //    public int RoleId { get; set; }        
    //}

    //[Table("webpages_Roles")]
    //public class webpages_Roles
    //{
    //    [Key]
    //    [DatabaseGeneratedAttribute(DatabaseGeneratedOption.Identity)]
    //    public int RoleId { get; set; }
    //    public string RoleName { get; set; }
    //}
    //public class Webpages_RolesContext : DbContext
    //{
    //    public Webpages_RolesContext()
    //        : base("DefaultConnection")
    //    {
    //    }

    //    public DbSet<webpages_Roles> Roles { get; set; }
    //}
    
}
