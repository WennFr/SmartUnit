using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models
{
    public class LampTelemetryDataModel
    {
        public bool IsLampOn { get; set; }

        public double TemperatureCelsius { get; set; }

        public DateTime CurrentTime { get; set; }

    }
}
