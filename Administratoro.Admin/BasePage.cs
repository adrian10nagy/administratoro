
namespace Admin
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Managers;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public class BasePage : Page
    {
        public Administratoro.DAL.Associations Association
        {
            get
            {
                Administratoro.DAL.Associations result = null;
                if (Session[SessionConstants.SelectedAssociation] != null)
                {
                    result = (Administratoro.DAL.Associations)Session[SessionConstants.SelectedAssociation];
                }

                return result;
            }
        }

        public List<Administratoro.DAL.Associations> Associations
        {
            get
            {
                List<Administratoro.DAL.Associations> result = null;
                if (Session[SessionConstants.AllAssociations] != null)
                {
                    result = (List<Administratoro.DAL.Associations>)Session[SessionConstants.AllAssociations];
                }

                return result;
            }
        }

        public static List<Administratoro.DAL.Expenses> Expenses
        {
            get
            {
                return ExpensesManager.GetAllExpenses().ToList();
            }
        }

        public void RefreshAssociation()
        {
            Session[SessionConstants.SelectedAssociation] = AssociationsManager.GetById(Association.Id);
        }

        public static ListItem[] GetStairCasesAsListItemsWithExtradummyValue(Administratoro.DAL.Associations association, int controlId)
        {
            ListItem[] result = new ListItem[association.StairCases.Count + 1];
            int i = 0;

            var defaultExpense = new ListItem
            {
                Value = "dummyStair" + controlId,
                Text = "Contor pe bloc"
            };
            result[i] = defaultExpense;
            i++;

            foreach (var srairCase in association.StairCases)
            {
                var stair = new ListItem
                {
                    Value = srairCase.Id + "dummyStair" + controlId,
                    Text = "Scara " + srairCase.Nume
                };
                result[i] = stair;
                i++;
            }

            return result;
        }

        public ListItem[] GetStairCasesAsListItems()
        {
            ListItem[] result = new ListItem[Association.StairCases.Count + 1];
            int i = 0;

            var defaultExpense = new ListItem
            {
                Value = "",
                Text = "Contor pe bloc"
            };
            result[i] = defaultExpense;
            i++;

            foreach (var srairCase in Association.StairCases)
            {
                var stair = new ListItem
                {
                    Value = srairCase.Id.ToString() ,
                    Text = "Scara: " + srairCase.Nume
                };
                result[i] = stair;
                i++;
            }

            return result;
        }
    }
}