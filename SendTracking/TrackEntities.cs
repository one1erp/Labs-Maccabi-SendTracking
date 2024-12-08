using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SendTracking
{
    class TrackEntities
    {
        public string id { get; set; }
        public string tableName { get; set; }

        public TrackEntities(string id,string tableName)
        {
            this.id = id;
            this.tableName = tableName;
        }
    }
}
