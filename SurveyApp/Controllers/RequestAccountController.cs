using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using SurveyApp.Models;
using System.Data.Entity;

namespace SurveyApp.Controllers
{
    public class RequestAccountController : Controller
    {
        public RequestAccountController()
        {
            Database.SetInitializer<AccountRequestContext>(null);
        }
        public ActionResult Index(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        public ActionResult RequestAccount(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        public ActionResult RequestAccount(AccountRequest mAccountRequest, FormCollection collection)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Please provide required information.");
                return View(mAccountRequest);
            }

            using (var arContext = new AccountRequestContext())
            {
                List<AccountRequest> lstAR = arContext.AccountRequests.Where(ar => ar.Email == mAccountRequest.Email).ToList();
                if(lstAR.Count > 0)
                {
                    ModelState.AddModelError("", "There is already an account with this email address, please provide a different one.");
                    return View(mAccountRequest);
                }

                AccountRequest objAccountRequest = new AccountRequest();
                objAccountRequest.Email = mAccountRequest.Email;
                objAccountRequest.FullName = mAccountRequest.FullName;
                objAccountRequest.SchoolName = mAccountRequest.SchoolName;
                objAccountRequest.Comments = mAccountRequest.Comments;

                arContext.AccountRequests.Add(objAccountRequest);
                arContext.SaveChanges();

                return RedirectToAction("Login", "Account", new { ar = true});
            }            
        }
    }
}
