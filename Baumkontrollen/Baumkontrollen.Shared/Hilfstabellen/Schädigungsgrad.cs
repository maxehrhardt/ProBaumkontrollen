using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
namespace Baumkontrollen.Hilfstabellen
{
    [Table("tabSchaedigungsgrad")]
    class Schädigungsgrad
    {
        [PrimaryKey]
        public int id { get; set; }
        public string name { get; set; }
    }
}
