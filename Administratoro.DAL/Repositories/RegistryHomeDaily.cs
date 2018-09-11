
namespace Administratoro.DAL.Repositories
{
  using DAL;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public interface IRegistryHomeDailyRepository
    {
        int Add(RegistriesHomeDaily registriesHomeDaily);
        RegistriesHomeDaily Get(int assId, DateTime date);
        RegistriesHomeDaily GetById(int id);
    }

    public partial class Repository : IRegistryHomeDailyRepository
    {
        public int Add(RegistriesHomeDaily registriesHomeDaily)
        {
             var id = 0;

            _dbRead.Execute(
               "RegistriesHomeDailyAdd",
           new[] { 
                new SqlParameter("@ClosingAmount", registriesHomeDaily.ClosingAmount), 
                new SqlParameter("@Id_association", registriesHomeDaily.Id_association), 
                new SqlParameter("@OpeningAmount", registriesHomeDaily.OpeningAmount), 
                new SqlParameter("@TransactionDate", registriesHomeDaily.TransactionDate), 
            },
                r => id = Read<int>(r, "id")
            );

            return id;
        }

        public DAL.RegistriesHomeDaily Get(int assId, DateTime date)
        {
            RegistriesHomeDaily registriesHomeDaily = null;

            _dbRead.Execute(
                "RegistriesHomeDailyGet",
             new[]
            {
                new SqlParameter("@assId", assId),
                new SqlParameter("@date", date)
            },
                r => registriesHomeDaily = new RegistriesHomeDaily()
                {
                    Id = Read<int>(r, "Id"),
                    TransactionDate = date,
                    OpeningAmount = Read<decimal>(r, "OpeningAmount"),
                    ClosingAmount = Read<decimal>(r, "ClosingAmount"),
                    Id_association = assId
                });

            return registriesHomeDaily;
        }

        public RegistriesHomeDaily GetById(int id)
        {
            RegistriesHomeDaily registriesHomeDaily = null;

            _dbRead.Execute(
                "RegistriesHomeDailyGetById",
             new[]
            {
                new SqlParameter("@id", id),
            },
                r => registriesHomeDaily = new RegistriesHomeDaily()
                {
                    Id = id,
                    TransactionDate = Read<DateTime>(r, "TransactionDate"),
                    OpeningAmount = Read<decimal>(r, "OpeningAmount"),
                    ClosingAmount = Read<decimal>(r, "ClosingAmount"),
                    Id_association = Read<int>(r, "Id_association"),
                });

            return registriesHomeDaily;
        }
    }
}
