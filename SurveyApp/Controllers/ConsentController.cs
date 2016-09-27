using SurveyApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;

namespace SurveyApp.Controllers
{
    public class ConsentController : Controller
    {
        public ActionResult Index(string cid, int? sts = 0)
        {
            ViewBag.Cid = cid;
            cid = Encryption.Decrypt(cid, System.Web.Configuration.WebConfigurationManager.AppSettings["key5"].ToString(), false);
            if (String.IsNullOrEmpty(cid))
            {
                return RedirectToAction("Login", "Account");
            }
            if(sts == 0)
            {
                using (var cContext = new SurveyApp.Models.ChildContext())
                {
                    Child objChild = cContext.Children.Find(Convert.ToInt32(cid));
                    if (objChild.Agreed == true)
                    {
                        return RedirectToAction("Index", "Consent", new { cid = HttpUtility.HtmlEncode(ViewBag.Cid), sts = 1 });
                    }
                }
            }
            
            return View(Convert.ToInt32(cid));
        }

        [System.Web.Http.HttpPost]
        public ActionResult Agreed(string cid, int? sts = 0)
        {
            int status = 0;
            try
            {
                using (var cConext = new ChildContext())
                {
                    int childId = Convert.ToInt32(HttpUtility.HtmlDecode(Encryption.Decrypt(cid, System.Web.Configuration.WebConfigurationManager.AppSettings["key5"].ToString(), false)));
                    Child objChild = cConext.Children.Find(childId);
                    objChild.Agreed = true;
                    objChild.AgreeDate = DateTime.Now;
                    cConext.SaveChanges();
                }
                status = 1;
            }
            catch(Exception ex)
            {
                status = 0;
            }

            return RedirectToAction("Index", "Consent", new { cid = HttpUtility.HtmlEncode(cid), sts = status });
        }
    }
}
