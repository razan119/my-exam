using Razan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Razan.Controllers
{
    public class HomeController : Controller
    {
        ContextDatabase _context = new ContextDatabase();
        public ActionResult Index()
        {
            ViewBag.Message = TempData["Name"] + "  ";
            ViewBag.Message += TempData["Email"] + "  ";
            ViewBag.Message += TempData["Password"] + "  ";
            ViewBag.Message += TempData["PasswordConfirmation"] + "  ";
            ViewBag.Message += TempData["NoUser"] + "  ";
            ViewBag.Message += TempData["ExistingUser"] + "  ";
            ViewBag.MessageLogin = TempData["IncorrectInfo"] + "  ";
            ViewBag.MessageLogin += TempData["LoginBasic"] + "  ";

            return View();
        }


        [HttpPost]
        public ActionResult RegisterUser(User newuser)
        {
            if (ModelState.IsValid)
            {
                List<User> userlist = _context.Users.Where(u => u.Email == newuser.Email).ToList();
                if (userlist.Count() > 0)
                {
                    TempData["ExistingUser"] = "A user with this email already exists";
                    return RedirectToAction("Index");
                }

                User registeringuser = new User
                {
                    Name = newuser.Name,
                    Email = newuser.Email,
                    Password = newuser.Password,
                    PasswordConfirmation = newuser.PasswordConfirmation,
                };
                try
                {
                    _context.Users.Add(registeringuser);
                    _context.SaveChanges();
                    Session["UserId"] = registeringuser.Id;
                }
                catch (Exception ex)
                {
                }

                return RedirectToAction("Index", "Meetups");
            }
            else
            {
                foreach (var MSkey in ModelState.Keys)
                {
                    var val = ModelState[MSkey];
                    foreach (var error in val.Errors)
                    {
                        var key = MSkey;
                        var EM = error.ErrorMessage;
                        TempData[key] = EM;
                    }
                }

                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public ActionResult LoginUser(string Email, string Password)
        {
            if (Password == null || Email == null)
            {
                TempData["LoginBasic"] = "Please Enter Your Email and Password";
                return RedirectToAction("Index");
            }

            User user = _context.Users.Where(u => u.Email == Email && u.Password == Password).Select(u => u).FirstOrDefault();
            if (user == null)
            {
                TempData["IncorrectInfo"] = "Information is Incorrect";
                return RedirectToAction("Index");
            }
            else
            {
                Session["UserId"] = user.Id;
                return RedirectToAction("Index", "Meetups");
            }
        }

        public ActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }

    }
}