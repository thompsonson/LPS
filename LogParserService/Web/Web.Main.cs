using System;
using Nancy;
using Nancy.Authentication.Forms;
using Nancy.Security;

namespace LogParserService.Web
{
    public class MainModule : BaseModule
    {
        public MainModule()
        {
            Get["/"] = x =>
            {
                //Model.index = new indexModel();
                //Model.index.HelloWorld = "HelloWorld!";
                //Model.index = new { Helloworld = "Hello World" };
                return View["Web\\Views\\index.html", baseModel];
                //return Response.AsRedirect("/monitor");
            };

            Get["/login"] = x =>
            {
                //Model.login = new loginModel() { Error = this.Request.Query.error.HasValue, ReturnUrl = this.Request.Url };
                baseModel.login = new { Error = this.Request.Query.error.HasValue, ReturnUrl = this.Request.Url };
                return View["Web\\Views\\Login.html", baseModel];
            };

            Post["/login"] = x =>
            {
                var userGuid = UserMapper.ValidateUser((string)this.Request.Form.Username, (string)this.Request.Form.Password);

                if (userGuid == null)
                {
                    return Response.AsRedirect("~/login?error=true&username=" + (string)this.Request.Form.Username);
                }

                DateTime? expiry = null;
                if (this.Request.Form.RememberMe.HasValue)
                {
                    expiry = DateTime.Now.AddDays(7);
                }

                return this.LoginAndRedirect(userGuid.Value, expiry);
            };

            Get["/logout"] = x =>
            {
                return this.LogoutAndRedirect("/");
            };

        }
    }
}
