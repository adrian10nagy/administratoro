
namespace Administratoro.BL.Managers
{
    using System;
    using DAL;
    using DAL.SDK;
    using System.Collections.Generic;
    using System.Text;
    using Toolbox.DocumentGenerator;

    public static class RegistriesHome
    {
        private static AdministratoroEntities _administratoroEntities;

        private static AdministratoroEntities GetContext(bool shouldRefresh = false)
        {
            if (_administratoroEntities == null || shouldRefresh)
            {
                _administratoroEntities = new AdministratoroEntities();
            }

            return _administratoroEntities;
        }

        public static void Add(DAL.RegistriesHome registryHome)
        {
            GetContext().RegistriesHome.Add(registryHome);
            GetContext().SaveChanges();
        }

        public static byte[] GenerateDailyReport(int associationId, DateTime date)
        {
            var stringFormatH1 = "<h1>{0}</h1>";
            var stringFormatH2 = "<h2>{0}</h2>";
            var soldInitial = 100;
            var nrCrt = 0;
            decimal rulajIncome = 0;
            decimal rulajOutcome = 0;
            var sb = new StringBuilder();

            var registries = GetByDateTime(associationId, date);
            sb.Append(string.Format(stringFormatH1,"Jurnal de casa " + date.ToShortDateString()));
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
                sb.Append("<td>" + registry.TransactionDate.ToShortDateString() + "</td>");
                sb.Append("<td>" + registry.DocumentNr + "</td>");
                sb.Append("<td>" + registry.Explanations + "</td>");
                sb.Append("<td>" + income + "</td>");
                sb.Append("<td>" + outcome + "</td>");
                sb.Append("<td>" + 0 + "</td>");
                sb.Append("</tr>");
            }
            var rulaj = rulajIncome - rulajOutcome;
            sb.Append("</table>");
            sb.Append("<br><br>");
            sb.Append(string.Format(stringFormatH2,"Sold initial: " + soldInitial));
            sb.Append(string.Format(stringFormatH2,"Rulaj: " + rulaj));
            sb.Append(string.Format(stringFormatH2,"Sold final: " + (soldInitial + rulaj)));

            return ConverterHelper.ConvertHtMLtoBytes(sb.ToString());
        }

        private static IEnumerable<DAL.RegistriesHome> GetByDateTime(int associationId, DateTime date)
        {
            return Kit.Instance.RegistriesHome.GetByAssandDate(associationId, date);
        }
    }
}
