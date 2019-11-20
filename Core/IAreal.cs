using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlowMatters.Source.DODOC.Core
{
    public interface IAreal
    {
        double Area { get; }

        double Elevation { get; }
        double MaxArea { get; }
        double MaxElevation { get; }

        DateTime SimulationNow { get; set; }
    }
}
