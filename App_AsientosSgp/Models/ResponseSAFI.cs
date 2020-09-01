using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace App_Mundial_Miles.Models
{
    public class ResponseSAFI
    {
        public string Version { get; set; }
        public int StatusCode { get; set; }
        public object Data { get; set; }
        public List<DataMessage> Messages { get; set; }

    }
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse); 
    public class DataMessage
    {
        public object ID { get; set; }
        public string Message { get; set; }
        public string InfoAdditional { get; set; }
        public object Type { get; set; }

    }
}