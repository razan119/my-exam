using Razan.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Razan.Controllers
{
    public class MeetupsController : Controller
    {
        ContextDatabase _context = new ContextDatabase();

        public ActionResult Index()
        {
            int? UserId = (int?)Session["UserId"];
            if (UserId == null)
            {
                return RedirectToAction("Index", "Home");
            }
            List<Activity> activitis = _context.Activities.OrderBy(a => a.Date).ToList();
            for (int i = 0; i < activitis.Count; i++)
            {
                if (activitis[i].Date < DateTime.Now)
                {
                    activitis.RemoveAt(i);
                    i--;
                }
            }

            int id = (int)Session["UserId"];
            User user = _context.Users.Where(u => u.Id == id).FirstOrDefault();
            ViewBag.userName = user.Name;
            ViewBag.Message = TempData["Conflict"];

            return View(activitis);

        }

        public ActionResult Activity(int id)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            Activity activity = _context.Activities.Where(w => w.Id == id).FirstOrDefault();

            return View(activity);
        }

        public ActionResult New()
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Message = TempData["Future"] + "  ";
            ViewBag.Message += TempData["Title"] + "  ";
            ViewBag.Message += TempData["Date"] + "  ";
            ViewBag.Message += TempData["Duration"] + "  ";
            ViewBag.Message += TempData["Description"] + "  ";
            return View();
        }


        [HttpPost]
        public ActionResult CreateActivity(Models.Activity newactivity)
        {
            if (Session["UserId"] == null)
            {
                return RedirectToAction("Index", "Home");
            }

            if (ModelState.IsValid)
            {
                if (Convert.ToDateTime(newactivity.Date) > DateTime.Now)
                {
                    Activity creatingactivity = new Activity()
                    {
                        CreatorId = (int)Session["UserId"],
                        Title = newactivity.Title,
                        Date = newactivity.Date,
                        Duration = newactivity.Duration,
                        Units = newactivity.Units,
                        Description = newactivity.Description,
                    };
                    try
                    {
                        _context.Activities.Add(creatingactivity);
                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {

                    }
                    return RedirectToAction("Activity", new { id = creatingactivity.Id });
                }
                else
                {
                    TempData["Future"] = "Must enter a Time and Date in the future";
                    return RedirectToAction("New", "Meetups");
                }
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

                return RedirectToAction("New", "Meetups");
            }

        }

        [HttpGet]
        public ActionResult Delete(int id)
        {
            Models.Activity activity = _context.Activities.Where(w => w.Id == id).SingleOrDefault();
            _context.Activities.Remove(activity);
            _context.SaveChanges();
            return RedirectToAction("Index", "Meetups");
        }

        [HttpGet]
        public ActionResult Join(int id)
        {
            Activity currentActivity = _context.Activities.Where(a => a.Id == id).FirstOrDefault();
            List<Activity> activities = _context.Activities.ToList();
            foreach (var activity in activities)
            {
                if (activity.Id != currentActivity.Id)
                {
                    foreach (var guest in activity.Guests)
                    {
                        if (guest.User.Id == (int)Session["UserId"])
                        {
                            TempData["Conflict"] = "permission denied";
                            return RedirectToAction("Index", "Meetups");
                        }
                    }
                }
            }

            Guest newguest = new Guest()
            {
                UserId = (int)Session["UserId"],
                ActivityId = currentActivity.Id,
            };

            try
            {
                _context.Guests.Add(newguest);
                _context.SaveChanges();
            }
            catch (Exception ex)
            {

            }
            return RedirectToAction("Index", "Meetups");

        }

        [HttpGet]
        public ActionResult Leave(int id)
        {
            int UserId = (int)Session["UserId"];
            Guest guest = _context.Guests.Where(l => l.Activity.Id == id && l.User.Id == UserId).SingleOrDefault();
            _context.Guests.Remove(guest);
            _context.SaveChanges();
            return RedirectToAction("Index", "Meetups");
        }

    }
}