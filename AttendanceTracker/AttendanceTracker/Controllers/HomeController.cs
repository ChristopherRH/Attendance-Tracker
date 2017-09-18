using System;
using System.Web.Mvc;
using System.Linq;
using Newtonsoft.Json;

namespace AttendanceTracker.Controllers
{
    public class HomeController : BaseController
    {
        #region Views

        public ActionResult Index()
        {
            var list = GetUserAttendance();
            return View(list);
        }

        public ActionResult RosterInfo()
        {
            return View();
        }

        #endregion 
    }
}