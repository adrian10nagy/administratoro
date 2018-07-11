using Administratoro.BL.Constants;
using Administratoro.BL.Managers;
using Administratoro.BL.Extensions;
using Administratoro.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Administratoro.BL.Extensions;
using System.Globalization;

namespace Admin.Invoices
{
    public partial class Add : BasePage
    {
        public int? _year
        {
            get
            {
                return Request.QueryString["year"].GetYear();
            }
        }

        public int? _month
        {
            get
            {
                return Request.QueryString["month"].GetMonth();
            }
        }

        private int? _expense
        {
            get
            {
                return Request.QueryString["expense"].GetInt();

            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                InitializeYearMonth();
                InitializeExpenses();
            }

            InitializeValueFields(Page.IsPostBack);
        }

        private void InitializeYearMonth()
        {
            drpInvoiceYearMonth.Items.Clear();
            var yearMonths = AssociationExpensesManager.GetAllMonthsAndYearsNotClosedByAssociationId(Association.Id);
            bool hasValue = _year.HasValue && _month.HasValue;
            for (int i = 0; i < yearMonths.Count; i++)
            {
                var yearMonth = yearMonths[i];
                drpInvoiceYearMonth.Items.Add(new ListItem
                {
                    Value = yearMonth.Year + "-" + yearMonth.Month,
                    Text = "Anul:" + yearMonth.Year + " Luna:" + yearMonth.Month,
                    Selected = hasValue ? (yearMonth.Month == _month.Value && yearMonth.Year == _year.Value) : i + 1 == yearMonths.Count
                });
            }

            if (hasValue)
            {
                drpInvoiceYearMonth.Enabled = false;
            }
        }

        private void InitializeExpenses()
        {
            var associationExpenses = GetAssociationExpenses();
            drpInvoiceExpenses.Items.Clear();
            foreach (var associationExpense in associationExpenses)
            {
                drpInvoiceExpenses.Items.Add(new ListItem
                {
                    Value = associationExpense.Expenses.Id.ToString(),
                    Text = associationExpense.Expenses.Name.ToString(),
                    Selected = _expense.HasValue && _expense.Value == associationExpense.Id_Expense
                });
            }

            drpInvoiceExpenses.Items.Add(new ListItem
            {
                Value = "24",
                Text = "Diverse",
                Selected = _expense.HasValue && _expense.Value == 24
            });

            if (_expense.HasValue)
            {
                drpInvoiceExpenses.Enabled = false;
            }
        }

        private List<AssociationExpenses> GetAssociationExpenses()
        {
            var result = new List<AssociationExpenses>();

            if (!string.IsNullOrEmpty(drpInvoiceYearMonth.SelectedValue))
            {
                var yearMonth = drpInvoiceYearMonth.SelectedValue.Split('-');
                if (yearMonth.Length == 2)
                {
                    int year;
                    int month;

                    if (int.TryParse(yearMonth[0], out year) && int.TryParse(yearMonth[1], out month))
                    {
                        result = AssociationExpensesManager.GetForAddPage(Association.Id, year, month).ToList();
                    }
                }
            }

            return result;
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            Response.Redirect("~/Expenses/Invoices.aspx?year=" + _year.Value + "&month=" + _month.Value);
        }

        #region save

        protected void btnSave_Click(object sender, EventArgs e)
        {
            //InitializeValueField();
            var yearMonth = drpInvoiceYearMonth.SelectedValue.Split('-');

            if (yearMonth.Length == 2)
            {

                int year;
                int month;
                int expenseId;
                if (int.TryParse(drpInvoiceExpenses.SelectedValue, out expenseId) &&
                    int.TryParse(yearMonth[0], out year) &&
                    int.TryParse(yearMonth[1], out month))
                {
                    if (expenseId == (int)Expense.Diverse)
                    {
                        SaveDiverse(year, month, Association.Id, expenseId);
                    }
                    else
                    {
                        SaveDefault(year, month, Association.Id, expenseId);
                    }
                }
            }

            var estate = AssociationsManager.GetById(Association.Id);
            Session[SessionConstants.SelectedAssociation] = estate;
            if (_year.HasValue && _month.HasValue)
            {
                Response.Redirect("~/Expenses/Invoices.aspx?year=" + _year.Value + "&month=" + _month.Value);
            }
            else
            {
                Response.Redirect("~/");
            }
        }

        private void SaveDiverse(int year, int month, int associationId, int expenseId)
        {
            foreach (var control in pnlInvoiceDiverseValues.Controls)
            {
                if (control is Panel)
                {
                    var innerControls = control as Panel;
                    if (innerControls != null && innerControls.Controls.Count == 6)
                    {
                        var theDateControl = innerControls.Controls[4] as Panel;
                        var theNumberControl = innerControls.Controls[5] as Panel;
                        var theRedistributionControl = innerControls.Controls[3] as Panel;
                        var thestairCaseControl = innerControls.Controls[2] as Panel;
                        var theDescriptionControl = innerControls.Controls[1] as Panel;
                        var theInvoiceValuecontrol = innerControls.Controls[0] as Panel;
                        if (theDescriptionControl != null && theDescriptionControl.Controls.Count == 1 && theDescriptionControl.Controls[0] is TextBox &&
                            theInvoiceValuecontrol != null && theInvoiceValuecontrol.Controls.Count == 1 && theInvoiceValuecontrol.Controls[0] is TextBox &&
                            thestairCaseControl != null && thestairCaseControl.Controls.Count == 1 && thestairCaseControl.Controls[0] is DropDownList &&
                            theRedistributionControl != null && theRedistributionControl.Controls.Count == 1 && theRedistributionControl.Controls[0] is DropDownList &&
                            theDateControl != null && theDateControl.Controls.Count == 1 && theDateControl.Controls[0] is TextBox &&
                            theNumberControl != null && theNumberControl.Controls.Count == 1 && theNumberControl.Controls[0] is TextBox)
                        {
                            var theDescription = theDescriptionControl.Controls[0] as TextBox;
                            var theInvoiceValue = theInvoiceValuecontrol.Controls[0] as TextBox;
                            var thestairCase = thestairCaseControl.Controls[0] as DropDownList;
                            var theRedistribution = theRedistributionControl.Controls[0] as DropDownList;
                            var theNumber = theNumberControl.Controls[0] as TextBox;
                            var theDate = theDateControl.Controls[0] as TextBox;

                            // get invoice
                            int? invoiceId = null;
                            int invoiceIdValue;
                            if (int.TryParse(theInvoiceValue.ID.Replace("tbInvoiceId", string.Empty), out invoiceIdValue))
                            {
                                invoiceId = invoiceIdValue;
                            }

                            // get value
                            decimal? theValue = null;
                            decimal tempValue;
                            var valueAltered = theInvoiceValue.Text.Replace(".", ",");
                            if (decimal.TryParse(valueAltered, out tempValue))
                            {
                                theValue = Math.Round(tempValue, ConfigConstants.InvoicePrecision);
                            }

                            // get stairCase
                            int? stairCaseId = null;
                            int stairCaseIdValue;
                            if (int.TryParse(thestairCase.SelectedValue, out stairCaseIdValue))
                            {
                                stairCaseId = stairCaseIdValue;
                            }

                            // get redistribution
                            int? redistributionId = null;
                            int redistributionIdValue;
                            if (int.TryParse(theRedistribution.SelectedValue, out redistributionIdValue))
                            {
                                redistributionId = redistributionIdValue;
                            }

                            // get issue date
                            DateTime? theDateId = null;
                            DateTime theDateIdValue;
                            if (DateTime.TryParse(theDate.Text, out theDateIdValue))
                            {
                                theDateId = theDateIdValue;
                            }

                            if (invoiceId.HasValue)
                            {
                                if (theValue.HasValue)
                                {
                                    var invoice = InvoicesManager.GetDiverseById(invoiceId.Value);
                                    if (invoice != null)
                                    {
                                        InvoicesManager.Update(invoice, theValue, stairCaseId, theDescription.Text, redistributionId, theNumber.Text, theDateId, null);
                                    }
                                }
                                else
                                {
                                    InvoicesManager.Remove(invoiceId.Value);
                                }
                            }
                            else if (theValue.HasValue)
                            {
                                var ee = AssociationExpensesManager.GetMonthYearAssoiationExpense(associationId, expenseId, year, month);
                                if (ee == null)
                                {
                                    AssociationExpensesManager.Add(associationId, expenseId, month, year, ((int)ExpenseType.PerNrTenants).ToString(), false);
                                    ee = AssociationExpensesManager.GetMonthYearAssoiationExpense(associationId, expenseId, year, month);
                                }

                                InvoicesManager.AddDiverse(ee, theValue, theDescription.Text, stairCaseId, redistributionId, theNumber.Text, theDateId);
                            }
                        }
                    }
                }
            }
        }

        private void SaveDefault(int year, int month, int associationId, int expenseId)
        {
            AssociationExpenses associationExpense = AssociationExpensesManager.GetMonthYearAssoiationExpense(associationId, expenseId, year, month);
            bool isIndexExpense = associationExpense != null && associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex;

            decimal? theInvoideValue = null;
            decimal tempInvoideValue;
            var invoiceValueAltered = txtInvoiceValue.Text.Replace(".", ",");
            if (decimal.TryParse(invoiceValueAltered, out tempInvoideValue))
            {
                theInvoideValue = tempInvoideValue;
            }

            int? assCounter = null;
            int assCounterId;
            if (int.TryParse(drpInvoiceCounters.SelectedValue, out assCounterId))
            {
                assCounter = assCounterId;
            }

            // get issue date
            DateTime? theInvoiceDate = null;
            DateTime theInvoiceDateValue;
            if (DateTime.TryParse(txtInvoiceDate.Text, out theInvoiceDateValue))
            {
                theInvoiceDate = theInvoiceDateValue;
            }

            var theInvoice = InvoicesManager.GetAllByAssotiationYearMonthExpenseId(Association.Id, expenseId, year, month).ToList();

            //update
            if (theInvoice != null && theInvoice.Count > 0)
            {
                var invoice = theInvoice.FirstOrDefault(i => i.id_assCounter == assCounter);
                InvoicesManager.Update(invoice, theInvoideValue, null, txtInvoiceDescription.Text, null, txtInvoiceNumber.Text, theInvoiceDate, assCounter);
            }
            else
            {
                //error
            }

            var addedInvoiceSubcategories = new List<InvoiceSubcategories>();
            if (associationExpense != null && isIndexExpense && theInvoideValue.HasValue)
            {
                decimal? addedIndices = SaveAndGetInvoicesIndices();
                if (pnInvoiceSubcategories.Visible)
                {
                    addedInvoiceSubcategories = SaveAndGetSubcategories();
                }

                AssociationExpensesManager.UpdatePricePerUnit(associationExpense.Id, addedInvoiceSubcategories);

                //decimal pricePerUnit = theInvoideValue.Value / addedIndices.Value;
                //AssociationExpensesManager.UpdatePricePerUnit(associationExpense.Id, pricePerUnit);
            }
        }

        private List<InvoiceSubcategories> SaveAndGetSubcategories()
        {
            List<InvoiceSubcategories> addedInvoiceSubcategories = new List<InvoiceSubcategories>();

            foreach (var control in pnInvoiceSubcategories.Controls)
            {
                if (control is Panel)
                {
                    var thePanelControl = control as Panel;
                    if (thePanelControl != null && thePanelControl.Controls.Count == 7)
                    {
                        Panel theNameControl = thePanelControl.Controls[0] as Panel;
                        Panel theQuantityControl = thePanelControl.Controls[1] as Panel;
                        Panel thePriceControl = thePanelControl.Controls[2] as Panel;
                        Panel theVATControl = thePanelControl.Controls[3] as Panel;
                        Panel theServicesControl = thePanelControl.Controls[4] as Panel;
                        Panel thePenaltiesControl = thePanelControl.Controls[5] as Panel;
                        Panel theValueControl = thePanelControl.Controls[6] as Panel;

                        if (theNameControl != null && theNameControl.Controls.Count == 1 && theNameControl.Controls[0] is Label &&
                            (theQuantityControl != null && theQuantityControl.Controls.Count == 1 && theQuantityControl.Controls[0] is TextBox) &&
                            (thePriceControl != null && thePriceControl.Controls.Count == 1 && thePriceControl.Controls[0] is TextBox) &&
                            (theVATControl != null && theVATControl.Controls.Count == 1 && theVATControl.Controls[0] is TextBox) &&
                            (theServicesControl != null && theServicesControl.Controls.Count == 1 && theServicesControl.Controls[0] is TextBox) &&
                            (thePenaltiesControl != null && thePenaltiesControl.Controls.Count == 1 && thePenaltiesControl.Controls[0] is TextBox) &&
                            (theValueControl != null && theValueControl.Controls.Count == 1 && theValueControl.Controls[0] is TextBox))
                        {
                            Label theName = theNameControl.Controls[0] as Label;
                            TextBox theQuantity = theQuantityControl.Controls[0] as TextBox;
                            TextBox thePrice = thePriceControl.Controls[0] as TextBox;
                            TextBox theVAT = theVATControl.Controls[0] as TextBox;
                            TextBox theServices = theServicesControl.Controls[0] as TextBox;
                            TextBox thePenalties = thePenaltiesControl.Controls[0] as TextBox;
                            TextBox theValue = theValueControl.Controls[0] as TextBox;

                            int theSubcategoryId;
                            int theInvoiceId;
                            int theAssCounterId;
                            int? theAssCounterIdValue = null;

                            decimal? theQuantityToUpdate = DecimalExtensions.GetNullableDecimal(theQuantity.Text);
                            decimal? thePriceToUpdate = DecimalExtensions.GetNullableDecimal(thePrice.Text);
                            decimal? theVATToUpdate = DecimalExtensions.GetNullableDecimal(theVAT.Text);
                            decimal? theServicesToUpdate = DecimalExtensions.GetNullableDecimal(theServices.Text);
                            decimal? thePenaltiesToUpdate = DecimalExtensions.GetNullableDecimal(thePenalties.Text);
                            decimal? theValueToUpdate = DecimalExtensions.GetNullableDecimal(theValue.Text);

                            var nameSubStringIndex = theName.ID.IndexOf("tbInvoice") + 9;
                            var valueSubStringIndex = theValue.ID.IndexOf("tbinvoiceSubcategory") + 20;
                            var assCounterSubStringIndex = theValue.ID.IndexOf("tbinvoiceSubcategory");

                            if (int.TryParse(theValue.ID.Substring(0, assCounterSubStringIndex), out theAssCounterId))
                            {
                                theAssCounterIdValue = theAssCounterId;
                            }

                            if (int.TryParse(theValue.ID.Substring(valueSubStringIndex), out theSubcategoryId) &&
                                int.TryParse(theName.ID.Substring(nameSubStringIndex), out theInvoiceId))
                            {

                                var invoiceSubcategory = new InvoiceSubcategories
                                {
                                    Value = theValueToUpdate,
                                    Id_subCategType = theSubcategoryId,
                                    Id_Invoice = theInvoiceId,
                                    quantity = theQuantityToUpdate,
                                    PricePerUnit = thePriceToUpdate,
                                    VAT = theVATToUpdate,
                                    service = theServicesToUpdate,
                                    penalties = thePenaltiesToUpdate,
                                    id_assCounter = theAssCounterIdValue
                                };

                                InvoicesSubcategoriesManager.Update(invoiceSubcategory);
                                addedInvoiceSubcategories.Add(invoiceSubcategory);
                            }
                        }
                    }
                }
            }


            return addedInvoiceSubcategories;
        }

        private decimal? SaveAndGetInvoicesIndices()
        {
            decimal? addedIndices = null;

            foreach (var control in pnInvoiceValues.Controls)
            {
                if (control is Panel)
                {
                    var thePanelControl = control as Panel;
                    if (thePanelControl != null && thePanelControl.Controls.Count == 3)
                    {
                        Panel theCounterControl = thePanelControl.Controls[0] as Panel;
                        Panel theIndexOldControl = thePanelControl.Controls[1] as Panel;
                        Panel theIndexNewControl = thePanelControl.Controls[2] as Panel;

                        if (theCounterControl != null && theCounterControl.Controls.Count == 1 && theCounterControl.Controls[0] is DropDownList &&
                            (theIndexOldControl == null || theIndexOldControl.Controls.Count == 1 && theIndexOldControl.Controls[0] is TextBox) &&
                            (theIndexNewControl == null || theIndexNewControl.Controls.Count == 1 && theIndexNewControl.Controls[0] is TextBox)
                            )
                        {
                            TextBox theIndexOld = theIndexOldControl.Controls[0] as TextBox;
                            TextBox theIndexNew = theIndexNewControl.Controls[0] as TextBox;
                            DropDownList theControl = theCounterControl.Controls[0] as DropDownList;

                            int? indexInvoiceId = null;
                            int invoiceIdValue;
                            if (int.TryParse(theControl.ID.Replace("drpCuntersID", string.Empty), out invoiceIdValue))
                            {
                                indexInvoiceId = invoiceIdValue;
                            }

                            //index old
                            decimal? indexOld = null;
                            decimal indexOldValue;
                            if (theIndexOld != null && decimal.TryParse(theIndexOld.Text, out indexOldValue))
                            {
                                indexOld = indexOldValue;
                            }

                            //index new
                            decimal? indexNew = null;
                            decimal indexNewValue;
                            if (theIndexNew != null && decimal.TryParse(theIndexNew.Text, out indexNewValue))
                            {
                                indexNew = indexNewValue;
                            }

                            if (indexNew.HasValue && indexOld.HasValue)
                            {
                                var indexDiference = indexNew.Value - indexOld.Value;
                                if (!addedIndices.HasValue)
                                {
                                    addedIndices = 0.0m;
                                }

                                addedIndices = addedIndices + indexDiference;
                            }

                            if (indexInvoiceId.HasValue)
                            {
                                var invoice = InvoicesManager.GetByIndexInvoiceId(indexInvoiceId.Value);
                                if (invoice != null)
                                {
                                    InvoiceIndexesManager.Update(indexInvoiceId.Value, invoice.Id, indexOld, indexNew);
                                }
                            }
                        }
                    }
                }
            }

            return addedIndices;
        }

        #endregion

        #region onchange events

        protected void drpInvoiceAssociations_SelectedIndexChanged(object sender, EventArgs e)
        {
            InitializeYearMonth();
            InitializeExpenses();
            pnInvoiceValues.Controls.Clear();
            pnInvoiceSubcategories.Controls.Clear();
            InitializeValueFields();
        }

        protected void drpInvoiceYearMonth_SelectedIndexChanged(object sender, EventArgs e)
        {
            InitializeExpenses();
            pnInvoiceValues.Controls.Clear();
            pnInvoiceSubcategories.Controls.Clear();
            InitializeValueFields();
        }

        protected void drpInvoiceExpenses_SelectedIndexChanged(object sender, EventArgs e)
        {
            pnInvoiceValues.Controls.Clear();
            pnInvoiceSubcategories.Controls.Clear();
            pnlInvoiceDiverseValues.Controls.Clear();
            InitializeValueFields();
        }

        #endregion

        private void InitializeValueFields(bool isPostbackFromLoadEvent = false)
        {
            var yearMonth = drpInvoiceYearMonth.SelectedValue.Split('-');

            if (yearMonth.Length == 2)
            {
                int year;
                int month;
                int expenseId;
                if (int.TryParse(drpInvoiceExpenses.SelectedValue, out expenseId) &&
                    int.TryParse(yearMonth[0], out year) &&
                    int.TryParse(yearMonth[1], out month))
                {
                    AssociationExpenses associationExpense = AssociationExpensesManager.GetMonthYearAssoiationExpense(Association.Id, expenseId, year, month);

                    if (associationExpense == null && expenseId == (int)Expense.Diverse)
                    {
                        AssociationExpensesManager.Add(Association.Id, expenseId, month, year, ((int)ExpenseType.PerNrTenants).ToString(), false);
                        associationExpense = AssociationExpensesManager.GetMonthYearAssoiationExpense(Association.Id, expenseId, year, month);
                    }

                    if (expenseId == (int)Expense.Diverse)
                    {
                        pnlInvoiceBody.Visible = false;
                        InitializeValueFieldAddExtraControlsForDiverse(associationExpense);
                    }
                    else
                    {
                        pnlInvoiceBody.Visible = true;
                        InitializeValueFieldAddcontrols(year, month, Association.Id, expenseId, associationExpense, isPostbackFromLoadEvent);
                    }
                }
            }
        }

        private void InitializeValueFieldAddExtraControlsForDiverse(AssociationExpenses associationExpense)
        {
            InitializeValueFieldAddColumnHeadersForDiverse();

            if (associationExpense != null)
            {
                var invoices = InvoicesManager.GetDiverseByAssociationAssociationExpense(associationExpense.Id);

                DiverseInitializeValueFieldAddInvoices(invoices);
            }

            DiverseInitializeValueFieldAddInvoices(new List<Administratoro.DAL.Invoices> { new Administratoro.DAL.Invoices() });
        }

        private void InitializeInvoicesForWatherCold(List<Administratoro.DAL.InvoiceIndexes> invoicesIndexes, List<Administratoro.DAL.AssociationCounters> counters)
        {
            foreach (var invoicesIndexe in invoicesIndexes)
            {
                Panel panelMain = new Panel
                {
                    CssClass = "col-md-12 col-xs-12 invoicesRow"
                };

                Panel panel1 = new Panel
                {
                    CssClass = "col-md-4"
                };

                DropDownList drpCunters = new DropDownList
                {
                    CssClass = "form-control",
                    Enabled = false,
                    ID = "drpCuntersID" + invoicesIndexe.Id
                };
                if (invoicesIndexe.AssociationCounters != null)
                {
                    drpCunters.Items.Add(new ListItem
                    {
                        Text = "Contorul " + invoicesIndexe.AssociationCounters.Value,
                        Value = invoicesIndexe.AssociationCounters.Id.ToString()
                    });
                }
                else
                {
                    foreach (var counter in counters)
                    {
                        drpCunters.Items.Add(new ListItem
                        {
                            Text = "Contorul " + counter.Value,
                            Value = counter.Id.ToString(),
                            Selected = invoicesIndexe.Id_Counter.HasValue && invoicesIndexe.Id_Counter.Value == counter.Id
                        });
                    }
                }

                panel1.Controls.Add(drpCunters);
                panelMain.Controls.Add(panel1);

                Panel panel2 = new Panel
                {
                    CssClass = "col-md-4"
                };

                TextBox tbInsexOld = new TextBox
                {
                    Text = invoicesIndexe != null && invoicesIndexe.IndexOld.HasValue ? invoicesIndexe.IndexOld.Value.ToString() : string.Empty,
                    CssClass = "form-control",
                    ID = "tbInsexOld" + invoicesIndexe.Id,
                    AutoCompleteType = AutoCompleteType.Disabled
                };
                panel2.Controls.Add(tbInsexOld);
                panelMain.Controls.Add(panel2);

                Panel panel3 = new Panel
                {
                    CssClass = "col-md-4"
                };
                TextBox tbIndexNew = new TextBox
                {
                    Text = invoicesIndexe != null && invoicesIndexe.IndexNew.HasValue ? invoicesIndexe.IndexNew.Value.ToString() : string.Empty,
                    CssClass = "form-control",
                    ID = "tbIndexNew" + invoicesIndexe.Id,
                    AutoCompleteType = AutoCompleteType.Disabled
                };
                panel3.Controls.Add(tbIndexNew);
                panelMain.Controls.Add(panel3);

                pnInvoiceValues.Controls.Add(panelMain);
            }
        }

        private void InitializeValueFieldAddcontrols(int year, int month, int associationId, int expenseId, AssociationExpenses associationExpense, bool isPostbackFromLoadEvent)
        {
            pnInvoiceSubcategories.Visible = false;
            bool isIndexExpense = associationExpense != null && associationExpense.Id_ExpenseType == (int)ExpenseType.PerIndex;
            bool isSplitPerStairCase = associationExpense != null && associationExpense.SplitPerStairCase.HasValue && associationExpense.SplitPerStairCase == true;

            var invoices = InvoicesManager.GetAllByAssotiationYearMonthExpenseId(associationId, expenseId, year, month).ToList();

            if (isSplitPerStairCase)
            {
                var assCounters = AssociationCountersManager.GetAllByExpenseType(Association.Id, associationExpense.Id_Expense);
                InitializeInvoiceCounterSplit(assCounters, isPostbackFromLoadEvent);
                if (invoices.Count() == 0)
                {
                    InvoicesManager.AddDefault(associationExpense);
                }
                else
                {
                    if (invoices.Count() != assCounters.Count())
                    {
                        foreach (var counter in assCounters)
                        {

                        }
                    }

                }

                invoices = InvoicesManager.GetAllByAssotiationYearMonthExpenseId(associationId, expenseId, year, month).ToList();
            }
            else
            {
                pnlInvoiceCounter.Visible = false;
                if (invoices.Count != 1)
                {
                    InvoicesManager.AddDefault(associationId, expenseId, year, month);
                    invoices = InvoicesManager.GetAllByAssotiationYearMonthExpenseId(associationId, expenseId, year, month).ToList();
                }
            }

            var theInvoice = invoices[0];

            if (!isPostbackFromLoadEvent)
            {
                int assCounterId;
                if (pnlInvoiceCounter.Visible && int.TryParse(drpInvoiceCounters.SelectedValue, out assCounterId))
                {
                    var assCounter = AssociationCountersManager.GetById(assCounterId);
                    if (assCounter != null)
                    {
                        theInvoice = InvoicesManager.GetByAssociationExpenseIdAndCounter(theInvoice.Id_EstateExpense.Value, assCounter.Id);
                    }
                }

                InitializeInvoiceFields(theInvoice);
            }

            if (isIndexExpense)
            {
                List<AssociationCounters> counters = AssociationCountersManager.GetAllByExpenseType(Association.Id, expenseId).ToList();
                invoices = InvoicesSubcategoriesManager.ConfigureSubcategories(invoices, counters).ToList();

                var invoicesIndexes = InvoiceIndexesManager.Get(theInvoice.Id).ToList();
                invoicesIndexes = InvoiceIndexesManager.ConfigureWatherCold(invoices, invoicesIndexes, counters);

                if (invoicesIndexes.Count() > 0)
                {
                    // add Counters Index old-new
                    InitializeValueFieldAddColumnHeadersForIndexExpenses();
                    InitializeInvoicesForWatherCold(invoicesIndexes, counters);
                }

                if (expenseId == (int)Expense.ApaRece)
                {
                    // add InvoicesSubcategories
                    InitializeSubInvoices(invoices);
                }
                else if (expenseId == (int)Expense.ApaCalda)
                {
                    //InvoicesSubcategoriesManager.ConfigureWatherHot(invoices, counters);
                    // add InvoicesSubcategories
                    InitializeSubInvoices(invoices);
                }
            }
        }

        private void InitializeInvoiceCounterSplit(IEnumerable<AssociationCounters> assCounters, bool postbackFromLoad)
        {
            pnlInvoiceCounter.Visible = true;
            if (!postbackFromLoad)
            {
                drpInvoiceCounters.Items.Clear();

                foreach (var assCounter in assCounters)
                {
                    drpInvoiceCounters.Items.Add(new ListItem
                    {
                        Text = "Contorul " + assCounter.Value,
                        Value = assCounter.Id.ToString(),
                    });
                }
            }
        }

        private void InitializeSubInvoices(List<Administratoro.DAL.Invoices> invoices)
        {
            pnInvoiceSubcategories.Visible = true;

            Panel panelMain = new Panel
            {
                CssClass = "col-md-12 col-xs-12 invoicesRow"
            };

            Panel panel1 = new Panel
            {
                CssClass = "col-md-2"
            };
            Label lb1 = new Label
            {
                Text = "Denumire"
            };
            panel1.Controls.Add(lb1);
            panelMain.Controls.Add(panel1);

            Panel panel2 = new Panel
            {
                CssClass = "col-md-1"
            };
            Label lb2 = new Label
            {
                Text = "Cantitate"
            };
            panel2.Controls.Add(lb2);
            panelMain.Controls.Add(panel2);

            Panel panel3 = new Panel
            {
                CssClass = "col-md-1"
            };
            Label lb3 = new Label
            {
                Text = "Preț mp"
            };
            panel3.Controls.Add(lb3);
            panelMain.Controls.Add(panel3);

            Panel panel4 = new Panel
            {
                CssClass = "col-md-2"
            };
            Label lb4 = new Label
            {
                Text = "TVA"
            };
            panel4.Controls.Add(lb4);
            panelMain.Controls.Add(panel4);

            Panel panel5 = new Panel
            {
                CssClass = "col-md-2"
            };
            Label lb5 = new Label
            {
                Text = "Servicii"
            };
            panel5.Controls.Add(lb5);
            panelMain.Controls.Add(panel5);


            Panel panel6 = new Panel
            {
                CssClass = "col-md-2"
            };
            Label lb6 = new Label
            {
                Text = "Penalizari"
            };
            panel6.Controls.Add(lb6);
            panelMain.Controls.Add(panel6);

            Panel panel7 = new Panel
            {
                CssClass = "col-md-2"
            };
            Label lb7 = new Label
            {
                Text = "Valoare"
            };
            panel7.Controls.Add(lb7);
            panelMain.Controls.Add(panel7);

            pnInvoiceSubcategories.Controls.Add(panelMain);

            foreach (var invoice in invoices)
            {
                // print the subgategories
                foreach (var invoiceSubcategory in invoice.InvoiceSubcategories)
                {
                    Panel innerPanelMain = new Panel
                    {
                        CssClass = "col-md-12 col-xs-12 invoicesRow"
                    };

                    Panel textPanel = new Panel
                    {
                        CssClass = "col-md-2"
                    };
                    Label lb01 = new Label
                    {
                        Text = invoiceSubcategory.InvoiceSubcategoryTypes != null ? invoiceSubcategory.InvoiceSubcategoryTypes.Value.ToString() : string.Empty,
                        ID = invoiceSubcategory.Id + "tbInvoice" + invoice.Id
                    };
                    textPanel.Controls.Add(lb01);
                    innerPanelMain.Controls.Add(textPanel);

                    Panel quantityPanel = new Panel
                    {
                        CssClass = "col-md-1"
                    };

                    TextBox tbquantity = new TextBox
                    {
                        Text = invoiceSubcategory.quantity.HasValue ? invoiceSubcategory.quantity.Value.ToString() : string.Empty,
                        CssClass = "form-control",
                        AutoCompleteType = AutoCompleteType.Disabled
                    };
                    quantityPanel.Controls.Add(tbquantity);
                    innerPanelMain.Controls.Add(quantityPanel);

                    Panel pricePanel = new Panel
                    {
                        CssClass = "col-md-1"
                    };

                    TextBox tbPrice = new TextBox
                    {
                        Text = invoiceSubcategory.PricePerUnit.HasValue ? invoiceSubcategory.PricePerUnit.Value.ToString() : string.Empty,
                        CssClass = "form-control",
                        AutoCompleteType = AutoCompleteType.Disabled
                    };
                    pricePanel.Controls.Add(tbPrice);
                    innerPanelMain.Controls.Add(pricePanel);

                    Panel vatPanel = new Panel
                    {
                        CssClass = "col-md-2"
                    };

                    TextBox tbVat = new TextBox
                    {
                        Text = invoiceSubcategory.VAT.HasValue ? invoiceSubcategory.VAT.ToString() : string.Empty,
                        CssClass = "form-control",
                        AutoCompleteType = AutoCompleteType.Disabled
                    };
                    vatPanel.Controls.Add(tbVat);
                    innerPanelMain.Controls.Add(vatPanel);

                    Panel servicePanel = new Panel
                    {
                        CssClass = "col-md-2"
                    };

                    TextBox tbService = new TextBox
                    {
                        Text = invoiceSubcategory.service.HasValue ? invoiceSubcategory.service.ToString() : string.Empty,
                        CssClass = "form-control",
                        AutoCompleteType = AutoCompleteType.Disabled
                    };
                    servicePanel.Controls.Add(tbService);
                    innerPanelMain.Controls.Add(servicePanel);

                    Panel penaltiesPanel = new Panel
                    {
                        CssClass = "col-md-2"
                    };

                    TextBox tbPenalties = new TextBox
                    {
                        Text = invoiceSubcategory.penalties.HasValue ? invoiceSubcategory.penalties.ToString() : string.Empty,
                        CssClass = "form-control",
                        AutoCompleteType = AutoCompleteType.Disabled
                    };
                    penaltiesPanel.Controls.Add(tbPenalties);
                    innerPanelMain.Controls.Add(penaltiesPanel);

                    Panel valuePanel = new Panel
                    {
                        CssClass = "col-md-2"
                    };

                    TextBox tb = new TextBox
                    {
                        Text = invoiceSubcategory.InvoiceSubcategoryTypes != null ? invoiceSubcategory.Value.ToString() : string.Empty,
                        CssClass = "form-control",
                        ID = invoiceSubcategory.id_assCounter.HasValue ?
                                            invoiceSubcategory.id_assCounter.Value + "tbinvoiceSubcategory" + invoiceSubcategory.Id_subCategType :
                                            "tbinvoiceSubcategory" + invoiceSubcategory.Id_subCategType,
                        AutoCompleteType = AutoCompleteType.Disabled
                    };
                    valuePanel.Controls.Add(tb);
                    innerPanelMain.Controls.Add(valuePanel);


                    pnInvoiceSubcategories.Controls.Add(innerPanelMain);
                }
            }
        }

        private void InitializeInvoiceFields(Administratoro.DAL.Invoices theInvoice)
        {
            var descriptionName = string.Empty;
            if (theInvoice.AssociationExpenses == null)
            {
                var ee = AssociationExpensesManager.GetById(theInvoice.Id_EstateExpense.Value);
                descriptionName = ee.Expenses.Name + " " + ee.Year + " " + ee.Month;
            }
            txtInvoiceValue.Text = theInvoice.Value.HasValue ? theInvoice.Value.Value.ToString() : string.Empty;
            txtInvoiceDescription.Text = !string.IsNullOrEmpty(theInvoice.Description) ? theInvoice.Description : descriptionName;
            txtInvoiceDate.Text = theInvoice.issueDate.HasValue ? theInvoice.issueDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty;
            txtInvoiceNumber.Text = theInvoice.issueNumber;
        }

        #region initialize headers

        private void InitializeValueFieldAddColumnHeadersForIndexExpenses()
        {
            Panel panel1 = new Panel
            {
                CssClass = "col-md-4"
            };
            Label lb1 = new Label
            {
                Text = "Contoare"
            };
            panel1.Controls.Add(lb1);
            pnInvoiceValues.Controls.Add(panel1);

            Panel panel5 = new Panel
            {
                CssClass = "col-md-4"
            };
            Label lb5 = new Label
            {
                Text = "Index Vechi"
            };
            panel5.Controls.Add(lb5);
            pnInvoiceValues.Controls.Add(panel5);

            Panel panel6 = new Panel
            {
                CssClass = "col-md-4"
            };
            Label lb6 = new Label
            {
                Text = "Index nou"
            };
            panel6.Controls.Add(lb6);
            pnInvoiceValues.Controls.Add(panel6);
        }

        private void InitializeValueFieldAddColumnHeadersForDiverse()
        {
            Panel panel1 = new Panel
            {
                CssClass = "col-md-2"
            };
            Label lb1 = new Label
            {
                Text = "Valoare"
            };
            panel1.Controls.Add(lb1);
            pnlInvoiceDiverseValues.Controls.Add(panel1);

            Panel panel2 = new Panel
            {
                CssClass = "col-md-2"
            };
            Label lb2 = new Label
            {
                Text = "Descriere"
            };
            panel2.Controls.Add(lb2);
            pnlInvoiceDiverseValues.Controls.Add(panel2);

            Panel panel3 = new Panel
            {
                CssClass = "col-md-2"
            };
            Label lb3 = new Label
            {
                Text = "Împărțire"
            };
            panel3.Controls.Add(lb3);
            pnlInvoiceDiverseValues.Controls.Add(panel3);

            Panel panel4 = new Panel
            {
                CssClass = "col-md-2"
            };
            Label lb4 = new Label
            {
                Text = "Redistribuit"
            };
            panel4.Controls.Add(lb4);
            pnlInvoiceDiverseValues.Controls.Add(panel4);

            Panel panel5 = new Panel
            {
                CssClass = "col-md-2"
            };
            Label lb5 = new Label
            {
                Text = "Dată"
            };
            panel5.Controls.Add(lb5);
            pnlInvoiceDiverseValues.Controls.Add(panel5);

            Panel panel6 = new Panel
            {
                CssClass = "col-md-2"
            };
            Label lb6 = new Label
            {
                Text = "Număr"
            };
            panel6.Controls.Add(lb6);
            pnlInvoiceDiverseValues.Controls.Add(panel6);
        }

        #endregion

        #region diverse

        private void DiverseInitializeValueFieldAddInvoices(IEnumerable<Administratoro.DAL.Invoices> invoices)
        {
            foreach (var invoice in invoices)
            {
                Panel panelMain = new Panel
                {
                    CssClass = "col-md-12 col-xs-12 invoicesRow"
                };

                Panel panel1 = new Panel
                {
                    CssClass = "col-md-2"
                };

                TextBox tbValue = new TextBox
                {
                    CssClass = "form-control",
                    Text = invoice.Value.HasValue ? invoice.Value.Value.ToString() : string.Empty,
                    ID = invoice.Id != 0 ? invoice.Id + "tbInvoiceId" : "tbInvoiceId",
                    AutoCompleteType = AutoCompleteType.Disabled
                };
                panel1.Controls.Add(tbValue);

                Panel panel2 = new Panel
                {
                    CssClass = "col-md-2"
                };

                TextBox tbDescription = new TextBox
                {
                    CssClass = "form-control",
                    Text = invoice.Description,
                    ID = invoice.Id != 0 ? invoice.Id + "tbStairCase" + invoice.Id_StairCase : "-1tbStairCase",
                    AutoCompleteType = AutoCompleteType.Disabled
                };
                panel2.Controls.Add(tbDescription);

                Panel panel3 = new Panel
                {
                    CssClass = "col-md-2"
                };

                DropDownList drpStairCase = new DropDownList
                {
                    CssClass = "form-control",
                    ID = invoice.Id != 0 ? "drpStairCase" + invoice.Id : "-1drpStairCase"
                };

                drpStairCase.Items.Add(new ListItem
                {
                    Text = "Pe tot blocul",
                    Value = "",
                    Selected = !invoice.Id_StairCase.HasValue
                });

                foreach (var stairCase in Association.StairCases)
                {
                    drpStairCase.Items.Add(new ListItem
                    {
                        Text = "Scara " + stairCase.Nume,
                        Value = stairCase.Id.ToString(),
                        Selected = invoice.Id_StairCase.HasValue && invoice.Id_StairCase.Value == stairCase.Id
                    });
                }

                panel3.Controls.Add(drpStairCase);

                Panel panel4 = new Panel
                {
                    CssClass = "col-md-2"
                };

                DropDownList drpExpenseredistribute = new DropDownList
                {
                    CssClass = "form-control",
                    ID = invoice.Id != 0 ? "drpExpenseredistribute" + invoice.Id : "-1drpExpenseredistribute"
                };

                IEnumerable<AssociationExpensesRedistributionTypes> eert = ExpensesManager.GetRedistributiontypesForDiverse();

                foreach (var type in eert)
                {
                    drpExpenseredistribute.Items.Add(new ListItem
                    {
                        Text = type.Value,
                        Value = type.Id.ToString(),
                        Selected = invoice.id_Redistributiontype.HasValue ? invoice.id_Redistributiontype.Value == type.Id : false
                    });
                }

                drpExpenseredistribute.ID = "drpExpenseredistribute" + invoice.Id;
                panel4.Controls.Add(drpExpenseredistribute);

                /* panel 5 */
                Panel panel5 = new Panel
                {
                    CssClass = "col-md-2"
                };

                TextBox tbDate = new TextBox
                {
                    Text = invoice.issueDate.HasValue ? invoice.issueDate.Value.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) : string.Empty,
                    CssClass = "form-control datepicker",
                    ID = invoice.Id != 0 ? "tbDate" + invoice.Id : "-1tbDate",
                    AutoCompleteType = AutoCompleteType.Disabled
                };
                panel5.Controls.Add(tbDate);

                /* panel 6 */
                Panel panel6 = new Panel
                {
                    CssClass = "col-md-2"
                };
                TextBox tbIssueNr = new TextBox
                {
                    Text = invoice.issueNumber,
                    CssClass = "form-control",
                    ID = invoice.Id != 0 ? "tbIssueNr" + invoice.Id : "-1tbIssueNr",
                    AutoCompleteType = AutoCompleteType.Disabled
                };
                panel6.Controls.Add(tbIssueNr);

                panelMain.Controls.Add(panel1);
                panelMain.Controls.Add(panel2);
                panelMain.Controls.Add(panel3);
                panelMain.Controls.Add(panel4);
                panelMain.Controls.Add(panel5);
                panelMain.Controls.Add(panel6);

                pnlInvoiceDiverseValues.Controls.Add(panelMain);
            }
        }

        #endregion

        protected void drpInvoiceCounters_SelectedIndexChanged(object sender, EventArgs e)
        {
            var yearMonth = drpInvoiceYearMonth.SelectedValue.Split('-');
            int assCounterId;

            if (pnlInvoiceCounter.Visible && yearMonth.Length == 2 && int.TryParse(drpInvoiceCounters.SelectedValue, out assCounterId))
            {
                int year;
                int month;
                int expenseId;
                if (int.TryParse(drpInvoiceExpenses.SelectedValue, out expenseId) &&
                    int.TryParse(yearMonth[0], out year) &&
                    int.TryParse(yearMonth[1], out month))
                {
                    AssociationExpenses associationExpense = AssociationExpensesManager.GetMonthYearAssoiationExpense(Association.Id, expenseId, year, month);
                    var assCounter = AssociationCountersManager.GetById(assCounterId);

                    if (associationExpense != null && assCounter != null)
                    {
                        var theInvoice = InvoicesManager.GetByAssociationExpenseIdAndCounter(associationExpense.Id, assCounter.Id);
                        if (theInvoice != null)
                        {
                            InitializeInvoiceFields(theInvoice);
                        }
                    }
                }
            }
        }
    }
}