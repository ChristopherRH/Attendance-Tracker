﻿using System;
using FireSharp.Interfaces;
using System.Web.Mvc;
using FireSharp.Config;
using FireSharp;
using System.Linq;
using AttendanceTracker.Models;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace AttendanceTracker.Controllers
{
    public class HomeController : Controller
    {
        private readonly IFirebaseClient _client;
        private readonly UInt64 _hash = 5746450151961340805;

        #region Constructor
        public HomeController()
        {
            var config = new FirebaseConfig()
            {
                BasePath = "https://attendance-7f6fe.firebaseio.com/"
            };
            _client = new FirebaseClient(config);
        }

        #endregion

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

        #endregion 

        #region Verbs

        [HttpPost]
        public JsonResult SubmitMethod(string name, string date, string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(date) || string.IsNullOrEmpty(password))
            {
                var msg = "missing data or invalid password.";
                return Json(JsonConvert.SerializeObject(msg));
            }
            if (!(IsValidDate(date) && name.Length > 0 && CalculateHash(password) == _hash))
            {
                return Json("invalid password.");
            }

            var dt = Convert.ToDateTime(date);
            var day = dt.DayOfWeek;
            _client.PushAsync("", new
            {
                name = name,
                text = date + $" ({day})"
            }).Wait();

            return Json("");
        }

        [HttpPost]
        public JsonResult DeletePlayer(string id, string password)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(password))
            {
                return Json("invalid password");
            }

            if (CalculateHash(password) != _hash)
            {
                return Json("invalid password");
            }

            _client.Delete(id);

            return Json("");
        }

        #endregion

        #region Public Methods
        public List<PlayerAttendance> GetCurrentDataBase()
        {
            var list = new List<PlayerAttendance>();
            var results = _client.Get("");
            var attendance = results.Body;
            var players = attendance.Split(new string[] { "\"-" }, StringSplitOptions.None).Where(x => x.Contains("text"));
            foreach(var split in players)
            {
                // the id of each player in db
                var id = split.Split(new string[] { "\":{" }, StringSplitOptions.None)[0];

                // get the name/date now
                var newSplit = split.Replace(id, string.Empty).Replace("name", string.Empty);
                var player = newSplit.Split(new string[] { "text" }, StringSplitOptions.None);
                var name = player[0].Substring(7, player[0].Length - 10);
                var dateSplit = player[1].Split(new string[] { "(" }, StringSplitOptions.None);
                var date = dateSplit[0].Substring(3, dateSplit[0].Length - 3);
                var playerAttendance = new PlayerAttendance()
                {
                    Id = $"-{id}",
                    Name = name,
                    Date = Convert.ToDateTime(date)
                };

                list.Add(playerAttendance);
            }

            return list;
        }

        public int GetTotalDatesLogged(List<PlayerAttendance> list)
        {
            return list.Select(x => x.Date).Distinct().ToList().Count;
        }

        public int GetOccurrences(string name, List<PlayerAttendance> list)
        {
            return list.Where(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList().Count;
        }

        public bool IsValidDate(string date)
        {
            DateTime dt;
            return DateTime.TryParse(date, out dt);
        }

        static UInt64 CalculateHash(string read)
        {
            UInt64 hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
        }

        #endregion
    }
}