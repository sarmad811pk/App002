﻿using System;
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
       
        public ActionResult Index()
        {
            TempData["ID"] = WebSecurity.CurrentUserId;
            
            return View();
            
        }

        public ActionResult Question(int? surveyID)
        {
            
            return View(surveyID);
        }

     
        [HttpPost]
        public void SaveUserQuestion(string QuestionID, string AnswerID, string score, string childid, string SurveyID, string statusparm)
        {
        
            var userID = WebSecurity.CurrentUserId;


            int rsl = SurveyApp.DataHelper.SaveuserQuestions(userID, QuestionID, AnswerID, score, childid, SurveyID,statusparm);
            
        }  
    }
}
