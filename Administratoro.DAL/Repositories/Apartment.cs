
namespace Administratoro.DAL.Repositories
{

    using DAL;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;

    public interface IApartmentRepository
    {
        decimal GetSumOfIndiviza(int associationId);
        List<Apartments> GetByAss(int associationId, int stairCaseId);
        List<Apartments> GetByAss(int associationId);
        Apartments Get(int id);
        List<Apartments> GetAllEnabledForHeatHelp(int associationId);
        int GetDependentsNr(int associationId);
        int GetDependentsNr(int associationId, int? stairCase);
    }

    public partial class Repository : IApartmentRepository
    {
        public decimal GetSumOfIndiviza(int associationId)
        {
            decimal sum = 0m;

            _dbRead.Execute(
                "ApartmentsGetSumOfIndiviza",
            new[] { 
                new SqlParameter("@associationId", associationId),
             },
                r => sum = Read<decimal>(r, "sum"));

            return sum;
        }

        public int GetDependentsNr(int associationId)
        {
            int nr = 0;

            _dbRead.Execute(
                "ApartmentsGetDependentsNr",
            new[] { 
                new SqlParameter("@associationId", associationId),
             },
                r => nr = Read<int>(r, "nr"));

            return nr;
        }

        public int GetDependentsNr(int associationId, int? stairCase)
        {
            int nr = 0;

            _dbRead.Execute(
                "ApartmentsGetDependentsNrForStairCase",
            new[] { 
                new SqlParameter("@associationId", associationId),
                new SqlParameter("@stairCase", stairCase),
             },
                r => nr = Read<int>(r, "nr"));

            return nr;
        }

        public List<Apartments> GetByAss(int associationId, int stairCaseId)
        {
            var apartments = new List<Apartments>();

            _dbRead.Execute(
                "ApartmentsGetByAssStairCase",
             new[]
            {
                new SqlParameter("@associationId", associationId),
                new SqlParameter("@stairCaseId", stairCaseId),
            },
                r => apartments.Add(new Apartments()
                {
                    Id = Read<int>(r, "Id"),
                    id_Estate = Read<int>(r, "id_Estate"),
                    Id_StairCase = Read<int?>(r, "Id_StairCase"),
                    Name = Read<string>(r, "Name"),
                    Number = Read<int>(r, "Number"),
                    HasHeatHelp = Read<bool?>(r, "HasHeatHelp"),
                    Telephone = Read<string>(r, "Telephone"),
                    Password = Read<string>(r, "Password"),
                    Email = Read<string>(r, "Email"),
                    ExtraInfo = Read<string>(r, "ExtraInfo"),
                    Dependents = Read<int>(r, "Dependents"),
                    CreatedDate = Read<DateTime>(r, "CreatedDate"),
                    CotaIndiviza = Read<decimal?>(r, "CotaIndiviza")
                }));

            return apartments;
        }

        public List<Apartments> GetByAss(int associationId)
        {
            var apartments = new List<Apartments>();

            _dbRead.Execute(
                "ApartmentsGetByAss",
             new[]
            {
                new SqlParameter("@associationId", associationId)
            },
                r => apartments.Add(new Apartments()
                {
                    Id = Read<int>(r, "Id"),
                    id_Estate = Read<int>(r, "id_Estate"),
                    Id_StairCase = Read<int?>(r, "Id_StairCase"),
                    Name = Read<string>(r, "Name"),
                    Number = Read<int>(r, "Number"),
                    HasHeatHelp = Read<bool?>(r, "HasHeatHelp"),
                    Telephone = Read<string>(r, "Telephone"),
                    Password = Read<string>(r, "Password"),
                    Email = Read<string>(r, "Email"),
                    ExtraInfo = Read<string>(r, "ExtraInfo"),
                    Dependents = Read<int>(r, "Dependents"),
                    CreatedDate = Read<DateTime>(r, "CreatedDate"),
                    CotaIndiviza = Read<decimal?>(r, "CotaIndiviza")
                }));

            return apartments;
        }

        public Apartments Get(int id)
        {
            Apartments apartment = null;

            _dbRead.Execute(
                "ApartmentsGetById",
             new[]
            {
                new SqlParameter("@id", id)
            },
                r => apartment = new Apartments()
                {
                    Id = Read<int>(r, "Id"),
                    id_Estate = Read<int>(r, "id_Estate"),
                    Id_StairCase = Read<int?>(r, "Id_StairCase"),
                    Name = Read<string>(r, "Name"),
                    Number = Read<int>(r, "Number"),
                    HasHeatHelp = Read<bool?>(r, "HasHeatHelp"),
                    Telephone = Read<string>(r, "Telephone"),
                    Password = Read<string>(r, "Password"),
                    Email = Read<string>(r, "Email"),
                    ExtraInfo = Read<string>(r, "ExtraInfo"),
                    Dependents = Read<int>(r, "Dependents"),
                    CreatedDate = Read<DateTime>(r, "CreatedDate"),
                    CotaIndiviza = Read<decimal?>(r, "CotaIndiviza"),
                    FondRulment = Read<decimal?>(r, "FondRulment"),
                    FondReparatii = Read<decimal?>(r, "FondReparatii")
                });

            return apartment;
        }

        public List<Apartments> GetAllEnabledForHeatHelp(int associationId)
        {
            var apartments = new List<Apartments>();

            _dbRead.Execute(
                "ApartmentsGetAllEnabledForHeatHelp",
             new[]
            {
                new SqlParameter("@associationId", associationId)
            },
                r => apartments.Add(new Apartments()
                {
                    Id = Read<int>(r, "Id"),
                    id_Estate = Read<int>(r, "id_Estate"),
                    Id_StairCase = Read<int?>(r, "Id_StairCase"),
                    Name = Read<string>(r, "Name"),
                    Number = Read<int>(r, "Number"),
                    HasHeatHelp = Read<bool?>(r, "HasHeatHelp"),
                    Telephone = Read<string>(r, "Telephone"),
                    Password = Read<string>(r, "Password"),
                    Email = Read<string>(r, "Email"),
                    ExtraInfo = Read<string>(r, "ExtraInfo"),
                    Dependents = Read<int>(r, "Dependents"),
                    CreatedDate = Read<DateTime>(r, "CreatedDate"),
                    CotaIndiviza = Read<decimal?>(r, "CotaIndiviza")
                }));

            return apartments;
        }
    }
}
