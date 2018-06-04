
namespace Admin
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Web.Script.Serialization;
    using System.Web.Script.Services;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public partial class _Default : BasePage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["message"] != null)
            {
                if (Request.QueryString["message"] == "newEstate")
                {
                    dashboardMessage.InnerText = "Asociație adăugată cu succes!";
                    dashboardMessage.Attributes.Add("style", "background-color: #2ecc71; padding:10px; color:white");
                }
            }
            PopulateNotesWarnings();
        }

        private void PopulateNotesWarnings()
        {
            StringBuilder sb = new StringBuilder();
            CheckStairCases(sb);
            lblNotesWarnings.Text = sb.ToString();
        }

        private void CheckStairCases(StringBuilder sb)
        {
            decimal sumStairCaseIndiviza = 0;
            decimal sumApartmentIndiviza = 0;

            if (Association.HasStaircase)
            {
                foreach (var stairCase in Association.StairCases)
                {
                    if (stairCase.Indiviza.HasValue)
                    {
                        sumStairCaseIndiviza = sumStairCaseIndiviza + stairCase.Indiviza.Value;
                    }
                    else
                    {
                        sb.Append("- Scara cu numele <b>" + stairCase.Nume + "</b> nu are indiviza setată <a href='Associations/Index.aspx'>Modifică</a><br />");
                    }
                }
            }

            if (sumStairCaseIndiviza != 100.0m)
            {
                sb.Append("- Suma cotelor de indiviză a scărilor este de <b>" + sumStairCaseIndiviza.ToString() + "</b>. Trebuie sa fie <b>100 (100%)</b>  <a href='Associations/Index.aspx'>Modifică</a><br />");
            }

            foreach (var apartment in Association.Apartments)
            {
                if (apartment.CotaIndiviza.HasValue)
                {
                    sumApartmentIndiviza = sumApartmentIndiviza + apartment.CotaIndiviza.Value;
                }
                else
                {
                    sb.Append("- Apartamentul cu numele <b>" + apartment.Name+ "</b> nu are indiviza setată <a href='Apartments/Add.aspx?apartmentid=" 
                        + apartment.Id + "'>Modifică</a><br />");
                }

            }

            if (string.IsNullOrEmpty(sb.ToString()))
            {
                sb.Append("- nici o notă/atenționare");
            }
        }
    }
}