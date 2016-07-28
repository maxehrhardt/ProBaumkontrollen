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
using System.Collections.ObjectModel;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkID=390556 dokumentiert.

namespace Baumkontrollen
{
    public sealed partial class KontrollePage : Page
    {

        /*
         * Variablen für die Datenbankkommunikation
         */
        DatenbankVerwalter dbVerwalter = new DatenbankVerwalter();
        SQLiteConnection connection_to_projekteDB;
        SQLiteConnection connection_to_benutzerDB;
        //SQLiteConnection connection_to_baumartDB;
        SQLiteConnection connection_to_arbeitsDB;

        /*
         * Liste der Bäume die Suchkriterien erfüllen
         */
        List<Baum> list_bäume_gefiltert=new List<Baum>();
        /*
         * Globale Liste aus ListView elementen für die gefilterten Bäume
         */
        ObservableCollection<ListView_bäume_item> list_of_listviewitems_bäume_item_global = new ObservableCollection<ListView_bäume_item>();

        /*
         * Variable für den Verkehrssicherheitsbutton
         */
        //Gibt die Zustände der eigenschaft: "Verkehrssicherheit" an
        enum verkehrssicherheit { ungesetzt, Ja, Nein };
        verkehrssicherheit ist_baum_verkehrssicher = verkehrssicherheit.ungesetzt;

        //

        public KontrollePage()
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

            //Setzen der globalen Baumliste als Datengrundlage für das Listview element
            listView_bäume.DataContext = list_of_listviewitems_bäume_item_global;
            listView_bäume.ItemsSource = list_of_listviewitems_bäume_item_global;

            //Aufbauen der Verbindung zu den Datenbanken
            connection_to_projekteDB = dbVerwalter.connectToProjektDB();
            connection_to_benutzerDB = dbVerwalter.connectToBenutzerDB();

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
                /*
                 * Füllen der Listview mit den aus den Suchkriterien ermittelten Bäumen
                 */

                connection_to_arbeitsDB = dbVerwalter.connectToArbeitsDB(list_aktives_projekt.ElementAt(0).Name);

                
                /*
                 * Anzeigen des richtigen pivotelements zu Beginn
                 */
                pivot_suchkriterien.Visibility = Visibility.Visible;
                pivot_baumliste.Visibility = Visibility.Collapsed;
                pivot_kontrolle.Visibility = Visibility.Collapsed;

                List<Straße> list_straßen = connection_to_arbeitsDB.Table<Straße>().ToList();               
                List<String> list_straßen_string = new List<string>();

                
                if (list_straßen.Count != 0)
                {
                    foreach (var straße in list_straßen)
                    {
                        list_straßen_string.Add(straße.name);
                    }
                }
                //throw new NullReferenceException("Blabla");

                list_straßen_string.Sort();

                autosuggestbox_straße.ItemsSource = list_straßen_string;
                autosuggestbox_straße.UpdateLayout();
                
                
         
                try
                {
                    List<Baumhöhenbereiche> list_baumhöhenbereiche = connection_to_arbeitsDB.Table<Baumhöhenbereiche>().ToList();
                    foreach (var baumhöhenbereich in list_baumhöhenbereiche)
                    {
                        combo_baumhöhenbereich.Items.Add(baumhöhenbereich.name);
                        combo_baumhöhenbereich_letzte_kontrolle.Items.Add(baumhöhenbereich.name);
                    }

                }
                catch (Exception)
                {
                    System.NullReferenceException ex = new System.NullReferenceException("list_baumhöhenbereiche could not be loaded");
                    throw ex;
                }

                
                try
                {
                    List<Entwicklungsphase> list_entwicklungsphasen = connection_to_arbeitsDB.Table<Entwicklungsphase>().ToList();
                    foreach (var entwicklungsphase in list_entwicklungsphasen)
                    {
                        combo_entwicklungsphase.Items.Add(entwicklungsphase.name);
                        combo_entwicklungsphase_letzte_kontrolle.Items.Add(entwicklungsphase.name);
                    }
                }
                catch (Exception)
                {        
                    System.NullReferenceException ex = new System.NullReferenceException("list_entwicklungsphase could not be loaded");
                    throw ex;
                }

                
                try
                {
                    List<Schädigungsgrad> list_schädigungsgrad = connection_to_arbeitsDB.Table<Schädigungsgrad>().ToList();
                    foreach (var schädigungsgrad in list_schädigungsgrad)
                    {
                        combo_schädigungsgrad.Items.Add(schädigungsgrad.name);
                        combo_schädigungsgrad_letzte_kontrolle.Items.Add(schädigungsgrad.name);
                    }
                }
                catch (Exception)
                {
                    System.NullReferenceException ex = new System.NullReferenceException("list_schädigungsgrad could not be loaded");
                    throw ex;
                }

                

                try
                {
                    List<Kronenzustand> list_kronenzustände = connection_to_arbeitsDB.Table<Kronenzustand>().ToList();
                    List<string> list_kronenzustände_string = new List<string>();
                    foreach (var kronenzustand in list_kronenzustände)
                    {
                        list_kronenzustände_string.Add(kronenzustand.name);
                    }
                    if (list_kronenzustände_string.Count != 0)
                    {
                        list_kronenzustände_string.Sort();
                        autotext_kronenzustand.ItemsSource = list_kronenzustände_string;
                    }
                }
                catch (Exception)
                {
                    System.NullReferenceException ex = new System.NullReferenceException("list_kronenzustände could not be loaded");
                    throw ex;
                }



                

                try
                {
                    List<Stammzustand> list_stammzustände = connection_to_arbeitsDB.Table<Stammzustand>().ToList();
                    List<string> list_stammzustände_string = new List<string>();
                    foreach (var stammzustand in list_stammzustände)
                    {
                        list_stammzustände_string.Add(stammzustand.name);
                    }
                    if (list_stammzustände_string.Count != 0)
                    {
                        list_stammzustände_string.Sort();
                        autotext_stammzustand.ItemsSource = list_stammzustände_string;
                    }
                }
                catch (Exception)
                {
                    System.NullReferenceException ex = new System.NullReferenceException("list_stammzustände could not be loaded");
                    throw ex;
                }




                try
                {
                    List<Wurzelzustand> list_wurzelzustände = connection_to_arbeitsDB.Table<Wurzelzustand>().ToList();
                    List<string> list_wurzelzustände_string = new List<string>();
                    foreach (var wurzelzustand in list_wurzelzustände)
                    {
                        list_wurzelzustände_string.Add(wurzelzustand.name);
                    }
                    if (list_wurzelzustände_string.Count != 0)
                    {
                        list_wurzelzustände_string.Sort();
                        autotext_wurzelzustand.ItemsSource = list_wurzelzustände_string;
                    }
                }
                catch (Exception)
                {
                    System.NullReferenceException ex = new System.NullReferenceException("list_wurzelzustände could not be loaded");
                    throw ex;
                }


                
                try
                {
                    List<AusführenBis> list_ausführenBis = connection_to_arbeitsDB.Table<AusführenBis>().ToList();
                    foreach (var ausführenBis in list_ausführenBis)
                    {
                        combo_ausführen_bis.Items.Add(ausführenBis.name);
                    }
                }
                catch (Exception)
                {
                    System.NullReferenceException ex = new System.NullReferenceException("list_ausführenBis could not be loaded");
                    throw ex;
                }

            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            connection_to_projekteDB.Close();
            connection_to_benutzerDB.Close();
            //connection_to_baumartDB.Close();
            connection_to_arbeitsDB.Close();
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        /*
         *   Event um den Hardware Backbutton zu behnadeln
         */
        async void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {
            e.Handled = true;
            MessageDialog message = new MessageDialog("Sie sind dabei die Seite zu verlassen!" + "\r\n" + "Nicht gespeicherte Eingaben werden verworfen." + "\r\n" + "Soll die Seite verlassen werden?");

            message.Commands.Add(new UICommand("Ja"));
            message.Commands.Add(new UICommand("Nein"));

            var command = await message.ShowAsync();

            switch (command.Label)
            {
                case "Ja":
                    Frame rootFrame = Window.Current.Content as Frame;

                    if (rootFrame != null && rootFrame.CanGoBack)
                    {
                        rootFrame.GoBack();
                    }
                    break;
                case "Nein":
                    break;

                default:
                    break;
            }
        }


        /*
         *  Events um die Details der Suchkriterien zu zeigen/verbergen, falls eine Checkbox gedrückt wurde 
         */
        private void checkbox_baumnummer_Checked(object sender, RoutedEventArgs e)
        {
            row_baumnummer_details.Height = new GridLength(1, GridUnitType.Star);
        }

        private void checkbox_baumnummer_Unchecked(object sender, RoutedEventArgs e)
        {
            row_baumnummer_details.Height = new GridLength(0);
        }

        private void checkbox_straße_Checked(object sender, RoutedEventArgs e)
        {
            row_straße_details.Height = new GridLength(1, GridUnitType.Star);
        }

        private void checkbox_straße_Unchecked(object sender, RoutedEventArgs e)
        {
            row_straße_details.Height = new GridLength(0);
        }


        /*
         * Aktualisieren der Autosuggestbox_straße bei Texteingabe
         */
        private void autosuggestbox_straße_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason==AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<Straße> list_alle_straßen = connection_to_arbeitsDB.Table<Straße>().ToList();
                List<string> list_straßen_gefiltert_string = new List<string>();

                string benutzereingabe = autosuggestbox_straße.Text;

                foreach (var straße in list_alle_straßen)
                {
                    if (straße.name.StartsWith(benutzereingabe))
                    {
                        list_straßen_gefiltert_string.Add(straße.name);
                    }
                }
                list_straßen_gefiltert_string.Sort();
                autosuggestbox_straße.ItemsSource = list_straßen_gefiltert_string;
            }
        }

        /*
         * Events wenn auf das Pivot Baumliste gewechselt wird
         */
        private void change_to_baumliste()
        {
            pivot_suchkriterien.Visibility = Visibility.Collapsed;
            pivot_suchkriterien.Visibility = Visibility.Collapsed;
            pivot_baumliste.Visibility = Visibility.Visible;
            pivot_kontrolle.Visibility = Visibility.Collapsed;

            // Die Bottombar verbergen, da ihre Funktionen auf der Seite nicht zur Verfügung stehen
            commandbar.Visibility = Visibility.Visible;
            button_abbrechen.Visibility = Visibility.Collapsed;
            button_speichern.Visibility = Visibility.Collapsed;
            button_zurück.Visibility = Visibility.Visible;
            button_kontrolle_anzeigen.Visibility = Visibility.Visible;
        }

        private void letzte_kontrolle_laden(Kontrolle letzte_kontrolle, ListView_bäume_item ausgewähltes_listview_bäume_item)
        {
            List<Baumhöhenbereiche> letzter_baumhöhenbereich = connection_to_arbeitsDB.Query<Baumhöhenbereiche>("SELECT * FROM tabBaumhoehenbereiche WHERE id=?", letzte_kontrolle.baumhöhe_bereichIDs);
            if (letzter_baumhöhenbereich.Count!=0)
            {
                combo_baumhöhenbereich_letzte_kontrolle.SelectedItem = letzter_baumhöhenbereich.ElementAt(0).name;
                combo_baumhöhenbereich.SelectedItem = letzter_baumhöhenbereich.ElementAt(0).name;
            }
            
            
            textbox_kontrolldatum_letzte_kontrolle.Text = letzte_kontrolle.kontrolldatum;

            textbox_kontrollintervall_letzte_kontrolle.Text = Convert.ToString(letzte_kontrolle.kontrollintervall);
            textbox_kontrollintervall.Text = Convert.ToString(letzte_kontrolle.kontrollintervall);

            if (letzte_kontrolle.baumhöhe == 0)
            {
                textbox_baumhöhe_letzte_kontrolle.Text = "";
            }
            else
            {
                textbox_baumhöhe_letzte_kontrolle.Text = Convert.ToString(letzte_kontrolle.baumhöhe);
                textbox_baumhöhe.Text = Convert.ToString(letzte_kontrolle.baumhöhe);
            }

            if (letzte_kontrolle.kronendurchmesser == 0)
            {
                textbox_kronendurchmesser_letzte_kontrolle.Text = "";
            }
            else
            {
                textbox_kronendurchmesser_letzte_kontrolle.Text = Convert.ToString(letzte_kontrolle.kronendurchmesser);
                textbox_kronendurchmesser.Text = Convert.ToString(letzte_kontrolle.kronendurchmesser);
            }

            if (letzte_kontrolle.stammdurchmesser == 0)
            {
                textbox_stammdurchmesser_letzte_kontrolle.Text = "";
            }
            else
            {
                textbox_stammdurchmesser_letzte_kontrolle.Text = Convert.ToString(letzte_kontrolle.stammdurchmesser);
                textbox_stammdurchmesser.Text = Convert.ToString(letzte_kontrolle.stammdurchmesser);
            }

            if (letzte_kontrolle.stammanzahl == 0)
            {
                textbox_stammanzahl_letzte_kontrolle.Text = "";
            }
            else
            {
                textbox_stammanzahl_letzte_kontrolle.Text = Convert.ToString(letzte_kontrolle.stammanzahl);
                textbox_stammanzahl.Text = Convert.ToString(letzte_kontrolle.stammanzahl);
            }

            if (letzte_kontrolle.entwicklungsphaseID!=0)
            {
                List<Entwicklungsphase> letzte_entwicklungsphase = connection_to_arbeitsDB.Query<Entwicklungsphase>("SELECT * FROM tabEntwicklungsphase WHERE id=?", letzte_kontrolle.entwicklungsphaseID);
                combo_entwicklungsphase_letzte_kontrolle.SelectedItem = letzte_entwicklungsphase.ElementAt(0).name;             
            }

            if (letzte_kontrolle.schädigungsgradID!=0)
            {
                List<Schädigungsgrad> letzter_schädigungsgrad = connection_to_arbeitsDB.Query<Schädigungsgrad>("SELECT * FROM tabSchaedigungsgrad WHERE id=?", letzte_kontrolle.schädigungsgradID);
                combo_schädigungsgrad_letzte_kontrolle.SelectedItem = letzter_schädigungsgrad.ElementAt(0).name;
            }


            if (letzte_kontrolle.kronenzustandSonstiges.Length!=0)
            {
                string[] kronenzustände = letzte_kontrolle.kronenzustandSonstiges.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var kronenzustand in kronenzustände)
                {
                    listview_kronenzustand_letzte_kontrolle.Items.Add(kronenzustand.Trim());
                }
            }

            if (letzte_kontrolle.stammzustandSonstiges.Length != 0)
            {
                string[] stammzustände = letzte_kontrolle.stammzustandSonstiges.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var stammzustand in stammzustände)
                {
                    listview_stammzustand_letzte_kontrolle.Items.Add(stammzustand.Trim());
                }
            }

            if (letzte_kontrolle.wurzelzustandSonstiges.Length != 0)
            {
                string[] wurzelzustände = letzte_kontrolle.wurzelzustandSonstiges.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var wurzelzustand in wurzelzustände)
                {
                    listview_wurzelzustand_letzte_kontrolle.Items.Add(wurzelzustand.Trim());
                }
            }


            if (letzte_kontrolle.verkehrssicher == true)
            {
                togglebutton_verkehrssicherheit_letzte_kontrolle.Content = "Verkehrssicher: Ja";
                togglebutton_verkehrssicherheit_letzte_kontrolle.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                togglebutton_verkehrssicherheit_letzte_kontrolle.Content = "Verkehrssicher: Nein";
                togglebutton_verkehrssicherheit_letzte_kontrolle.Background = new SolidColorBrush(Colors.Red);
            }

            if (letzte_kontrolle.maßnahmenSonstiges.Length!=0)
            {
                string[] maßnahmen = letzte_kontrolle.maßnahmenSonstiges.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                listview_maßnahmen_sonstiges.Items.Clear();

                foreach (var maßnahme in maßnahmen)
                {
                    listview_maßnahmen_sonstiges_letzte_kontrolle.Items.Add(maßnahme);
                }
            }

            //if (letzte_kontrolle.maßnahmenIDs != null)
            //{
            //    string[] maßnahmenIDs = letzte_kontrolle.maßnahmenIDs.Split(new Char[] { ' ' });
            //    foreach (var id in maßnahmenIDs)
            //    {
            //        if (id.Trim() != "")
            //        {
            //            switch (Convert.ToInt32(id))
            //            {
            //                case 1:
            //                    checkbox_maßnahmen_1_letzte_kontrolle.IsChecked = true;
            //                    break;
            //                case 2:
            //                    checkbox_maßnahmen_2_letzte_kontrolle.IsChecked = true;
            //                    break;
            //                case 3:
            //                    checkbox_maßnahmen_3_letzte_kontrolle.IsChecked = true;
            //                    break;
            //                case 4:
            //                    checkbox_maßnahmen_4_letzte_kontrolle.IsChecked = true;
            //                    break;
            //                case 5:
            //                    checkbox_maßnahmen_5_letzte_kontrolle.IsChecked = true;
            //                    break;
            //                case 6:
            //                    checkbox_maßnahmen_6_letzte_kontrolle.IsChecked = true;
            //                    break;
            //                case 7:
            //                    checkbox_maßnahmen_7_letzte_kontrolle.IsChecked = true;
            //                    break;
            //                case 8:
            //                    checkbox_maßnahmen_8_letzte_kontrolle.IsChecked = true;
            //                    break;
            //                case 9:
            //                    checkbox_maßnahmen_9_letzte_kontrolle.IsChecked = true;
            //                    break;
            //                default:
            //                    break;
            //            }
            //        }
            //    }
            //    stackpanel_maßnahmen.Visibility = Visibility.Visible;
            //}


            if (letzte_kontrolle.ausführenBisIDs != 0)
            {
                List<AusführenBis> letztes_ausführen_bis = connection_to_arbeitsDB.Query<AusführenBis>("SELECT * FROM tabAusfuehrenBis WHERE id=?", letzte_kontrolle.ausführenBisIDs);
                combo_ausführen_bis.SelectedItem = letztes_ausführen_bis.ElementAt(0).name;
            }
        }

        private async void button_zurück_Click(object sender, RoutedEventArgs e)
        {            
            MessageDialog message = new MessageDialog("Sie sind dabei die Seite zu verlassen!" + "\r\n" + "Nicht gespeicherte Eingaben werden verworfen." + "\r\n" + "Soll die Seite trotzdem verlassen werden?");

            message.Commands.Add(new UICommand("Ja"));
            message.Commands.Add(new UICommand("Nein"));

            var command = await message.ShowAsync();

            switch (command.Label)
            {
                case "Ja":
                    /*
                     * Anzeigen des richtigen pivotelements
                     */
                    pivot_suchkriterien.Visibility = Visibility.Visible;
                    pivot_baumliste.Visibility = Visibility.Collapsed;
                    pivot_kontrolle.Visibility = Visibility.Collapsed;
                    
                    //Verstecken der Bottombar
                    commandbar.Visibility = Visibility.Collapsed;

                    break;
                case "Nein":
                    break;

                default:
                    break;
            }

        }

        private async void button_kontrolle_anzeigen_Click(object sender, RoutedEventArgs e)
        {


            //Je nachdem ob in der Baumliste ein Baum ausgewählt wurde, ist das pivot_item_kontrolle aktiviert
            if (listView_bäume.SelectedItem == null)
            {
                pivot_item_kontrolle.IsEnabled = false;
                MessageDialog message_no_item_selected = new MessageDialog("Es wurde kein Baum ausgewählt, dessen Kontrolle bearbeitet werden kann.");
                await message_no_item_selected.ShowAsync();
            }
            else
            {
                pivot_suchkriterien.Visibility = Visibility.Collapsed;
                pivot_baumliste.Visibility = Visibility.Collapsed;
                pivot_kontrolle.Visibility = Visibility.Visible;

                commandbar.Visibility = Visibility.Visible;
                button_abbrechen.Visibility = Visibility.Visible;
                button_speichern.Visibility = Visibility.Visible;
                button_zurück.Visibility = Visibility.Collapsed;
                button_kontrolle_anzeigen.Visibility = Visibility.Collapsed;

                pivot_item_kontrolle.IsEnabled = true;

                //Abfragen der zugehörigen Kontrolle, zu dem ausgewähltem Baum                   
                ListView_bäume_item ausgewähltes_listview_bäume_item = (ListView_bäume_item)listView_bäume.SelectedItem;


                //Abfrage ob der ausgewählte baum bereits bearbeitet wurde, wenn ja werden zu der letzten Kontrolle auch noch die aktuell eingegebenen Parameter geladen
                if (ausgewähltes_listview_bäume_item.bearbeitet == true)
                {
                    List<Kontrolle> letzte_kontrolle_list = connection_to_arbeitsDB.Query<Kontrolle>("SELECT * FROM tabKontrolle WHERE baumID=?", ausgewähltes_listview_bäume_item.baumID).ToList();             //Variable für die letzte Kontrolle, die am ausgewähltem BAum durchgeführt wurde
                    letzte_kontrolle_list = letzte_kontrolle_list.OrderBy(x => x.kontrolldatum).ToList();

                    Kontrolle letzte_kontrolle = letzte_kontrolle_list.ElementAt(1);
                    letzte_kontrolle_laden(letzte_kontrolle, ausgewähltes_listview_bäume_item);
                }
                else
                {
                    List<Kontrolle> letzte_kontrolle_list = connection_to_arbeitsDB.Query<Kontrolle>("SELECT * FROM tabKontrolle WHERE baumID=?", ausgewähltes_listview_bäume_item.baumID).ToList();             //Variable für die letzte Kontrolle, die am ausgewähltem BAum durchgeführt wurde
                    letzte_kontrolle_list = letzte_kontrolle_list.OrderBy(x => x.kontrolldatum).ToList();

                    Kontrolle letzte_kontrolle = letzte_kontrolle_list.ElementAt(0);
                    letzte_kontrolle_laden(letzte_kontrolle, ausgewähltes_listview_bäume_item);
                }
            }
        }

        private void pivot_suchkriterien_GotFocus(object sender, RoutedEventArgs e)
        {
            commandbar.Visibility = Visibility.Collapsed;
        }

        //Anpassen des Baumhöhenbereiches wenn die absolute Baumhöhe geändert wird
        private void textbox_baumhöhe_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (textbox_baumhöhe.Text != "")
            {
                int baumhöhe = Convert.ToInt32(textbox_baumhöhe.Text);

                if (baumhöhe <= 5)
                {
                    combo_baumhöhenbereich.SelectedItem = "0-5";
                }
                if ((baumhöhe > 5) && (baumhöhe <= 10))
                {
                    combo_baumhöhenbereich.SelectedItem = "5-10";
                }
                if ((baumhöhe > 10) && (baumhöhe <= 15))
                {
                    combo_baumhöhenbereich.SelectedItem = "10-15";
                }
                if ((baumhöhe > 15) && (baumhöhe <= 20))
                {
                    combo_baumhöhenbereich.SelectedItem = "15-20";
                }
                if ((baumhöhe > 20) && (baumhöhe <= 25))
                {
                    combo_baumhöhenbereich.SelectedItem = "20-25";
                }
                if ((baumhöhe > 25) && (baumhöhe <= 30))
                {
                    combo_baumhöhenbereich.SelectedItem = "25-30";
                }
                if ((baumhöhe > 30) && (baumhöhe <= 35))
                {
                    combo_baumhöhenbereich.SelectedItem = "30-35";
                }
                if ((baumhöhe > 35))
                {
                    combo_baumhöhenbereich.SelectedItem = ">35";
                }
            }
        }

        //Verhalten des Verkehrssicherheit Buttons

        private void togglebutton_verkehrssicherheit_Click(object sender, RoutedEventArgs e)
        {
            if ((ist_baum_verkehrssicher == verkehrssicherheit.ungesetzt) || (ist_baum_verkehrssicher == verkehrssicherheit.Nein))
            {
                ist_baum_verkehrssicher = verkehrssicherheit.Ja;
                togglebutton_verkehrssicherheit.Content = "Verkehrssicher: Ja";
                togglebutton_verkehrssicherheit.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                ist_baum_verkehrssicher = verkehrssicherheit.Nein;
                togglebutton_verkehrssicherheit.Content = "Verkehrssicher: Nein";
                togglebutton_verkehrssicherheit.Background = new SolidColorBrush(Colors.Red);
            }
        }

        //Funktionen um die Zustände und die Maßnahmen einzuklappen
        private void header_kronenzustand_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (stackpanel_kronenzustand.Visibility == Visibility.Collapsed)
            {
                stackpanel_kronenzustand.Visibility = Visibility.Visible;
            }
            else
            {
                stackpanel_kronenzustand.Visibility = Visibility.Collapsed;
            }
        }

        private void header_stammzustand_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (stackpanel_stammzustand.Visibility == Visibility.Collapsed)
            {
                stackpanel_stammzustand.Visibility = Visibility.Visible;
            }
            else
            {
                stackpanel_stammzustand.Visibility = Visibility.Collapsed;
            }
        }

        private void header_wurzelzustand_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (stackpanel_wurzelzustand.Visibility == Visibility.Collapsed)
            {
                stackpanel_wurzelzustand.Visibility = Visibility.Visible;
            }
            else
            {
                stackpanel_wurzelzustand.Visibility = Visibility.Collapsed;
            }
        }

        private void header_maßnahmen_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (stackpanel_maßnahmen.Visibility == Visibility.Collapsed)
            {

                stackpanel_maßnahmen.Visibility = Visibility.Visible;
            }
            else
            {
                stackpanel_maßnahmen.Visibility = Visibility.Collapsed;
            }
        }

        /*
         * Verhalten der "Übernehmen"_Button
         */
        private void button_kontrollintervall_übernehmen_Click(object sender, RoutedEventArgs e)
        {
            textbox_kontrollintervall.Text = textbox_kontrollintervall_letzte_kontrolle.Text;
        }

        private void button_baumhöhenbereich_übernehmen_Click(object sender, RoutedEventArgs e)
        {
            combo_baumhöhenbereich.SelectedItem = combo_baumhöhenbereich_letzte_kontrolle.SelectedItem;   
        }

        private void button_baumhöhe_übernehmen_Click(object sender, RoutedEventArgs e)
        {
            textbox_baumhöhe.Text = textbox_baumhöhe_letzte_kontrolle.Text;
        }

        private void button_kronendurchmesser_übernehmen_Click(object sender, RoutedEventArgs e)
        {
            textbox_kronendurchmesser.Text = textbox_kronendurchmesser_letzte_kontrolle.Text;
        }

        private void button_stammdurchmesser_übernehmen_Click(object sender, RoutedEventArgs e)
        {
            textbox_stammdurchmesser.Text = textbox_stammdurchmesser_letzte_kontrolle.Text;
        }

        private void button_stammanzahl_übernehmen_Click(object sender, RoutedEventArgs e)
        {
            textbox_stammanzahl.Text = textbox_stammanzahl_letzte_kontrolle.Text;
        }

        private void button_entwicklungsphase_übernehmen_Click(object sender, RoutedEventArgs e)
        {
            combo_entwicklungsphase.SelectedItem = combo_entwicklungsphase_letzte_kontrolle.SelectedItem;
        }

        private void button_schädigungsgrad_übernehmen_Click(object sender, RoutedEventArgs e)
        {
            combo_schädigungsgrad.SelectedItem = combo_schädigungsgrad_letzte_kontrolle.SelectedItem;
        }

        private void button_ausführen_bis_übernehmen_Click(object sender, RoutedEventArgs e)
        {
            combo_ausführen_bis.SelectedItem = combo_ausführen_bis_letzte_kontrolle.SelectedItem;
        }

        /*
         * Behandeln der Elemente für die Baumzustände 
         */
        //Kronenzustand
        private void button_kronenzustand_sonstiges_übernehmen_Click(object sender, RoutedEventArgs e)
        {
            //listview_kronenzustand.ItemsSource = listview_kronenzustand_letzte_kontrolle.Items;

            foreach (var kronenzustand in listview_kronenzustand_letzte_kontrolle.Items.ToList())
            {
                if (listview_kronenzustand.Items.Contains(kronenzustand) == false)
                {
                    listview_kronenzustand.Items.Add(kronenzustand);
                }
            }
        }

        private void listview_kronenzustand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listview_kronenzustand.SelectedItem != null)
            {
                button_kronenzustand_entfernen.Visibility = Visibility.Visible;
            }
            else
            {
                button_kronenzustand_entfernen.Visibility = Visibility.Collapsed;
            }
        }

        private async void button_kronenzustand_entfernen_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog message_kronenzustand_entfernen = new MessageDialog("Woraus soll das Element entfernt werden?");
            message_kronenzustand_entfernen.Commands.Add(new UICommand("Aus Datenbank"));
            message_kronenzustand_entfernen.Commands.Add(new UICommand("Nur aus Liste"));
            var command = await message_kronenzustand_entfernen.ShowAsync();

            switch (command.Label)
            {
                case "Aus Datenbank":

                    Kronenzustand kronenzustand = new Kronenzustand();
                    List<Kronenzustand> list_kronenzustand = connection_to_arbeitsDB.Query<Kronenzustand>("SELECT * FROM tabKronenzustand WHERE name=?", listview_kronenzustand.SelectedItem);
                    if (list_kronenzustand.Count != 0)
                    {
                        kronenzustand = list_kronenzustand.ElementAt(0);
                        connection_to_arbeitsDB.Delete(kronenzustand);
                    }
                    else
                    {
                        //Error log
                    }

                    listview_kronenzustand.Items.Remove(listview_kronenzustand.SelectedItem);
                    break;
                case "Nur aus Liste":
                    listview_kronenzustand.Items.Remove(listview_kronenzustand.SelectedItem);
                    break;
                default:
                    break;
            }
        }

        private void autotext_kronenzustand_GotFocus(object sender, RoutedEventArgs e)
        {
            List<Kronenzustand> list_kronenzustände = connection_to_arbeitsDB.Table<Kronenzustand>().ToList();
            List<string> list_kronenzustände_string = new List<string>();

            foreach (var kronenzustand in list_kronenzustände)
            {
                list_kronenzustände_string.Add(kronenzustand.name);
            }
            if (list_kronenzustände_string.Count != 0)
            {
                list_kronenzustände_string.Sort();
                autotext_kronenzustand.ItemsSource = list_kronenzustände_string;
            }
            autotext_kronenzustand.IsSuggestionListOpen = true;
        }

        private void autotext_kronenzustand_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<Kronenzustand> list_kronenzustände = connection_to_arbeitsDB.Table<Kronenzustand>().ToList();
                List<string> gefilterte_kronenzustände = new List<string>();

                string benutzereingabe = autotext_kronenzustand.Text;

                foreach (var kronenzustand in list_kronenzustände)
                {
                    if (kronenzustand.name.StartsWith(benutzereingabe) == true)
                    {
                        gefilterte_kronenzustände.Add(kronenzustand.name);
                    }
                }

                gefilterte_kronenzustände.Sort();
                autotext_kronenzustand.ItemsSource = gefilterte_kronenzustände;
            }
        }

        private async void button_kronenzustand_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            if (autotext_kronenzustand.Text.Length != 0)
            {
                if (listview_kronenzustand.Items.Contains(autotext_kronenzustand.Text))
                {
                    MessageDialog message_kronenzustand_vorhanden = new MessageDialog("Der eingegebene Wert ist bereits in der Liste vorhanden.");
                    await message_kronenzustand_vorhanden.ShowAsync();
                }
                else
                {
                    List<Kronenzustand> kronenzustand_exists = connection_to_arbeitsDB.Query<Kronenzustand>("SELECT * FROM tabKronenzustand WHERE name=?", autotext_kronenzustand.Text);

                    if (kronenzustand_exists.Count != 0)
                    {
                        //Es gibt bereits einen Eintrag in der Datenbank für den eingegebenen Kronenzustand, nominaler Fall
                        listview_kronenzustand.Items.Add(autotext_kronenzustand.Text);
                        autotext_kronenzustand.Text = "";
                    }
                    else
                    {
                        MessageDialog message_kronenzustand_hinzufügen = new MessageDialog("Der eingegebene Kronenzustand ist noch nicht in der Datenbank gespeichert. Soll dieser Wert jetzt gespeichert und der Liste hinzugefügt werden?");

                        message_kronenzustand_hinzufügen.Commands.Add(new UICommand("Ja"));
                        message_kronenzustand_hinzufügen.Commands.Add(new UICommand("Nein"));

                        var command = await message_kronenzustand_hinzufügen.ShowAsync();

                        switch (command.Label)
                        {
                            case "Ja":
                                listview_kronenzustand.Items.Add(autotext_kronenzustand.Text.Trim());
                                Kronenzustand kronenzustand = new Kronenzustand();
                                kronenzustand.name = autotext_kronenzustand.Text.Trim();
                                connection_to_arbeitsDB.Insert(kronenzustand);
                                autotext_kronenzustand.Text = "";
                                break;
                            case "Nein":
                                break;
                        }

                    }
                }

            }
            else
            {
                MessageDialog message_kein_wert = new MessageDialog("Es wurde kein Kronenzustand eingegeben, der übernommen werden kann");
                await message_kein_wert.ShowAsync();
            }
        }

        //Stammzustand
        private void button_stammzustand_sonstiges_übernehmen_Click(object sender, RoutedEventArgs e)
        {
            //listview_stammzustand.ItemsSource = listview_stammzustand_letzte_kontrolle.Items;

            foreach (var stammzustand in listview_stammzustand_letzte_kontrolle.Items.ToList())
            {
                if (listview_stammzustand.Items.Contains(stammzustand) == false)
                {
                    listview_stammzustand.Items.Add(stammzustand);
                }
            }
        }

        private void listview_stammzustand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listview_stammzustand.SelectedItem != null)
            {
                button_stammzustand_entfernen.Visibility = Visibility.Visible;
            }
            else
            {
                button_stammzustand_entfernen.Visibility = Visibility.Collapsed;
            }
        }

        private async void button_stammzustand_entfernen_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog message_stammzustand_entfernen = new MessageDialog("Woraus soll das Element entfernt werden?");
            message_stammzustand_entfernen.Commands.Add(new UICommand("Aus Datenbank"));
            message_stammzustand_entfernen.Commands.Add(new UICommand("Nur aus Liste"));
            var command = await message_stammzustand_entfernen.ShowAsync();

            switch (command.Label)
            {
                case "Aus Datenbank":

                    Stammzustand stammzustand = new Stammzustand();
                    List<Stammzustand> list_stammzustand = connection_to_arbeitsDB.Query<Stammzustand>("SELECT * FROM tabStammzustand WHERE name=?", listview_stammzustand.SelectedItem);
                    if (list_stammzustand.Count != 0)
                    {
                        stammzustand = list_stammzustand.ElementAt(0);
                        connection_to_arbeitsDB.Delete(stammzustand);
                    }
                    else
                    {
                        //Error log
                    }

                    listview_stammzustand.Items.Remove(listview_stammzustand.SelectedItem);
                    break;
                case "Nur aus Liste":
                    listview_stammzustand.Items.Remove(listview_stammzustand.SelectedItem);
                    break;
                default:
                    break;
            }
        }

        private void autotext_stammzustand_GotFocus(object sender, RoutedEventArgs e)
        {
            List<Stammzustand> list_stammzustände = connection_to_arbeitsDB.Table<Stammzustand>().ToList();
            List<string> list_stammzustände_string = new List<string>();

            foreach (var stammzustand in list_stammzustände)
            {
                list_stammzustände_string.Add(stammzustand.name);
            }
            if (list_stammzustände_string.Count != 0)
            {
                list_stammzustände_string.Sort();
                autotext_stammzustand.ItemsSource = list_stammzustände_string;
            }
            autotext_stammzustand.IsSuggestionListOpen = true;
        }

        private void autotext_stammzustand_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<Stammzustand> list_stammzustände = connection_to_arbeitsDB.Table<Stammzustand>().ToList();
                List<string> gefilterte_stammzustände = new List<string>();

                string benutzereingabe = autotext_stammzustand.Text;

                foreach (var stammzustand in list_stammzustände)
                {
                    if (stammzustand.name.StartsWith(benutzereingabe) == true)
                    {
                        gefilterte_stammzustände.Add(stammzustand.name);
                    }
                }

                gefilterte_stammzustände.Sort();
                autotext_stammzustand.ItemsSource = gefilterte_stammzustände;
            }
        }

        private async void button_stammzustand_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            if (autotext_stammzustand.Text.Length != 0)
            {
                if (listview_stammzustand.Items.Contains(autotext_stammzustand.Text))
                {
                    MessageDialog message_stammzustand_vorhanden = new MessageDialog("Der eingegebene Wert ist bereits in der Liste vorhanden.");
                    await message_stammzustand_vorhanden.ShowAsync();
                }
                else
                {
                    List<Stammzustand> stammzustand_exists = connection_to_arbeitsDB.Query<Stammzustand>("SELECT * FROM tabStammzustand WHERE name=?", autotext_stammzustand.Text);

                    if (stammzustand_exists.Count != 0)
                    {
                        //Es gibt bereits einen Eintrag in der Datenbank für den eingegebenen Stammzustand, nominaler Fall
                        listview_stammzustand.Items.Add(autotext_stammzustand.Text);
                        autotext_stammzustand.Text = "";
                    }
                    else
                    {
                        MessageDialog message_stammzustand_hinzufügen = new MessageDialog("Der eingegebene Stammzustand ist noch nicht in der Datenbank gespeichert. Soll dieser Wert jetzt gespeichert und der Liste hinzugefügt werden?");

                        message_stammzustand_hinzufügen.Commands.Add(new UICommand("Ja"));
                        message_stammzustand_hinzufügen.Commands.Add(new UICommand("Nein"));

                        var command = await message_stammzustand_hinzufügen.ShowAsync();

                        switch (command.Label)
                        {
                            case "Ja":
                                listview_stammzustand.Items.Add(autotext_stammzustand.Text.Trim());
                                Stammzustand stammzustand = new Stammzustand();
                                stammzustand.name = autotext_stammzustand.Text.Trim();
                                connection_to_arbeitsDB.Insert(stammzustand);
                                autotext_stammzustand.Text = "";
                                break;
                            case "Nein":
                                break;
                        }

                    }
                }

            }
            else
            {
                MessageDialog message_kein_wert = new MessageDialog("Es wurde kein Stammzustand eingegeben, der übernommen werden kann");
                await message_kein_wert.ShowAsync();
            }
        }

        //Wurzelzustand
        private void button_wurzelzustand_sonstiges_übernehmen_Click(object sender, RoutedEventArgs e)
        {
            //listview_wurzelzustand.ItemsSource = listview_wurzelzustand_letzte_kontrolle.Items;

            foreach (var wurzelzustand in listview_wurzelzustand_letzte_kontrolle.Items.ToList())
            {
                if (listview_wurzelzustand.Items.Contains(wurzelzustand) == false)
                {
                    listview_wurzelzustand.Items.Add(wurzelzustand);
                }
            }
        }

        private void listview_wurzelzustand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listview_wurzelzustand.SelectedItem != null)
            {
                button_wurzelzustand_entfernen.Visibility = Visibility.Visible;
            }
            else
            {
                button_wurzelzustand_entfernen.Visibility = Visibility.Collapsed;
            }
        }

        private async void button_wurzelzustand_entfernen_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog message_wurzelzustand_entfernen = new MessageDialog("Woraus soll das Element entfernt werden?");
            message_wurzelzustand_entfernen.Commands.Add(new UICommand("Aus Datenbank"));
            message_wurzelzustand_entfernen.Commands.Add(new UICommand("Nur aus Liste"));
            var command = await message_wurzelzustand_entfernen.ShowAsync();

            switch (command.Label)
            {
                case "Aus Datenbank":

                    Wurzelzustand wurzelzustand = new Wurzelzustand();
                    List<Wurzelzustand> list_wurzelzustand = connection_to_arbeitsDB.Query<Wurzelzustand>("SELECT * FROM tabWurzelzustand WHERE name=?", listview_wurzelzustand.SelectedItem);
                    if (list_wurzelzustand.Count != 0)
                    {
                        wurzelzustand = list_wurzelzustand.ElementAt(0);
                        connection_to_arbeitsDB.Delete(wurzelzustand);
                    }
                    else
                    {
                        //Error log
                    }

                    listview_wurzelzustand.Items.Remove(listview_wurzelzustand.SelectedItem);
                    break;
                case "Nur aus Liste":
                    listview_wurzelzustand.Items.Remove(listview_wurzelzustand.SelectedItem);
                    break;
                default:
                    break;
            }
        }

        private void autotext_wurzelzustand_GotFocus(object sender, RoutedEventArgs e)
        {
            List<Wurzelzustand> list_wurzelzustände = connection_to_arbeitsDB.Table<Wurzelzustand>().ToList();
            List<string> list_wurzelzustände_string = new List<string>();

            foreach (var wurzelzustand in list_wurzelzustände)
            {
                list_wurzelzustände_string.Add(wurzelzustand.name);
            }
            if (list_wurzelzustände_string.Count != 0)
            {
                list_wurzelzustände_string.Sort();
                autotext_wurzelzustand.ItemsSource = list_wurzelzustände_string;
            }
            autotext_wurzelzustand.IsSuggestionListOpen = true;
        }

        private void autotext_wurzelzustand_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<Wurzelzustand> list_wurzelzustände = connection_to_arbeitsDB.Table<Wurzelzustand>().ToList();
                List<string> gefilterte_wurzelzustände = new List<string>();

                string benutzereingabe = autotext_wurzelzustand.Text;

                foreach (var wurzelzustand in list_wurzelzustände)
                {
                    if (wurzelzustand.name.StartsWith(benutzereingabe) == true)
                    {
                        gefilterte_wurzelzustände.Add(wurzelzustand.name);
                    }
                }

                gefilterte_wurzelzustände.Sort();
                autotext_wurzelzustand.ItemsSource = gefilterte_wurzelzustände;
            }
        }

        private async void button_wurzelzustand_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            if (autotext_wurzelzustand.Text.Length != 0)
            {
                if (listview_wurzelzustand.Items.Contains(autotext_wurzelzustand.Text))
                {
                    MessageDialog message_wurzelzustand_vorhanden = new MessageDialog("Der eingegebene Wert ist bereits in der Liste vorhanden.");
                    await message_wurzelzustand_vorhanden.ShowAsync();
                }
                else
                {
                    List<Wurzelzustand> wurzelzustand_exists = connection_to_arbeitsDB.Query<Wurzelzustand>("SELECT * FROM tabWurzelzustand WHERE name=?", autotext_wurzelzustand.Text);

                    if (wurzelzustand_exists.Count != 0)
                    {
                        //Es gibt bereits einen Eintrag in der Datenbank für den eingegebenen Wurzelzustand, nominaler Fall
                        listview_wurzelzustand.Items.Add(autotext_wurzelzustand.Text);
                        autotext_wurzelzustand.Text = "";
                    }
                    else
                    {
                        MessageDialog message_wurzelzustand_hinzufügen = new MessageDialog("Der eingegebene Wurzelzustand ist noch nicht in der Datenbank gespeichert. Soll dieser Wert jetzt gespeichert und der Liste hinzugefügt werden?");

                        message_wurzelzustand_hinzufügen.Commands.Add(new UICommand("Ja"));
                        message_wurzelzustand_hinzufügen.Commands.Add(new UICommand("Nein"));

                        var command = await message_wurzelzustand_hinzufügen.ShowAsync();

                        switch (command.Label)
                        {
                            case "Ja":
                                listview_wurzelzustand.Items.Add(autotext_wurzelzustand.Text.Trim());
                                Wurzelzustand wurzelzustand = new Wurzelzustand();
                                wurzelzustand.name = autotext_wurzelzustand.Text.Trim();
                                connection_to_arbeitsDB.Insert(wurzelzustand);
                                autotext_wurzelzustand.Text = "";
                                break;
                            case "Nein":
                                break;
                        }

                    }
                }

            }
            else
            {
                MessageDialog message_kein_wert = new MessageDialog("Es wurde kein Wurzelzustand eingegeben, der übernommen werden kann");
                await message_kein_wert.ShowAsync();
            }
        }

 //Maßnahmen
        private void button_maßnahmen_sonstiges_übernehmen_Click(object sender, RoutedEventArgs e)
        {
            foreach (var maßnahmen in listview_maßnahmen_sonstiges_letzte_kontrolle.Items.ToList())
            {
                if (listview_maßnahmen_sonstiges.Items.Contains(maßnahmen) == false)
                {
                    listview_maßnahmen_sonstiges.Items.Add(maßnahmen);
                }
            }
        }

        private void listview_maßnahmen_sonstiges_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listview_maßnahmen_sonstiges.SelectedItem != null)
            {
                button_maßnahmen_sonstiges_entfernen.Visibility = Visibility.Visible;
            }
            else
            {
                button_maßnahmen_sonstiges_entfernen.Visibility = Visibility.Collapsed;
            }
        }

        private async void  button_maßnahmen_sonstiges_entfernen_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog message_maßnahmen_sonstiges_entfernen = new MessageDialog("Woraus soll das Element entfernt werden?");
            message_maßnahmen_sonstiges_entfernen.Commands.Add(new UICommand("Aus Datenbank"));
            message_maßnahmen_sonstiges_entfernen.Commands.Add(new UICommand("Nur aus Liste"));
            var command = await message_maßnahmen_sonstiges_entfernen.ShowAsync();

            switch (command.Label)
            {
                case "Aus Datenbank":

                    Maßnahmen maßnahme = new Maßnahmen();
                    List<Maßnahmen> list_maßnahmen = connection_to_arbeitsDB.Query<Maßnahmen>("SELECT * FROM tabMassnahmen WHERE name=?", listview_maßnahmen_sonstiges.SelectedItem);
                    if (list_maßnahmen.Count != 0)
                    {
                        maßnahme = list_maßnahmen.ElementAt(0);
                        connection_to_arbeitsDB.Delete(maßnahme);
                    }
                    else
                    {
                        //Error log
                    }

                    listview_maßnahmen_sonstiges.Items.Remove(listview_maßnahmen_sonstiges.SelectedItem);
                    break;
                case "Nur aus Liste":
                    listview_maßnahmen_sonstiges.Items.Remove(listview_maßnahmen_sonstiges.SelectedItem);
                    break;
                default:
                    break;
            }
        }

        private void autotext_maßnahmen_sonstiges_GotFocus(object sender, RoutedEventArgs e)
        {
            List<Maßnahmen> list_maßnahmen = connection_to_arbeitsDB.Table<Maßnahmen>().ToList();
            List<string> list_maßnahmen_string = new List<string>();

            foreach (var maßnahme in list_maßnahmen)
            {
                list_maßnahmen_string.Add(maßnahme.name);
            }
            if (list_maßnahmen_string.Count != 0)
            {
                list_maßnahmen_string.Sort();
                autotext_maßnahmen_sonstiges.ItemsSource = list_maßnahmen_string;
            }
            autotext_maßnahmen_sonstiges.IsSuggestionListOpen = true;
        }

        private void autotext_maßnahmen_sonstiges_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<Maßnahmen> list_maßnahmen = connection_to_arbeitsDB.Table<Maßnahmen>().ToList();
                List<string> gefilterte_maßnahmen = new List<string>();

                string benutzereingabe = autotext_maßnahmen_sonstiges.Text;

                foreach (var maßnahme in list_maßnahmen)
                {
                    if (maßnahme.name.StartsWith(benutzereingabe) == true)
                    {
                        gefilterte_maßnahmen.Add(maßnahme.name);
                    }
                }

                gefilterte_maßnahmen.Sort();
                autotext_maßnahmen_sonstiges.ItemsSource = gefilterte_maßnahmen;
            }
        }
        
        private async void button_maßnahmen_sonstiges_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            if (autotext_maßnahmen_sonstiges.Text.Length != 0)
            {
                if (listview_maßnahmen_sonstiges.Items.Contains(autotext_maßnahmen_sonstiges.Text))
                {
                    MessageDialog message_maßnahmen_vorhanden = new MessageDialog("Der eingegebene Wert ist bereits in der Liste vorhanden.");
                    await message_maßnahmen_vorhanden.ShowAsync();
                }
                else
                {
                    List<Maßnahmen> maßnahmen_exists = connection_to_arbeitsDB.Query<Maßnahmen>("SELECT * FROM tabMassnahmen WHERE name=?", autotext_maßnahmen_sonstiges.Text);

                    if (maßnahmen_exists.Count != 0)
                    {
                        //Es gibt bereits einen Eintrag in der Datenbank für den eingegebenen Maßnahmen, nominaler Fall
                        listview_maßnahmen_sonstiges.Items.Add(autotext_maßnahmen_sonstiges.Text);
                        autotext_maßnahmen_sonstiges.Text = "";
                    }
                    else
                    {
                        MessageDialog message_maßnahmen_hinzufügen = new MessageDialog("Die eingegebene Maßnahme ist noch nicht in der Datenbank gespeichert. Soll dieser Wert jetzt gespeichert und der Liste hinzugefügt werden?");

                        message_maßnahmen_hinzufügen.Commands.Add(new UICommand("Ja"));
                        message_maßnahmen_hinzufügen.Commands.Add(new UICommand("Nein"));

                        var command = await message_maßnahmen_hinzufügen.ShowAsync();

                        switch (command.Label)
                        {
                            case "Ja":
                                listview_maßnahmen_sonstiges.Items.Add(autotext_maßnahmen_sonstiges.Text.Trim());
                                Maßnahmen maßnahme = new Maßnahmen();
                                maßnahme.name = autotext_maßnahmen_sonstiges.Text.Trim();
                                connection_to_arbeitsDB.Insert(maßnahme);
                                autotext_maßnahmen_sonstiges.Text = "";
                                break;
                            case "Nein":
                                break;
                        }

                    }
                }

            }
            else
            {
                MessageDialog message_kein_wert = new MessageDialog("Es wurde keine Maßnahme eingegeben, die übernommen werden kann");
                await message_kein_wert.ShowAsync();
            }
        }
        
        /*
         * Verhalten der Button in der Page.Bottombar
         */
        private void button_speichern_Click(object sender, RoutedEventArgs e)
        {
            if (auf_gültige_eingabe_testen()==true)
            {
                //In der suche ausgewählten Baum ermitteln
                ListView_bäume_item ausgewähltes_listview_bäume_item = (ListView_bäume_item)listView_bäume.SelectedItem;

                //Anlegen einer neuen Kontrolle
                Kontrolle kontrolle = new Kontrolle();

                kontrolle.baumID = ausgewähltes_listview_bäume_item.baumID;

                kontrolle.kontrolldatum = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString();

                kontrolle.kontrollintervall = Convert.ToInt32(textbox_kontrollintervall.Text);

                if (textbox_baumhöhe.Text != "")
                {
                    kontrolle.baumhöhe = Convert.ToInt32(textbox_baumhöhe.Text);
                }

                if (combo_baumhöhenbereich.SelectedItem != null)
                {
                    List<Baumhöhenbereiche> list_ausgewählter_baumhöhenbereich = connection_to_arbeitsDB.Query<Baumhöhenbereiche>("SELECT * FROM tabBaumhoehenbereiche WHERE name=?", combo_baumhöhenbereich.SelectedItem);
                    if (list_ausgewählter_baumhöhenbereich.Count != 0)
                    {
                        Baumhöhenbereiche ausgewählter_baumhöhenbereich = list_ausgewählter_baumhöhenbereich.ElementAt(0);
                        kontrolle.baumhöhe_bereichIDs = ausgewählter_baumhöhenbereich.id;
                    }
                }

                if (textbox_kronendurchmesser.Text != "")
                {
                    kontrolle.kronendurchmesser = Convert.ToInt32(textbox_kronendurchmesser.Text);
                }

                kontrolle.stammdurchmesser = Convert.ToInt32(textbox_stammdurchmesser.Text);
                kontrolle.stammanzahl = Convert.ToInt32(textbox_stammanzahl.Text);


                if (combo_entwicklungsphase.SelectedItem != null)
                {
                    List<Entwicklungsphase> list_ausgewählte_entwicklungsphase = connection_to_arbeitsDB.Query<Entwicklungsphase>("SELECT * FROM tabEntwicklungsphase WHERE Name=?", combo_entwicklungsphase.SelectedItem);
                    if (list_ausgewählte_entwicklungsphase.Count != 0)
                    {
                        Entwicklungsphase ausgewählte_entwicklungsphase = list_ausgewählte_entwicklungsphase.ElementAt(0);
                        kontrolle.entwicklungsphaseID = ausgewählte_entwicklungsphase.id;
                    }
                }


                if (combo_schädigungsgrad.SelectedItem != null)
                {
                    List<Schädigungsgrad> list_augewählter_schädigungsgrad = connection_to_arbeitsDB.Query<Schädigungsgrad>("SELECT * FROM tabSchaedigungsgrad WHERE Name=?", combo_schädigungsgrad.SelectedItem);
                    if (list_augewählter_schädigungsgrad.Count != 0)
                    {
                        Schädigungsgrad ausgewählter_schädigungsgrad = list_augewählter_schädigungsgrad.ElementAt(0);
                        kontrolle.schädigungsgradID = ausgewählter_schädigungsgrad.id;
                    }
                }

                //////
                //Kronenzustand
                //////
                string kronenzustand_sonstiges = "";
                if (listview_kronenzustand.Items.Count > 0)
                {
                    foreach (var kronenzustand in listview_kronenzustand.Items.ToList())
                    {
                        kronenzustand_sonstiges = kronenzustand_sonstiges + "; " + kronenzustand.ToString();
                    }
                }
                kronenzustand_sonstiges = kronenzustand_sonstiges.TrimStart(';', ' ');
                kontrolle.kronenzustandSonstiges = kronenzustand_sonstiges;

                //////
                //Stammzustand
                /////
                string stammzustand_sonstiges = "";
                if (listview_stammzustand.Items.Count > 0)
                {

                    foreach (var eigenschaft in listview_stammzustand.Items.ToList())
                    {
                        stammzustand_sonstiges = stammzustand_sonstiges + "; " + eigenschaft.ToString();
                    }
                }
                stammzustand_sonstiges = stammzustand_sonstiges.TrimStart(';', ' ');
                kontrolle.stammzustandSonstiges = stammzustand_sonstiges;

                //////
                //Wurzelzustand
                /////
                string wurzelzustand_sonstiges = "";
                if (listview_wurzelzustand.Items.Count > 0)
                {

                    foreach (var eigenschaft in listview_wurzelzustand.Items.ToList())
                    {
                        wurzelzustand_sonstiges = wurzelzustand_sonstiges + "; " + eigenschaft.ToString();
                    }
                }
                wurzelzustand_sonstiges = wurzelzustand_sonstiges.TrimStart(';', ' ');
                kontrolle.wurzelzustandSonstiges = wurzelzustand_sonstiges;

                //////
                //Verkehrssicherheit
                /////


                if (ist_baum_verkehrssicher == verkehrssicherheit.Ja)
                {
                    kontrolle.verkehrssicher = true;
                }
                else
                {
                    kontrolle.verkehrssicher = false;
                }


                //////
                //Maßnahmen
                /////
                string maßnahmen_sonstiges = "";

                if (checkbox_maßnahmen_1.IsChecked == true)
                {
                    maßnahmen_sonstiges = maßnahmen_sonstiges + "; " + checkbox_maßnahmen_1.Content;
                }
                if (checkbox_maßnahmen_2.IsChecked == true)
                {
                    maßnahmen_sonstiges = maßnahmen_sonstiges + "; " + checkbox_maßnahmen_2.Content;
                }
                if (checkbox_maßnahmen_3.IsChecked == true)
                {
                    maßnahmen_sonstiges = maßnahmen_sonstiges + "; " + checkbox_maßnahmen_3.Content;
                }
                if (checkbox_maßnahmen_4.IsChecked == true)
                {
                    maßnahmen_sonstiges = maßnahmen_sonstiges + "; " + checkbox_maßnahmen_4.Content;
                }
                if (checkbox_maßnahmen_5.IsChecked == true)
                {
                    maßnahmen_sonstiges = maßnahmen_sonstiges + "; " + checkbox_maßnahmen_5.Content;
                }
                if (checkbox_maßnahmen_6.IsChecked == true)
                {
                    maßnahmen_sonstiges = maßnahmen_sonstiges + "; " + checkbox_maßnahmen_6.Content;
                }
                if (checkbox_maßnahmen_7.IsChecked == true)
                {
                    maßnahmen_sonstiges = maßnahmen_sonstiges + "; " + checkbox_maßnahmen_7.Content;
                }
                if (checkbox_maßnahmen_8.IsChecked == true)
                {
                    maßnahmen_sonstiges = maßnahmen_sonstiges + "; " + checkbox_maßnahmen_8.Content;
                }
                if (checkbox_maßnahmen_9.IsChecked == true)
                {
                    maßnahmen_sonstiges = maßnahmen_sonstiges + "; " + checkbox_maßnahmen_9.Content;
                }

                if (listview_maßnahmen_sonstiges.Items.Count > 0)
                {

                    foreach (var eigenschaft in listview_maßnahmen_sonstiges.Items.ToList())
                    {
                        maßnahmen_sonstiges = maßnahmen_sonstiges + "; " + eigenschaft.ToString();
                    }
                }
                maßnahmen_sonstiges = maßnahmen_sonstiges.TrimStart(';', ' ');
                kontrolle.maßnahmenSonstiges = maßnahmen_sonstiges;

                //////
                //Ausführen Bis
                /////
                if (combo_ausführen_bis.SelectedItem != null)
                {
                    List<AusführenBis> list_ausführen_bis = connection_to_arbeitsDB.Query<AusführenBis>("SELECT * FROM tabAusfuehrenBis WHERE name=?", combo_ausführen_bis.SelectedItem);
                    if (list_ausführen_bis.Count != 0)
                    {
                        kontrolle.ausführenBisIDs = list_ausführen_bis.ElementAt(0).id;
                    }
                }


                /*
                 * Der zugehörige Baum wird in der Liste als bearbeitet markiert und die Kontrolle in der Datenbank gespeichert
                 */
                //list_bäume_gefiltert.Find(x => x.id==ausgewähltes_listview_bäume_item.baumID).bearbeitet=true;
                var selected_listview_item = listView_bäume.SelectedItem;                

                foreach (var item in list_of_listviewitems_bäume_item_global)
                {
                    if (item==selected_listview_item)
                    {
                        item.bearbeitet = true;                                               
                    }
                }

                //Schreiben der Kontrolle in die Datenbank
                connection_to_arbeitsDB.Insert(kontrolle);

                //Alle Elemente in Kontrolle drhführen auf Standard zurücksetzen
                auf_standard_zurücksetzen();

                //Bestätigung für das speichern der Kontrolle
                icon_kontrolle_speichern_fehler.Visibility = Visibility.Collapsed;
                icon_kontrolle_speichern_ok.Visibility = Visibility.Visible;

                textbox_kontrolle_speichern_fehler.Visibility = Visibility.Collapsed;
                textbox_kontrolle_speichern_ok.Visibility = Visibility.Visible;

                Flyout.ShowAttachedFlyout((FrameworkElement)sender);

                change_to_baumliste();
                      
            }
            else
            {
                //Eingabe ungültig
                icon_kontrolle_speichern_fehler.Visibility = Visibility.Visible;
                icon_kontrolle_speichern_ok.Visibility = Visibility.Collapsed;

                textbox_kontrolle_speichern_fehler.Visibility = Visibility.Visible;
                textbox_kontrolle_speichern_ok.Visibility = Visibility.Collapsed;

                Flyout.ShowAttachedFlyout((FrameworkElement)sender);
            }

        }

        private void auf_standard_zurücksetzen()
        {
            textbox_kontrollintervall.Text = "";
            textbox_kontrollintervall_letzte_kontrolle.Text = "";

            combo_baumhöhenbereich.SelectedItem = null;
            combo_baumhöhenbereich_letzte_kontrolle.SelectedItem = null;

            textbox_baumhöhe_letzte_kontrolle.Text = "";
            textbox_baumhöhe.Text = "";

            textbox_kronendurchmesser.Text = "";
            textbox_kronendurchmesser_letzte_kontrolle.Text = "";

            textbox_stammdurchmesser.Text = "";
            textbox_stammdurchmesser_letzte_kontrolle.Text = "";

            textbox_stammanzahl.Text = Convert.ToString(1);
            textbox_stammanzahl_letzte_kontrolle.Text = "";

            combo_entwicklungsphase.SelectedItem = null;
            combo_entwicklungsphase_letzte_kontrolle.SelectedItem = null;

            combo_schädigungsgrad.SelectedItem = null;
            combo_schädigungsgrad_letzte_kontrolle.SelectedItem = null;

            listview_kronenzustand.Items.Clear();
            listview_kronenzustand_letzte_kontrolle.Items.Clear();

            listview_stammzustand.Items.Clear();
            listview_stammzustand_letzte_kontrolle.Items.Clear();

            listview_wurzelzustand.Items.Clear();
            listview_wurzelzustand_letzte_kontrolle.Items.Clear();


            togglebutton_verkehrssicherheit.Background = null;
            togglebutton_verkehrssicherheit.Content = "Verkehrssicher: ";
            ist_baum_verkehrssicher = verkehrssicherheit.ungesetzt;


            togglebutton_verkehrssicherheit_letzte_kontrolle.Background = null;
            togglebutton_verkehrssicherheit_letzte_kontrolle.Content = "Verkehrssicher: ";


            checkbox_maßnahmen_1.IsChecked = false;
            checkbox_maßnahmen_2.IsChecked = false;
            checkbox_maßnahmen_3.IsChecked = false;
            checkbox_maßnahmen_4.IsChecked = false;
            checkbox_maßnahmen_5.IsChecked = false;
            checkbox_maßnahmen_6.IsChecked = false;
            checkbox_maßnahmen_7.IsChecked = false;
            checkbox_maßnahmen_8.IsChecked = false;
            checkbox_maßnahmen_9.IsChecked = false;


            checkbox_maßnahmen_1_letzte_kontrolle.IsChecked = false;
            checkbox_maßnahmen_2_letzte_kontrolle.IsChecked = false;
            checkbox_maßnahmen_3_letzte_kontrolle.IsChecked = false;
            checkbox_maßnahmen_4_letzte_kontrolle.IsChecked = false;
            checkbox_maßnahmen_5_letzte_kontrolle.IsChecked = false;
            checkbox_maßnahmen_6_letzte_kontrolle.IsChecked = false;
            checkbox_maßnahmen_7_letzte_kontrolle.IsChecked = false;
            checkbox_maßnahmen_8_letzte_kontrolle.IsChecked = false;
            checkbox_maßnahmen_9_letzte_kontrolle.IsChecked = false;

            listview_maßnahmen_sonstiges.Items.Clear();
            listview_maßnahmen_sonstiges_letzte_kontrolle.Items.Clear();

            combo_ausführen_bis.SelectedItem = null;
            combo_ausführen_bis_letzte_kontrolle.SelectedItem = null; 
        }

        private bool auf_gültige_eingabe_testen()
        {
            bool eingabe_gültig = true;

            if ((textbox_baumhöhe.Text == "") && (combo_baumhöhenbereich.SelectedItem == null))
            {
                eingabe_gültig = false;
                textbox_fehler_keine_baumhöhe.Visibility = Visibility.Visible;
            }
            else
            {
                textbox_fehler_keine_baumhöhe.Visibility = Visibility.Collapsed;
            }

            if (textbox_stammdurchmesser.Text == "")
            {
                eingabe_gültig = false;
                textbox_fehler_kein_stammdurchmesser.Visibility = Visibility.Visible;
            }
            else
            {
                textbox_fehler_kein_stammdurchmesser.Visibility = Visibility.Collapsed;
            }

            if (textbox_stammanzahl.Text == "")
            {
                eingabe_gültig = false;
                textbox_fehler_keine_stammanzahl.Visibility = Visibility.Visible;
            }
            else
            {
                textbox_fehler_keine_stammanzahl.Visibility = Visibility.Collapsed;
            }

            if (ist_baum_verkehrssicher == verkehrssicherheit.ungesetzt)
            {
                eingabe_gültig = false;
                textbox_fehler_verkehrssicherheit_nicht_geändert.Visibility = Visibility.Visible;
            }
            else
            {
                textbox_fehler_verkehrssicherheit_nicht_geändert.Visibility = Visibility.Collapsed;
            }


            return eingabe_gültig;
        }

        private async void button_abbrechen_Click(object sender, RoutedEventArgs e)
        {
            

            MessageDialog message = new MessageDialog("Sie sind dabei die Seite zu verlassen!" + "\r\n" + "Nicht gespeicherte Eingaben werden verworfen." + "\r\n" + "Soll die Seite verlassen werden?");

            message.Commands.Add(new UICommand("Ja"));
            message.Commands.Add(new UICommand("Nein"));

            var command = await message.ShowAsync();

            switch (command.Label)
            {
                case "Ja":
                    auf_standard_zurücksetzen();
                    change_to_baumliste();
                    break;
                case "Nein":
                    break;

                default:
                    break;
            }

        }

        private void button_baumliste_anzeigen_Click(object sender, RoutedEventArgs e)
        {
            //Zusammenstellen der Baumliste nach den eingegebenen Eigenschaften
            /*
             * Es wird ein SQL Abfrage string erstellt je nach den eingegebenen Kriterien
             */
            string sqlite_query_command = "SELECT * FROM tabBaeume";

            if ((checkbox_baumnummer.IsChecked == true) || (checkbox_straße.IsChecked == true))
            {

                sqlite_query_command += " WHERE ";
                if (checkbox_baumnummer.IsChecked == true)
                {
                    int min_baumnummer = 0;
                    int max_baumnummer = 0;

                    if (textbox_min_baumnummer.Text != "")
                    {
                        try
                        {
                            min_baumnummer = Convert.ToInt32(textbox_min_baumnummer.Text);
                        }
                        catch (FormatException)
                        {
                            MessageDialogHelper.Show("Die Eingabe der Baumnummer entsprach keinem gültigen Format. Es werden nur positive ganze Zahlen akzeptiert.", "Error");
                        }

                        sqlite_query_command += "baumNr >= " + Convert.ToString(min_baumnummer) + " AND ";
                    }

                    if (textbox_max_baumnummer.Text != "")
                    {
                        try
                        {
                            max_baumnummer = Convert.ToInt32(textbox_max_baumnummer.Text);
                        }
                        catch (FormatException)
                        {
                            MessageDialogHelper.Show("Die Eingabe der Baumnummer entsprach keinem gültigen Format. Es werden nur positive ganze Zahlen akzeptiert.", "Error");
                        }

                        sqlite_query_command += "baumNr <= " + Convert.ToString(max_baumnummer) + " AND ";
                    }
                }

                if (checkbox_straße.IsChecked == true)
                {
                    if (autosuggestbox_straße.Text != "")
                    {
                        int straßeID = connection_to_arbeitsDB.Query<Straße>("SELECT * FROM tabStrassen WHERE name=?", autosuggestbox_straße.Text).ElementAt(0).id;
                        sqlite_query_command += "straßeID= " + Convert.ToString(straßeID) + " AND ";
                    }
                }


                /*
                 * AND und WHERE werden am Ende des Abfrage Kommandos entfernt
                 */
                if (sqlite_query_command.EndsWith(" AND "))
                {
                    sqlite_query_command = sqlite_query_command.Remove(sqlite_query_command.Length - 5);
                }
                if (sqlite_query_command.EndsWith(" WHERE "))
                {
                    sqlite_query_command = sqlite_query_command.Remove(sqlite_query_command.Length - 7);
                }

            }


            /*
             * Abfrage der gefilterten Baumliste
             */
            list_bäume_gefiltert = connection_to_arbeitsDB.Query<Baum>(sqlite_query_command);

            if (list_of_listviewitems_bäume_item_global.Count == 0)
            {
                foreach (var baum in list_bäume_gefiltert)
                {

                    ListView_bäume_item listView_bäume_item = new ListView_bäume_item();

                    List<Kontrolle> list_zugehörige_kontrolle = connection_to_arbeitsDB.Query<Kontrolle>("SELECT * FROM tabKontrolle WHERE baumID=?", baum.id);

                    if (list_zugehörige_kontrolle.Count!=0)
                    {
                        list_zugehörige_kontrolle = list_zugehörige_kontrolle.OrderBy(x => x.kontrolldatum).ToList();
                       
                        Kontrolle zugehörige_kontrolle=list_zugehörige_kontrolle.ElementAt(0);
                        listView_bäume_item.kontrolldatum = zugehörige_kontrolle.kontrolldatum;
                    }

                    listView_bäume_item.baumID = baum.id;

                    listView_bäume_item.baumNr = baum.baumNr;
                    listView_bäume_item.plakettenNr = baum.plakettenNr;

                    

                    listView_bäume_item.straße = connection_to_arbeitsDB.Query<Straße>("SELECT * FROM tabStrassen WHERE id=?", baum.straßeId).ToList().ElementAt(0).name;

                    listView_bäume_item.baumart_deutsch = connection_to_arbeitsDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE id=?", baum.baumartId).ToList().ElementAt(0).NameDeutsch;
                    listView_bäume_item.baumart_botanisch = connection_to_arbeitsDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE id=?", baum.baumartId).ToList().ElementAt(0).NameBotanisch;

                    list_of_listviewitems_bäume_item_global.Add(listView_bäume_item);
                }
            }

            //Wechseln zum Baumlisten Pivotelement
            change_to_baumliste();


        }















    }



    /*
     *  Diese Klasse wird verwendet um die async anweisung im catch Block verwenden zu können
     */
    public class MessageDialogHelper
    {
        public static async void Show(string content, string title)
        {
            MessageDialog messageDialog = new MessageDialog(content, title);
            await messageDialog.ShowAsync();
        }
    }
}

