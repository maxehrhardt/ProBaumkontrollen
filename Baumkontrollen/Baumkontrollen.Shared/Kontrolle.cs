using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Baumkontrollen
{
    [Table("tabKontrolle")]
    class Kontrolle
    {
        [PrimaryKey, AutoIncrement]
        public int id { get; set; }
        public int baumID { get; set; }
        public string kontrolldatum { get; set; }
        public decimal kontrollintervall { get; set; }
        public int entwicklungsphaseID { get; set; }
        public int vitalitätsstufeID { get; set; }
        public int schädigungsgradID { get; set; }
        public int baumhöhe_bereichIDs { get; set; }
        public decimal baumhöhe { get; set; }
        public decimal kronendurchmesser { get; set; }
        public decimal stammdurchmesser { get; set; }
        public int stammanzahl { get; set; }
        public string kronenzustandIDs { get; set; }
        public string kronenzustandSonstiges { get; set; }
        public string stammzustandIDs { get; set; }
        public string stammzustandSonstiges { get; set; }
        public string stammfußzustandIDs { get; set; }
        public string stammfußzustandSonstiges { get; set; }
        public string wurzelzustandIDs { get; set; }
        public string wurzelzustandSonstiges { get; set; }
        public bool verkehrssicher { get; set; }
        public string maßnahmenIDs { get; set; }
        public string maßnahmenSonstiges { get; set; }
        public int ausführenBisIDs { get; set; }        
    }
}
