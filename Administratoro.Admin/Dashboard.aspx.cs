
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
            if (Estate.HasStaircase)
            {
                decimal sum = 0;
                foreach (var stairCase in Estate.StairCases)
                {
                    if (stairCase.Indiviza.HasValue)
                    {
                        sum = sum + stairCase.Indiviza.Value;
                    }
                    else
                    {
                        sb.Append("- Scara cu numele <b>" + stairCase.Nume + "</b> nu are indiviza setată <a href='Associations/Index.aspx'>Modifică</a><br />");
                    }
                }

                if (sum != 100.0m)
                {
                    //sb.Append("- Suma cotelor de indiviză a scărilor este de <b>" + sum.ToString() + "</b>. Trebuie sa fie <b>100 (100%)</b>  <a href='Associations/Index.aspx'>Modifică</a><br />");
                }

                if(string.IsNullOrEmpty(sb.ToString()))
                {
                    sb.Append("- nici o notă/atenționare");
                }
            }
        }
    }
}