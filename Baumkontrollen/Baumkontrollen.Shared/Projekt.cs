using System;
using System.Collections.Generic;
using System.Text;

using SQLite;

namespace Baumkontrollen
{
    [Table("tabProjekt")]
    class Projekt
    {
        [PrimaryKey]
        public string Name { get; set; }
        public bool Aktiv { get; set; }
    }
}
