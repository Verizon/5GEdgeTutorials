using System;
using Newtonsoft.Json;

namespace Wavelength.Models
{
    public class BidDto
    {
        [JsonProperty("deviceId")]
        public string DeviceId { get; set; }
        
        [JsonProperty("bidId")]
        public string BidId { get; set; }
        
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("sent")]
        public DateTimeOffset Sent { get; set; } 
    }
}