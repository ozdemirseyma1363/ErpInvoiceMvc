using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FaturaWeb.Models
{

    public class PostObject//ileti
    {
        public int No { get; set; }
        public string Find { get; set; }

    }
    public class ResponseObject//yanıt
    {
        public bool Result { get; set; }
        public string Message { get; set; }
    }
}