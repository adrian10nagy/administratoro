
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

            PopulateCounters(Estate, apId);
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                PopulateStairCase(Estate);
                if (!string.IsNullOrEmpty(Request["apartmentid"]))
                {
                    var apartmentid = Request["apartmentid"].ToNullableInt();
                    if (apartmentid != null && apartmentid != 0)
                    {
                        var user = ApartmentsManager.GetById(apartmentid.Value);
                        if (user != null)
                        {
                            userName.Value = user.Name;
                            userExtraInfo.Value = user.ExtraInfo;
                            userPhone.Value = user.Telephone;
                            userEmail.Value = user.Email;
                            userDependents.Value = user.Dependents.ToString();
                            apartmentCota.Value = user.CotaIndiviza.ToString();
                            userNr.Value = user.Number.ToString();
                            btnSave.Text = "Actualizează datele proprietatății";
                            lblUserId.Text = Request["apartmentid"];
                            userStairCase.SelectedValue = (user.Id_StairCase != null) ? user.Id_StairCase.ToString() : null;
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
                    PopulateApartmentLogic(Estate);
                }
            }


        }

        private void PopulateCounters(Estates estate, int? apartmentId)
        {
            estateCounters.Visible = false;

            if (estate.HasStaircase)
            {
                var expenses = EstateExpensesManager.GetFromLastesOpenedMonth(estate.Id);
                Tenants apartment = null;
                if (apartmentId.HasValue)
                {
                    apartment = ApartmentsManager.GetById(apartmentId.Value);
                }

                foreach (var expense in expenses)
                {
                    PopulateCountersData(estate, expense, apartment);
                }
            }
        }

        private void PopulateCountersData(Estates estate, EstateExpenses esEx, Tenants apartment)
        {
            Label lb = new Label
            {
                Text = esEx.Expenses.Name,
                CssClass = "col-md-6 col-xs-6"
            };

            DropDownList drp = new DropDownList()
            {
                CssClass = "col-md-6 col-xs-6"
            };

            ListItem defaultNull = new ListItem
            {
                Value = null
            };
            drp.Items.Add(defaultNull);

            var counters = estate.Counters.Where(c => c.Id_Expense == esEx.Id_Expense).ToList();
            if (apartment != null)
            {
                List<int> apCounters = apartment.ApartmentCounters.Where(ac=>ac.;
                //List<int> apCounters = counters.Select(c => c.Id).Union(apartment.ApartmentCounters.Where(ac => ac.Id_Counters == esEx.Id_Expense).ToList().Select(ac => ac.Id_Counters)).ToList();
            }

            if (counters.Count == 1)
            {
                ListItem li = new ListItem
                {
                    Text = counters[0].Value,
                    Value = counters[0].Id.ToString(),
                    Selected = true
                };
                drp.Items.Add(li);

                estateCounters.Controls.Add(lb);
                estateCounters.Controls.Add(drp);
                estateCounters.Visible = true;
                estateCounters.Controls.Add(new LiteralControl("<br />"));
                estateCounters.Controls.Add(new LiteralControl("<br />"));
            }
            else if (counters.Count != 0)
            {
                foreach (var counter in counters)
                {
                    ListItem li = new ListItem
                    {
                        Text = counter.Value,
                        Value = counter.Id.ToString()
                    };
                    drp.Items.Add(li);

                    estateCounters.Controls.Add(lb);
                    estateCounters.Controls.Add(drp);
                }
                estateCounters.Visible = true;
                estateCounters.Controls.Add(new LiteralControl("<br />"));
                estateCounters.Controls.Add(new LiteralControl("<br />"));
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
                id_Estate = Estate.Id,
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
                CleanFields();
            }

            ProcessSaveCounters(tenant);

            var estate = EstatesManager.GetById(Estate.Id);
            Session[SessionConstants.SelectedEstate] = estate;

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

            foreach (var control in estateCounters.Controls)
            {
                if (control is DropDownList)
                {
                    var drp = (DropDownList)control;

                    int counterId;
                    if (int.TryParse(drp.SelectedValue, out counterId))
                    {
                        var counter = new ApartmentCounters
                        {
                            Id_Counters = counterId,
                            Id_Apartment = tenant.Id,
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

        private void CleanFields()
        {
            userName.Value = string.Empty;
            userDependents.Value = string.Empty;
            userPhone.Value = string.Empty;
            userEmail.Value = string.Empty;
        }
    }
}