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

using SQLite;


using Baumkontrollen.Hilfstabellen;
using Windows.UI;
using Windows.UI.Popups;
using Windows.Phone.UI.Input;
// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkID=390556 dokumentiert.

namespace Baumkontrollen
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class AufgenommeneBäumePage : Page
    {

        DatenbankVerwalter dbVerwalter = new DatenbankVerwalter();
        SQLiteConnection connection_to_projekteDB;
        SQLiteConnection connection_to_benutzerDB;
        SQLiteConnection connection_to_baumartDB;
        SQLiteConnection connection_to_arbeitsDB;


        public AufgenommeneBäumePage()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        /// <summary>
        /// Wird aufgerufen, wenn diese Seite in einem Frame angezeigt werden soll.
        /// </summary>
        /// <param name="e">Ereignisdaten, die beschreiben, wie diese Seite erreicht wurde.
        /// Dieser Parameter wird normalerweise zum Konfigurieren der Seite verwendet.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            connection_to_projekteDB = dbVerwalter.connectToProjektDB();
            connection_to_benutzerDB = dbVerwalter.connectToBenutzerDB();

            //connection_to_projekteDB = null;

            //Der aktive Benutzer/Projekt wird abgefragt. Dies kann nur als Liste geschehen, auch wenn es nur einen geben sollte.
            //Aus der Liste wird das erste Element gewählt
            List<Projekt> list_aktives_projekt = connection_to_projekteDB.Query<Projekt>("SELECT * FROM tabProjekt WHERE Aktiv=?", true);

            List<Benutzer> list_aktiver_benutzer = connection_to_benutzerDB.Query<Benutzer>("SELECT * FROM tabBenutzer WHERE Aktiv=?", true);


            if (list_aktives_projekt.Count == 0 || list_aktiver_benutzer.Count == 0)
            {
                MessageDialog error_message = new MessageDialog("Es wurde noch kein Benutzer und/oder Projekt gewählt. Dies kann im Startmenü unter: \"Projekt/Benutzer wählen\" durchgeführt werden.");
                await error_message.ShowAsync();
            }
            else
            {
                connection_to_arbeitsDB = dbVerwalter.connectToArbeitsDB(list_aktives_projekt.ElementAt(0).Name);

                List<Baum> list_alle_bäume = connection_to_arbeitsDB.Table<Baum>().ToList();


                List<ListView_bäume_item> list_of_listView_bäume_item = new List<ListView_bäume_item>();


                foreach (var baum in list_alle_bäume)
                {
                    ListView_bäume_item listView_bäume_item = new ListView_bäume_item();
                    Kontrolle zugehörige_kontrolle=null;
                    List<Kontrolle> list_zugehörige_kontrolle = connection_to_arbeitsDB.Query<Kontrolle>("SELECT * FROM tabKontrolle WHERE baumID=?", baum.id).ToList();
                    if (list_zugehörige_kontrolle.Count!=0)
                    {
                        list_zugehörige_kontrolle=list_zugehörige_kontrolle.OrderBy(x => x.kontrolldatum).ToList();
                        zugehörige_kontrolle = list_zugehörige_kontrolle.ElementAt(0);
                    }
                    

                    listView_bäume_item.baumID = baum.id;

                    listView_bäume_item.baumNr = baum.baumNr;
                    listView_bäume_item.plakettenNr = baum.plakettenNr;

                    try
                    {
                        listView_bäume_item.straße = connection_to_arbeitsDB.Query<Straße>("SELECT * FROM tabStrassen WHERE id=?", baum.straßeId).ToList().ElementAt(0).name;
                    }
                    catch (System.ArgumentOutOfRangeException)
                    {
                        listView_bäume_item.straße = "";                       
                    }

                    try
                    {
                        listView_bäume_item.baumart_deutsch = connection_to_arbeitsDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE id=?", baum.baumartId).ToList().ElementAt(0).NameDeutsch;
                    }
                    catch (System.ArgumentOutOfRangeException)
                    {
                        listView_bäume_item.baumart_deutsch = "";
                    }

                    try
                    {
                        listView_bäume_item.baumart_botanisch = connection_to_arbeitsDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE id=?", baum.baumartId).ToList().ElementAt(0).NameBotanisch;
                    }
                    catch (System.ArgumentOutOfRangeException)
                    {
                        listView_bäume_item.baumart_botanisch = "";
                    }


                    if (zugehörige_kontrolle!=null)
                    {
                        listView_bäume_item.kontrolldatum = zugehörige_kontrolle.kontrolldatum;

                        if (zugehörige_kontrolle.kontrollintervall == 0)
                        {
                            listView_bäume_item.kontrollintervall = "";
                        }
                        else
                        {
                            listView_bäume_item.kontrollintervall = Convert.ToString(zugehörige_kontrolle.kontrollintervall);
                        }


                        List<Entwicklungsphase> list_entwicklungsphase = connection_to_arbeitsDB.Query<Entwicklungsphase>("SELECT * FROM tabEntwicklungsphase WHERE id=?", zugehörige_kontrolle.entwicklungsphaseID).ToList();
                        if (list_entwicklungsphase.Count != 0)
                        {
                            listView_bäume_item.entwicklungsphase = list_entwicklungsphase.ElementAt(0).name;
                        }

                        List<Schädigungsgrad> list_schädigungsgrad = connection_to_arbeitsDB.Query<Schädigungsgrad>("SELECT * FROM tabSchaedigungsgrad WHERE id=?", zugehörige_kontrolle.schädigungsgradID).ToList();
                        if (list_schädigungsgrad.Count != 0)
                        {
                            listView_bäume_item.schädigungsgrad = list_schädigungsgrad.ElementAt(0).name;
                        }

                        List<Baumhöhenbereiche> list_baumhöhenbereich = connection_to_arbeitsDB.Query<Baumhöhenbereiche>("SELECT * FROM tabBaumhoehenbereiche WHERE id=?", zugehörige_kontrolle.baumhöhe_bereichIDs).ToList();
                        if (list_baumhöhenbereich.Count != 0)
                        {
                            listView_bäume_item.baumhöhe_bereich = list_baumhöhenbereich.ElementAt(0).name;
                        }

                        if (zugehörige_kontrolle.baumhöhe == 0)
                        {
                            listView_bäume_item.baumhöhe = "";
                        }
                        else
                        {
                            listView_bäume_item.baumhöhe = Convert.ToString(zugehörige_kontrolle.baumhöhe);
                        }

                        if (zugehörige_kontrolle.kronendurchmesser == 0)
                        {
                            listView_bäume_item.kronendurchmesser = "";
                        }
                        else
                        {
                            listView_bäume_item.kronendurchmesser = Convert.ToString(zugehörige_kontrolle.kronendurchmesser);
                        }

                        if (zugehörige_kontrolle.stammdurchmesser == 0)
                        {
                            listView_bäume_item.stammdurchmesser = "";
                        }
                        else
                        {
                            listView_bäume_item.stammdurchmesser = Convert.ToString(zugehörige_kontrolle.stammdurchmesser);
                        }

                        if (zugehörige_kontrolle.stammanzahl == 0)
                        {
                            listView_bäume_item.stammanzahl = "";
                        }
                        else
                        {
                            listView_bäume_item.stammanzahl = Convert.ToString(zugehörige_kontrolle.stammanzahl);
                        }

                        string zeichenkette_mängel = "";

                        if (zugehörige_kontrolle.kronenzustandSonstiges.Length != 0)
                        {
                            zeichenkette_mängel = zeichenkette_mängel + zugehörige_kontrolle.kronenzustandSonstiges+ "; ";
                        }

                        if (zugehörige_kontrolle.stammzustandSonstiges.Length != 0)
                        {
                            zeichenkette_mängel = zeichenkette_mängel + zugehörige_kontrolle.stammzustandSonstiges + "; ";
                        }

                        if (zugehörige_kontrolle.wurzelzustandSonstiges.Length != 0)
                        {
                            zeichenkette_mängel = zeichenkette_mängel + zugehörige_kontrolle.wurzelzustandSonstiges + "; ";
                        }
                        zeichenkette_mängel = zeichenkette_mängel.TrimEnd(new Char[] { ';', ' ', });
                        listView_bäume_item.mängel = zeichenkette_mängel;


                        if (zugehörige_kontrolle.verkehrssicher == true)
                        {
                            listView_bäume_item.verkehrssicher = "Ja";
                        }
                        else
                        {
                            if (zugehörige_kontrolle.verkehrssicher == false)
                            {
                                listView_bäume_item.verkehrssicher = "Nein";
                            }
                        }

                        string zeichenkette_maßnahmen = "";
                        if (zugehörige_kontrolle.maßnahmenIDs != null)
                        {
                            string[] maßnahmenIDs = zugehörige_kontrolle.maßnahmenIDs.Split(new Char[] { ' ' });
                            foreach (var id in maßnahmenIDs)
                            {
                                if (id.Trim() != "")
                                {
                                    List<Maßnahmen> list_maßnahmen = connection_to_arbeitsDB.Query<Maßnahmen>("SELECT * FROM tabMassnahmen WHERE id=?", Convert.ToInt32(id));
                                    if (list_maßnahmen.Count != 0)
                                    {
                                        zeichenkette_maßnahmen = zeichenkette_maßnahmen + list_maßnahmen.ElementAt(0).name + "/ ";
                                    }
                                }
                            }
                        }

                        if (zugehörige_kontrolle.maßnahmenSonstiges != null)
                        {
                            zeichenkette_maßnahmen = zeichenkette_maßnahmen + zugehörige_kontrolle.maßnahmenSonstiges + "/ ";
                        }
                        zeichenkette_maßnahmen = zeichenkette_maßnahmen.TrimEnd(new Char[] { '/', ' ', });
                        listView_bäume_item.maßnahmen = zeichenkette_maßnahmen;

                        List<AusführenBis> list_ausführenBis = connection_to_arbeitsDB.Query<AusführenBis>("SELECT * FROM tabAusfuehrenBis WHERE id=?", zugehörige_kontrolle.ausführenBisIDs);
                        if (list_ausführenBis.Count != 0)
                        {
                            listView_bäume_item.ausführenBis = list_ausführenBis.ElementAt(0).name;
                        }
                    }
                    else
                    {
                        MessageDialog message_kontrolle_nicht_gefunden = new MessageDialog("Für den Baum mit der Baumnummer: "+Convert.ToString(listView_bäume_item.baumNr)+" in der Straße: "+listView_bäume_item.straße+", wurde keine zugehörige Kontrolle gefunden.");
                        await message_kontrolle_nicht_gefunden.ShowAsync();
                    }


                    list_of_listView_bäume_item.Add(listView_bäume_item);
                }


                listView_bäume.DataContext = list_of_listView_bäume_item;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
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

        private async void button_bearbeiten_Click(object sender, RoutedEventArgs e)
        {
            if (listView_bäume.SelectedItem!=null)
            {
                this.Frame.Navigate(typeof(BaumBearbeitenPage), listView_bäume.SelectedItem);
            }
            else
            {
                MessageDialog message_nichts_ausgewählt = new MessageDialog("Es wurde kein Element ausgewählt. Wählen sie zunächst ein Element aus der Liste durch antippen aus.");
                await message_nichts_ausgewählt.ShowAsync();
            }
            
        }

        private async void button_löschen_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog messageDialog_no_selection = new MessageDialog("Es wurde noch kein Baum ausgewählt!");
            MessageDialog messageDialog_delete = new MessageDialog("Soll der ausgewählte Baum wirklich gelöscht werden?");
            messageDialog_delete.Commands.Add(new UICommand("Ja", new UICommandInvokedHandler(this.CommandInvokedHandler_delete_ja)));
            messageDialog_delete.Commands.Add(new UICommand("Nein", new UICommandInvokedHandler(this.CommandInvokedHandler_delete_nein)));


            if (listView_bäume.SelectedItem==null)
            {                
                await messageDialog_no_selection.ShowAsync();
            }
            else
            {
                await messageDialog_delete.ShowAsync();
            }

            
        }


        private async void CommandInvokedHandler_delete_ja(IUICommand command)
        {
            var item = listView_bäume.SelectedItem as ListView_bäume_item;

            List<Baum> list_ausgewählter_baum = connection_to_arbeitsDB.Query<Baum>("SELECT * FROM tabBaeume WHERE id=?",item.baumID);

            if (list_ausgewählter_baum.Count==0)
            {
                MessageDialog error_message = new MessageDialog("Fehler beim löschen! Der Baum wurde nicht gelöscht.");
                await error_message.ShowAsync();
            }
            else
            {
                Baum ausgewählter_Baum = list_ausgewählter_baum.ElementAt(0);
                connection_to_arbeitsDB.Delete(ausgewählter_Baum);

                var list = listView_bäume.Items.ToList();

                list.Remove(listView_bäume.SelectedItem);
                listView_bäume.ItemsSource = list;
                
            }

        }
        private void CommandInvokedHandler_delete_nein(IUICommand command)
        {
        }

    }
}
