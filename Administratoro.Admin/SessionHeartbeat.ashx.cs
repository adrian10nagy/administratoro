
namespace Admin
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.SessionState;

    /// <summary>
    /// Summary description for SessionHeartbeat
    /// </summary>
    public class SessionHeartbeat : IHttpHandler, IRequiresSessionState
    {

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        public void ProcessRequest(HttpContext context)
        {
            context.Session["Heartbeat"] = DateTime.Now;
        }
    }
}