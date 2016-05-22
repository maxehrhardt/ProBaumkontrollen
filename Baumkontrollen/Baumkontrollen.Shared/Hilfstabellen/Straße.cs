using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Baumkontrollen.Hilfstabellen
{
    [Table("tabStrassen")]
    class Straße
    {
        [PrimaryKey,AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
        public bool aktiv { get; set; }
    }
}
