
namespace Administratoro.BL.Managers
{
    using DAL;
    using System.Collections.Generic;
    using System.Linq;

    public static class AssociationsManager
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

        public static Associations AddNew(Associations association)
        {
            GetContext(true).Associations.Add(association);
            GetContext().SaveChanges();

            return association;
        }


        public static List<Associations> GetAllAssociationsByPartner(int partnerId)
        {
            return GetContext(true).Associations.Where(e => e.Id_Partner == partnerId).ToList();
        }

        public static Associations GetById(int assocId)
        {
            return GetContext(true).Associations.FirstOrDefault(e => e.Id == assocId);
        }

        public static void UpdateStairs(Associations es, bool hasStairs)
        {
            Associations associations = GetContext().Associations.FirstOrDefault(b => b.Id == es.Id);

            if (associations != null)
            {
                associations.HasStaircase = hasStairs;
                PerformUpdate(associations);
            }
        }

        public static void Update(Associations association)
        {
            if (association == null)
            {
                return;
            }

            Associations associations = GetContext().Associations.FirstOrDefault(b => b.Id == association.Id);

            if (associations == null) return;

            associations.Address = association.Address;
            associations.BanckAccont = association.BanckAccont;
            associations.CotaIndivizaAparments = association.CotaIndivizaAparments;
            associations.FiscalCode = association.FiscalCode;
            associations.HasStaircase = association.HasStaircase;
            associations.Name = association.Name;

            PerformUpdate(associations);
        }

        private static void PerformUpdate(Associations associations)
        {
            GetContext().Entry(associations).CurrentValues.SetValues(associations);

            GetContext().SaveChanges();
        }

        public static void UpdateRoundUpColumn(Associations association, bool hasRoundCoulmn)
        {
            Associations existingAssociation = GetContext().Associations.FirstOrDefault(b => b.Id == association.Id);

            if (existingAssociation == null) return;

            existingAssociation.HasRoundUpColumn = hasRoundCoulmn;
            PerformUpdate(existingAssociation);
        }

        public static void UpdatePenaltyRate(int associationId, decimal? penaltyRate)
        {
            Associations existingAssociation = GetContext().Associations.FirstOrDefault(b => b.Id == associationId);

            if (existingAssociation == null) return;

            existingAssociation.penaltyRate = penaltyRate;
            PerformUpdate(existingAssociation);
        }
    }
}
