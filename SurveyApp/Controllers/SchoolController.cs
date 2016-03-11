using SurveyApp.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SurveyApp.Controllers
{
    public class SchoolController : Controller
    {
        //
        // GET: /School/

        public ActionResult Index()
        {
            DataSet dsSchools = DataHelper.SchoolGetAll();
            return View(dsSchools);
        }

        public ActionResult SchoolAddEdit(int? ID)
        {
            if (ID == null)
            {
                School school = new School();
                school.SchoolId = 0;
                return View(school);
            }
            else
            {
                var db = new SchoolContext();
                School school = db.Schools.Find(ID);
                return View(school);
            }
        }

        [HttpPost]
        public ActionResult SchoolAddEdit(School model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            using (var db = new SchoolContext())
            {
                School school = null;
                if (model.SchoolId > 0)
                {
                    var result = db.Schools.SingleOrDefault(s => s.SchoolId == model.SchoolId);
                    if (result != null)
                    {
                        result.Name = model.Name;
                    }
                    
                    //school = new School { SchoolId = Convert.ToInt32(ID), Name = "" };
                }
                else
                {
                    school = new School { Name = model.Name };
                    db.Schools.Add(school);
                }

                //db.Schools            
                db.SaveChanges();
            }


            return RedirectToAction("Index", "School");
            
        }
    }
}
