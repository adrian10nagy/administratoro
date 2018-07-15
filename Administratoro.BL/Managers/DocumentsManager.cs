using System.IO;

namespace Administratoro.BL.Managers
{
    public static class DocumentsManager
    {
        public static int SaveFlyer(int associationId, int apartmentId, int year, int month, string fileType, MemoryStream fileToPut)
        {
            return DAL.SDK.Kit.Instance.Documents.SaveFlyer(associationId, apartmentId, year, month, fileType, fileToPut);
        }
    }
}
