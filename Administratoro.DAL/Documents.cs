
namespace Administratoro.DAL
{
    using Repositories;
    using System.IO;

    public class Documents
    {
        private static IDocumentRepository _repository;

        static Documents()
        {
            _repository = new Repository();
        }

        public int SaveFlyer(int associationId, int apartmentId, int year, int month, string fileType, MemoryStream fileToPut)
        {
            return _repository.SaveFlyer(associationId, apartmentId, year, month, fileType, fileToPut);
        }
    }
}
