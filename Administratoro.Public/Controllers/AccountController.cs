using System.Web.Mvc;
using Administratoro.BL.Managers;
using Administratoro.Public.Helpers;

namespace Administratoro.Public.Controllers
{
    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult LogIn()
        {
            if (Session["userId"] != null)
            {
                return RedirectToAction("LogOff");
            }

            return View();
        }

        [HttpPost]
        public ActionResult AsyncUserLogin(string userId, string userPass)
        {
            int userIdValue;
            if (!int.TryParse(userId, out userIdValue))
            {
                return Json(new { response = JSONResponse.Success });                
            }

            var user = ApartmentsManager.GetByIdAndPassword(userIdValue, userPass);

            if (user == null || user.Id == 0)
            {
                return Json(new { response = JSONResponse.IncorrectAuthentication });
            }

            Session["userId"] = user.Id;
            Session["userName"] = user.Name;
            Session["assId"] = user.id_Estate;

            return Json(new { response = JSONResponse.Success });
        }

        public ActionResult LogOff()
        {
            Session.Remove("userId");
            Session.Remove("userName");

            return RedirectToAction("Index", "Home");
        }
    }
}