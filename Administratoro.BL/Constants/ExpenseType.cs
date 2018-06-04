
namespace Administratoro.BL.Constants
{
    using System.ComponentModel;

    public enum ExpenseType
    {
        Unknown = 0,

        [Description("Per index")]
        PerIndex = 1,

        [Description("Per Cota Indiviza")]
        PerCotaIndiviza = 2,

        [Description("Egal per persoana")]
        PerNrTenants = 3,

        [Description("Individual")]
        Individual = 6,

        [Description("Egal per apartament")]
        PerApartament = 7
    }
}
