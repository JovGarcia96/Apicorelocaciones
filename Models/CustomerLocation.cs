using Microsoft.SqlServer.Server;
using Microsoft.SqlServer.Types;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.SqlServer.Server;

namespace Apicorelocaciones.Models
{
    [Serializable]
    
    public class CustomerLocation
    {
        [Key]
        public int CustomerCode { get; set; }

        public int LocationCode { get; set; }

        public string ZoneCode { get; set; }

        public int RouteCode { get; set; }

        public int TypeCode { get; set; }

        public string MapX { get; set; }

        public string MapY { get; set; }

        public string Frequency { get; set; }
        public string? VisitStatus { get; internal set; }
        public string? Coordinates { get; internal set; }
    }
}

