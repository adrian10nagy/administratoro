
namespace Administratoro.BL.Managers
{
    public static class FilesManager
    {
        public static string GetMonthlyExpensefilePath(int year, int month, int apartmentId)
        {
            const string filePath = "C:\\adminRo\\fluturasi\\{0}\\{1}-{2}\\p{3}.pdf";
            string result = null;

            var apartment = ApartmentsManager.GetById(apartmentId);
            if(apartment != null)
            {
                result = string.Format(filePath, apartmentId, year, month, apartment.Number);
            }

            return result;
        }
    }
}
