using Administratoro.BL.Constants;
using Administratoro.BL.Extensions;
using Administratoro.BL.Managers;
using Administratoro.DAL;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Admin.Reports
{
    public partial class Flyers : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!Page.IsPostBack)
            {
                InitializeYearMonth();
            }
        }

        private void InitializeYearMonth()
        {
            drpAvailableMonths.Items.Clear();
            var yearMonths = AssociationExpensesManager.GetAllMonthsAndYearsAvailableByAssociationId(Association.Id);
            for (int i = 0; i < yearMonths.Count; i++)
            {
                var yearMonth = yearMonths[i];
                drpAvailableMonths.Items.Add(new System.Web.UI.WebControls.ListItem
                {
                    Value = yearMonth.Year + "-" + yearMonth.Month,
                    Text = "Anul:" + yearMonth.Year + " Luna:" + yearMonth.Month,
                });
            }
        }

        protected void btnPreview_Click(object sender, EventArgs e)
        {

        }

        protected void btnEmail_Click(object sender, EventArgs e)
        {
            btnEmail_Confirm.Visible = true;
            StringBuilder sb = new StringBuilder();

            foreach (var apartment in Association.Apartments)
            {
                if (string.IsNullOrEmpty(apartment.Email))
                {
                    sb.Append("Apartamentul " + apartment.Number + " nu are email-ul setat <br>");
                }
            }

            lblMessage.Text = sb.ToString();
            lblMessage.Style.Add("color", "red");
        }
        
        protected void btnEmail_Confirm_Click(object sender, EventArgs e)
        {
            foreach (var apartment in Association.Apartments)
            {
                if(!string.IsNullOrEmpty(apartment.Email))
                {
                    EmailsManager.SendEmail("adrian10nagy@gmail.com", "adrian10nagy@gmail.com", "Fluturasi de cheltuieli pentru luna Mai", "Buna ziua " + apartment.Name + ", <br> <br> Atasat gasiti fluturasii cu cheltuieli pentru luna mai 2018. <br> Cu stima, asociatia de proprietari" );
                }
            }

            btnEmail_Confirm.Visible = false;
            lblMessage.Text = "Email-uri trimise cu success<br>";
            lblMessage.Style.Add("color", "green");
        }

        protected void btnDownload_Click(object sender, EventArgs e)
        {
            //server folder path which is stored your PDF documents       
            string path = Server.MapPath("");
            string filename = path + "\\Doc1.pdf";


            var apartments = ApartmentsManager.Get(Association.Id);

            //Create new PDF document
            Document document = new Document(PageSize.A4, 20f, 20f, 20f, 20f);

            try
            {
                PdfWriter.GetInstance(document, new FileStream(filename, FileMode.Create));
                document.Open();

                foreach (var apartment in apartments)
                {

                    document.Add(new Paragraph(Association.Name));
                    document.Add(new Paragraph("Apartamentul: " + apartment.Number));

                    var associationExpensesGrouped = AssociationExpensesManager.GetByMonthAndYearNotDisabled(Association.Id, GetYear(), GetMonth())
                        .GroupBy(ee => ee.Id_ExpenseType).OrderBy(er => er.Key);

                    foreach (var assocExpenses in associationExpensesGrouped)
                    {
                        // add header
                        document.Add(new Paragraph("Cheltuielile de tipul " + assocExpenses.FirstOrDefault().ExpenseTypes.Name));

                        if (assocExpenses.FirstOrDefault().Id_ExpenseType == (int)ExpenseType.PerIndex)
                        {
                            foreach (var assocExpense in assocExpenses)
                            {
                                var apExpenses = assocExpense.ApartmentExpenses.Where(w => w.Id_Tenant == apartment.Id).ToList();
                                if (apExpenses.Count() > 0)
                                {
                                    PdfPTable table = AddIndexTable(apExpenses);
                                    document.Add(table);
                                }
                            }
                        }
                        else if (assocExpenses.FirstOrDefault().Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
                        {
                            PdfPTable table = AddCotaTable(assocExpenses.ToList(), apartment.Id);
                            document.Add(table);
                        }
                        else if (assocExpenses.FirstOrDefault().Id_ExpenseType == (int)ExpenseType.PerNrTenants)
                        {
                            PdfPTable table = AddTenantsTable(assocExpenses.ToList(), apartment.Id);
                            document.Add(table);
                        }
                        // add rows
                        // add subtotal
                    }

                    document.NewPage();

                }
            }
            catch (Exception)
            {

            }
            finally
            {
                document.Close();
                ShowPdf(filename);
            }

        }

        private int GetYear()
        {
            var yearMonth = drpAvailableMonths.SelectedValue.Split('-');
            if (yearMonth.Length == 2)
            {
                int year;

                if (int.TryParse(yearMonth[0], out year))
                {
                    return year;
                }
            }

            throw new NotImplementedException();
        }

        private int GetMonth()
        {
            var yearMonth = drpAvailableMonths.SelectedValue.Split('-');
            if (yearMonth.Length == 2)
            {
                int month;

                if (int.TryParse(yearMonth[1], out month))
                {
                    return month;
                }
            }

            throw new NotImplementedException();
        }

        private static PdfPTable AddTenantsTable(List<AssociationExpenses> assocExpenses, int apartmentId)
        {
            PdfPTable table = new PdfPTable(2);
            table.TotalWidth = 550f;
            //fix the absolute width of the table
            table.LockedWidth = true;
            table.HorizontalAlignment = 1;

            //leave a gap before and after the table
            table.SpacingBefore = 10f;
            table.SpacingAfter = 10f;

            table.AddCell("Cheltuială");
            table.AddCell("Valoare");
            decimal? sum = null;

            foreach (var assocExpense in assocExpenses)
            {
                table.AddCell(assocExpense.Expenses.Name);
                if (assocExpense.ApartmentExpenses.Count() != 0)
                {
                    foreach (var item in assocExpense.ApartmentExpenses.Where(ae => ae.Id_Tenant == apartmentId))
                    {
                        if (item.Value.HasValue)
                        {
                            sum = sum.HasValue ? (sum + item.Value) : item.Value;
                            table.AddCell(ConvertToDecimalPrintable(item.Value, 4));
                        }
                    }
                }
                else
                {
                    table.AddCell(string.Empty);
                }
            }

            table.AddCell("TOTAL");
            table.AddCell(ConvertToDecimalPrintable(sum));

            return table;
        }

        private static PdfPTable AddCotaTable(List<AssociationExpenses> assocExpenses, int apartmentId)
        {
            PdfPTable table = new PdfPTable(2);
            table.TotalWidth = 550f;
            //fix the absolute width of the table
            table.LockedWidth = true;
            table.HorizontalAlignment = 1;

            //leave a gap before and after the table
            table.SpacingBefore = 10f;
            table.SpacingAfter = 10f;

            table.AddCell("Cheltuială");
            table.AddCell("Valoare");
            decimal? sum = null;

            foreach (var assocExpense in assocExpenses)
            {
                table.AddCell(assocExpense.Expenses.Name);
                if (assocExpense.ApartmentExpenses.Count() != 0)
                {
                    foreach (var item in assocExpense.ApartmentExpenses.Where(ae => ae.Id_Tenant == apartmentId))
                    {
                        if (item.Value.HasValue)
                        {
                            sum = sum.HasValue ? (sum + item.Value) : item.Value;
                            table.AddCell(ConvertToDecimalPrintable(item.Value, 4));
                        }
                    }
                }
                else
                {
                    table.AddCell(string.Empty);
                }
            }

            table.AddCell("TOTAL");
            table.AddCell(ConvertToDecimalPrintable(sum));

            return table;
        }

        private static PdfPTable AddIndexTable(List<ApartmentExpenses> apExpenses)
        {
            PdfPTable table = new PdfPTable(6);
            table.TotalWidth = 550f;
            //fix the absolute width of the table
            table.LockedWidth = true;
            table.HorizontalAlignment = 1;

            //leave a gap before and after the table
            table.SpacingBefore = 10f;
            table.SpacingAfter = 10f;

            table.AddCell(apExpenses.FirstOrDefault().AssociationExpenses.Expenses.Name);
            table.AddCell("Index vechi");
            table.AddCell("Index nou");
            table.AddCell("Consum");
            table.AddCell("Pret / m2");
            table.AddCell("Valoare");

            decimal? sumIndexOld = null;
            decimal? sumIndexNew = null;
            decimal? sumConsum = null;
            decimal? sumValue = null;

            foreach (var apExpense in apExpenses)
            {
                table.AddCell("");
                table.AddCell(ConvertToDecimalPrintable(apExpense.IndexOld, 1));
                table.AddCell(ConvertToDecimalPrintable(apExpense.IndexNew, 1));
                table.AddCell(ConvertToDecimalPrintable((apExpense.IndexNew - apExpense.IndexOld), 1));
                table.AddCell(string.Empty);
                //table.AddCell(ConvertToDecimalPrintable(apExpense.AssociationExpenses.PricePerExpenseUnit, 4));
                //var subValue = ((apExpense.IndexNew - apExpense.IndexOld) * apExpense.AssociationExpenses.PricePerExpenseUnit);
                //table.AddCell(ConvertToDecimalPrintable(subValue, 4));
                table.AddCell(string.Empty);

                if (apExpense.IndexOld.HasValue)
                {
                    sumIndexOld = sumIndexOld.HasValue ? (sumIndexOld + apExpense.IndexOld.Value) : apExpense.IndexOld.Value;
                }

                if (apExpense.IndexNew.HasValue)
                {
                    sumIndexNew = sumIndexNew.HasValue ? (sumIndexNew + apExpense.IndexNew.Value) : apExpense.IndexNew.Value;
                }

                if (apExpense.IndexNew.HasValue && apExpense.IndexOld.HasValue)
                {
                    sumConsum = sumConsum.HasValue ? (sumConsum + (apExpense.IndexNew - apExpense.IndexOld)) : (apExpense.IndexNew - apExpense.IndexOld);
                }

                //if (subValue.HasValue)
                //{
                //    sumValue = sumValue.HasValue ? (sumValue + subValue) : subValue;
                //}
            }

            table.AddCell("TOTAL");
            table.AddCell(ConvertToDecimalPrintable(sumIndexOld));
            table.AddCell(ConvertToDecimalPrintable(sumIndexNew));
            table.AddCell(ConvertToDecimalPrintable(sumConsum));
            table.AddCell("");
            table.AddCell(ConvertToDecimalPrintable(sumValue));

            return table;
        }

        private static string ConvertToDecimalPrintable(decimal? theValue, int places = 2)
        {
            var result = theValue.HasValue ? DecimalExtensions.RoundUp((double)theValue, places).ToString("N" + places) : string.Empty;

            return result;
        }

        public void ShowPdf(string filename)
        {

            //Clears all content output from Buffer Stream

            Response.ClearContent();

            //Clears all headers from Buffer Stream

            Response.ClearHeaders();

            //Adds an HTTP header to the output stream

            Response.AddHeader("Content-Disposition", "inline;filename=" + filename);

            //Gets or Sets the HTTP MIME type of the output stream

            Response.ContentType = "application/pdf";

            //Writes the content of the specified file directory to an HTTP response output stream as a file block

            Response.WriteFile(filename);

            //sends all currently buffered output to the client

            Response.Flush();

            //Clears all content output from Buffer Stream

            Response.Clear();

        }
    }
}