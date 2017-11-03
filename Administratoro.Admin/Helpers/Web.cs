using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Admin.Helpers
{
    interface IWeb
    {
        SessionHelper Session { get; }
    }

    public class Web : IWeb
    {
        private static Web _instance = new Web();
        public static Web Instance { get { return _instance ?? getInstance(); } }

        static Web getInstance()
        {
            return new Web();
        }

        public SessionHelper Session { get { return new SessionHelper(); } }
    }
}