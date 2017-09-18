using AttendanceTracker.Models;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace AttendanceTracker.Controllers
{
    public class AdminController : BaseController
    {
        public const string msg = "ERROR: bad data.";

        #region Views

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

        #endregion

        #region Verbs

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


            var list = GetUserAttendance();
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

            var list = GetUserAttendance();
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

        /// <summary>
        /// Update the user
        /// </summary>
        /// <param name="id"></param>
        /// <param name="highlighted"></param>
        /// <returns></returns>
        [HttpPost]
        public JsonResult UpdateUser(string id, string role)
        {
            var list = GetCurrentUsers();
            var user = list.FirstOrDefault(x => x.Id.Equals(id));
            if (user == null)
            {
                return Json(JsonConvert.SerializeObject(msg));
            }


            user.Role = role;
            _client.UpdateAsync($"users/{id}", new
            {
                Role = role
            }).Wait();

            return Json("");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Check to see if the logged in user is authorized, or false if not logged in.
        /// </summary>
        /// <returns></returns>
        private bool UserAuthorized()
        {
            var authorized = false;
            var role = Session["Role"];
            if (role == null)
            {
                return false;
            }

            if (role.Equals("Admin"))
            {
                authorized = true;
            }
            if (role.Equals("Super"))
            {
                authorized = true;
            }


            return authorized;
        }

        /// <summary>
        /// If this user's role can be changed
        /// </summary>
        /// <returns></returns>
        public bool RoleCanBeChanged(User user)
        {
            var currentUserRole = Session["Role"];
            if (currentUserRole.Equals("Super"))
            {
                return true;
            }
            if (currentUserRole.Equals("Admin"))
            {
                if (user.Role.Equals("Admin"))
                {
                    return false;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the available this user can set
        /// </summary>
        /// <param name="roles"></param>
        /// <returns></returns>
        public List<Roles> GetRolesForThisUser(IEnumerable<Roles> roles)
        {
            if (Session["Role"].Equals("Super"))
            {
                return roles.ToList();
            }
            return roles.ToList().Where(x => !x.Role.Equals("Super")).Where(x => !x.Role.Equals("Admin")).ToList();
        }

        #endregion
    }
}