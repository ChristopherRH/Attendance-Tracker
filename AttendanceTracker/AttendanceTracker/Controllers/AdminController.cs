using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AttendanceTracker.Controllers
{
    public class AdminController : BaseController
    {
        public ActionResult Index()
        {
            var username = Session["UserName"];
            if (username == null)
            {
                return Redirect("Admin/NotAuthorized");
            }

            var list = GetCurrentUsers();
            var user = list.FirstOrDefault(x => x.Name.Equals(username.ToString(), StringComparison.InvariantCultureIgnoreCase));
            if (user == null)
            {
                return Redirect("Admin/NotAuthorized");
            }

            if (user.Role.Equals("Admin"))
            {
                return View();
            }

            // return unauthorized
            return Redirect("Admin/NotAuthorized");
        }

        public ActionResult NotAuthorized()
        {
            return View();
        }
    }
}