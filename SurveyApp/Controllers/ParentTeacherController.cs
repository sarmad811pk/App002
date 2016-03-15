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
                DataSet ds = DataHelper.UserProfileGetUserByID(ID.Value);
                if (ds != null && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    registerModel.FullName = ds.Tables[0].Rows[0]["FullName"].ToString();
                    parentTeacherModel.Name = ds.Tables[0].Rows[0]["FullName"].ToString();
                    registerModel.UserName = ds.Tables[0].Rows[0]["UserName"].ToString();
                    registerModel.Password = ds.Tables[0].Rows[0]["Password"].ToString();
                    registerModel.ConfirmPassword = ds.Tables[0].Rows[0]["Password"].ToString();
                }
                if (ds != null && ds.Tables[1] != null && ds.Tables[1].Rows.Count > 0)
                {
                    parentTeacherModel.SchoolId = ds.Tables[1].Rows[0]["SchoolId"] != DBNull.Value ? Convert.ToInt32(ds.Tables[1].Rows[0]["SchoolId"]) : -1;
                    parentTeacherModel.Role = ds.Tables[0].Rows[0]["RoleId"] != DBNull.Value ? Convert.ToInt32(ds.Tables[0].Rows[0]["RoleId"]) : -1;
                }
                ViewData["Studies"] = ds != null && ds.Tables[2].Rows.Count > 0 ? ds.Tables[2] : null;

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
                        
            if (parentTeacherModel.Role == (int)SurveyAppRoles.Teacher 
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
            

            int ptId = 0, schoolId = 0;
            try
            {
                //save school info
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

                //save account info                
                try
                {
                    registerModel.FullName = parentTeacherModel.Name;
                    AccountController.CreateAccount(registerModel, (parentTeacherModel.Role == (int)SurveyAppRoles.Parent ? "Parent" : (parentTeacherModel.Role == (int)SurveyAppRoles.Teacher ? "Teacher" : "")));
                    ptId = WebSecurity.GetUserId(registerModel.UserName);
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", AccountController.ErrorCodeToString(e.StatusCode));
                    return View(parentTeacher_RegisterModel);
                }

                //save parent teacher school relationship info
                if (parentTeacherModel.Role == (int)SurveyAppRoles.Teacher)
                {
                    using (var ptScContext = new PParentTeacher_SchoolContext())
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
        
    }
}
