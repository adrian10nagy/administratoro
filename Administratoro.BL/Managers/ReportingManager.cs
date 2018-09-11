using Administratoro.BL.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Toolbox.DocumentGenerator;

namespace Administratoro.BL.Managers
{
    public static class ReportingManager
    {
        public static byte[] GenerateDailyReport(int associationId, DateTime date)
        {
            var stringFormatH1 = "<h1>{0}</h1>";
            var stringFormatH2 = "<h2>{0}</h2>";
            var nrCrt = 0;
            decimal rulajIncome = 0;
            decimal rulajOutcome = 0;
            var sb = new StringBuilder();

            var dailyregistry = RegistriesHomeDailyManager.Get(associationId, date);
            if (dailyregistry == null) { return ConverterHelper.ConvertHtMLtoBytes(sb.ToString()); }
            var soldInitial = dailyregistry.OpeningAmount;

            var registries = RegistriesHomeManager.GetByRegHomeDaily(dailyregistry.Id);

            sb.Append(string.Format(stringFormatH1, "Jurnal de casa " + date.ToShortDateString()));
            sb.Append("<br><br>");
            sb.Append("<table border='1'>");
            sb.Append("<tr>");
            sb.Append("<td>Nr Crt</td><td>Data</td><td>Nr. Document</td><td>Explicatie</td><td>Incasare</td><td>Plati</td><td>Sold</td>");
            sb.Append("</tr>");

            foreach (var registry in registries)
            {
                nrCrt++;
                var income = (registry.Income.HasValue) ? registry.Income.Value.ToString() : string.Empty;
                var outcome = (registry.Outcome.HasValue) ? registry.Outcome.Value.ToString() : string.Empty;
                rulajIncome = rulajIncome + registry.Income ?? rulajIncome;
                rulajOutcome = rulajOutcome + registry.Outcome ?? rulajOutcome;

                sb.Append("<tr>");
                sb.Append("<td>" + nrCrt + "</td>");
                sb.Append("<td>" + date.ToShortDateString() + "</td>");
                sb.Append("<td>" + registry.DocumentNr + "</td>");
                sb.Append("<td>" + registry.Explanations + "</td>");
                sb.Append("<td>" + income + "</td>");
                sb.Append("<td>" + outcome + "</td>");
                sb.Append("<td>" + (soldInitial + rulajIncome - rulajOutcome) + "</td>");
                sb.Append("</tr>");
            }
            var rulaj = rulajIncome - rulajOutcome;
            sb.Append("</table>");
            sb.Append("<br><br>");
            sb.Append(string.Format(stringFormatH2, "Sold initial: " + soldInitial));
            sb.Append(string.Format(stringFormatH2, "Rulaj: " + rulaj));
            sb.Append(string.Format(stringFormatH2, "Sold final: " + (soldInitial + rulaj)));

            return ConverterHelper.ConvertHtMLtoBytes(sb.ToString());
        }

        public static byte[] GenerateFondsReport(int apartmentId, DebtType debtType)
        {
            var stringFormatH1 = "<h1>{0}</h1>";
            var stringFormatH2 = "<h2>{0}</h2>";
            var nrCrt = 0;
            decimal rulajIncome = 0;
            decimal rulajOutcome = 0;
            var sb = new StringBuilder();

            var apartament = ApartmentsManager.GetById(apartmentId);

            if (apartament == null) { return ConverterHelper.ConvertHtMLtoBytes(sb.ToString()); }

            var debts = ApartmentDebtsManager.GetAllPaidOfType(apartmentId, debtType);

            if(debtType == DebtType.RulmentFond && apartament.FondRulment.HasValue)
            {
                sb.Append(string.Format(stringFormatH2, apartament.FondRulment));
            }
            else if (debtType == DebtType.Repairfond && apartament.FondReparatii.HasValue)
            {
                sb.Append(string.Format(stringFormatH2, apartament.FondReparatii));
            }

            sb.Append(string.Format(stringFormatH1, "Fișă fond de reparații "));
            sb.Append("<br><br>");
            sb.Append("<table border='1'>");
            sb.Append("<tr>");
            sb.Append("<td>Nr Crt</td><td>Data</td><td>Explicații</td><td>Încasat</td><td>Restituit</td><td>Sold contabil</td><td>Sold restant</td>");
            sb.Append("</tr>");

            foreach (var debt in debts)
            {
                nrCrt++;
                var income = (debt.Value.HasValue) ? debt.Value.Value.ToString() : string.Empty;
                var createdDate = (debt.created.HasValue) ? debt.created.Value.ToShortDateString() : string.Empty;

                sb.Append("<tr>");
                sb.Append("<td>" + nrCrt + "</td>");
                sb.Append("<td>" + createdDate + "</td>");
                sb.Append("<td>" + debt.Year + " " + debt.Month + "</td>");
                sb.Append("<td>" + income + "</td>");
                sb.Append("<td>" + "- </td>");
                sb.Append("<td>" + "</td>");
                sb.Append("<td>" + "</td>");
                sb.Append("</tr>");
            }

            sb.Append("</table>");
            sb.Append("<br><br>");

            return ConverterHelper.ConvertHtMLtoBytes(sb.ToString());
        }
    }
}
