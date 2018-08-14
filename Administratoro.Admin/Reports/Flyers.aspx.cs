using Administratoro.BL.Constants;
using Administratoro.BL.Extensions;
using Administratoro.BL.Managers;
using Administratoro.DAL;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Font = iTextSharp.text.Font;

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
            EmailsManager.SendMonthlyEmails(Association.Apartments, "August" , 2018);

            btnEmail_Confirm.Visible = false;
            lblMessage.Text = "Email-uri trimise cu success<br>";
            lblMessage.Style.Add("color", "green");
        }

        protected void btnDownload_Click(object sender, EventArgs e)
        {
            BaseFont bfTimes = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1250, false);

            Font times = new Font(bfTimes, 12, Font.ITALIC, BaseColor.BLACK);

            //server folder path which is stored your PDF documents       
            string path = Server.MapPath("");
            string filename = path + "\\Doc1.pdf";


            var apartments = ApartmentsManager.Get(Association.Id);
            Font boldFont = new Font(null, 12, Font.BOLD);

            //Create new PDF document
            Document document = new Document(PageSize.A4, 80f, 80f, 20f, 20f);
            using (Document doc = new Document())
            {
                MemoryStream msPDFData = new MemoryStream();
                doc.Open();
                doc.Add(new Paragraph("I'm a pdf!"));
                byte[] pdfData = msPDFData.ToArray();

            }

            var association = Association;

            try
            {
                PdfWriter.GetInstance(document, new FileStream(filename, FileMode.Create));
                document.Open();

                foreach (var apartment in apartments)
                {
                    decimal? sumToPay = 0;
                    PdfPTable tblHeader = new PdfPTable(2) { WidthPercentage = 100 };
                    tblHeader.AddCell(GetCell("ASOCIATIA DE PROPRIETARI " + association.Name + ", CF " + association.FiscalCode, PdfPCell.ALIGN_LEFT));
                    tblHeader.AddCell(GetCell("CONTUL BANCAR " + association.BanckAccont, PdfPCell.ALIGN_RIGHT));
                    document.Add(tblHeader);
                    document.Add(new Phrase("\n"));

                    PdfPTable tbAp = new PdfPTable(4);
                    tbAp.WidthPercentage = 100;
                    tbAp.AddCell(GetCell("Ap.: " + apartment.Number, PdfPCell.ALIGN_CENTER));
                    tbAp.AddCell(GetCell("Nume: " + apartment.Name, PdfPCell.ALIGN_CENTER));
                    tbAp.AddCell(GetCell("Cota: " + (apartment.CotaIndiviza.HasValue ? apartment.CotaIndiviza.Value.ToString(CultureInfo.InvariantCulture) : string.Empty), PdfPCell.ALIGN_CENTER));
                    tbAp.AddCell(GetCell("Nr. Pers: " + apartment.Dependents, PdfPCell.ALIGN_CENTER));
                    document.Add(tbAp);

                    document.Add(new Phrase("\n"));

                    var associationExpensesGrouped = AssociationExpensesManager.GetByMonthAndYearNotDisabled(association.Id, GetYear(), GetMonth())
                        .GroupBy(ee => ee.Id_ExpenseType).OrderBy(er => er.Key);

                    foreach (var assocExpenses in associationExpensesGrouped)
                    {
                        // add header
                        document.Add(new Paragraph("Cheltuielile de tipulîățșț " + assocExpenses.FirstOrDefault().ExpenseTypes.Name, times));

                        if (assocExpenses.FirstOrDefault().Id_ExpenseType == (int)ExpenseType.PerIndex)
                        {
                            foreach (var assocExpense in assocExpenses)
                            {
                                var apExpenses = assocExpense.ApartmentExpenses.Where(w => w.Id_Tenant == apartment.Id).ToList();
                                if (apExpenses.Any())
                                {
                                    sumToPay = sumToPay + AddIndexTable(document, apExpenses, apartment, assocExpense.Id);
                                }
                            }
                        }
                        else if (assocExpenses.FirstOrDefault().Id_ExpenseType == (int)ExpenseType.PerCotaIndiviza)
                        {
                            sumToPay = sumToPay + AddCotaTable(document, assocExpenses.ToList(), apartment.Id);
                        }
                        else if (assocExpenses.FirstOrDefault().Id_ExpenseType == (int)ExpenseType.PerNrTenants)
                        {
                            sumToPay= sumToPay + AddTenantsTable(document, assocExpenses.ToList(), apartment.Id);
                        }
                        // add rows
                        // add subtotal
                    }

                    document.Add(new Paragraph("TOTAL DE PLATA: " + sumToPay, boldFont));


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

        public PdfPCell GetCell(String text, int alignment)
        {
            return new PdfPCell(new Phrase(text))
            {
                HorizontalAlignment = alignment,
                Border = 0
            };
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

        private static decimal? AddTenantsTable(Document document, List<AssociationExpenses> assocExpenses, int apartmentId)
        {
            Font font = new Font(null, 12, Font.BOLD);
            PdfPTable table = new PdfPTable(2);
            table.TotalWidth = 450f;
            //fix the absolute width of the table
            table.LockedWidth = true;
            table.HorizontalAlignment = 1;

            //leave a gap before and after the table
            table.SpacingBefore = 10f;
            table.SpacingAfter = 10f;

            table.AddCell("Cheltuiala");
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

            var tc = new Phrase("TOTAL", font);
            table.AddCell(tc);
            table.AddCell(ConvertToDecimalPrintable(sum));
            document.Add(table);

            return sum;
        }

        private static decimal? AddCotaTable(Document document, List<AssociationExpenses> assocExpenses, int apartmentId)
        {
            PdfPTable table = new PdfPTable(2);
            table.TotalWidth = 450f;
            //fix the absolute width of the table
            table.LockedWidth = true;
            table.HorizontalAlignment = 1;

            //leave a gap before and after the table
            table.SpacingBefore = 10f;
            table.SpacingAfter = 10f;

            table.AddCell("Cheltuiala");
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
           
            document.Add(table);

            return sum;
        }

        private static decimal? AddIndexTable(Document document, List<ApartmentExpenses> apExpenses, Administratoro.DAL.Apartments apartment, int assExpenseId)
        {
            PdfPTable table = new PdfPTable(6);
            table.TotalWidth = 450f;
            //fix the absolute width of the table
            table.LockedWidth = true;
            table.HorizontalAlignment = PdfPCell.ALIGN_CENTER;

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
                decimal? subValue = null;
                table.AddCell(apExpense.CounterOrder.HasValue ? "Contor " + apExpense.CounterOrder.Value : string.Empty);
                table.AddCell(ConvertToDecimalPrintable(apExpense.IndexOld, 1));
                table.AddCell(ConvertToDecimalPrintable(apExpense.IndexNew, 1));
                table.AddCell(ConvertToDecimalPrintable((apExpense.IndexNew - apExpense.IndexOld), 1));

                var pricePerUnit = UnitPricesManager.Get(apExpense.AssociationExpenses.Id, apExpense.Apartments.Id_StairCase);
                if (pricePerUnit != null)
                {
                    table.AddCell(ConvertToDecimalPrintable(pricePerUnit.PricePerExpenseUnit, 4));
                    subValue = ((apExpense.IndexNew - apExpense.IndexOld) * pricePerUnit.PricePerExpenseUnit);
                    table.AddCell(ConvertToDecimalPrintable(subValue, 4));
                }
                else
                {
                    table.AddCell(string.Empty);
                    table.AddCell(string.Empty);
                }

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

                if (subValue.HasValue)
                {
                    sumValue = sumValue.HasValue ? (sumValue + subValue) : subValue;
                }
            }

            var apartmentExpenseRedistributionValue = RedistributionManager.CalculateRedistributeValueForStairCase(
                assExpenseId, apartment, apExpenses);

            if(apartmentExpenseRedistributionValue.HasValue)
            {

                table.AddCell("Diferenta");
                table.AddCell(string.Empty);
                table.AddCell(string.Empty);
                table.AddCell(string.Empty);
                table.AddCell(string.Empty);
                table.AddCell(ConvertToDecimalPrintable(apartmentExpenseRedistributionValue));
                
                sumValue = sumValue + apartmentExpenseRedistributionValue;
            }

            table.AddCell("TOTAL");
            table.AddCell(ConvertToDecimalPrintable(sumIndexOld));
            table.AddCell(ConvertToDecimalPrintable(sumIndexNew));
            table.AddCell(ConvertToDecimalPrintable(sumConsum));
            table.AddCell("");
            table.AddCell(ConvertToDecimalPrintable(sumValue));

            document.Add(table);

            return sumValue;
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