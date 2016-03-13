using SurveyApp.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SurveyApp.Controllers
{
    public class ChildController : Controller
    {
        //
        // GET: /Child/
        public ChildController() {         
            Database.SetInitializer<StudyContext>(null);        
        }

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ChildAddEdit(int? id)
        {

            return View(id);
        }

        [HttpPost]
        public ActionResult ChildAddEdit(Child childModel)
        {
            return View(childModel);
        }
    }
}
