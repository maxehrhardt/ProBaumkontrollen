using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Baumkontrollen
{
    [Table("tabBaeume")]
    public class Baum
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public string benutzer { get; set; }
        public string projekt { get; set; }
        public int straßeId { get; set; }
        public int baumNr { get; set; }
        public int plakettenNr { get; set; }
        public int baumartId { get; set; }        
        public string erstelldatum { get; set; }

    }
}
