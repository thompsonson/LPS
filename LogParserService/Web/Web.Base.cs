using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy;
using System.Dynamic;

namespace LogParserService.Web
{
    public abstract class BaseModule : NancyModule
    {
        public dynamic baseModel = new ExpandoObject();

        public BaseModule()
        {
            SetupModelDefaults();
        }

        public BaseModule(string modulepath)
            : base(modulepath)
        {
            SetupModelDefaults();
        }

        private void SetupModelDefaults()
        {
            Before.AddItemToEndOfPipeline(ctx =>
            {
                baseModel.MasterPage = new Model.MasterPageView();
                baseModel.MasterPage.Title = "Log Parser Monitor and Reporting";
                if (Request.Cookies.ContainsKey("lastvisit"))
                {
                    baseModel.MasterPage.LastVisit = Uri.UnescapeDataString(Request.Cookies["lastvisit"]);
                }
                else
                {
                    baseModel.MasterPage.LastVisit = "No cookie value yet";
                }
                baseModel.MasterPage.IsAuthenticated = (ctx.CurrentUser != null);
                if (ctx.CurrentUser != null)
                    baseModel.MasterPage.UserName = ctx.CurrentUser.UserName;
                // TODO: Get a list of the user claims here - set in model for use in view?
                return null;
            });
            After.AddItemToEndOfPipeline(ctx =>
            {
                var now = DateTime.Now;
                ctx.Response.AddCookie("lastvisit", now.ToShortDateString() + " " + now.ToShortTimeString(), now.AddYears(1));
            });
        }
    }
}
