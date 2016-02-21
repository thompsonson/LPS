using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Nancy.ErrorHandling;
using Nancy;
using System.IO;
using Nancy.ViewEngines;

namespace LogParserService.Web
{
    public class PageNotFoundHandler : DefaultViewRenderer, IStatusCodeHandler 
    { 
        public PageNotFoundHandler(IViewFactory factory) 
            : base(factory) 
        { 

        } 
        
        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context) 
        { 
            return statusCode == HttpStatusCode.NotFound; 
        } 
        
        public void Handle(HttpStatusCode statusCode, NancyContext context) 
        {
            var response = RenderView(context, "Web\\Views\\404.html"); 
            response.StatusCode = HttpStatusCode.NotFound; 
            context.Response = response; 
        } 
    }

    public class ForbiddenHandler : DefaultViewRenderer, IStatusCodeHandler
    {
        public ForbiddenHandler(IViewFactory factory)
            : base(factory)
        {

        }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return statusCode == HttpStatusCode.Forbidden;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            var response = RenderView(context, "Web\\Views\\403.html");
            response.StatusCode = HttpStatusCode.Forbidden;
            context.Response = response;
        }
    }


    public class HTTPExceptionHandler : DefaultViewRenderer, IStatusCodeHandler
    {
        public HTTPExceptionHandler(IViewFactory factory)
            : base(factory)
        {

        }

        public bool HandlesStatusCode(HttpStatusCode statusCode, NancyContext context)
        {
            return statusCode == HttpStatusCode.InternalServerError;
        }

        public void Handle(HttpStatusCode statusCode, NancyContext context)
        {
            // TODO: if possible catch the exception and log it
            var response = RenderView(context, "Web\\Views\\500.html");
            response.StatusCode = HttpStatusCode.NotFound;
            context.Response = response;
        }
    }
}
