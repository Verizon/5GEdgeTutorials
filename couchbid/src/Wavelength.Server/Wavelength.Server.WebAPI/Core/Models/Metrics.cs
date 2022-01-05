using System;
using System.Collections.Generic;

namespace Wavelength.Core.Models
{
    public class Metrics
    {
        public double ApiLatency { get; set; }
        public decimal DbExecutionTime { get; set; }
        public decimal DbElapsedTime { get; set; }
        public DateTimeOffset ApiSendDateTime { get; set; }

        public IEnumerable<string> ToHeaders()
        {
            var headers = new List<string>();
            headers.Add($"dbqexec;desc=\"Query Exec\";dur={DbExecutionTime}");
            headers.Add($"dbqelp;desc=\"Query Elapsed\";dur={DbElapsedTime}");
            headers.Add($"api;desc=\"API\";dur={ApiLatency}");
            return headers;
        }
    }
}
