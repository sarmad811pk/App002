﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using DotNetOpenAuth.AspNet;
using Microsoft.Web.WebPages.OAuth;
using WebMatrix.WebData;
using SurveyApp.Filters;
using SurveyApp.Models;
using System.Web.UI;
using System.Data.Entity;

namespace SurveyApp.Controllers
{
    [Authorize]
    //[InitializeSimpleMembership]
    public class AccountController : Controller
    {
        public AccountController()
        {
            Database.SetInitializer<UsersContext>(null);
            Database.SetInitializer<ConsentContext>(null);
            Database.SetInitializer<Child_Study_RespondentContext>(null);
            Database.SetInitializer<UsersContext>(null);
        }
        //
        // GET: /Account/Login

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (Request.IsAuthenticated)
            {                
                return RedirectToAction("Index", "Home");
            }

            //sendReminderEmails();

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //public static void sendReminderEmails()
        //{

        //    List<string> lstEmails = new List<string>();
        //    lstEmails.Add("shazeb140@gmail.com");
        //    string body = "test";

        //    SMTPHelper.SendGridEmail("testt", body, lstEmails, true);
        //}

        //
        // POST: /Account/Login

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginModel model, string returnUrl, FormCollection frm)
        {
            if (ModelState.IsValid)
            {
                UserProfile objUP = DataHelper.UserProfileGetUserByUserName(model.UserName);
                string role = Roles.GetRolesForUser(model.UserName)[0];
                if(role != "Administrator")
                {
                    using (var csrContext = new Child_Study_RespondentContext())
                    {                        
                        List<Child_Study_Respondent> lstCSRs = csrContext.Child_Study_Respondents.Where(csr => csr.RespondentId == objUP.UserId && csr.Agreed == false).ToList();
                        if (lstCSRs.Count > 0)
                        {
                            foreach(Child_Study_Respondent obj in lstCSRs)
                            {
                                using (var conContext = new ConsentContext())
                                {
                                    Consent objCon = conContext.Consents.Find(obj.ConsentId);
                                    if(objCon != null)
                                    {
                                        ModelState.AddModelError("", "You have not given consent for all the studies.");
                                        return View(model);
                                    }
                                }                                
                            }                            
                        }
                    }
                }
                
            }
            if (ModelState.IsValid && WebSecurity.Login(model.UserName, model.Password, persistCookie: model.RememberMe))
            {   
                using (var acContext = new ActivityLogContext())
                {
                    ActivityLog objALog = new ActivityLog();
                    objALog.Activity = "Login";
                    objALog.Date = DateTime.Now;
                    objALog.Information = Request.Url.ToString();
                    objALog.UserId = WebSecurity.GetUserId(model.UserName);

                    acContext.Activities.Add(objALog);
                    acContext.SaveChanges();
                }
                
                return RedirectToLocal(returnUrl);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "The user name or password provided is incorrect.");
            return View(model);
        }

        //
        // POST: /Account/LogOff

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            using (var acContext = new ActivityLogContext())
            {
                ActivityLog objALog = new ActivityLog();
                objALog.Activity = "LogOff";
                objALog.Date = DateTime.Now;
                objALog.Information = Request.Url.ToString();
                objALog.UserId = WebSecurity.CurrentUserId;

                acContext.Activities.Add(objALog);
                acContext.SaveChanges();
            }

            WebSecurity.Logout();            
            return RedirectToAction("Login", "Account");
        }

        //
        // GET: /Account/Register

        [AllowAnonymous]
        public ActionResult Register()
        {
            return View();
        }

        //
        // POST: /Account/Register

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                // Attempt to register the user
                try
                {
                    CreateAccount(model,"Administrator");
                    WebSecurity.Login(model.UserName, model.Password);
                    return RedirectToAction("Index", "Home");
                }
                catch (MembershipCreateUserException e)
                {
                    ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }
        public ActionResult AccountEdit(string userName, string newUserName, string password, bool updatePassword)
        {
            bool isSuccess = false;
            string msg = "";
            try
            {
                if (updatePassword == true)
                {
                    WebSecurity.ResetPassword(WebSecurity.GeneratePasswordResetToken(userName), password);
                }

                if(userName != newUserName)
                {
                    using (var uContext = new UsersContext())
                    {
                        UserProfile objUser = uContext.UserProfiles.SingleOrDefault(u => u.UserName == userName);
                        objUser.UserName = newUserName;
                        uContext.SaveChanges();                                                
                    }
                }

                try
                {
                    if(updatePassword == true)
                    {
                        DataHelper.saveCred(newUserName, password);
                    }                    
                }
                catch (Exception ex)
                {

                }

                List<string> lstEmails = new List<string>();
                lstEmails.Add(newUserName);
                string body = "";
                using (System.IO.StreamReader reader = new System.IO.StreamReader(Server.MapPath("~/Attachments/Account_Update_UP.html")))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("_ROOTPATH_", System.Web.Configuration.WebConfigurationManager.AppSettings["_RootPath"].ToString());
                body = body.Replace("_USERNAME_", newUserName);
                body = body.Replace("_PASSWORD_", AccountController.getPwd(newUserName));

                isSuccess = SMTPHelper.SendGridEmail("eBit - Account Updated", body, lstEmails, true);
            }
            catch(Exception ex) {
                isSuccess = false;
                msg = ex.Message;
            }

            return Json(new { success = isSuccess, error = msg });
        }        

        public static bool CreateAccount(RegisterModel model, string role = ""){
            bool isSuccess = false;
            try
            {
                //MembershipUser mem = Membership.CreateUser(model.UserName, model.Password);
                WebSecurity.CreateUserAndAccount(model.UserName, model.Password, new { FullName = model.FullName });
                if (role != "")
                {
                    var roles = (SimpleRoleProvider)Roles.Provider;
                    if (!roles.RoleExists(role))
                    {
                        roles.CreateRole(role);
                    }
                    Roles.AddUserToRole(model.UserName, role);
                }

                try
                {
                    DataHelper.saveCred(model.UserName, model.Password);
                }
                catch(Exception ex)
                {
                    
                }
                

                List<string> lstEmails = new List<string>();
                lstEmails.Add(model.UserName);

                string body = "";
                using (System.IO.StreamReader reader = new System.IO.StreamReader(System.Web.HttpContext.Current.Server.MapPath("~/Attachments/Account_Add.html")))
                {
                    body = reader.ReadToEnd();
                }
                body = body.Replace("_ROOTPATH_", System.Web.Configuration.WebConfigurationManager.AppSettings["_RootPath"].ToString());
                body = body.Replace("_USERNAME_", model.UserName);
                body = body.Replace("_PASSWORD_", model.Password);
                body = body.Replace("_FULLNAME_", model.FullName);

                isSuccess = SMTPHelper.SendGridEmail("eBIT - Account Created", body, lstEmails, true);                
            }
            catch (Exception ex)
            {
                throw;
            }

            return isSuccess;
        }

        public static string getPwd(string em)
        {
            string pwd = "";
            if (String.IsNullOrEmpty(em) == false)
            {
                System.Data.DataSet dsCred = DataHelper.getCred(em);
                if (dsCred != null && dsCred.Tables[0].Rows.Count > 0)
                {
                    pwd = dsCred.Tables[0].Rows[0]["pwd"] != DBNull.Value ? dsCred.Tables[0].Rows[0]["pwd"].ToString() : "";
                }
            }

            return pwd;
        }

        //
        // POST: /Account/Disassociate

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Disassociate(string provider, string providerUserId)
        {
            string ownerAccount = OAuthWebSecurity.GetUserName(provider, providerUserId);
            ManageMessageId? message = null;

            // Only disassociate the account if the currently logged in user is the owner
            if (ownerAccount == User.Identity.Name)
            {
                // Use a transaction to prevent the user from deleting their last login credential
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TransactionOptions { IsolationLevel = IsolationLevel.Serializable }))
                {
                    bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
                    if (hasLocalAccount || OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name).Count > 1)
                    {
                        OAuthWebSecurity.DeleteAccount(provider, providerUserId);
                        scope.Complete();
                        message = ManageMessageId.RemoveLoginSuccess;
                    }
                }
            }

            return RedirectToAction("Manage", new { Message = message });
        }

        //
        // GET: /Account/Manage

        public ActionResult Manage(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : "";
            ViewBag.HasLocalPassword = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.ReturnUrl = Url.Action("Manage");
            return View();
        }

        //
        // POST: /Account/Manage

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Manage(LocalPasswordModel model)
        {
            bool hasLocalAccount = OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            ViewBag.HasLocalPassword = hasLocalAccount;
            ViewBag.ReturnUrl = Url.Action("Manage");
            if (hasLocalAccount)
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    bool changePasswordSucceeded;
                    try
                    {
                        changePasswordSucceeded = WebSecurity.ChangePassword(User.Identity.Name, model.OldPassword, model.NewPassword);
                    }
                    catch (Exception)
                    {
                        changePasswordSucceeded = false;
                    }

                    if (changePasswordSucceeded)
                    {
                        return RedirectToAction("Manage", new { Message = ManageMessageId.ChangePasswordSuccess });
                    }
                    else
                    {
                        ModelState.AddModelError("", "The current password is incorrect or the new password is invalid.");
                    }
                }
            }
            else
            {
                // User does not have a local password so remove any validation errors caused by a missing
                // OldPassword field
                ModelState state = ModelState["OldPassword"];
                if (state != null)
                {
                    state.Errors.Clear();
                }

                if (ModelState.IsValid)
                {
                    try
                    {
                        WebSecurity.CreateAccount(User.Identity.Name, model.NewPassword);
                        return RedirectToAction("Manage", new { Message = ManageMessageId.SetPasswordSuccess });
                    }
                    catch (Exception)
                    {
                        ModelState.AddModelError("", String.Format("Unable to create local account. An account with the name \"{0}\" may already exist.", User.Identity.Name));
                    }
                }
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // POST: /Account/ExternalLogin

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            return new ExternalLoginResult(provider, Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
        }

        //
        // GET: /Account/ExternalLoginCallback

        [AllowAnonymous]
        public ActionResult ExternalLoginCallback(string returnUrl)
        {
            AuthenticationResult result = OAuthWebSecurity.VerifyAuthentication(Url.Action("ExternalLoginCallback", new { ReturnUrl = returnUrl }));
            if (!result.IsSuccessful)
            {
                return RedirectToAction("ExternalLoginFailure");
            }

            if (OAuthWebSecurity.Login(result.Provider, result.ProviderUserId, createPersistentCookie: false))
            {
                return RedirectToLocal(returnUrl);
            }

            if (User.Identity.IsAuthenticated)
            {
                // If the current user is logged in add the new account
                OAuthWebSecurity.CreateOrUpdateAccount(result.Provider, result.ProviderUserId, User.Identity.Name);
                return RedirectToLocal(returnUrl);
            }
            else
            {
                // User is new, ask for their desired membership name
                string loginData = OAuthWebSecurity.SerializeProviderUserId(result.Provider, result.ProviderUserId);
                ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(result.Provider).DisplayName;
                ViewBag.ReturnUrl = returnUrl;
                return View("ExternalLoginConfirmation", new RegisterExternalLoginModel { UserName = result.UserName, ExternalLoginData = loginData });
            }
        }

        //
        // POST: /Account/ExternalLoginConfirmation

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLoginConfirmation(RegisterExternalLoginModel model, string returnUrl)
        {
            string provider = null;
            string providerUserId = null;

            if (User.Identity.IsAuthenticated || !OAuthWebSecurity.TryDeserializeProviderUserId(model.ExternalLoginData, out provider, out providerUserId))
            {
                return RedirectToAction("Manage");
            }

            if (ModelState.IsValid)
            {
                // Insert a new user into the database
                using (UsersContext db = new UsersContext())
                {
                    UserProfile user = db.UserProfiles.FirstOrDefault(u => u.UserName.ToLower() == model.UserName.ToLower());
                    // Check if user already exists
                    if (user == null)
                    {
                        // Insert name into the profile table
                        db.UserProfiles.Add(new UserProfile { UserName = model.UserName });
                        db.SaveChanges();

                        OAuthWebSecurity.CreateOrUpdateAccount(provider, providerUserId, model.UserName);
                        OAuthWebSecurity.Login(provider, providerUserId, createPersistentCookie: false);

                        return RedirectToLocal(returnUrl);
                    }
                    else
                    {
                        ModelState.AddModelError("UserName", "User name(email) already exists. Please enter a different user name.");
                    }
                }
            }

            ViewBag.ProviderDisplayName = OAuthWebSecurity.GetOAuthClientData(provider).DisplayName;
            ViewBag.ReturnUrl = returnUrl;
            return View(model);
        }

        //
        // GET: /Account/ExternalLoginFailure

        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return View();
        }

        [AllowAnonymous]
        [ChildActionOnly]
        public ActionResult ExternalLoginsList(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return PartialView("_ExternalLoginsListPartial", OAuthWebSecurity.RegisteredClientData);
        }

        [ChildActionOnly]
        public ActionResult RemoveExternalLogins()
        {
            ICollection<OAuthAccount> accounts = OAuthWebSecurity.GetAccountsFromUserName(User.Identity.Name);
            List<ExternalLogin> externalLogins = new List<ExternalLogin>();
            foreach (OAuthAccount account in accounts)
            {
                AuthenticationClientData clientData = OAuthWebSecurity.GetOAuthClientData(account.Provider);

                externalLogins.Add(new ExternalLogin
                {
                    Provider = account.Provider,
                    ProviderDisplayName = clientData.DisplayName,
                    ProviderUserId = account.ProviderUserId,
                });
            }

            ViewBag.ShowRemoveButton = externalLogins.Count > 1 || OAuthWebSecurity.HasLocalAccount(WebSecurity.GetUserId(User.Identity.Name));
            return PartialView("_RemoveExternalLoginsPartial", externalLogins);
        }

        #region Helpers
        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                return RedirectToAction("Index", "Home");                
            }
        }

        public enum ManageMessageId
        {
            ChangePasswordSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
        }

        internal class ExternalLoginResult : ActionResult
        {
            public ExternalLoginResult(string provider, string returnUrl)
            {
                Provider = provider;
                ReturnUrl = returnUrl;
            }

            public string Provider { get; private set; }
            public string ReturnUrl { get; private set; }

            public override void ExecuteResult(ControllerContext context)
            {
                OAuthWebSecurity.RequestAuthentication(Provider, ReturnUrl);
            }
        }

        public static string ErrorCodeToString(MembershipCreateStatus createStatus)
        {
            // See http://go.microsoft.com/fwlink/?LinkID=177550 for
            // a full list of status codes.
            switch (createStatus)
            {
                case MembershipCreateStatus.DuplicateUserName:
                    return "User name already exists. Please enter a different user name.";

                case MembershipCreateStatus.DuplicateEmail:
                    return "A user name for that e-mail address already exists. Please enter a different e-mail address.";

                case MembershipCreateStatus.InvalidPassword:
                    return "The password provided is invalid. Please enter a valid password value.";

                case MembershipCreateStatus.InvalidEmail:
                    return "The e-mail address provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidAnswer:
                    return "The password retrieval answer provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidQuestion:
                    return "The password retrieval question provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.InvalidUserName:
                    return "The user name provided is invalid. Please check the value and try again.";

                case MembershipCreateStatus.ProviderError:
                    return "The authentication provider returned an error. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                case MembershipCreateStatus.UserRejected:
                    return "The user creation request has been canceled. Please verify your entry and try again. If the problem persists, please contact your system administrator.";

                default:
                    return "An unknown error occurred. Please verify your entry and try again. If the problem persists, please contact your system administrator.";
            }
        }
        #endregion
    }
}
