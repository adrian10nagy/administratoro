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
            foreach (var apartment in Association.Apartments)
            {
                if (!string.IsNullOrEmpty(apartment.Email))
                {
                    var filePath = string.Format("C:\\Users\\Adrian\\Documents\\fluturasi\\p{0}.pdf", apartment.Number);

                    string message = @"Buna ziua, <br> <br> 
Vă  facem  cunoscut  că  s-au  afișat  listele  de  cheltuileli  aferente  lunii  mai  2018.<br> 
Nota  de  plată  aferentă  apartamentului  dumneavoastră  este  anexata prezentului  mail.<br> 
Vă  rugăm  să  efectuați  plata  în  contul  acociației  deschis  la  Banca  Transilvania  Cluj  cu  IBAN  RO13BTRLRONCRT0409298101  specificând  numărul  
apartamentului  pentru  care  faceți  plata.  Pentru  încasari  în  numerar  vă  așteptăm  joi  19.07.2018  între  orele  19.30 - 21  la  etajul  tehnic.<br> <br> 
Termenul  scadent  este  28.07.2018.<br> <br> 
PS:<br> 
<b>MIERCURI  11.07.2018, ORA   19.30</b>   SE  CONVOACĂ   <b>ADUNAREA  GENERALĂ</b>, CU  ORDINEA  DE  ZI:<br>
1.RAPORT  DE  ACTIVITATE  COMITET  EXECUTIV <br>
2.PROPUNERI  RECALCULARE  COSTURI  AGENT  TERMIC  PENTRU  SEZONUL  RECE  2017 – 2018  SAU  ASUMAREA  ACTIONARII  IN  JUDECATA  A  ASOCIATIEI  DE  PROPRIETARI  SI  INSUSIREA  SENTINTEI  JUDECATORESTI<br>
3.ALTE  PROBLEME  ORGANIZATORICE  <br><br>

<b>DACA  NU  SE  INTRUNESTE  CVORUMUL  DE  50% + 1  DINTRE  MEMBRII  ASOCIATIEI,  SEDINTA  SE  VA  RECONVOCA  PENTRU  DATA   DE  MIERCURI  11.07.2018  ORA  20.30  CAND  HOTARARILE  SE  VOR  LUA  CU  VOTUL  MAJORITATII  CELOR  PREZENTI.</b>
<b>CHIRIASII  AU  OBLIGATIA  SA  ANUNTE  PROPRIETARII  DE  DATA  SI  ORA  ADUNARII  GENERALE.</b>                           
<br><br>
COMITET  EXECUTIV <br>03.07.2018";

                    EmailsManager.SendEmail("asociatie.online@gmail.com", apartment.Email, "Fluturasi de cheltuieli pentru luna Mai 2018", message, filePath);
                }
            }

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

            //Create new PDF document
            Document document = new Document(PageSize.A4, 80f, 80f, 20f, 20f);
            using (Document doc = new Document())
            {
                MemoryStream msPDFData = new MemoryStream();
                PdfWriter writer = PdfWriter.GetInstance(doc, msPDFData);
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
                    PdfPTable tblHeader = new PdfPTable(2) {WidthPercentage = 100};
                    tblHeader.AddCell(getCell("ASOCIAIA DE PROPRIETARI " + association.Name + ", CF " + association.FiscalCode, PdfPCell.ALIGN_LEFT));
                    tblHeader.AddCell(getCell("CONTUL BANCAR " + association.BanckAccont, PdfPCell.ALIGN_RIGHT));
                    document.Add(tblHeader);
                    document.Add(new Phrase("\n"));

                    PdfPTable tbAp = new PdfPTable(4);
                    tbAp.WidthPercentage = 100;
                    tbAp.AddCell(getCell("Ap.: " + apartment.Number, PdfPCell.ALIGN_CENTER));
                    tbAp.AddCell(getCell("Nume: " + apartment.Name, PdfPCell.ALIGN_CENTER));
                    tbAp.AddCell(getCell("Cota: " + (apartment.CotaIndiviza.HasValue ? apartment.CotaIndiviza.Value.ToString(CultureInfo.InvariantCulture) : string.Empty), PdfPCell.ALIGN_CENTER));
                    tbAp.AddCell(getCell("Nr. Pers: " + apartment.Dependents, PdfPCell.ALIGN_CENTER));
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

        public PdfPCell getCell(String text, int alignment)
        {
            PdfPCell cell = new PdfPCell(new Phrase(text));
            cell.HorizontalAlignment = alignment;
            cell.Border = 0; ;
            return cell;
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

            return table;
        }

        private static PdfPTable AddCotaTable(List<AssociationExpenses> assocExpenses, int apartmentId)
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

            return table;
        }

        private static PdfPTable AddIndexTable(List<ApartmentExpenses> apExpenses)
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