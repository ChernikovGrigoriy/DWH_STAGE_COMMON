using Microsoft.IdentityModel.Tokens;
using Parquet.Meta;
using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parquet2CSV
{
    public class exports_ProcessingOrders_BonusPointChanges
    {
        public int? id { get; set; }
        public string kindSystemName { get; set; }
        public string mechanicsInternalId { get; set; }
        public string balanceInternalId { get; set; }
        public decimal? changeAmount { get; set; }
        public DateTime? availableFromDateTimeUtc { get; set; }
        public DateTime? expirationDateTimeUtc { get; set; }
        public DateTime? dateTimeUtc { get; set; }
        public long? unmergedCustomerId { get; set; }
        public string orderId { get; set; }
        public string comments { get; set; }
        public string pointOfContactInternalId { get; set; }
        public string brandInternalId { get; set; }
        public bool? _isDeleted { get; set; }
        public DateTime? _rowversion_ts { get; set; }
    }
}
