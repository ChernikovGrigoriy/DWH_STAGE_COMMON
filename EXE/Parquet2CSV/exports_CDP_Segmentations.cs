using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parquet2CSV
{
    public class exports_CDP_Segmentations
    {
        public int? id { get; set; }
        public string externalId { get; set; }
        public string name { get; set; }
        public string systemName { get; set; }
        public string entityType { get; set; }
        public DateTime? creationDateTimeUtc { get; set; }
        public bool? _isDeleted { get; set; }
        public DateTime? _rowversion_ts { get; set; }

    }
}
