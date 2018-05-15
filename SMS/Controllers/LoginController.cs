using SMS.Helpers;
using SMS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace SMS.Controllers
{

    public class LoginController : Controller
    {
        SMSEntities db = new SMSEntities();
        // GET: Login
        public ActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
                RedirectToAction("Direct", "Login");

            ViewBag.type = new SelectList(db.Roles, "RoleID", "Role1", "T");
            return View();
        }

        public ActionResult Direct()
        {
            if (db.Teachers.Find(User.Identity.Name).RoleID.Equals("A"))
                RedirectToAction("Index", "Admin");
            else if (db.Teachers.Find(User.Identity.Name).RoleID.Equals("T"))
                RedirectToAction("Index", "Teacher");

            
            return RedirectToAction("Index", "Login");

        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();
            ViewBag.type = new SelectList(db.Roles, "RoleID", "Role1", "T");
            return View("Index");

        }

        [HttpPost]
        public ActionResult Authenticate(Login login, string returnUrl)
        {
                        

            if (ModelState.IsValid)
            {
                
                var crypt = new System.Security.Cryptography.SHA256Managed();
                var hash = new System.Text.StringBuilder();
                byte[] crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(login.password));
                foreach (byte theByte in crypto)
                {
                    hash.Append(theByte.ToString("x2"));
                }
                string password = hash.ToString();
                var user = db.Teachers.Find(login.user.ToLower());
                


                if (user != null && user.RoleID.Equals(login.type) && new SMSMembershipProvider().ValidateUser(login.user.ToLower(), password.ToUpper()))
                {
                    
                    FormsAuthentication.SetAuthCookie(user.Teacher_username, false);

                    if (Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    else
                    {
                        if (user.RoleID.Equals("A"))
                            return RedirectToAction("Index", "Admin");
                        else if (user.RoleID.Equals("T"))                            
                            return RedirectToAction("Index", "Teachers");
                    }

                }
                ViewBag.Error = "The username, password or login type is incorrect";
            }

            ViewBag.type = new SelectList(db.Roles, "RoleID", "Role1", login.type);
            return View("Index");

        }

    }
}
