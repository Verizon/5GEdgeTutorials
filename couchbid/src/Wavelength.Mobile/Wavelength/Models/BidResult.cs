using System;

namespace Wavelength.Models
{
    public class BidResult : Bid
    {
        public Metrics PerformanceMetrics { get; set; }
    }

    public class Metrics
    {
        public double ApiLatency { get; set; }
        public decimal DbExecutionTime { get; set; }
        public decimal DbElapsedTime { get; set; }
        public DateTimeOffset ApiSendDateTime { get; set; }    
        
        public double TotalHttpLatency { get; set; }
    }
}