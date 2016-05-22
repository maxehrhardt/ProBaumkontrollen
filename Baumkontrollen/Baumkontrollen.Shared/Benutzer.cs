using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Baumkontrollen
{
    [Table("tabBenutzer")]
    class Benutzer
    {
        [PrimaryKey]
        public string Name { get; set; }
        public bool Aktiv { get; set; }
    }
}
