
namespace Administratoro.DAL.Repositories
{
    using DAL;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public interface IRegistryHomeRepository
    {
        List<RegistriesHome> GetByRegHomeDaily(int regHomeDailyId);
    }

    public partial class Repository : IRegistryHomeRepository
    {
        public List<RegistriesHome> GetByRegHomeDaily(int regHomeDailyId)
        {
            var registriesHome = new List<RegistriesHome>();

            _dbRead.Execute(
                "RegistriesHomeGetByDailyId",
                new[]
                {
                    new SqlParameter("@regHomeDailyId", regHomeDailyId),
                },
                r => registriesHome.Add(new RegistriesHome()
                {
                    Id = Read<int>(r, "Id"),
                    CreatedDate = Read<DateTime>(r, "CreatedDate"),
                    Explanations = Read<string>(r, "Explanations"),
                    DocumentNr = Read<string>(r, "DocumentNr"),
                    Income = Read<decimal?>(r, "Income"),
                    Outcome = Read<decimal?>(r, "Outcome"),
                    Id_apartment = Read<int>(r, "Id_apartment")
                }));

            return registriesHome;
        }
    }
}
