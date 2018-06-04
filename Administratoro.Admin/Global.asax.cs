
namespace Admin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.Optimization;
    using System.Web.Routing;
    using System.Web.Security;
    using System.Web.SessionState;

    public class Global : HttpApplication
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        void Application_Start(object sender, EventArgs e)
        {
            // Code that runs on application startup
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        void Application_Error(object sender, EventArgs e)
        {
            // Code that runs when an unhandled error occurs

            // Get the exception object.
            Exception exc = Server.GetLastError();
            var stacktrace = string.Empty;
            if (exc != null && exc.InnerException != null && exc.InnerException.StackTrace != null)
            {
                stacktrace = exc.InnerException.StackTrace;
            }
            else if(exc != null && exc.StackTrace != null)
            {
                stacktrace = exc.StackTrace;
            }
            else if (exc != null && exc.Message!= null)
            {
                stacktrace = exc.Message;
            }

            //LogsManager.LogError(new DAL.Entities.ErrorLog
            //{
            //    Message = "Inner message: " + exc.InnerException + " StackTace: " + stacktrace,
            //    CreatedDate = DateTime.Now
            //});

            Response.Redirect("~/Error.aspx");
        }
    }
}