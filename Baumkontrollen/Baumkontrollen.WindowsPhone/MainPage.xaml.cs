using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using Windows.Storage;
using System.Threading;
using System.Threading.Tasks;
// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkId=234238 dokumentiert.
using SQLite;

using Windows.Phone.UI.Input;
using Windows.UI.Popups;
using Baumkontrollen.Hilfstabellen;

namespace Baumkontrollen
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Frames navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        DatenbankVerwalter dbVerwalter = new DatenbankVerwalter();
        SQLiteConnection connection_to_projekteDB;
        SQLiteConnection connection_to_benutzerDB;

        public MainPage()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;

            if (rootFrame != null && rootFrame.CanGoBack)
            {

                e.Handled = true;
                rootFrame.GoBack();
            }
        }
        /// <summary>
        /// Wird aufgerufen, wenn diese Seite in einem Rahmen angezeigt werden soll.
        /// </summary>
        /// <param name="e">Ereignisdaten, die beschreiben, wie diese Seite erreicht wurde.
        /// Dieser Parameter wird normalerweise zum Konfigurieren der Seite verwendet.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Verbindung zu den Datenbanken: Benutzer und Projekt aufbauen
            connection_to_projekteDB = dbVerwalter.connectToProjektDB();
            connection_to_benutzerDB = dbVerwalter.connectToBenutzerDB();

            //Überprüfen ob die ProjektDb aktuell ist
            //dbVerwalter.copy_arbeitsDB_to_internal("Zwickau Kunz");

            //Die Werte der Projekt und Benutzer Textbox müssen zurückgesetzt werden.
            textblock_aktuelles_projekt.Text = "";
            textblock_aktueller_benutzer.Text = "";

            //Der aktive Benutzer/Projekt wird abgefragt. Dies kann nur als Liste geschehen, auch wenn es nur einen geben sollte.
            //Aus der Liste wird das erste Element gewählt
            List<Projekt> list_aktives_projekt = connection_to_projekteDB.Query<Projekt>("SELECT * FROM tabProjekt WHERE Aktiv=?", true);
            if (list_aktives_projekt.Count!=0)
            {
                textblock_aktuelles_projekt.Text = list_aktives_projekt.ElementAt(0).Name;
            }            
            List<Benutzer> list_aktiver_benutzer = connection_to_benutzerDB.Query<Benutzer>("SELECT * FROM tabBenutzer WHERE Aktiv=?", true);
            if (list_aktiver_benutzer.Count!=0)
            {
                textblock_aktueller_benutzer.Text = list_aktiver_benutzer.ElementAt(0).Name;
            }


            datenbanken_aktualisieren();

        }



        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            connection_to_projekteDB.Close();
            connection_to_benutzerDB.Close();

            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        private void button_neuaufnahme_Click(object sender, RoutedEventArgs e)
        {
            List<Projekt> list_aktives_projekt = connection_to_projekteDB.Query<Projekt>("SELECT * FROM tabProjekt WHERE Aktiv=?", true);
            List<Benutzer> list_aktiver_benutzer = connection_to_benutzerDB.Query<Benutzer>("SELECT * FROM tabBenutzer WHERE Aktiv=?", true);

            if ((list_aktives_projekt.Count()!=0)&&(list_aktiver_benutzer.Count()!=0))
            {
                this.Frame.Navigate(typeof(BaumkontrollenPage));
            }
            else
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }           
        }

        private void button_projektWählen_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(ProjektWählenPage));            
        }





        private  void button_auf_sd_kopieren_Click(object sender, RoutedEventArgs e)
        {
                dbVerwalter.copy_arbeitsDB_to_SD();
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
        


        private void datenbanken_aktualisieren()
        {
            List<Projekt> list_alle_projekte = connection_to_projekteDB.Table<Projekt>().ToList();
            SQLiteConnection hilfs_connection;
            if (list_alle_projekte.Count==0)
            {
                return;
            }
            foreach (var projekt in list_alle_projekte)
            {
                hilfs_connection = dbVerwalter.connectToArbeitsDB(projekt.Name);
                
                hilfs_connection.CreateTable<Kontrolle>();

                hilfs_connection.CreateTable<DBVersion>();
                List<DBVersion> list_dbVersion = hilfs_connection.Table<DBVersion>().ToList();
               

                if (list_dbVersion.Count==0)
                {
                    //Diese Aktualisierung ist durchzuführen, wenn in der Datenban noch keine Version angegeben ist

                    //1. Einfügen der Datenbankversion
                    DBVersion dbVersion = new DBVersion();
                    dbVersion.id = 1;
                    dbVersion.version = "1.0";
                    hilfs_connection.Insert(dbVersion);

                    //2. Überschreiben der Daten in der Kontrolle für Kronenzustand
                    List<Kontrolle> list_kontrollen = hilfs_connection.Table<Kontrolle>().ToList();

                    foreach (var kontrolle in list_kontrollen)
                    {

                        //Kronenzustand aktualisieren
                        string string_kronenzustand = "";
                        if (kontrolle.kronenzustandIDs!=null)
                        {
                            string[] kronenzustandIDs=kontrolle.kronenzustandIDs.Split(new Char[] { ' ' });
                            foreach (var id in kronenzustandIDs)
                            {
                                if (id.Trim() != "")
                                {
                                    List<Kronenzustand> list_kronenzustände = hilfs_connection.Query<Kronenzustand>("SELECT * FROM tabKronenzustand WHERE id=?", Convert.ToInt32(id)).ToList();
                                    if (list_kronenzustände.Count!=0)
                                    {
                                        string_kronenzustand = string_kronenzustand + list_kronenzustände.ElementAt(0).name + "; ";                                      
                                    }
                                }
                            }
                            string_kronenzustand = string_kronenzustand.TrimEnd(';',' ');

                            if (kontrolle.kronenzustandSonstiges!=null)
                            {
                                if (kontrolle.kronenzustandSonstiges.Length>0)
                                {
                                    string_kronenzustand = string_kronenzustand + "; " + kontrolle.kronenzustandSonstiges;
                                }                                
                            }                                                                                 
                        }
                        kontrolle.kronenzustandSonstiges = string_kronenzustand; 

                        //Stammzustand/ Stammfußzustand aktualisieren
                        string string_stammzustand = "";
                        if (kontrolle.stammzustandIDs != null)
                        {
                            string[] stammzustandIDs = kontrolle.stammzustandIDs.Split(new Char[] { ' ' });
                            foreach (var id in stammzustandIDs)
                            {
                                if (id.Trim() != "")
                                {
                                    List<Stammzustand> list_stammzustände = hilfs_connection.Query<Stammzustand>("SELECT * FROM tabStammzustand WHERE id=?", Convert.ToInt32(id)).ToList();
                                    if (list_stammzustände.Count != 0)
                                    {
                                        string_stammzustand = string_stammzustand + list_stammzustände.ElementAt(0).name + "; ";
                                    }
                                }
                            }
                            string_stammzustand = string_stammzustand.TrimEnd(';', ' ');

                            if (kontrolle.stammzustandSonstiges != null)
                            {
                                if (kontrolle.stammzustandSonstiges.Length > 0)
                                {
                                    string_stammzustand = string_stammzustand + "; " + kontrolle.stammzustandSonstiges;
                                }
                            }
                            
                        }

                        string string_stammfußzustand = "";
                        if (kontrolle.stammfußzustandIDs != null)
                        {
                            string[] stammfußzustandIDs = kontrolle.stammfußzustandIDs.Split(new Char[] { ' ' });
                            foreach (var id in stammfußzustandIDs)
                            {
                                if (id.Trim() != "")
                                {
                                    List<Stammfußzustand> list_stammfußzustände = hilfs_connection.Query<Stammfußzustand>("SELECT * FROM tabStammfusszustand WHERE id=?", Convert.ToInt32(id)).ToList();
                                    if (list_stammfußzustände.Count != 0)
                                    {
                                        string_stammfußzustand = string_stammfußzustand + list_stammfußzustände.ElementAt(0).name + "; ";
                                    }
                                }
                            }
                            string_stammfußzustand = string_stammfußzustand.TrimEnd(';', ' ');

                            if (kontrolle.stammfußzustandSonstiges != null)
                            {
                                if (kontrolle.stammfußzustandSonstiges.Length > 0)
                                {
                                    string_stammfußzustand = string_stammfußzustand + "; " + kontrolle.stammfußzustandSonstiges;
                                }
                            }

                        }

                        kontrolle.stammzustandSonstiges = string_stammzustand+"; "+string_stammfußzustand;

                        string string_wurzelzustand = "";
                        if (kontrolle.wurzelzustandIDs != null)
                        {
                            string[] wurzelzustandIDs = kontrolle.wurzelzustandIDs.Split(new Char[] { ' ' });
                            foreach (var id in wurzelzustandIDs)
                            {
                                if (id.Trim() != "")
                                {
                                    List<Wurzelzustand> list_wurzelzustände = hilfs_connection.Query<Wurzelzustand>("SELECT * FROM tabWurzelzustand WHERE id=?", Convert.ToInt32(id)).ToList();
                                    if (list_wurzelzustände.Count != 0)
                                    {
                                        string_wurzelzustand = string_wurzelzustand + list_wurzelzustände.ElementAt(0).name + "; ";
                                    }
                                }
                            }
                            string_wurzelzustand = string_wurzelzustand.TrimEnd(';', ' ');

                            if (kontrolle.wurzelzustandSonstiges != null)
                            {
                                if (kontrolle.wurzelzustandSonstiges.Length > 0)
                                {
                                    string_wurzelzustand = string_wurzelzustand + "; " + kontrolle.wurzelzustandSonstiges;
                                }
                            }

                        }
                        kontrolle.wurzelzustandSonstiges = string_wurzelzustand; 

                        hilfs_connection.Update(kontrolle);
                        hilfs_connection.Close();
                    }
                }

                if (list_dbVersion.ElementAt(0).version == "1.0")
                {
                    // In DB Version 1.1 werden die vorgegebenen Maßnahmen in eine Tabelle MaßnahmenSontiges überschrieben
                    //1. Aktualisieren der Datenbankversion
                    DBVersion dbVersion = new DBVersion();
                    dbVersion.id = 1;
                    dbVersion.version = "1.1";
                    hilfs_connection.Update(dbVersion);

                    //2. Überschreiben der Daten in der Kontrolle für Kronenzustand
                    List<Kontrolle> list_kontrollen = hilfs_connection.Table<Kontrolle>().ToList();

                    foreach (var kontrolle in list_kontrollen)
                    {
                        string string_maßnahmen = "";
                        if (kontrolle.maßnahmenIDs != null)
                        {
                            string[] maßnahmenIDs = kontrolle.maßnahmenIDs.Split(new Char[] { ' ' });
                            foreach (var id in maßnahmenIDs)
                            {
                                if (id.Trim() != "")
                                {
                                    List<Maßnahmen> list_maßnahmen = hilfs_connection.Query<Maßnahmen>("SELECT * FROM tabMassnahmen WHERE id=?", Convert.ToInt32(id)).ToList();
                                    if (list_maßnahmen.Count != 0)
                                    {
                                        string_maßnahmen = string_maßnahmen + list_maßnahmen.ElementAt(0).name + "; ";
                                    }
                                }
                            }
                            string_maßnahmen = string_maßnahmen.TrimEnd(';', ' ');

                            if (kontrolle.maßnahmenSonstiges != null)
                            {
                                if (kontrolle.maßnahmenSonstiges.Length > 0)
                                {
                                    string_maßnahmen = string_maßnahmen + "; " + kontrolle.maßnahmenSonstiges;
                                }
                            }

                        }
                        kontrolle.maßnahmenSonstiges = string_maßnahmen;
                        hilfs_connection.Update(kontrolle);
                    }
                }



                hilfs_connection.Close();
            }
        }


        private async void button_aufgenommene_Bäume_anzeigen_Click(object sender, RoutedEventArgs e)
        {
            List<Projekt> list_aktives_projekt = connection_to_projekteDB.Query<Projekt>("SELECT * FROM tabProjekt WHERE Aktiv=?", true);
            List<Benutzer> list_aktiver_benutzer = connection_to_benutzerDB.Query<Benutzer>("SELECT * FROM tabBenutzer WHERE Aktiv=?", true);

            if ((list_aktives_projekt.Count() != 0) && (list_aktiver_benutzer.Count() != 0))
            {
                Exception ex_out = null;
                try
                {
                    this.Frame.Navigate(typeof(AufgenommeneBäumePage));
                }
                catch (Exception ex)
                {
                    ex_out = ex;
                    
                }

                if (ex_out != null)
                {
                    MessageDialog errorMessage = new MessageDialog(ex_out.Message);
                    await errorMessage.ShowAsync();
                }


            }
            else
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }     
            
        }

        private void button_kontrolle_durchführen_Click(object sender, RoutedEventArgs e)
        {
            List<Projekt> list_aktives_projekt = connection_to_projekteDB.Query<Projekt>("SELECT * FROM tabProjekt WHERE Aktiv=?", true);
            List<Benutzer> list_aktiver_benutzer = connection_to_benutzerDB.Query<Benutzer>("SELECT * FROM tabBenutzer WHERE Aktiv=?", true);

            if ((list_aktives_projekt.Count() != 0) && (list_aktiver_benutzer.Count() != 0))
            {
                this.Frame.Navigate(typeof(KontrollePage));
            }
            else
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }     
        }

        private void button_db_importieren_Click(object sender, RoutedEventArgs e)
        {
            //dbVerwalter.copy_arbeitsDB_to_internal("Zwickau Kunz");           
        }

        private async void button_errorLog_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            StorageFile errorFile = await local.GetFileAsync("ErrorFile.txt");
            if (errorFile!=null)
	        {
                string message=await Windows.Storage.FileIO.ReadTextAsync(errorFile);
                MessageDialog error_message = new MessageDialog(message);
                await error_message.ShowAsync();
	        }
            

        }



        
        





    }
}
