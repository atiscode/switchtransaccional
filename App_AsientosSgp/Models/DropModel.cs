using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace App_Mundial_Miles.Models
{
    public class DropModel
    {
        public long IdActual { get; set; }
        public string Nombre { get; set; }
        public string Label { get; set; }
        public IEnumerable<SelectListItem> Items { get; set; }
        public string Role { get; set; }
        public string Class { get; set; }

        public DropModel()
        {
            Role = "edit";
            Class = "filter";
        }
    }
}