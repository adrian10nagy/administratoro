
namespace Admin.Tenants
{
    using Administratoro.BL.Constants;
    using Administratoro.BL.Extensions;
    using Administratoro.BL.Managers;
    using Administratoro.DAL;
    using Helpers.Constants;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.UI;
    using System.Web.UI.WebControls;

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
                            btnSave.Text = "Actualizează datele proprietatății";
                            lblUserId.Text = Request["apartmentid"];
                            userStairCase.SelectedValue = (apartment.Id_StairCase != null) ? apartment.Id_StairCase.ToString() : null;
                            userHeatHelp.SelectedValue = (apartment.HasHeatHelp.HasValue && apartment.HasHeatHelp.Value? "1" : "0");
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

        private void PopulateCounters(Associations association, int? apartmentId)
        {
            estateCounters.Visible = false;

            if (association.HasStaircase)
            {
                Apartments apartment = null;
                if (apartmentId.HasValue)
                {
                    apartment = ApartmentsManager.GetById(apartmentId.Value);
                }

                var expenses = ExpensesManager.GetAllExpenses();
                foreach (var expense in expenses)
                {
                    PopulateCountersData(association, expense, apartment);
                }
            }
        }

        private void PopulateCountersData(Associations association, Expenses expense, Apartments apartment)
        {
            Panel mainPanel = new Panel();

            Label lb = new Label
            {
                Text = expense.Name,
                CssClass = "col-md-6 col-xs-6"
            };

            DropDownList drp = new DropDownList()
            {
                CssClass = "col-md-6 col-xs-6"
            };

            ListItem defaultNull = new ListItem
            {
                Value = null,
                Text = " -Fără contor- "
            };
            drp.Items.Add(defaultNull);

            AssociationCountersApartment ac = null;
            var counters = association.AssociationCounters.Where(c => c.Id_Expense == expense.Id).ToList();

            if (apartment != null)
            {
                foreach (var counter in counters)
                {
                    var assApCounter = apartment.AssociationCountersApartment.FirstOrDefault(a => a.Id_Counters == counter.Id);
                    if (assApCounter != null)
                    {
                        ac = assApCounter;
                    }
                }
            }

            if (counters.Count != 0)
            {
                Label lbApCounter = new Label
                {
                    Text = (ac != null) ? ac.Id.ToString() : string.Empty,
                    CssClass = "col-md-6 col-xs-6",
                    Visible = false
                };

                int i = 0;
                foreach (var counter in counters)
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

                mainPanel.Controls.Add(lbApCounter);
                mainPanel.Controls.Add(lb);
                mainPanel.Controls.Add(drp);
                mainPanel.Controls.Add(new LiteralControl("<br />"));
                mainPanel.Controls.Add(new LiteralControl("<br />"));
                estateCounters.Visible = true;
                estateCounters.Controls.Add(mainPanel);
            }
        }

        private void PopulateApartmentLogic(Associations association)
        {
            if (association.CotaIndivizaAparments.HasValue)
            {
                apartmentCota.Value = association.CotaIndivizaAparments.Value.ToString();
            }
        }

        private void PopulateStairCase(Associations association)
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
            var apartment = new Apartments
            {
                Name = userName.Value,
                Dependents = userDependents.Value.ToNullableInt().Value,
                ExtraInfo = userExtraInfo.Value,
                CotaIndiviza = (cota.HasValue) ? cota.Value : 0,
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

        private void ProcessSaveCounters(Apartments apartment)
        {
            List<AssociationCountersApartment> counters = GetAllCounters(apartment);
            CountersManager.AddOrUpdateAssociationCountersApartment(counters);
        }

        private List<AssociationCountersApartment> GetAllCounters(Apartments apartment)
        {
            var result = new List<AssociationCountersApartment>();

            for (int i = 0; i < estateCounters.Controls.Count; i++)
            {
                if(estateCounters.Controls[i] is Panel)
                {
                    var mainPanel = (Panel)estateCounters.Controls[i];

                    if (mainPanel.Controls.Count > 2 && mainPanel.Controls[0] is Label
                        && mainPanel.Controls[2] is DropDownList)
                    {
                        var apCounterId = (Label)mainPanel.Controls[0];
                        var counterId = (DropDownList)mainPanel.Controls[2];
                        
                        if (apCounterId != null  && counterId != null )
                        {
                            int apCntId;
                            int apCntIdResult = -1;
                            if(int.TryParse(apCounterId.Text, out apCntId))
                            {
                                apCntIdResult = apCntId;
                            }

                            int cntId;
                            int cntIdResult = -1;
                            if(int.TryParse(counterId.Text, out cntId))
                            {
                                cntIdResult = cntId;
                            }

                            var counter = new AssociationCountersApartment
                            {
                                Id = apCntIdResult,
                                Id_Counters = cntIdResult,
                                Id_Apartment = apartment.Id,
                            };

                            result.Add(counter);
                        }

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