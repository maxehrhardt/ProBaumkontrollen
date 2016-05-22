using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
namespace Baumkontrollen.Hilfstabellen
{
    [Table("tabMassnahmen")]
    class Maßnahmen
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
    }
}
