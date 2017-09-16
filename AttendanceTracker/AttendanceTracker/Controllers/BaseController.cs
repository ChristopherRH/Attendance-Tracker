﻿using AttendanceTracker.Models;
using FireSharp.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;

namespace AttendanceTracker.Controllers
{
    public class BaseController : Controller
    {
        public IFirebaseClient _client;

        #region Public Methods
        public List<PlayerAttendance> GetCurrentDataBase()
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

        public static UInt64 CalculateHash(string read)
        {
            UInt64 hashedValue = 3074457345618258791ul;
            for (int i = 0; i < read.Length; i++)
            {
                hashedValue += read[i];
                hashedValue *= 3074457345618258799ul;
            }
            return hashedValue;
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

        public List<BossesNeeded> GetUserBosses()
        {
            var needs = "true";
            var list = new List<BossesNeeded>();
            var results = _client.Get("bosses");
            var userBosses = results.Body;
            var splits = userBosses.Split(new string[] { "\"-" }, StringSplitOptions.None).Where(x => x.Contains("User"));
            foreach (var split in splits)
            {
                // the id of each player in db
                var id = split.Split(new string[] { "\":{" }, StringSplitOptions.None)[0];
                var goroth = split.Substring(split.IndexOf("Goroth") + "Goroth".Length, 6).IndexOf(needs) != -1;
                var di = split.Substring(split.IndexOf("DI") + "DI".Length, 6).IndexOf(needs) != -1;
                var harj = split.Substring(split.IndexOf("Harjatan") + "Harjatan".Length, 6).IndexOf(needs) != -1;
                var sisters = split.Substring(split.IndexOf("Sisters") + "Sisters".Length, 6).IndexOf(needs) != -1;
                var host = split.Substring(split.IndexOf("Host") + "Host".Length, 6).IndexOf(needs) != -1;
                var mistress = split.Substring(split.IndexOf("Mistress") + "Mistress".Length, 6).IndexOf(needs) != -1;
                var maiden = split.Substring(split.IndexOf("Maiden") + "Maiden".Length, 6).IndexOf(needs) != -1;
                var fa = split.Substring(split.IndexOf("FallenAvatar") + "FallenAvatar".Length, 6).IndexOf(needs) != -1;
                var kj = split.Substring(split.IndexOf("Kiljaeden") + "Kiljaeden".Length, 6).IndexOf(needs) != -1;
                var player = split.Split(new string[] { "User" }, StringSplitOptions.None);
                var name = player[1].Substring(3, player[1].Length - 6);
                               
                var bossNeeds = new BossesNeeded()
                {
                    Id = $"-{id}",
                    Goroth = goroth,
                    Di = di,
                    Harjatan = harj,
                    Sisters = sisters,
                    Host = host,
                    Mistress = mistress,
                    Maiden = maiden,
                    FallenAvatar = fa,
                    Kiljaeden = kj,
                    User = name
                };

                list.Add(bossNeeds);
            }

            return list;
        }

        #endregion
    }
}