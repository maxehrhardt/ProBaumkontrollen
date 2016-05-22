using System;
using System.Collections.Generic;
using System.Text;
using SQLite;
namespace Baumkontrollen
{
    [Table("tabBaumart")]
    class Baumart
    {
        [PrimaryKey, AutoIncrement]
        public int ID { get; set; }
        public string NameDeutsch { get; set; }
        public string NameBotanisch { get; set; }
    }
}
