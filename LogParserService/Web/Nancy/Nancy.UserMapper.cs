using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy.Authentication.Forms;
using Nancy.Security;
using Nancy;

namespace LogParserService.Web
{
    public class UserMapper : IUserMapper
    {
        private static List<Tuple<string, string, string[], Guid>> users = new List<Tuple<string, string, string[], Guid>>();

        static UserMapper()
        {
            users.Add(new Tuple<string, string, string[], Guid>("admin", "password", new[] { "user", "admin"}, new Guid("55E1E49E-B7E8-4EEA-8459-7A906AC4D4C0")));
            users.Add(new Tuple<string, string, string[], Guid>("user", "password", new[] { "user", "viewMonitor", "viewJob" }, new Guid("56E1E49E-B7E8-4EEA-8459-7A906AC4D4C1")));
            users.Add(new Tuple<string, string, string[], Guid>("suser", "password", new[] { "user", "editMonitor", "viewJob" }, new Guid("AC9836E7-4563-4BC2-AABB-D1B1EE0ABE8D")));
        }

        public IUserIdentity GetUserFromIdentifier(Guid identifier, NancyContext context)
        {
            var userRecord = users.Where(u => u.Item4 == identifier).FirstOrDefault();

            return userRecord == null 
                ? null 
                : new UserIdentity { UserName = userRecord.Item1 , Claims = userRecord.Item3};

        }

        public static Guid? ValidateUser(string username, string password)
        {
            var userRecord = users.Where(u => u.Item1 == username && u.Item2 == password).FirstOrDefault();

            if (userRecord == null) return null;

            return userRecord.Item4;
        }

    }
}
