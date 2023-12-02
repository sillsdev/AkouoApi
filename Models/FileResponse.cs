using System.Net;

namespace AkouoApi.Models
{ /* This is an Identifiable only so that we can pass it back without JsonApiDotNetCore puking on it */
    public class Fileresponse 
    {
        public Fileresponse() : base()
        {
            Message = "";
            FileURL = "";
            ContentType = "application/json";
            Startindex = "";
        }

        public HttpStatusCode Status { get; set; }
        public string Message { get; set; }
        public string FileURL { get; set; }
        public string ContentType { get; set; }
        public string Startindex { get; set; }
    }
#pragma warning disable IDE1006 // Naming Styles
    public class JFRData
    {
        public JFRData() : base()
        {
            attributes = new();
            type = "";
        }

        public JFRAttributes attributes { get; set; }
        public string type { get; set; }
        public int id { get; set; }
    }

    public class JFRAttributes
    {
        public JFRAttributes() : base()
        {
            message = "";
            fileurl = "";
            contenttype = "application/json";
        }

        public string message { get; set; }
        public string fileurl { get; set; }
        public string contenttype { get; set; }
    }

    public class JsonedFileresponse
    {
        public JsonedFileresponse() : base()
        {
            data = new();
        }

        public JFRData data;
    }
#pragma warning restore IDE1006 // Naming Styles
}
