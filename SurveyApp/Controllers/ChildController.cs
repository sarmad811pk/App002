using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SurveyApp.Controllers
{
    public class ChildController : Controller
    {
        //
        // GET: /Child/

        public ActionResult Index()
        {
            return View();
        }
        public ActionResult ChildAddEdit(int? id)
        {
            return View(id);
        }
    }
}
