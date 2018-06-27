
namespace Administratoro.DAL.Repositories
{
    using DAL;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public interface IPartnerRepository
    {
        Partners GetSumOfIndiviza(string email, string password);
    }

    public partial class Repository : IPartnerRepository
    {
        public Partners GetSumOfIndiviza(string email, string password)
        {
            Partners partner = null;

            _dbRead.Execute(
                "PartnetGetByEmailPassword",
            new[] { 
                new SqlParameter("@email", email), 
                new SqlParameter("@password", password), 
            },
                r => partner = new Partners()
                {
                    Id = Read<int>(r, "id"),
                    Name = Read<string>(r, "Name"),
                });

            return partner;
        }
    }
}
