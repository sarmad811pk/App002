using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using WebMatrix.WebData;


namespace SurveyApp.Controllers
{
    public class UserQuestionController : Controller
    {
        //
        // GET: /UserQuestion/


        public ActionResult Index(int? childID)
        {
            if (!Request.IsAuthenticated)
            {
                WebSecurity.Logout();
                return RedirectToAction("Login", "Account");
            }
            TempData["ChildID"] = childID;
            TempData["ID"] = WebSecurity.CurrentUserId;
            return View();

        }
        public ActionResult Question(int? surveyID)
        {
            
            return View(surveyID);
        }

     
        [HttpPost]
        public void SaveUserQuestion(string QuestionID, string AnswerID, string score, string childid, string SurveyID, string statusparm, string percentage, string scheduleDate)
        {        
            var userID = WebSecurity.CurrentUserId;
            //if (SurveyID == "15" || SurveyID == "28" || SurveyID == "29") { statusparm = ""; }
            int rsl = SurveyApp.DataHelper.SaveuserQuestions(userID, QuestionID, AnswerID, score, childid, SurveyID,statusparm, percentage, scheduleDate != "" ? Convert.ToDateTime(scheduleDate) : DateTime.MinValue);
            
        }
        [HttpPost]
        public void savequestionsAdverseReaction(
                string AdverseReaction, 
                string DateOccured, 
                string DateResolved, 
                string Medication, 
                string DateStart, 
                string DateEnd, 
                string DateSubmitted, 
                int ChildID)
        {
            var userID = WebSecurity.CurrentUserId;

            int rsl = SurveyApp.DataHelper.savequestionsAdverseReaction(AdverseReaction, 
                DateOccured, 
                DateResolved, 
                Medication, 
                DateStart, 
                DateEnd, 
                DateSubmitted, 
                ChildID,
                userID);

        } 
    }
}
