using SurveyApp.Filters;
using SurveyApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using WebMatrix.WebData;

namespace SurveyApp.Controllers
{
    
    public class ParentTeacherController : Controller
    {
        //
        // GET: /ParentTeacher/
        public ParentTeacherController()
        {         
            Database.SetInitializer<StudyContext>(null);        
        }

        public static RegisterModel RegisterModel;        

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ParentTeacherAddEdit(int? ID)
        {
            ParentTeacher parentTeacherModel = new ParentTeacher();
            RegisterModel registerModel = new RegisterModel();
            
            if (ID.HasValue)
            {
                parentTeacherModel.Id = ID.Value;
                DataSet ds = DataHelper.UserProfileGetUserByID(ID.Value);
                if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    registerModel.FullName = ds.Tables[0].Rows[0]["FullName"].ToString();
                    parentTeacherModel.Name = ds.Tables[0].Rows[0]["FullName"].ToString();
                    registerModel.UserName = ds.Tables[0].Rows[0]["UserName"].ToString();
                    registerModel.Password = ds.Tables[0].Rows[0]["Password"].ToString();
                    registerModel.ConfirmPassword = ds.Tables[0].Rows[0]["Password"].ToString();
                    parentTeacherModel.Role = ds.Tables[0].Rows[0]["RoleId"] != DBNull.Value ? Convert.ToInt32(ds.Tables[0].Rows[0]["RoleId"]) : -1;
                }
                if (ds != null && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    parentTeacherModel.SchoolId = ds.Tables[1].Rows[0]["SchoolId"] != DBNull.Value ? Convert.ToInt32(ds.Tables[1].Rows[0]["SchoolId"]) : -1;                    
                }
            }
                         
            return View(new ParentTeacher_Register() { vw_ParentTeacher = parentTeacherModel, vw_Register = registerModel });
                     
        }

        [HttpPost]
        public ActionResult ParentTeacherAddEdit(ParentTeacher_Register parentTeacher_RegisterModel, FormCollection collection, int? ID)
        {
            ParentTeacher parentTeacherModel = parentTeacher_RegisterModel.vw_ParentTeacher;
            RegisterModel registerModel = parentTeacher_RegisterModel.vw_Register;

            if (!ModelState.IsValid)
            {
                return View(parentTeacher_RegisterModel);
            }

            DataTable dtRoles = SurveyApp.DataHelper.RolesGetAll().Tables[0];
            int teacherRoleId = (int)dtRoles.Select("RoleName = 'Teacher'")[0]["RoleId"];
            int parentRoleId = (int)dtRoles.Select("RoleName = 'Parent'")[0]["RoleId"];

            if (parentTeacherModel.Role == teacherRoleId
                && (parentTeacherModel.SchoolId == null || parentTeacherModel.SchoolId == 0)
                && String.IsNullOrEmpty(parentTeacherModel.SchoolName))
            {
                ModelState.AddModelError("", "Please select a school for the teacher.");
                return View(parentTeacher_RegisterModel);
            }

            int studyCount = 0;
            foreach (var key in collection.Keys)
            {
                if (key.ToString().Contains("StudyId_"))
                {
                    studyCount++;
                }
            }
            if (studyCount == 0)
            {
                ModelState.AddModelError("", "Please select at least one study.");
                return View(parentTeacher_RegisterModel);
            }

            bool doesUserExists = false;
            MembershipUser objUser = Membership.GetUser(registerModel.UserName);
            if(objUser != null)
            {
                doesUserExists = true;
            }

            if (parentTeacherModel.Id <= 0 && (doesUserExist(parentTeacherModel.Name) || doesUserExists == true))
            {
                ModelState.AddModelError("", "This user already exists in the system, please provide different details.");
                return View(parentTeacher_RegisterModel);
            }

            int ptId = 0, schoolId = 0;
            try
            {
                //save school info
                if (parentTeacherModel.Role == teacherRoleId)
                {
                    if (!parentTeacherModel.SchoolId.HasValue && !String.IsNullOrEmpty(parentTeacherModel.SchoolName))
                    {
                        using (var sContext = new SchoolContext())
                        {
                            School objSchool = null;
                            objSchool = new School { Name = parentTeacherModel.SchoolName };
                            sContext.Schools.Add(objSchool);
                            sContext.SaveChanges();

                            schoolId = sContext.Schools.Max(item => item.SchoolId);
                        }
                    }
                }                            

                //save account info                
                try
                {                    
                    string roleName = (parentTeacherModel.Role == parentRoleId ? "Parent" : (parentTeacherModel.Role == teacherRoleId ? "Teacher" : ""));
                    if (parentTeacherModel.Id > 0)
                    {
                        string[] roles = Roles.GetRolesForUser(registerModel.UserName);
                        if (roles.Length > 0)
                        {
                            foreach (string role in roles)
                            {
                                Roles.RemoveUserFromRole(registerModel.UserName, role);
                            }                            
                        }                        
                        Roles.AddUserToRole(registerModel.UserName, roleName);

                        using (var uContext = new UsersContext())
                        {
                            UserProfile objUP = uContext.UserProfiles.Find(parentTeacherModel.Id);
                            objUP.FullName = parentTeacherModel.Name;
                            uContext.SaveChanges();

                            try
                            {
                                List<string> lstEmails = new List<string>();
                                lstEmails.Add(registerModel.UserName);

                                string body = "";
                                using (System.IO.StreamReader reader = new System.IO.StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Attachments/Account_Update.html")))
                                {
                                    body = reader.ReadToEnd();
                                }
                                body = body.Replace("_ROOTPATH_", System.Web.Configuration.WebConfigurationManager.AppSettings["_RootPath"].ToString());
                                body = body.Replace("_USERNAME_", registerModel.UserName);
                                body = body.Replace("_PASSWORD_", AccountController.getPwd(registerModel.UserName));
                                body = body.Replace("_FULLNAME_", registerModel.FullName);

                                SMTPHelper.SendGridEmail("eBIT - Account Updated", body, lstEmails, true);
                            }
                            catch(Exception ex)
                            {
                                ModelState.AddModelError("", ex.Message);
                                return View(parentTeacher_RegisterModel);
                            }
                        }
                        ptId = parentTeacherModel.Id;
                    }
                    else
                    {                        
                        registerModel.FullName = parentTeacherModel.Name;
                        AccountController.CreateAccount(registerModel, roleName);
                        ptId = WebSecurity.GetUserId(registerModel.UserName);                        
                    }                    
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", AccountController.ErrorCodeToString(e.StatusCode));
                    return View(parentTeacher_RegisterModel);
                }

                //save parent teacher school relationship info
                
                using (var ptScContext = new PParentTeacher_SchoolContext())
                {
                    if ((parentTeacherModel.SchoolId.HasValue == true || schoolId > 0) && parentTeacherModel.Id > 0)
                    {
                        ptScContext.ParentTeacher_Schools.RemoveRange(ptScContext.ParentTeacher_Schools.Where(pts => pts.ParentTeacherId == parentTeacherModel.Id));
                        ptScContext.SaveChanges();
                    }
                    if (parentTeacherModel.Role == teacherRoleId)
                    {
                        ParentTeacher_School objPTS = new ParentTeacher_School();
                        objPTS.ParentTeacherId = ptId;
                        objPTS.SchoolId = parentTeacherModel.SchoolId.HasValue == true ? parentTeacherModel.SchoolId.Value : schoolId;

                        ptScContext.ParentTeacher_Schools.Add(objPTS);
                        ptScContext.SaveChanges();
                    }
                }

                //save parent teacher study relationship info                
                using (var ptsContext = new ParentTeacher_StudyContext())
                {
                    if (parentTeacherModel.Id > 0 && studyCount > 0)
                    {
                        ptsContext.ParentTeacher_Studys.RemoveRange(ptsContext.ParentTeacher_Studys.Where(pts => pts.ParentTeacherId == parentTeacherModel.Id));
                        ptsContext.SaveChanges();
                    }
                    foreach (Study objStudy in Study.StudyGetAll())
                    {
                        if (!String.IsNullOrEmpty(collection["StudyId_" + objStudy.Id]))
                        {                            
                            ParentTeacher_Study objPTS = new ParentTeacher_Study();
                            objPTS.ParentTeacherId = ptId;
                            objPTS.StudyId = Convert.ToInt32(collection["StudyId_" + objStudy.Id]);

                            ptsContext.ParentTeacher_Studys.Add(objPTS);
                        }
                    }

                    ptsContext.SaveChanges();
                }

                return RedirectToAction("Index", "ParentTeacher");
                
            }
            catch(Exception ex){
                ModelState.AddModelError("", ex.Message);
            }

            return View(parentTeacher_RegisterModel);
        }

        public bool doesUserExist(string name)
        {
            bool result = false;
            using (var cContext = new UsersContext())
            {
                UserProfile objUP = cContext.UserProfiles.Where(u => u.FullName == name).FirstOrDefault();
                if (objUP != null && objUP.UserId > 0)
                {
                    result = true;
                }
            }

            return result;
        }

    }
}
