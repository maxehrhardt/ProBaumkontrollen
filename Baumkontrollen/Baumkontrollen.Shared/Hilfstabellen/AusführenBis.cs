using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Baumkontrollen.Hilfstabellen
{
    [Table("tabAusfuehrenBis")]
    class AusführenBis
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name {get; set;}
    }
}
