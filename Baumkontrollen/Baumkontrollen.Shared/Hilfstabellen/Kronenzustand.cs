﻿using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace Baumkontrollen.Hilfstabellen
{
    [Table("tabKronenzustand")]
    class Kronenzustand
    {
        [PrimaryKey,AutoIncrement]
        public int id { get; set; }
        public string name { get; set; }
    }
}
