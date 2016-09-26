using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using SurveyApp.Models;
using Newtonsoft.Json;
using System.Web.Security;
using System.Data.Entity;

namespace SurveyApp.Controllers
{
    [Authorize]
    public class ConditionController : Controller
    {
        public ConditionController()
        {            
            Database.SetInitializer<Child_Study_RespondentContext>(null);
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ConditionAddEdit(int? sid)
        {
            return View(sid);
        }

        public ActionResult ConsentAddUpdate(string consents, int studyId)
        {
            int newId = 0;
            string msg = "";
            try
            {                
                List<Consent> lstConsitions = JsonConvert.DeserializeObject<List<Consent>>(consents);
                using (var cContext = new ConsentContext())
                {
                    //cContext.Consents.RemoveRange(cContext.Consents.Where(c => c.StudyId == studyId));
                    //cContext.SaveChanges();

                    foreach (Consent objConsent in lstConsitions)
                    {
                        Consent obj = cContext.Consents.Find(objConsent.Id);
                        if(obj != null && obj.Id > 0)
                        {
                            obj.Title = objConsent.Title;
                            obj.ParentConsent = System.Uri.UnescapeDataString(objConsent.ParentConsent);
                            obj.TeacherConsent = System.Uri.UnescapeDataString(objConsent.TeacherConsent);
                            obj.ChildConsent = System.Uri.UnescapeDataString(objConsent.ChildConsent);
                        }
                        else
                        {
                            objConsent.ParentConsent = System.Uri.UnescapeDataString(objConsent.ParentConsent);
                            objConsent.TeacherConsent = System.Uri.UnescapeDataString(objConsent.TeacherConsent);
                            objConsent.ChildConsent = System.Uri.UnescapeDataString(objConsent.ChildConsent);
                            cContext.Consents.Add(objConsent);
                        }
                    }

                    cContext.SaveChanges();
                    newId = cContext.Consents.Max(item => item.Id);
                    cContext.Dispose();
                }
                
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            return Json(new { success = newId > 0 ? true : false, msg = msg });
        }

        [AllowAnonymous]
        public ActionResult User_GetConsents(string userName, string password)
        {
            bool isValid = Membership.ValidateUser(userName, password);
            List<UserConsent> lstUserConsents = new List<UserConsent>();
            string msg = "";
            
            if(isValid == true)
            {                
                try
                {
                    UserProfile objUP = DataHelper.UserProfileGetUserByUserName(userName);
                    string role = Roles.GetRolesForUser(userName)[0];
                    if(role != "Admin")
                    {
                        using (var csrContext = new Child_Study_RespondentContext())
                        {
                            List<Child_Study_Respondent> lstCSRs = csrContext.Child_Study_Respondents.Where(csr => csr.RespondentId == objUP.UserId && csr.Agreed == false && csr.ConsentId > 0).ToList();
                            if (lstCSRs.Count > 0)
                            {
                                using (var conContext = new ConsentContext())
                                {
                                    List<int> lstStudies = new List<int>();
                                    foreach (Child_Study_Respondent obj in lstCSRs)
                                    {
                                        Consent objCon = conContext.Consents.Where(con => con.StudyId == obj.StudyId).FirstOrDefault();
                                        if (objCon != null && lstStudies.Contains(obj.StudyId) == false)
                                        {
                                            UserConsent objUC = new UserConsent { studyId = obj.StudyId, Consent = (role == "Parent" ? objCon.ParentConsent : (role == "Teacher" ? objCon.TeacherConsent : objCon.ChildConsent)), Title = objCon.Title, isAgreed = false };
                                            lstUserConsents.Add(objUC);
                                            lstStudies.Add(obj.StudyId);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                }
            }
            else
            {
                msg = "INVALID";
            }
            

            return Json(new { success = isValid, msg = msg, Consents = lstUserConsents });
        }

        [AllowAnonymous]
        public ActionResult User_UpdateConsents(string userName, string password)
        {
            string msg = "";
            bool isValid = Membership.ValidateUser(userName, password);
            if(isValid == true)
            {
                try
                {
                    UserProfile objUP = DataHelper.UserProfileGetUserByUserName(userName);
                    using (var csrContext = new Child_Study_RespondentContext())
                    {
                        List<Child_Study_Respondent> lstCSRs = csrContext.Child_Study_Respondents.Where(csr => csr.RespondentId == objUP.UserId).ToList();
                        foreach(Child_Study_Respondent obj in lstCSRs)
                        {
                            obj.Agreed = true;
                        }

                        csrContext.SaveChanges();
                        csrContext.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                }
            }
            else
            {
                msg = "INVALID";
            }
            
            return Json(new { success = isValid, msg = msg});
        }
    }    
}
