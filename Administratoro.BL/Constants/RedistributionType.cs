using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Administratoro.BL.Constants
{
    public enum RedistributionType
    {
        Unknown = 0,
        PerApartament = 1,
        PerTenants = 2,
        PerConsumption = 3,
        PerCotaIndiviza = 4
    }
}
