

namespace Administratoro.DAL.Repositories
{
    using System.Data.SqlClient;
    using System.IO;

    public interface IDocumentRepository
    {
        int SaveFlyer(int associationId, int apartmentId, int year, int month, string fileType, MemoryStream fileToPut);
    }

    public partial class Repository : IDocumentRepository
    {
        public int SaveFlyer(int associationId, int apartmentId, int year, int month, string fileType, MemoryStream fileToPut)
        {
            int id = 0;

            _dbRead.Execute(
                "DocumentApartmentFlyersAdd",
                new[] { 
                    new SqlParameter("@associationId", associationId),
                    new SqlParameter("@apartmentId", apartmentId),
                    new SqlParameter("@year", year),
                    new SqlParameter("@month", month),
                    new SqlParameter("@fileToPut", fileToPut),
                    new SqlParameter("@fileType", fileType),
                },
                r => id = Read<int>(r, "id"));

            return id;
        }
    }
}
