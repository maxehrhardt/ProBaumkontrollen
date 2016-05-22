using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Baumkontrollen.Hilfstabellen
{
    [Table("tabDBVersion")]
    class DBVersion
    {
        [PrimaryKey]
        public int id { get; set; }
        public string version { get; set; }
    }
}
