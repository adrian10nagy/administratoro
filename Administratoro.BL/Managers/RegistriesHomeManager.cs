
namespace Administratoro.BL.Managers
{
    using System;
    using DAL;
    using DAL.SDK;
    using System.Collections.Generic;
    using System.Text;
    using Toolbox.DocumentGenerator;
    using Administratoro.BL.Constants;

    public static class RegistriesHomeManager
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

     

        public static IEnumerable<DAL.RegistriesHome> GetByRegHomeDaily(int regHomeDailyId)
        {
            return Kit.Instance.RegistriesHome.GetByRegHomeDaily(regHomeDailyId);
        }

        public static void RegisterPay(decimal sumOfChecked, decimal totalToPay, int apartmentId, List<string> whatToPay, string documentNr,
            string explanations, string partiallyPay, int associationId, DateTime addedDate)
        {
            int? dailyRegId = null;
            var dailyReg = RegistriesHomeDailyManager.Get(associationId, addedDate);
            if(dailyReg == null)
            {
                dailyRegId = RegistriesHomeDailyManager.Add(new DAL.RegistriesHomeDaily
                {
                    Id_association = associationId,
                    TransactionDate = addedDate
                });
            }
            else
            {
                dailyRegId = dailyReg.Id;
            }

            if (!dailyRegId.HasValue) { return; }
            //update all RegistriesHomeDaily from that day
            RegistriesHomeDailyManager.RefreshOpenClosePricesFromDate(dailyRegId.Value);

            Add(new Administratoro.DAL.RegistriesHome
            {
                Id_apartment = apartmentId,
                Income = totalToPay,
                DocumentNr = documentNr,
                Outcome = null,
                Id_RegHomeDaily = dailyRegId,
                CreatedDate = DateTime.Now,
                Explanations = explanations,
            });

            var apDebts = GetAllCheckedApDebts(whatToPay);

            // mark ApartmentDebts as payed„
            if (sumOfChecked < totalToPay)
            {
                // advance in avans
                apDebts.Add(new Tuple<int, int, int, decimal, decimal?>(
                    DateTime.Now.Year,
                    DateTime.Now.Month,
                    (int)DebtType.AdvancePay,
                    totalToPay - sumOfChecked,
                    null));
            }
            else if (sumOfChecked > totalToPay)
            {
                // partially pay
                var items = partiallyPay.Split('!');
                int year;
                int month;
                int type;
                decimal value;
                if (items.Length == 4 && decimal.TryParse(items[3], out value) && int.TryParse(items[0], out year) &&
                    int.TryParse(items[1], out month) && int.TryParse(items[2], out type))
                {
                    apDebts.Add(new Tuple<int, int, int, decimal, decimal?>(year, month, type, value, sumOfChecked - totalToPay));
                }
            }

            ApartmentDebtsManager.Pay(apartmentId, apDebts);
        }

        private static List<Tuple<int, int, int, decimal, decimal?>> GetAllCheckedApDebts(List<string> whatToPay)
        {
            var result = new List<Tuple<int, int, int, decimal, decimal?>>();
            foreach (string item in whatToPay)
            {
                var items = item.Split('!');
                int year;
                int month;
                int type;
                decimal value;
                if (items.Length == 4 && decimal.TryParse(items[3], out value) && int.TryParse(items[0], out year) &&
                    int.TryParse(items[1], out month) && int.TryParse(items[2], out type))
                {
                    result.Add(new Tuple<int, int, int, decimal, decimal?>(year, month, type, value, null));
                }
            }

            return result;
        }
    }
}
