
namespace Admin.Helpers
{
    using System;

    public class SessionHelper : System.Web.UI.Page
    {
        private static SessionHelper _instance = new SessionHelper();
        public static SessionHelper Instance { get { return _instance ?? getMySession(); } }

        static SessionHelper getMySession()
        {
            if (_instance != null)
            {
                _instance = new SessionHelper();
            }

            return _instance;
        }


        //public void AddToMySession(
        //    String SessionKeyName,
        //    Object SessionItem)
        //{
        //    Session[SessionKeyName] = SessionItem;
        //}

        //public Object GetMySessionItem(String SessionKeyName)
        //{
        //    return Session[SessionKeyName] as Object;
        //}

        //public void RemoveSessionItem(string sessionKeyName)
        //{
        //    Session.Remove(sessionKeyName);
        //}
    }
}