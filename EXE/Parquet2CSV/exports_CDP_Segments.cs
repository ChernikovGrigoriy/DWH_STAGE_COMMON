using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Parquet2CSV
{
    public class exports_CDP_Segments
    {
        public int? id { get; set; }
        public string externalId { get; set; }
        public string name { get; set; }
        public string systemName { get; set; }
        public int? segmentationId { get; set; }
        public bool? _isDeleted { get; set; }
        public DateTime? _rowversion_ts { get; set; }

    }
}
