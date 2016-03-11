using SurveyApp.Filters;
using SurveyApp.Models;
using System;
using System.Collections.Generic;
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
        public static RegisterModel RegisterModel;
        public static School SchoolModel;

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ParentTeacherAddEdit(ParentTeacher parentTeacherModel, RegisterModel registerModel, School schoolModel, FormCollection collection, int? ID)
        {
            //if (!ModelState.IsValid)
            //{
            //    return View(parentTeacherModel);
            //}

            int ptId = 0, schoolId = 0;
            try
            {

                //save school info                
                using (var sContext = new SchoolContext())
                {
                    School objSchool = null;
                    objSchool = new School { Name = schoolModel.Name };
                    sContext.Schools.Add(objSchool);
                    sContext.SaveChanges();

                    schoolId = sContext.Schools.Max(item => item.SchoolId);
                }                

                //save account info                
                try
                {
                    AccountController.CreateAccount(registerModel, (parentTeacherModel.Role == (int)SurveyAppRoles.Parent ? "Parent" : (parentTeacherModel.Role == (int)SurveyAppRoles.Teacher ? "Teacher" : "")));
                    ptId = WebSecurity.GetUserId(registerModel.UserName);
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", AccountController.ErrorCodeToString(e.StatusCode));
                    return View(parentTeacherModel);
                }

                //save parent teacher school relationship info
                using (var ptScContext = new PParentTeacher_SchoolContext())
                {
                    ParentTeacher_School objPTS = new ParentTeacher_School();
                    objPTS.ParentTeacherId = ptId;
                    objPTS.SchoolId = parentTeacherModel.SchoolId;

                    ptScContext.ParentTeacher_Schools.Add(objPTS);
                    ptScContext.SaveChanges();
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

                
            }
            catch(Exception ex){
            
            }



            return View(parentTeacherModel);
        }
        
    }
}
