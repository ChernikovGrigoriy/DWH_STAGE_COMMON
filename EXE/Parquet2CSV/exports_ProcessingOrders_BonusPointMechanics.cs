using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parquet2CSV
{
    public class exports_ProcessingOrders_BonusPointsMechanics
    {
        public int? id { get; set; }
        public string internalId { get; set; }
        public string discriminator { get; set; }
        public string name { get; set; }
        public string ownerId { get; set; }
        public string ownerType { get; set; }
        public bool? _isDeleted { get; set; }
        public DateTime? _rowversion_ts { get; set; }
    }
}
