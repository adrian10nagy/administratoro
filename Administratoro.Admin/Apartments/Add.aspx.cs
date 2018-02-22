
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

        private void PopulateCounters(Estates estate, int? apartmentId)
        {
            estateCounters.Visible = false;

            if (estate.HasStaircase)
            {
                Tenants apartment = null;
                if (apartmentId.HasValue)
                {
                    apartment = ApartmentsManager.GetById(apartmentId.Value);
                }

                var expenses = ExpensesManager.GetAllExpenses();
                foreach (var expense in expenses)
                {
                    PopulateCountersData(estate, expense, apartment);
                }
            }
        }

        private void PopulateCountersData(Estates estate, Expenses expense, Tenants apartment)
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

            ApartmentCounters ac = null;
            var counters = estate.Counters.Where(c => c.Id_Expense == expense.Id).ToList();

            if (apartment != null)
            {
                foreach (var counter in counters)
                {
                    var apCounter = apartment.ApartmentCounters.FirstOrDefault(a => a.Id_Counters == counter.Id);
                    if (apCounter != null)
                    {
                        ac = apCounter;
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

        private void PopulateApartmentLogic(Estates estate)
        {
            if (estate.CotaIndivizaAparments.HasValue)
            {
                apartmentCota.Value = estate.CotaIndivizaAparments.Value.ToString();
            }
        }

        private void PopulateStairCase(Estates estate)
        {
            var staircases = StairCasesManager.GetAllByEstate(estate.Id);
            if (estate.HasStaircase)
            {
                ListItem defaultNull = new ListItem
                {
                    Value = null
                };
                userStairCase.Items.Add(defaultNull);
                divStaircase.Visible = true;

                foreach (var item in staircases)
                {
                    ListItem li = new ListItem
                    {
                        Text = item.Nume,
                        Value = item.Id.ToString()
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
            var tenant = new Tenants
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
                Id_StairCase = userStairCase.SelectedValue.ToNullableInt()
            };

            if (!string.IsNullOrEmpty(lblUserId.Text) && lblUserId.Text.ToNullableInt() != 0)
            {
                tenant.Id = lblUserId.Text.ToNullableInt().Value;
                ApartmentsManager.Update(tenant);
            }
            else
            {
                tenant = ApartmentsManager.Add(tenant);
                lblStatus.Text = FlowMessages.TenantAddSuccess;
                lblStatus.CssClass = "SuccessBox";
                //CleanFields();
            }

            ProcessSaveCounters(tenant);

            var estate = AssociationsManager.GetById(Association.Id);
            Session[SessionConstants.SelectedAssociation] = estate;

            Response.Redirect("~/Apartments/Manage.aspx?Message=UserUpdatedSuccess");
        }

        private void ProcessSaveCounters(Tenants tenant)
        {
            List<ApartmentCounters> counters = GetAllCounters(tenant);
            CountersManager.AddOrUpdateApartmentCounters(counters);
        }

        private List<ApartmentCounters> GetAllCounters(Tenants tenant)
        {
            var result = new List<ApartmentCounters>();

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

                            var counter = new ApartmentCounters
                            {
                                Id = apCntIdResult,
                                Id_Counters = cntIdResult,
                                Id_Apartment = tenant.Id,
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

        private void CleanFields()
        {
            userName.Value = string.Empty;
            userDependents.Value = string.Empty;
            userPhone.Value = string.Empty;
            userEmail.Value = string.Empty;
        }
    }
}