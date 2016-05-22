using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
namespace Baumkontrollen
{
    [Table("tabEntwicklungsphase")]
    class Entwicklungsphase
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
    }
}
