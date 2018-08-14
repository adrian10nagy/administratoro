
namespace Administratoro.DAL.Repositories
{
    using DAL;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public interface IRegistryHomeRepository
    {
        List<RegistriesHome> GetByAssAndDate(int associationId, DateTime date);
    }

    public partial class Repository : IRegistryHomeRepository
    {
        public List<RegistriesHome> GetByAssAndDate(int associationId, DateTime date)
        {
            var registriesHome = new List<RegistriesHome>();

            _dbRead.Execute(
                "RegistriesHomeGetByAssAndDate",
                new[]
                {
                    new SqlParameter("@associationId", associationId),
                    new SqlParameter("@TransactionDate", date),
                },
                r => registriesHome.Add(new RegistriesHome()
                {
                    Id = Read<int>(r, "Id"),
                    CreatedDate = Read<DateTime>(r, "CreatedDate"),
                    Explanations = Read<string>(r, "Explanations"),
                    DocumentNr = Read<string>(r, "DocumentNr"),
                    Income = Read<decimal?>(r, "Income"),
                    Outcome = Read<decimal?>(r, "Outcome"),
                    Id_apartment = Read<int>(r, "Id_apartment"),
                    TransactionDate = Read<DateTime>(r, "TransactionDate"),
                }));

            return registriesHome;
        }
    }
}
