using AttendanceTracker.Models;
using FireSharp;
using FireSharp.Config;
using FireSharp.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.Mvc;

namespace AttendanceTracker.Controllers
{
    public class AccountController : Controller
    {
        private readonly IFirebaseClient _client;

        #region Constructor
        public AccountController()
        {
            var config = new FirebaseConfig()
            {
                BasePath = "https://attendance-7f6fe.firebaseio.com/",
                AuthSecret = "ZZNsXOiCbqIYvy6HYQOQBUrrnzumJsv163EGqaA0"
            };
            _client = new FirebaseClient(config);
        }
        #endregion

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            return View();
        }

        public ActionResult Bosses()
        {
            return View();
        }

        [HttpPost]
        public JsonResult CreateAccount(string name, string password, string passwordVerify)
        {
            // not valid
            var msg = "ERROR: missing data: Username or matching passwords.";
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(passwordVerify))
            {
                return Json(msg);
            }
            if (password != passwordVerify)
            {
                return Json(msg);
            }

            // valid, hash this stuff and send it to the db
            var passwordHash = CalculateMD5Hash(password);

            // create the account
            var list = GetCurrentUsers();
            var account = list.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (account != null)
            {
                return Json("ERROR: already in list");
            }

            _client.PushAsync("users", new
            {
                name = name,
                password = passwordHash
            }).Wait();

            return Json("");
        }
        [HttpPost]
        public JsonResult Login(string name, string password)
        {
            // not valid
            var msg = "ERROR: missing data: Username or matching passwords.";
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password))
            {
                return Json(msg);
            }

            // valid, hash this stuff and send it to the db
            var passwordHash = CalculateMD5Hash(password);

            // create the account
            var list = GetCurrentUsers();
            var account = list.FirstOrDefault(x => x.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));
            if (account == null)
            {
                return Json("ERROR: account not found");
            }

            if (!account.Password.Equals(passwordHash))
            {
                return Json("ERROR: invalid password");
            }

            // set the username for this session
            Session["UserName"] = name;
            return Json("");
        }

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
                var newSplit = split.Replace(id, string.Empty).Replace("name", string.Empty);
                var player = newSplit.Split(new string[] { "password" }, StringSplitOptions.None);
                var name = player[0].Substring(7, player[0].Length - 10);
                var passwordSplit = player[1].Split(new string[] { "(" }, StringSplitOptions.None);
                var password = passwordSplit[0].Substring(3, passwordSplit[0].Length - 6);
                var user = new User()
                {
                    Id = $"-{id}",
                    Name = name,
                    Password = password
                };

                list.Add(user);
            }

            return list;
        }
    }
}