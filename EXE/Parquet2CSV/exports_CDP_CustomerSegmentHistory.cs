using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parquet2CSV
{
    public class exports_CDP_CustomerSegmentHistory
    {
        public long? id { get; set; }
        public long? unmergedCustomerId { get; set; }
        public int? segmentationId { get; set; }
        public int? segmentId { get; set; }
        public DateTime? calculatedDateTimeUtc { get; set; }
        public bool? _isDeleted { get; set; }
        public DateTime? _rowversion_ts { get; set; }

    }
}
