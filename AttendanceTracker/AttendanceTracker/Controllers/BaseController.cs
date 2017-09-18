using AttendanceTracker.Models;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;

namespace AttendanceTracker.Controllers
{
    public class BaseController : Controller
    {
        public IFirebaseClient _client;
        public enum BooleanAliases
        {
            Yes = 1,
            No = 0
        }

        #region Constructor
        public BaseController()
        {
            var config = new FirebaseConfig()
            {
                BasePath = WebConfigurationManager.AppSettings["FirebasePath"],
                AuthSecret = WebConfigurationManager.AppSettings["FirebaseAuth"]
            };
            _client = new FirebaseClient(config);
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Get the current attendance in the database
        /// </summary>
        /// <returns></returns>
        public List<PlayerAttendance> GetUserAttendance()
        {
            var list = new List<PlayerAttendance>();
            var results = _client.Get("attendance");
            var attendance = results.Body;
            var players = attendance.Split(new string[] { "\"-" }, StringSplitOptions.None).Where(x => x.Contains("text"));
            foreach (var split in players)
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

        /// <summary>
        /// Get the current roster in the database
        /// </summary>
        /// <returns></returns>
        public List<PlayerRoster> GetCurrentRoster()
        {
            var list = new List<PlayerRoster>();
            var results = _client.Get("roster");
            var attendance = results.Body;
            var players = attendance.Split(new string[] { "\"-" }, StringSplitOptions.None).Where(x => x.Contains("text"));
            foreach (var split in players)
            {
                // the id of each player in db
                var id = split.Split(new string[] { "\":{" }, StringSplitOptions.None)[0];

                // get the name/date now
                var newSplit = split.Replace(id, string.Empty).Replace("name", string.Empty);
                var player = newSplit.Split(new string[] { "text" }, StringSplitOptions.None);
                var name = player[0].Substring(7, player[0].Length - 10);
                var playerRoster = new PlayerRoster()
                {
                    Id = $"-{id}",
                    Name = name,
                };

                list.Add(playerRoster);
            }

            return list.OrderBy(x => x.Name).ToList();
        }

        /// <summary>
        /// Get the current users in the database
        /// </summary>
        /// <returns></returns>
        public List<User> GetCurrentUsers()
        {
            var list = new List<User>();
            var results = _client.Get("users");
            var users = results.Body;
            var accounts = users.Split(new string[] { "\"-" }, StringSplitOptions.None).Where(x => x.Contains("password"));
            foreach (var split in accounts)
            {
                // the id of each player in db
                var id = split.Split(new string[] { "\":{" }, StringSplitOptions.None)[0];

                // get the name/date now
                var split2 = ("{" + split.Split(new string[] { "\":{" }, StringSplitOptions.None)[1]);
                var split3 = split2.Substring(0, split2.Length - 1);
                var user = Newtonsoft.Json.JsonConvert.DeserializeObject<User>(split3);
                user.Id = $"-{id}";
                if (string.IsNullOrEmpty(user.Role))
                {
                    user.Role = "None";
                }
                list.Add(user);
            }
            return list;
        }

        /// <summary>
        /// Get the current Roles in the database
        /// </summary>
        /// <returns></returns>
        public List<Roles> GetUserRoles()
        {
            var list = new List<Roles>();
            var results = _client.Get("roles");
            var roles = results.Body;
            var roleOptions = roles.Split(new string[] { "\"-" }, StringSplitOptions.None).Where(x => x.Contains("Role"));
            foreach (var split in roleOptions)
            {
                // the id of each player in db
                var id = split.Split(new string[] { "\":{" }, StringSplitOptions.None)[0];

                // get the name/date now
                var split2 = ("{" + split.Split(new string[] { "\":{" }, StringSplitOptions.None)[1]);
                var split3 = split2.Substring(0, split2.Length - 1);
                var role = Newtonsoft.Json.JsonConvert.DeserializeObject<Roles>(split3);
                role.Id = $"-{id}";
                list.Add(role);
            }
            return list;
        }

        /// <summary>
        /// Get the boss list that users have specified they need
        /// </summary>
        /// <returns></returns>
        public List<BossesNeeded> GetUserBosses()
        {
            var list = new List<BossesNeeded>();
            var results = _client.Get("bosses");
            var userBosses = results.Body;
            var splits = userBosses.Split(new string[] { "\"-" }, StringSplitOptions.None).Where(x => x.Contains("User"));
            foreach (var split in splits)
            {
                // the id of each player in db
                var id = split.Split(new string[] { "\":{" }, StringSplitOptions.None)[0];

                // split to get teh JSON of the rest
                var split2 = ("{" + split.Split(new string[] { "\":{" }, StringSplitOptions.None)[1]);
                var split3 = split2.Substring(0, split2.Length - 1);
                var bosses = Newtonsoft.Json.JsonConvert.DeserializeObject<BossesNeeded>(split3);
                bosses.Id = id;
                list.Add(bosses);
            }

            return list;
        }
        
        /// <summary>
        /// The total number of dates logged that have been logged, should equal the number of raids
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public int GetTotalDatesLogged(List<PlayerAttendance> list)
        {
            return list.Select(x => x.Date).Distinct().ToList().Count;
        }

        /// <summary>
        /// Occurrences that someone's name appears in the attendance list
        /// </summary>
        /// <param name="name"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        public int GetOccurrences(string name, List<PlayerAttendance> list)
        {
            return list.Where(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase)).ToList().Count;
        }

        /// <summary>
        /// Determine if the date is valid
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public bool IsValidDate(string date)
        {
            DateTime dt;
            return DateTime.TryParse(date, out dt);
        }

        /// <summary>
        /// Calculate an MD5 hash
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public string CalculateMD5Hash(string input)

        {
            // step 1, calculate MD5 hash from input
            var md5 = MD5.Create();
            byte[] inputBytes = Encoding.ASCII.GetBytes(input);
            byte[] hash = md5.ComputeHash(inputBytes);
            var sb = new StringBuilder();
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            return sb.ToString();

        }

        /// <summary>
        /// Take the yes/no values and parse to true/false
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public static bool FromString(string str)
        {
            if (!Enum.IsDefined(typeof(BooleanAliases), str)){
                return false;
            }
            return Convert.ToBoolean(Enum.Parse(typeof(BooleanAliases), str));
        }

        #endregion
    }
}