using AttendanceTracker.Models;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;

namespace AttendanceTracker.Controllers
{
    public class AdminController : BaseController
    {
        public const string msg = "ERROR: bad data.";

        public ActionResult Index()
        {
            if (UserAuthorized())
            {
                return View();
            }

            // return unauthorized
            return Redirect("Admin/NotAuthorized");
        }

        public ActionResult RosterList()
        {
            if (UserAuthorized())
            {
                return View();
            }

            // return unauthorized
            return Redirect("Admin/NotAuthorized");
        }

        public ActionResult BossListConfiguration()
        {
            if (UserAuthorized())
            {
                var list = GetUserBosses();
                return View(list);
            }

            // return unauthorized
            return Redirect("Admin/NotAuthorized");
        }

        public ActionResult NotAuthorized()
        {
            return View();
        }


        /// <summary>
        /// Check to see if the logged in user is authorized, or false if not logged in.
        /// </summary>
        /// <returns></returns>
        private bool UserAuthorized()
        {
            var role = Session["Role"];
            if (role == null)
            {
                return false;
            }

            if (!role.Equals("Admin"))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks to see if the user is authorized, and then adds the player to the attendance sheet
        /// </summary>
        /// <param name="name"></param>
        /// <param name="date"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddPlayerAttendance(string name, string date)
        {
            if (!UserAuthorized())
            {
                return Json(JsonConvert.SerializeObject(msg));
            }
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(date) || !(IsValidDate(date)) || !(name.Length > 0))
            {
                return Json(JsonConvert.SerializeObject(msg));
            }

            // make sure name is in the roster list
            var roster = GetCurrentRoster();
            if (!(roster.Where(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList().Count > 0))
            {
                return Json(JsonConvert.SerializeObject(msg));
            }

            var dt = Convert.ToDateTime(date);
            var day = dt.DayOfWeek;
            _client.PushAsync("attendance", new
            {
                name = name,
                text = date + $" ({day})"
            }).Wait();


            var list = GetCurrentDataBase();
            var player = list.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase) &&
                                             x.Date == dt);
            var json = JsonConvert.SerializeObject(player);
            return Json(json);
        }
        
        /// <summary>
        /// Checks to see if the user is authorized, and then deletes the player from the attendance sheet
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeletePlayerAttendance(string id)
        {
            if (!UserAuthorized())
            {
                return Json(JsonConvert.SerializeObject(msg));
            }
            if (string.IsNullOrEmpty(id))
            {
                return Json(JsonConvert.SerializeObject(msg));
            }

            var list = GetCurrentDataBase();
            var player = list.FirstOrDefault(x => x.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            _client.Delete("attendance/" + id);
            var json = JsonConvert.SerializeObject(player);
            return Json(json);
        }

        /// <summary>
        /// Attempts to add a player to the roster list
        /// </summary>
        /// <param name="name"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult AddPlayerRoster(string name)
        {
            if (string.IsNullOrEmpty(name) || !UserAuthorized())
            {
                return Json(JsonConvert.SerializeObject(msg));
            }

            var list = GetCurrentRoster();
            var player = list.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (player != null)
            {
                return Json(JsonConvert.SerializeObject(msg));
            }

            _client.PushAsync("roster", new
            {
                name = name,
                text = ""
            }).Wait();

            list = GetCurrentRoster();
            player = list.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            var json = JsonConvert.SerializeObject(player);
            return Json(json);
        }

        /// <summary>
        /// Attempts to delete a player from the roster list
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult DeletePlayerRoster(string id)
        {
            if (string.IsNullOrEmpty(id) || !UserAuthorized())
            {
                return Json(JsonConvert.SerializeObject(msg));
            }

            var list = GetCurrentRoster();
            var player = list.FirstOrDefault(x => x.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            _client.Delete("roster/" + id);
            var json = JsonConvert.SerializeObject(player);
            return Json(json);
        }

        /// <summary>
        /// Update the boss configuration for the boss list table
        /// </summary>
        /// <param name="id"></param>
        /// <param name="highlighted"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateBossesConfiguration(string id, string highlighted)
        {
            var list = GetUserBosses();
            var user = list.FirstOrDefault(x => x.Id.Equals(id));
            if (user == null)
            {
                return Json(JsonConvert.SerializeObject(msg));
            }

            user.Highlighted = FromString(highlighted);
            _client.UpdateAsync($"bosses/-{id}", user).Wait();

            return Json("");
        }
    }
}