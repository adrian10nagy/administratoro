using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;

namespace Administratoro.Public.Controllers
{
    public class BaseController : Controller
    {
        public int AssociationId
        {
            get
            {
                int value;
                if (Session["assId"] != null && int.TryParse(Session["assId"].ToString(), out value))
                {
                    return value;
                }

                // log error
                return 0;
            }
        }

        public int ApartmentId
        {
            get
            {
                int value;
                if (Session["userId"] != null && int.TryParse(Session["userId"].ToString(), out value))
                {
                    return value;
                }

                // log error
                return 0;
            }
        }

    }
}
