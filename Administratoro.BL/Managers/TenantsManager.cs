
namespace Administratoro.BL.Managers
{
    using Administratoro.DAL;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Threading.Tasks;
    using System.Linq;
    using System;

    public static class ApartmentsManager
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


        public static List<Tenants> GetAllByEstateId(int estateId)
        {
            return GetContext().Tenants.Where(t => t.Estates.Id == estateId).ToList();
        }

        public static DbSet<Tenants> GetAllAsDbSet(int estateId)
        {
            return GetContext().Tenants;
        }

        public static Tenants GetById(int id)
        {
            return GetContext(true).Tenants.ToListAsync().Result.FindLast(x => x.Id == id);
        }

        public static void Update(Tenants tenant)
        {
            var result = GetContext().Tenants.SingleOrDefault(b => b.Id == tenant.Id);
            //GetContext().Tenants.Attach(tenant);
            //var x = GetContext().Entry(tenant);
            //x.Property(e => e.Email).IsModified = true;
            //GetContext().SaveChanges();

            if (result != null)
            {
                result.Email = tenant.Email;
                result.ExtraInfo = tenant.ExtraInfo;
                result.Telephone = tenant.Telephone;
                result.Dependents = tenant.Dependents;
                result.Name = tenant.Name;
                result.Password = tenant.Password;
                result.TenantPersons = null;
                GetContext().Entry(result).CurrentValues.SetValues(tenant);

                GetContext().SaveChanges();
            }
        }

        public static Tenants Add(Tenants tenant)
        {
            Tenants result = null;

            result = GetContext().Tenants.Add(tenant);
            GetContext().SaveChanges();

            return result;
        }
    }
}
