
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Admin.Helpers.Constants;
using Administratoro.BL.Constants;
using Administratoro.BL.Extensions;
using Administratoro.BL.Managers;
using Administratoro.DAL;

namespace Admin.Apartments
{
    public partial class Add : BasePage
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            int? apId = null;

            if (!string.IsNullOrEmpty(Request["apartmentid"]))
            {
                var apartmentid = Request["apartmentid"].ToNullableInt();
                if (apartmentid != null && apartmentid != 0)
                {
                    apId = apartmentid;
                }
            }

            PopulateCounters(Association, apId);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                PopulateStairCase(Association);
                if (!string.IsNullOrEmpty(Request["apartmentid"]))
                {
                    var apartmentid = Request["apartmentid"].ToNullableInt();
                    if (apartmentid != null && apartmentid != 0)
                    {
                        var apartment = ApartmentsManager.GetById(apartmentid.Value);
                        if (apartment != null)
                        {
                            userName.Value = apartment.Name;
                            userExtraInfo.Value = apartment.ExtraInfo;
                            userPhone.Value = apartment.Telephone;
                            userEmail.Value = apartment.Email;
                            userDependents.Value = apartment.Dependents.ToString();
                            apartmentCota.Value = apartment.CotaIndiviza.ToString();
                            userNr.Value = apartment.Number.ToString();
                            btnSave.Text = "Actualizează datele";
                            lblUserId.Text = Request["apartmentid"];
                            userStairCase.SelectedValue = (apartment.Id_StairCase != null) ? apartment.Id_StairCase.ToString() : null;
                            userHeatHelp.SelectedValue = (apartment.HasHeatHelp.HasValue && apartment.HasHeatHelp.Value ? "1" : "0");
                        }
                        else
                        {
                            Response.Redirect("~/Error.aspx?errorId=" + ErrorMessages.UserInvalid);
                        }
                    }
                    else
                    {
                        Response.Redirect("~/Error.aspx?errorId=" + ErrorMessages.UserInvalid);
                    }
                }
                else
                {
                    PopulateApartmentLogic(Association);
                }
            }


        }

        private void PopulateCounters(Administratoro.DAL.Associations association, int? apartmentId)
        {
            estateCounters.Visible = false;

            if (association.HasStaircase)
            {
                Administratoro.DAL.Apartments apartment = null;
                if (apartmentId.HasValue)
                {
                    apartment = ApartmentsManager.GetById(apartmentId.Value);
                }

                Panel headerPanel = new Panel();
                Label lbExpense = new Label
                {
                    Text = "Cheltuială",
                    CssClass = "col-md-4 col-xs-4 countersTableHeader"
                };

                Label lbName = new Label
                {
                    Text = "Contor alocat",
                    CssClass = "col-md-4 col-xs-4 countersTableHeader"
                };

                Label lbNrCountersPerApartment = new Label
                {
                    Text = "Numărul de contoare în apartament ",
                    CssClass = "col-md-4 col-xs-4 countersTableHeader"
                };

                headerPanel.Controls.Add(lbExpense);
                headerPanel.Controls.Add(lbName);
                headerPanel.Controls.Add(lbNrCountersPerApartment);

                estateCounters.Controls.Add(headerPanel);

                IEnumerable<Administratoro.DAL.Expenses> expenses = ExpensesManager.GetAllExpenses();
                foreach (var expense in expenses)
                {
                    PopulateCountersData(association, expense, apartment);
                }
            }
        }

        private void PopulateCountersData(Administratoro.DAL.Associations association, Administratoro.DAL.Expenses expense, Administratoro.DAL.Apartments apartment)
        {
            Panel mainPanel = new Panel();

            Label lb = new Label
            {
                Text = expense.Name,
                CssClass = "col-md-4 col-xs-4"
            };

            DropDownList drp = new DropDownList()
            {
                CssClass = "col-md-4 col-xs-4"
            };

            ListItem defaultNull = new ListItem
            {
                Value = null,
                Text = " -Fără contor- "
            };
            drp.Items.Add(defaultNull);

            AssociationCountersApartment ac = null;
            var assCounters = association.AssociationCounters.Where(c => c.Id_Expense == expense.Id).ToList();

            if (apartment != null)
            {
                foreach (var assCounter in assCounters)
                {
                    AssociationCountersApartment assApCounter = ApartmentCountersManager.Get(apartment.Id, assCounter.Id);
                    if (assApCounter != null)
                    {
                        ac = assApCounter;
                    }
                }
            }

            if (assCounters.Count != 0)
            {
                Label lbApCounter = new Label
                {
                    Text = (ac != null) ? ac.Id.ToString() : string.Empty,
                    Visible = false
                };

                int i = 0;
                foreach (var counter in assCounters)
                {
                    ListItem li = new ListItem
                    {
                        Text = counter.Value,
                        Value = counter.Id.ToString(),
                        Selected = (ac != null && ac.Id_Counters == counter.Id) || (apartment == null && i == 0)
                    };
                    drp.Items.Add(li);

                    i++;
                }

                TextBox txtNrOfCounters = new TextBox
                {
                    CssClass = "col-md-4 col-xs-4",
                    AutoCompleteType = AutoCompleteType.Disabled
                };

                if (ac == null)
                {
                    txtNrOfCounters.Text = "0";
                }
                else if (ac.CountersInsideApartment.HasValue)
                {
                    txtNrOfCounters.Text = ac.CountersInsideApartment.ToString();
                }
                else
                {
                    txtNrOfCounters.Text = "1";
                }

                mainPanel.Controls.Add(lbApCounter);
                mainPanel.Controls.Add(lb);
                mainPanel.Controls.Add(drp);
                mainPanel.Controls.Add(txtNrOfCounters);
                mainPanel.Controls.Add(new LiteralControl("<br />"));
                mainPanel.Controls.Add(new LiteralControl("<br />"));
                estateCounters.Visible = true;
                estateCounters.Controls.Add(mainPanel);
            }
        }

        private void PopulateApartmentLogic(Administratoro.DAL.Associations association)
        {
            if (association.CotaIndivizaAparments.HasValue)
            {
                apartmentCota.Value = association.CotaIndivizaAparments.Value.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void PopulateStairCase(Administratoro.DAL.Associations association)
        {
            var staircases = StairCasesManager.GetAllByAssociation(association.Id);
            if (association.HasStaircase)
            {
                ListItem defaultNull = new ListItem
                {
                    Value = null
                };
                userStairCase.Items.Add(defaultNull);
                divStaircase.Visible = true;

                foreach (var stairCase in staircases)
                {
                    ListItem li = new ListItem
                    {
                        Text = stairCase.Nume,
                        Value = stairCase.Id.ToString()
                    };
                    userStairCase.Items.Add(li);
                }
            }
        }

        protected void btnSave_Click(object sender, EventArgs e)
        {
            if (!this.ValidateInputs())
            {
                return;
            }

            var cota = apartmentCota.Value.ToNullableDecimal();
            var apartment = new Administratoro.DAL.Apartments
            {
                Name = userName.Value,
                Dependents = userDependents.Value.ToNullableInt().Value,
                ExtraInfo = userExtraInfo.Value,
                CotaIndiviza = cota ?? 0,
                Number = userNr.Value.ToNullableInt().Value,
                Telephone = userPhone.Value,
                Email = userEmail.Value,
                CreatedDate = DateTime.Now,
                id_Estate = Association.Id,
                Password = "dasd",
                Id_StairCase = userStairCase.SelectedValue.ToNullableInt(),
                HasHeatHelp = userHeatHelp.SelectedIndex == 1
            };

            if (!string.IsNullOrEmpty(lblUserId.Text) && lblUserId.Text.ToNullableInt() != 0)
            {
                apartment.Id = lblUserId.Text.ToNullableInt().Value;
                ApartmentsManager.Update(apartment);
            }
            else
            {
                apartment = ApartmentsManager.Add(apartment);
                lblStatus.Text = FlowMessages.ApartmentAddSuccess;
                lblStatus.CssClass = "SuccessBox";
            }

            ProcessSaveCounters(apartment);

            var association = AssociationsManager.GetById(Association.Id);
            Session[SessionConstants.SelectedAssociation] = association;

            Response.Redirect("~/Apartments/Manage.aspx?Message=UserUpdatedSuccess");
        }

        private void ProcessSaveCounters(Administratoro.DAL.Apartments apartment)
        {
            List<AssociationCountersApartment> counters = GetAllCounters(apartment);
            AssociationCountersManager.AddOrUpdateAssociationCountersApartment(counters);
        }

        private List<AssociationCountersApartment> GetAllCounters(Administratoro.DAL.Apartments apartment)
        {
            var result = new List<AssociationCountersApartment>();

            for (int i = 0; i < estateCounters.Controls.Count; i++)
            {
                if (estateCounters.Controls[i] is Panel)
                {
                    var mainPanel = (Panel)estateCounters.Controls[i];

                    if (mainPanel.Controls.Count > 2 && mainPanel.Controls[0] is Label
                        && mainPanel.Controls[2] is DropDownList)
                    {
                        var apCounterId = (Label)mainPanel.Controls[0];
                        var counterId = (DropDownList)mainPanel.Controls[2];
                        var counterNr = (TextBox)mainPanel.Controls[3];

                        int apCntId;
                        int apCntIdResult = -1;
                        if (int.TryParse(apCounterId.Text, out apCntId))
                        {
                            apCntIdResult = apCntId;
                        }

                        int cntId;
                        int cntIdResult = -1;
                        if (int.TryParse(counterId.Text, out cntId))
                        {
                            cntIdResult = cntId;
                        }

                        int nrCountersId;
                        int? nrCountersIdResult = null;
                        if (int.TryParse(counterNr.Text, out nrCountersId))
                        {
                            nrCountersIdResult = nrCountersId;
                        }

                        var counter = new AssociationCountersApartment
                        {
                            Id = apCntIdResult,
                            Id_Counters = cntIdResult,
                            Id_Apartment = apartment.Id,
                            CountersInsideApartment = nrCountersIdResult
                        };

                        result.Add(counter);
                    }
                }
            }

            return result;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Apartments/Manage.aspx");
        }

        private bool ValidateInputs()
        {
            bool isValid = true;

            userDependents.Attributes.CssStyle.Add("color", "");

            if (!userDependents.Value.ToNullableInt().HasValue)
            {
                userDependents.Attributes.CssStyle.Add("color", "red");
                isValid = false;
            }

            userName.Attributes.CssStyle.Add("color", "");

            if (string.IsNullOrEmpty(userName.Value))
            {
                userName.Attributes.CssStyle.Add("color", "red");
                isValid = false;
            }

            apartmentCota.Attributes.CssStyle.Add("color", "");
            decimal userCotaValue;
            if (string.IsNullOrEmpty(apartmentCota.Value) || !decimal.TryParse(apartmentCota.Value, out userCotaValue))
            {
                apartmentCota.Attributes.CssStyle.Add("color", "red");
                isValid = false;
            }

            userNr.Attributes.CssStyle.Add("color", "");
            int userNrValue;
            if (string.IsNullOrEmpty(userNr.Value) || !int.TryParse(userNr.Value, out userNrValue))
            {
                userNr.Attributes.CssStyle.Add("color", "red");
                isValid = false;
            }

            return isValid;
        }
    }
}