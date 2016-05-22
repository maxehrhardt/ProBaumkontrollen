using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Windows.UI.Xaml;
namespace Baumkontrollen
{
    public class ListView_bäume_item : INotifyPropertyChanged
    {
        private bool _bearbeitet;
        public bool bearbeitet 
        { 
            get
            {
                return _bearbeitet;
            } 
            set
            {
                _bearbeitet = value;
                RaisePropertyChanged("bearbeitet");
            }
        } 
        public int status { get; set; }

        public int baumID { get; set; }
        public int baumNr { get; set; }
        public int plakettenNr { get; set; }
        public string straße { get; set; }
        public string baumart_deutsch { get; set; }
        public string baumart_botanisch { get; set; }
        public string erstelldatum { get; set; }

        public string kontrolldatum { get; set; }
        public string kontrollintervall { get; set; }
        public string entwicklungsphase { get; set; }
        public string vitalitätsstufe { get; set; }
        public string schädigungsgrad { get; set; }
        public string baumhöhe_bereich { get; set; }
        public string baumhöhe { get; set; }
        public string kronendurchmesser { get; set; }
        public string stammdurchmesser { get; set; }
        public string stammanzahl { get; set; }

        //public string kronenzustand { get; set; }

        //public string stammzustand { get; set; }

        //public string stammfußzustand { get; set; }

        //public string wurzelzustand { get; set; }

        public string mängel { get; set; }

        public string verkehrssicher { get; set; }

        public string maßnahmen { get; set; }

        public string ausführenBis { get; set; }


        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

    }
}
