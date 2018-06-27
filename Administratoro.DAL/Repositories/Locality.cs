
namespace Administratoro.DAL.Repositories
{
    using DAL;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public interface ILocalityRepository
    {
        IEnumerable<Localities> GetLocalitiesAll();
    }

    public partial class Repository : ILocalityRepository
    {
        public IEnumerable<Localities> GetLocalitiesAll()
        {
            List<Localities> localites = new List<Localities>();

            _dbRead.Execute(
                "LocalitiesGetAll",
            null,
                r => localites.Add(new Localities()
                {
                    Id = Read<int>(r, "Id"),
                    Name = Read<string>(r, "Name"),
                }));

            return localites;
        }
    }
}
