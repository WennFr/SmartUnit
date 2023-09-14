using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Models
{
    public class FanTelemetryDataModel
    {
        public bool IsFanOn { get; set; }

        public string Speed { get; set; }

        public DateTime CurrentTime { get; set; }
    }
}
