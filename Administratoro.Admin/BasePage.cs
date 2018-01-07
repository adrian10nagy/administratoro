
namespace Admin
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using System.Web.UI;

    public class BasePage : Page
    {
        public Estates Estate
        {
            get
            {
                Estates result = null;
                if (Session[SessionConstants.SelectedEstate] != null)
                {
                    result = (Estates)Session[SessionConstants.SelectedEstate];
                }
                return result;
            }
        }

        public List<Administratoro.DAL.Expenses> Expenses
        {
            get
            {
                return ExpensesManager.GetAllExpensesAsList();
            }
        }

    }
}