using System;
using System.Web.Mvc;
using System.Linq;
using Newtonsoft.Json;

namespace AttendanceTracker.Controllers
{
    public class HomeController : BaseController
    {
        private readonly UInt64 _hash = 5746450151961340805;

        #region Views
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Graph()
        {
            var list = GetCurrentDataBase();
            return View(list);
        }

        public ActionResult RosterInfo()
        {
            return View();
        }

        public ActionResult RosterList()
        {
            return View();
        }

        #endregion 

        #region Verbs
        [HttpPost]
        public JsonResult ValidatePasswordSalt(string password)
        {
            var valid = "{\"isValid\": \"false\"}";
            if(CalculateHash(password) == _hash)
            {
                valid = "{\"isValid\": \"true\"}";
            }
            return Json(valid);
        }

        [HttpPost]
        public JsonResult AddPlayerRoster(string name, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                var msg = "ERROR: missing data or invalid password.";
                return Json(JsonConvert.SerializeObject(msg));
            }
            if (!( name.Length > 0 && CalculateHash(password) == _hash))
            {
                return Json("ERROR: invalid password.");
            }

            var list = GetCurrentRoster();
            var player = list.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if(player != null)
            {
                return Json("ERROR: already in list");
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

        [HttpPost]
        public JsonResult DeletePlayerRoster(string id, string password)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
            {
                var msg = "ERROR: missing data or invalid password.";
                return Json(JsonConvert.SerializeObject(msg));
            }
            if (CalculateHash(password) != _hash)
            {
                return Json("ERROR: invalid password.");
            }

            var list = GetCurrentRoster();
            var player = list.FirstOrDefault(x => x.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            _client.Delete("roster/" + id);
            var json = JsonConvert.SerializeObject(player);
            return Json(json);
        }

        [HttpPost]
        public JsonResult SubmitMethod(string name, string date, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(date) || string.IsNullOrEmpty(password))
            {
                var msg = "ERROR: missing data or invalid password.";
                return Json(JsonConvert.SerializeObject(msg));
            }
            if (!(IsValidDate(date) && name.Length > 0 && CalculateHash(password) == _hash))
            {
                return Json("ERROR: invalid password.");
            }

            // make sure name is in the roster list
            var roster = GetCurrentRoster();
            if (!(roster.Where(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList().Count > 0))
            {
                return Json("ERROR: invalid name.");
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

        [HttpPost]
        public JsonResult DeletePlayer(string id, string password)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
            {
                return Json("ERROR: invalid password");
            }

            if (CalculateHash(password) != _hash)
            {
                return Json("ERROR: invalid password");
            }

            var list = GetCurrentDataBase();
            var player = list.FirstOrDefault(x => x.Id.Equals(id, StringComparison.InvariantCultureIgnoreCase));
            _client.Delete("attendance/" + id);
            var json = JsonConvert.SerializeObject(player);
            return Json(json);
        }

        #endregion
    }
}