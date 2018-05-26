using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace jamwow.Models
{
    public class JSPerfModel
    {
        [JsonProperty("name")]
        public string name { get; set; }
        public string entryType { get; set; }
        public string startTime { get; set; }
        public string duration { get; set; }
        public string initiatorType { get; set; }
        public string nextHopProtocol { get; set; }
        public string workerStart { get; set; }
        public string redirectStart { get; set; }
        public string redirectEnd { get; set; }
        public string fetchStart { get; set; }
        public string domainLookupStart { get; set; }
        public string domainLookupEnd { get; set; }
        public string connectStart { get; set; }
        public string connectEnd { get; set; }
        public string secureConnectionStart { get; set; }
        public string requestStart { get; set; }
        public string responseStart { get; set; }
        public string responseEnd { get; set; }
        public string transferSize { get; set; }
        public string encodedBodySize { get; set; }
        public string decodedBodySize { get; set; }
        public string serverTiming { get; set; }
        public string unloadEventStart { get; set; }
        public string unloadEventEnd { get; set; }
        public string domInteractive { get; set; }
        public string domContentLoadedEventStart { get; set; }
        public string domContentLoadedEventEnd { get; set; }
        public string domComplete { get; set; }
        public string loadEventStart { get; set; }
        public string loadEventEnd { get; set; }
        public string type { get; set; }
        public string redirectCount { get; set; }
    }
}