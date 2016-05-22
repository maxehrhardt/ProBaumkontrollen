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

using Windows.UI.Popups;

// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkID=390556 dokumentiert.

using SQLite;

using Baumkontrollen.Hilfstabellen;
using Windows.UI;
using Windows.Storage.Pickers;
using Windows.Storage;

using Windows.Phone.UI.Input;

namespace Baumkontrollen
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class BaumkontrollenPage : Page
    {

        DatenbankVerwalter dbVerwalter = new DatenbankVerwalter();
        SQLiteConnection connection_to_projekteDB;
        SQLiteConnection connection_to_benutzerDB;
        SQLiteConnection connection_to_baumartDB;
        SQLiteConnection connection_to_arbeitsDB;

        //Gibt die Zustände der eigenschaft: "Verkehrssicherheit" an
        enum verkehrssicherheit {ungesetzt,Ja,Nein};
        verkehrssicherheit ist_baum_verkehrssicher=verkehrssicherheit.ungesetzt;

        Baum letzter_baum;
        Kontrolle letzte_kontrolle;

        public BaumkontrollenPage()
        {
            this.InitializeComponent();
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
        }

        /// <summary>
        /// Wird aufgerufen, wenn diese Seite in einem Frame angezeigt werden soll.
        /// </summary>
        /// <param name="e">Ereignisdaten, die beschreiben, wie diese Seite erreicht wurde.
        /// Dieser Parameter wird normalerweise zum Konfigurieren der Seite verwendet.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {

            ////////////////////////////////////
            //Verbindung zu den Datenbanken: Benutzer und Projekt aufbauen
            ////////////////////////////////////
            connection_to_projekteDB = dbVerwalter.connectToProjektDB();
            connection_to_benutzerDB = dbVerwalter.connectToBenutzerDB();

            //Der aktive Benutzer/Projekt wird abgefragt. Dies kann nur als Liste geschehen, auch wenn es nur einen geben sollte.
            //Aus der Liste wird das erste Element gewählt
            List<Projekt> list_aktives_projekt = connection_to_projekteDB.Query<Projekt>("SELECT * FROM tabProjekt WHERE Aktiv=?", true);
            if (list_aktives_projekt.Count != 0)
            {
                textbox_projekt.Text = list_aktives_projekt.ElementAt(0).Name;
            }
            List<Benutzer> list_aktiver_benutzer = connection_to_benutzerDB.Query<Benutzer>("SELECT * FROM tabBenutzer WHERE Aktiv=?", true);
            if (list_aktiver_benutzer.Count != 0)
            {
                textbox_benutzer.Text = list_aktiver_benutzer.ElementAt(0).Name;
            }


            ////////////////////////////////////
            //Verbindung zur Datenbank Baumart aufbauen
            ////////////////////////////////////
            connection_to_baumartDB = dbVerwalter.connectToBaumartDB();

            update_autotext_baumart();

            ////////////////////////////////////
            //Eintragen des aktuellen Datums
            ////////////////////////////////////
            textbox_baumerstelldatum.Text = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString();


            ////////////////////////////////////
            //Eintragen, des Standardkontrollintervalls
            ////////////////////////////////////
            textbox_kontrollintervall.Text = "1";
            
            ////////////////////////////////////
            //Eintragen der Standardstammanzahl
            ////////////////////////////////////
            textbox_stammanzahl.Text = "1";

            ////////////////////////////////////
            //Setzen der Standardverkehrsicherheit
            ////////////////////////////////////
            ist_baum_verkehrssicher = verkehrssicherheit.ungesetzt;

            ////////////////////////////////////
            //Werte der Arbeits DB abfragen
            ////////////////////////////////////
            connection_to_arbeitsDB = dbVerwalter.connectToArbeitsDB(list_aktives_projekt.ElementAt(0).Name);


            List<Straße> list_straßen = connection_to_arbeitsDB.Table<Straße>().ToList();

            foreach (var straße in list_straßen)
            {
                combo_straße.Items.Add(straße.name);
            }

            List<Baumhöhenbereiche> list_baumhöhenbereiche = connection_to_arbeitsDB.Table<Baumhöhenbereiche>().ToList();
            foreach (var baumhöhenbereich in list_baumhöhenbereiche)
            {
                combo_baumhöhenbereich.Items.Add(baumhöhenbereich.name);
            }

            List<Entwicklungsphase> list_entwicklungsphasen = connection_to_arbeitsDB.Table<Entwicklungsphase>().ToList();
            foreach (var entwicklungsphase in list_entwicklungsphasen)
            {
                combo_entwicklungsphase.Items.Add(entwicklungsphase.name);
            }

            List<Schädigungsgrad> list_schädigungsgrad = connection_to_arbeitsDB.Table<Schädigungsgrad>().ToList();
            foreach (var schädigungsgrad in list_schädigungsgrad)
            {
                combo_schädigungsgrad.Items.Add(schädigungsgrad.name);
            }


            combo_straße.Items.Clear();
            List<Straße> alle_straßen = connection_to_arbeitsDB.Table<Straße>().ToList();
            foreach (var straße in alle_straßen)
            {
                combo_straße.Items.Add(straße.name);
            }

            List<Straße> aktive_straße = connection_to_arbeitsDB.Query<Straße>("SELECT * FROM tabStrassen WHERE Aktiv=?", true).ToList();
            if (aktive_straße.Count != 0)
            {
                combo_straße.SelectedItem = aktive_straße.ElementAt(0).name;
            }


            List<AusführenBis> list_ausführenBis = connection_to_arbeitsDB.Table<AusführenBis>().ToList();
            foreach (var ausführenBis in list_ausführenBis)
            {
                combo_ausführen_bis.Items.Add(ausführenBis.name);
            }
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            connection_to_projekteDB.Close();
            connection_to_benutzerDB.Close();
            connection_to_baumartDB.Close();
            connection_to_arbeitsDB.Close();
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

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


        //Alle Funktionen zu Baumarten
        //Hinzufügen einer neuen Baumart und alle zugehörigen Funktionen
        private void button_baumart_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            if (autotext_baumart_deutsch.Text!="")
            {
                textbox_baumart_deutsch.Text = autotext_baumart_deutsch.Text;
            }
            if (autotext_baumart_botanisch.Text!="")
            {
                textbox_baumart_botanisch.Text = autotext_baumart_botanisch.Text;
            }

            autotext_baumart_deutsch.Visibility = Visibility.Collapsed;
            autotext_baumart_botanisch.Visibility = Visibility.Collapsed;
            button_baumart_hinzufügen.Visibility = Visibility.Collapsed;
            border_baumart_hinzufügen.Visibility = Visibility.Visible;
        }

        private void button_baumart_hinzufügen_bestätigen_Click(object sender, RoutedEventArgs e)
        {
            if ((textbox_baumart_deutsch.Text == "") && (textbox_baumart_botanisch.Text == ""))
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
            else
            {
                Baumart baumart = new Baumart
                {
                    NameBotanisch = textbox_baumart_botanisch.Text,
                    NameDeutsch = textbox_baumart_deutsch.Text
                };

                connection_to_baumartDB.Insert(baumart);
                connection_to_arbeitsDB.Insert(baumart);



                autotext_baumart_deutsch.Visibility = Visibility.Visible;
                autotext_baumart_botanisch.Visibility = Visibility.Visible;
                button_baumart_hinzufügen.Visibility = Visibility.Visible;

                update_autotext_baumart();

                textbox_baumart_botanisch.Text = "";
                textbox_baumart_deutsch.Text = "";
                border_baumart_hinzufügen.Visibility = Visibility.Collapsed;
            }

        }

        private void button_baumart_hinzufügen_abbrechen_Click(object sender, RoutedEventArgs e)
        {
            textbox_baumart_botanisch.Text = "";
            textbox_baumart_deutsch.Text = "";
            border_baumart_hinzufügen.Visibility = Visibility.Collapsed;

            autotext_baumart_deutsch.Visibility = Visibility.Visible;
            autotext_baumart_botanisch.Visibility = Visibility.Visible;
            button_baumart_hinzufügen.Visibility = Visibility.Visible;
        }

        private void update_autotext_baumart()
        {
            List<Baumart> liste_baumarten = connection_to_baumartDB.Table<Baumart>().ToList();

            List<string> liste_baumarten_deutsch = new List<string>();
            List<string> liste_baumarten_botanisch = new List<string>();

            foreach (var baumart in liste_baumarten)
            {
                if (baumart.NameDeutsch != "")
                {
                    liste_baumarten_deutsch.Add(baumart.NameDeutsch);
                    
                }
                if (baumart.NameBotanisch!="")
                {
                    liste_baumarten_botanisch.Add(baumart.NameBotanisch);
                }
            }

            if (liste_baumarten_deutsch.Count != 0)
            {
                liste_baumarten_deutsch.Sort();
                autotext_baumart_deutsch.ItemsSource = liste_baumarten_deutsch;
                autotext_baumart_deutsch.UpdateLayout();
                
            }

            if (liste_baumarten_botanisch.Count!=0)
            {
                liste_baumarten_botanisch.Sort();
                autotext_baumart_botanisch.ItemsSource = liste_baumarten_botanisch;
                autotext_baumart_botanisch.UpdateLayout();
            }

        }

        //aktualisieren der Baumarten-Autosuggestbox bei Benutzereingabe
        private void autotext_baumart_deutsch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason==AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<Baumart> alle_baumarten = connection_to_arbeitsDB.Table<Baumart>().ToList();
                List<string> gefilterte_baumarten = new List<string>();

                string benutzereingabe = autotext_baumart_deutsch.Text;

                foreach (var baumart in alle_baumarten)
                {
                    if (baumart.NameDeutsch.StartsWith(benutzereingabe)==true)
                    {
                        gefilterte_baumarten.Add(baumart.NameDeutsch);
                    }
                }
                gefilterte_baumarten.Sort();
                autotext_baumart_deutsch.ItemsSource = gefilterte_baumarten;
            }
        }
        private void autotext_baumart_deutsch_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            List<Baumart> ausgewählte_baumart = connection_to_arbeitsDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE NameDeutsch=?", args.SelectedItem);

            autotext_baumart_botanisch.Text = ausgewählte_baumart.ElementAt(0).NameBotanisch;
        }
        private void autotext_baumart_botanisch_SuggestionChosen(AutoSuggestBox sender, AutoSuggestBoxSuggestionChosenEventArgs args)
        {
            List<Baumart> ausgewählte_baumart = connection_to_arbeitsDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE NameBotanisch=?", args.SelectedItem);

            autotext_baumart_deutsch.Text = ausgewählte_baumart.ElementAt(0).NameDeutsch;
        }
        private void autotext_baumart_botanisch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<Baumart> alle_baumarten = connection_to_arbeitsDB.Table<Baumart>().ToList();
                List<string> gefilterte_baumarten = new List<string>();

                string benutzereingabe = autotext_baumart_botanisch.Text;

                foreach (var baumart in alle_baumarten)
                {
                    if (baumart.NameBotanisch.StartsWith(benutzereingabe) == true)
                    {
                        gefilterte_baumarten.Add(baumart.NameBotanisch);
                    }
                }
                gefilterte_baumarten.Sort();
                autotext_baumart_botanisch.ItemsSource = gefilterte_baumarten;
            }
        }

        private void autotext_baumart_deutsch_GotFocus(object sender, RoutedEventArgs e)
        {
            autotext_baumart_deutsch.IsSuggestionListOpen = true;
        }
        private void autotext_baumart_botanisch_GotFocus(object sender, RoutedEventArgs e)
        {
            autotext_baumart_botanisch.IsSuggestionListOpen = true;
        }


        //Hinzufügen, Löschen von Straßen und setzen der aktiven Straße
        private void button_straße_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            combo_straße.Visibility = Visibility.Collapsed;

            textbox_straße.Visibility = Visibility.Visible;
            button_straße_hinzufügen_bestätigen.Visibility = Visibility.Visible;
            button_straße_hinzufügen_abbrechen.Visibility = Visibility.Visible;
            textbox_straße.Focus(FocusState.Programmatic);
        }

        private void button_straße_hinzufügen_bestätigen_Click(object sender, RoutedEventArgs e)
        {
            if (textbox_straße.Text != "")
            {
                //Überprüfen, ob es bereits Straßen mit em selben Namen gibt
                List<Straße> gleiche_straße = connection_to_arbeitsDB.Query<Straße>("SELECT * FROM tabStrassen WHERE Name=?", textbox_straße.Text);
                if (gleiche_straße.Count == 0)
                {
                    Straße newStraße = new Straße
                    {
                        name = textbox_straße.Text,
                        aktiv = true
                    };

                    //Aktualisieren der Datenbank
                    List<Straße> list_alle_straßen = connection_to_arbeitsDB.Table<Straße>().ToList();
                    foreach (var straße in list_alle_straßen)
                    {
                        straße.aktiv = false;
                    }
                    connection_to_arbeitsDB.Insert(newStraße);



                    //ComboBox updaten
                    combo_straße.Items.Add(newStraße.name);
                    combo_straße.SelectedItem = newStraße.name;


                    //Zurücksetzen des UI's
                    textbox_straße.Text = "";
                    textbox_straße.Visibility = Visibility.Collapsed;
                    button_straße_hinzufügen_bestätigen.Visibility = Visibility.Collapsed;
                    button_straße_hinzufügen_abbrechen.Visibility = Visibility.Collapsed;
                    combo_straße.Visibility = Visibility.Visible;
                }
                else
                {
                    text_in_straßenflyout_keine_eingabe.Visibility = Visibility.Collapsed;
                    text_in_straßenflyout_straße_doppelt.Visibility = Visibility.Visible;
                    texteingabe_straße.Text = textbox_straße.Text;
                    FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
                }
            }
            else
            {

                text_in_straßenflyout_straße_doppelt.Visibility = Visibility.Collapsed;
                text_in_straßenflyout_keine_eingabe.Visibility = Visibility.Visible;
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
        }

        private void button_straße_hinzufügen_abbrechen_Click(object sender, RoutedEventArgs e)
        {
            textbox_straße.Text = "";

            combo_straße.Visibility = Visibility.Visible;

            textbox_straße.Visibility = Visibility.Collapsed;
            button_straße_hinzufügen_bestätigen.Visibility = Visibility.Collapsed;
            button_straße_hinzufügen_abbrechen.Visibility = Visibility.Collapsed;
        }

        private void combo_straße_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<Straße> liste_zu_deaktivierende_straßen = connection_to_arbeitsDB.Table<Straße>().ToList();
            foreach (var straße in liste_zu_deaktivierende_straßen)
            {
                straße.aktiv = false;
                connection_to_arbeitsDB.Update(straße);
            }

            if (combo_straße.SelectedItem != null)
            {
                List<Straße> liste_zu_aktivierende_straße = connection_to_arbeitsDB.Query<Straße>("SELECT * FROM tabStrassen WHERE Name=?", combo_straße.SelectedItem.ToString());

                foreach (var straße in liste_zu_aktivierende_straße)
                {
                    straße.aktiv = true;
                    connection_to_arbeitsDB.Update(straße);
                }
            }
        }

        private void button_straße_löschen_Click(object sender, RoutedEventArgs e)
        {
            if (combo_straße.SelectedValue != null)
            {
                List<Straße> liste_zu_löschende_straßen = connection_to_arbeitsDB.Query<Straße>("SELECT * FROM tabStrassen WHERE Name=?", combo_straße.SelectedItem);
                if (liste_zu_löschende_straßen.Count != 0)
                {
                    foreach (var straße in liste_zu_löschende_straßen)
                    {
                        connection_to_arbeitsDB.Delete<Straße>(straße.id);
                    }
                    combo_straße.Items.Remove(combo_straße.SelectedItem);
                }
            }
        }




        //Erhöhen oder erniedrigen der BaumNr/PlakettenNr
        private void button_baumNr_plus_Click(object sender, RoutedEventArgs e)
        {
            if (textbox_baumNr.Text == "")
            {
                textbox_baumNr.Text = "1";
            }
            else
            {
                textbox_baumNr.Text = Convert.ToString(Convert.ToInt32(textbox_baumNr.Text) + 1);
            }
        }

        private void button_baumNr_minus_Click(object sender, RoutedEventArgs e)
        {
            if (textbox_baumNr.Text == "" || Convert.ToUInt32(textbox_baumNr.Text) <= 0)
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
            else
            {
                textbox_baumNr.Text = Convert.ToString(Convert.ToInt32(textbox_baumNr.Text) - 1);
            }
        }

        private void button_plakettenNr_plus_Click(object sender, RoutedEventArgs e)
        {
            if (textbox_plakettenNr.Text == "")
            {
                textbox_plakettenNr.Text = "1";
            }
            else
            {
                textbox_plakettenNr.Text = Convert.ToString(Convert.ToInt32(textbox_plakettenNr.Text) + 1);
            }
        }

        private void button_plakettenNr_minus_Click(object sender, RoutedEventArgs e)
        {
            if (textbox_plakettenNr.Text == "" || Convert.ToUInt32(textbox_plakettenNr.Text) <= 0)
            {
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
            else
            {
                textbox_plakettenNr.Text = Convert.ToString(Convert.ToInt32(textbox_plakettenNr.Text) - 1);
            }
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


        //Sichtbar machen der "Sonstiges" Textboxen wenn die zugehörige Checkbox geklickt wird
        private void checkbox_maßnahmen_sonstiges_Checked(object sender, RoutedEventArgs e)
        {
            textbox_maßnahmen_sonstiges.Visibility = Visibility.Visible;
        }

        private void checkbox_maßnahmen_sonstiges_Unchecked(object sender, RoutedEventArgs e)
        {
            textbox_maßnahmen_sonstiges.Visibility = Visibility.Collapsed;
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



        //Hinzufügen eines neuen Baumes
        private async void button_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            if (auf_gültige_eingabe_testen()==true)
            {
                //////
                //Baum erstellen
                /////
                Baum baum = new Baum();

                baum.benutzer = textbox_benutzer.Text;
                baum.projekt = textbox_projekt.Text;

                if (combo_straße.SelectedItem != null)
                {
                    List<Straße> list_ausgewählte_straße = connection_to_arbeitsDB.Query<Straße>("SELECT * FROM tabStrassen WHERE Name=?", combo_straße.SelectedItem);
                    if (list_ausgewählte_straße.Count != 0)
                    {
                        Straße ausgewählte_straße = list_ausgewählte_straße.ElementAt(0);
                        baum.straßeId = ausgewählte_straße.id;
                    }
                    else
                    {
                            //Error log
                    }
                }

                baum.baumNr = Convert.ToInt32(textbox_baumNr.Text);


                if (textbox_plakettenNr.Text!="")
                {
                    baum.plakettenNr = Convert.ToInt32(textbox_plakettenNr.Text);
                }
                
                if (autotext_baumart_deutsch.Text != "")
                {
                    List<Baumart> list_ausgewählte_baumart = connection_to_arbeitsDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE NameDeutsch=? OR NameBotanisch=?", autotext_baumart_deutsch.Text,autotext_baumart_deutsch.Text);
                    if (list_ausgewählte_baumart.Count != 0)
                    {
                        Baumart ausgewählte_baumart = list_ausgewählte_baumart.ElementAt(0);
                        baum.baumartId = ausgewählte_baumart.ID;
                    }
                    else
                    {
                        //Error log
                    }
                    
                }

                baum.erstelldatum = textbox_baumerstelldatum.Text;

                List<Baum> baum_bereits_gespeichert = connection_to_arbeitsDB.Query<Baum>("SELECT * FROM tabBaeume WHERE baumNr=? AND straßeId=?", baum.baumNr, baum.straßeId);
                if (baum_bereits_gespeichert.Count==0)
                {
                    connection_to_arbeitsDB.Insert(baum);
                    letzter_baum = baum;
                }
                else
                {
                    MessageDialog message_baum_update = new MessageDialog("Es ist bereits ein Baum mit der angegebenen Baumnummer in der Straße vorhanden.\r\v Soll der bestehende Baum mit den neuen Werten überschrieben werden?");
                    message_baum_update.Commands.Add(new UICommand("Ja"));
                    message_baum_update.Commands.Add(new UICommand("Nein"));
                    
                    var command=await message_baum_update.ShowAsync();

                    switch (command.Label)
                    {
                        case "Ja":
                            baum.id = baum_bereits_gespeichert.ElementAt(0).id;
                            connection_to_arbeitsDB.Update(baum);
                            letzter_baum = baum;
                            break;
                        case "Nein":
                            letzter_baum = baum_bereits_gespeichert.ElementAt(0);
                            return;
                        default:
                            break;
                    }                    
                }
                

                //////
                //Kontrolle erstellen
                /////
                Kontrolle kontrolle = new Kontrolle();

                kontrolle.baumID = baum.id;

                kontrolle.kontrolldatum = DateTime.Now.Day.ToString() + "." + DateTime.Now.Month.ToString() + "." + DateTime.Now.Year.ToString();

                kontrolle.kontrollintervall = Convert.ToInt32(textbox_kontrollintervall.Text);

                if (textbox_baumhöhe.Text != "")
                {
                    kontrolle.baumhöhe = Convert.ToInt32(textbox_baumhöhe.Text);
                }

                if (combo_baumhöhenbereich.SelectedItem!=null)
                {
                    List<Baumhöhenbereiche> list_ausgewählter_baumhöhenbereich = connection_to_arbeitsDB.Query<Baumhöhenbereiche>("SELECT * FROM tabBaumhoehenbereiche WHERE name=?", combo_baumhöhenbereich.SelectedItem);
                    if (list_ausgewählter_baumhöhenbereich.Count!=0)
                    {
                        Baumhöhenbereiche ausgewählter_baumhöhenbereich = list_ausgewählter_baumhöhenbereich.ElementAt(0);
                        kontrolle.baumhöhe_bereichIDs = ausgewählter_baumhöhenbereich.id;
                    }
                    else
                    {
                        //Error log
                    }
                }

                if (textbox_kronendurchmesser.Text!="")
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
                string kronenzustand_sonstiges="";

                if (checkbox_kronenzustand_3.IsChecked == true)
                {
                    //kontrolle.kronenzustandIDs = kontrolle.kronenzustandIDs + "1 ";
                    kronenzustand_sonstiges = kronenzustand_sonstiges + "; " + "Verkehrsgefährdendes Astwerk/ Lichtraumprofil";
                }
                if (checkbox_kronenzustand_4.IsChecked == true)
                {
                    //kontrolle.kronenzustandIDs = kontrolle.kronenzustandIDs + "2 ";
                   kronenzustand_sonstiges = kronenzustand_sonstiges + "; " + "Trockenholz";
                }

                if (listview_kronenzustand.Items.Count>0)
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

                ////////
                ////Stammfußzustand
                ///////

                //Wurde herausgenommen

                ////////
                ////Wurzelzustand
                ///////
                string wurzelzustand_sonstiges = "";
                if (listview_wurzelzustand.Items.Count > 0)
                {
                    
                    foreach (var eigenschaft in listview_wurzelzustand.Items.ToList())
                    {
                        wurzelzustand_sonstiges = wurzelzustand_sonstiges + "; " + eigenschaft.ToString();
                    }
                }
                wurzelzustand_sonstiges = wurzelzustand_sonstiges.TrimStart(';',' ');
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
                if (checkbox_maßnahmen_1.IsChecked == true)
                {
                    kontrolle.maßnahmenIDs = "1 ";
                }
                if (checkbox_maßnahmen_2.IsChecked == true)
                {
                    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "2 ";
                }
                if (checkbox_maßnahmen_3.IsChecked == true)
                {
                    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "3 ";
                }
                if (checkbox_maßnahmen_4.IsChecked == true)
                {
                    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "4 ";
                }
                if (checkbox_maßnahmen_5.IsChecked == true)
                {
                    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "5 ";
                }
                if (checkbox_maßnahmen_6.IsChecked == true)
                {
                    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "6 ";
                }
                if (checkbox_maßnahmen_7.IsChecked == true)
                {
                    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "7 ";
                }
                if (checkbox_maßnahmen_8.IsChecked == true)
                {
                    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "8 ";
                }
                if (checkbox_maßnahmen_9.IsChecked == true)
                {
                    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "9 ";
                }

                if (checkbox_maßnahmen_sonstiges.IsChecked==true)
                {
                    kontrolle.maßnahmenSonstiges = textbox_maßnahmen_sonstiges.Text;
                }


                //////
                //Ausführen Bis
                /////
                if (combo_ausführen_bis.SelectedItem!=null)
                {
                    List<AusführenBis> list_ausführen_bis = connection_to_arbeitsDB.Query<AusführenBis>("SELECT * FROM tabAusfuehrenBis WHERE name=?", combo_ausführen_bis.SelectedItem);
                    if (list_ausführen_bis.Count!=0)
                    {
                        kontrolle.ausführenBisIDs = list_ausführen_bis.ElementAt(0).id;
                    }
                }

                //Es wird getestet ob an dem Datum für den Baum bereits eine Kontrolle erstellt wurde
                List<Kontrolle> kontrolle_bereits_gespeichert = connection_to_arbeitsDB.Query<Kontrolle>("SELECT * FROM tabKontrolle WHERE baumID=? AND kontrolldatum=?", kontrolle.baumID, kontrolle.kontrolldatum);

                if (kontrolle_bereits_gespeichert.Count==0)
                {
                    connection_to_arbeitsDB.Insert(kontrolle);
                    letzte_kontrolle = kontrolle;
                }
                else
                {
                    MessageDialog message_kontrolle_update = new MessageDialog("Für den angegebenen Baum ist bereits eine Kontrolle mit gleichem Datum vorhanden. \r\n Soll die vorhandene Kontrolle überschrieben werden?");

                    message_kontrolle_update.Commands.Add(new UICommand("Ja"));
                    message_kontrolle_update.Commands.Add(new UICommand("Nein"));

                    var command=await message_kontrolle_update.ShowAsync();

                    switch (command.Label)
                    {   
                        case "Ja":
                            kontrolle.id = kontrolle_bereits_gespeichert.ElementAt(0).id;
                            connection_to_arbeitsDB.Update(kontrolle);
                            letzte_kontrolle = kontrolle;
                            break;
                        case "Nein":
                            //Hier ist zu klären was sinnvoller ist, die letzte kontrolle sollte am besten auf dem alten Wert bleiben
                            //letzte_kontrolle = kontrolle_bereits_gespeichert.ElementAt(0);
                            return;
                        default:
                            break;
                    }
                }

                //Bestätigung für das hinzufügen des Baumes
                icon_baum_hinzufügen_fehler.Visibility = Visibility.Collapsed;
                icon_baum_hinzufügen_ok.Visibility = Visibility.Visible;

                textbox_baum_hinzufügen_fehler.Visibility = Visibility.Collapsed;
                textbox_baum_hinzufügen_ok.Visibility = Visibility.Visible;

                auf_standard_zurücksetzen();

                Flyout.ShowAttachedFlyout((FrameworkElement)sender);
            }
            else
            {
                //Eingabe ungültig
                icon_baum_hinzufügen_fehler.Visibility = Visibility.Visible;
                icon_baum_hinzufügen_ok.Visibility = Visibility.Collapsed;

                textbox_baum_hinzufügen_fehler.Visibility = Visibility.Visible;
                textbox_baum_hinzufügen_ok.Visibility = Visibility.Collapsed;

                Flyout.ShowAttachedFlyout((FrameworkElement)sender);
            }
        }

        private void CommandInvokedHandler_baum_update_ja(IUICommand command)
        {

        }
        private void CommandInvokedHandler_baum_update_nein(IUICommand command)
        {

        }

        //Testen ob die Eingaben gültig sind, wird vor dem Hinzufügen eines Baumes ausgeführt
        private bool auf_gültige_eingabe_testen()
        {
            bool eingabe_gültig=true;

            if (combo_straße.SelectedItem==null)
            {
                eingabe_gültig = false;
                textbox_fehler_keine_straße.Visibility = Visibility.Visible;
            }
            else
            {
                textbox_fehler_keine_straße.Visibility = Visibility.Collapsed;
            }


            if ((textbox_baumNr.Text=="")&&(combo_baumhöhenbereich.SelectedItem==null))
            {
                eingabe_gültig = false;
                textbox_fehler_keine_baumNr.Visibility = Visibility.Visible;
            }
            else
            {
                textbox_fehler_keine_baumNr.Visibility = Visibility.Collapsed;
            }

            if (autotext_baumart_deutsch.Text=="" && autotext_baumart_botanisch.Text=="")
            {
                eingabe_gültig = false;
                textbox_fehler_keine_baumart.Visibility = Visibility.Visible;
            }         
            else
            {
                textbox_fehler_keine_baumart.Visibility = Visibility.Collapsed;
            }

            List<Baumart> list_ausgewählte_baumart = connection_to_arbeitsDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE NameDeutsch=? OR NameBotanisch=?", autotext_baumart_deutsch.Text, autotext_baumart_deutsch.Text);
            if (list_ausgewählte_baumart.Count == 0)
            {
                eingabe_gültig = false;
                textbox_fehler_baumart_nicht_gefunden.Visibility = Visibility.Visible;
            }
            else
            {
                textbox_fehler_baumart_nicht_gefunden.Visibility = Visibility.Collapsed;
            }

            if ((textbox_baumhöhe.Text=="")&&(combo_baumhöhenbereich.SelectedItem==null))
            {
                eingabe_gültig = false;
                textbox_fehler_keine_baumhöhe.Visibility = Visibility.Visible;
            }
            else
            {
                textbox_fehler_keine_baumhöhe.Visibility = Visibility.Collapsed;
            }

            if (textbox_stammdurchmesser.Text=="")
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

            if (ist_baum_verkehrssicher==verkehrssicherheit.ungesetzt)
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

        private void auf_standard_zurücksetzen()
        {
            textbox_baumNr.Text = Convert.ToString(Convert.ToInt32(textbox_baumNr.Text) + 1);

            if (textbox_plakettenNr.Text!="")
            {
                textbox_plakettenNr.Text = Convert.ToString(Convert.ToInt32(textbox_plakettenNr.Text) + 1);
            }

            autotext_baumart_deutsch.Text = "";

            autotext_baumart_botanisch.Text = "";

            combo_entwicklungsphase.SelectedItem = null;

            combo_baumhöhenbereich.SelectedItem = null;

            textbox_baumhöhe.Text = "";

            textbox_kronendurchmesser.Text="";

            textbox_stammdurchmesser.Text = "";

            textbox_stammanzahl.Text = "1";


            combo_schädigungsgrad.SelectedItem = null;

            
            //Zurücksetzen der Checkboxen für den Zustand und leeren der Listview Elemente
            checkbox_kronenzustand_3.IsChecked = false;
            checkbox_kronenzustand_4.IsChecked = false;
            listview_kronenzustand.Items.Clear();

            listview_stammzustand.Items.Clear();

            listview_wurzelzustand.Items.Clear();


            togglebutton_verkehrssicherheit.Background = null;
            togglebutton_verkehrssicherheit.Content = "Verkehrssicher: ";
            ist_baum_verkehrssicher = verkehrssicherheit.ungesetzt;



            checkbox_maßnahmen_1.IsChecked = false;
            checkbox_maßnahmen_2.IsChecked = false;
            checkbox_maßnahmen_3.IsChecked = false;
            checkbox_maßnahmen_4.IsChecked = false;
            checkbox_maßnahmen_5.IsChecked = false;
            checkbox_maßnahmen_6.IsChecked = false;
            checkbox_maßnahmen_7.IsChecked = false;
            checkbox_maßnahmen_8.IsChecked = false;
            checkbox_maßnahmen_9.IsChecked = false;
            checkbox_maßnahmen_sonstiges.IsChecked = false;
            textbox_maßnahmen_sonstiges.Text = "";

            combo_ausführen_bis.SelectedItem = null;

            pivot_baumkontrolle.SelectedIndex = 0;
            
        }

        private async void button_abbrechen_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog messageDialog = new MessageDialog("Soll die Eingabe abgebrochen werden und auf Standard zurückgesetzt werden?");

            messageDialog.Commands.Add(new UICommand("Ja", new UICommandInvokedHandler(this.CommandInvokedHandler_button_abbrechen_ja)));
            messageDialog.Commands.Add(new UICommand("Nein", new UICommandInvokedHandler(this.CommandInvokedHandler_button_abbrechen_nein)));
            await messageDialog.ShowAsync();
        }

        private void CommandInvokedHandler_button_abbrechen_ja(IUICommand command)
        {
            if (textbox_baumNr.Text != "")
            {
                textbox_baumNr.Text = Convert.ToString(Convert.ToInt32(textbox_baumNr.Text) - 1);
            }


            if (textbox_plakettenNr.Text != "")
            {
                textbox_plakettenNr.Text = Convert.ToString(Convert.ToInt32(textbox_plakettenNr.Text) - 1);
            }
            auf_standard_zurücksetzen();
        }
        private void CommandInvokedHandler_button_abbrechen_nein(IUICommand command)
        {
        }


        //Aufrufen des letzten Baumes
        private async void button_letzter_baum_Click(object sender, RoutedEventArgs e)
        {
            if (letzter_baum==null)
            {
                MessageDialog message_kein_letzter_baum = new MessageDialog("Der zuletzt eingegebene Baum konnte nicht aufgerufen werden. Eventuell wurde die Anwendung in der Zwischenzeit neu gestartet.");
                await message_kein_letzter_baum.ShowAsync();
                return;
            }
            //Zunächst werden alle Werte auf Standard zurückgesetzt
            auf_standard_zurücksetzen();

            //Dan werden die Werte des letzten Baumes übernommen
            List<Straße> list_straße = connection_to_arbeitsDB.Query<Straße>("SELECT * FROM tabStrassen WHERE id=?", letzter_baum.straßeId);
            if (list_straße.Count!=0)
            {
                combo_straße.SelectedItem = list_straße.ElementAt(0).name;
            }
            
            textbox_baumNr.Text = Convert.ToString(letzter_baum.baumNr);
            if (letzter_baum.plakettenNr!=0)
            {
                textbox_plakettenNr.Text = Convert.ToString(letzter_baum.plakettenNr);
            }
            

            autotext_baumart_deutsch.Text = connection_to_arbeitsDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE id=?", letzter_baum.baumartId).ElementAt(0).NameDeutsch;
            autotext_baumart_botanisch.Text = connection_to_arbeitsDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE id=?", letzter_baum.baumartId).ElementAt(0).NameBotanisch;
            
            textbox_baumerstelldatum.Text = letzter_baum.erstelldatum;

            if (letzte_kontrolle== null)
            {
                MessageDialog message_keine_letzte_kontrolle = new MessageDialog("Der zuletzt eingegebene Baum konnte geladen werden, jedoch nicht die zugehörige Kontrolle. Eventuell existiert diese nicht, oder es liegt ein Fehler vor.");
                await message_keine_letzte_kontrolle.ShowAsync();
                return;
            }
            List<Entwicklungsphase> list_entwicklungsphase = connection_to_arbeitsDB.Query<Entwicklungsphase>("SELECT * FROM tabEntwicklungsphase WHERE id=?", letzte_kontrolle.entwicklungsphaseID);
            if (list_entwicklungsphase.Count!=0)
            {
                combo_entwicklungsphase.SelectedItem = list_entwicklungsphase.ElementAt(0).name; 
            }
         
            textbox_kontrollintervall.Text = Convert.ToString(letzte_kontrolle.kontrollintervall);


            List<Baumhöhenbereiche> list_baumhöhenbereich = connection_to_arbeitsDB.Query<Baumhöhenbereiche>("SELECT * FROM tabBaumhoehenbereiche WHERE id=?", letzte_kontrolle.baumhöhe_bereichIDs);
            if (list_baumhöhenbereich.Count!=0)
            {
                combo_baumhöhenbereich.SelectedItem = list_baumhöhenbereich.ElementAt(0).name;
            }

            if (letzte_kontrolle.baumhöhe!=0)
            {
                textbox_baumhöhe.Text = Convert.ToString(letzte_kontrolle.baumhöhe);
            }

            if (letzte_kontrolle.kronendurchmesser!=0)
            {
                textbox_kronendurchmesser.Text = Convert.ToString(letzte_kontrolle.kronendurchmesser);
            }

            if (letzte_kontrolle.stammdurchmesser!=0)
            {
                textbox_stammdurchmesser.Text = Convert.ToString(letzte_kontrolle.stammdurchmesser);
            }

            if (letzte_kontrolle.stammanzahl!=0)
            {
                textbox_stammanzahl.Text = Convert.ToString(letzte_kontrolle.stammanzahl);
            }

            List<Schädigungsgrad> list_schädigungsgrad = connection_to_arbeitsDB.Query<Schädigungsgrad>("SELECT * FROM tabSchaedigungsgrad WHERE id=?", letzte_kontrolle.schädigungsgradID);
            if (list_schädigungsgrad.Count!=0)
            {
                combo_schädigungsgrad.SelectedItem = list_schädigungsgrad.ElementAt(0).name;
            }

            if (letzte_kontrolle.kronenzustandSonstiges.Length!=0)
            {               
                string[] kronenzustände = letzte_kontrolle.kronenzustandSonstiges.Split(new Char[]{';' },StringSplitOptions.RemoveEmptyEntries);
                
                foreach (var kronenzustand in kronenzustände)
                {
                    if (kronenzustand.Trim().Contains("Verkehrsgefährdendes Astwerk/ Lichtraumprofil"))
                    {
                        checkbox_kronenzustand_3.IsChecked = true;
                    }
                    else
                    {
                        if (kronenzustand.Trim().Equals("Trockenholz"))
                        {
                            checkbox_kronenzustand_4.IsChecked = true;
                        }
                        else
                        {
                            listview_kronenzustand.Items.Add(kronenzustand.Trim());
                        }
                    }
                }
            }

            if (letzte_kontrolle.stammzustandSonstiges.Length != 0)
            {
                string[] stammzustände = letzte_kontrolle.stammzustandSonstiges.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var stammzustand in stammzustände)
                {
                    listview_stammzustand.Items.Add(stammzustand.Trim());
                }
            }

            if (letzte_kontrolle.wurzelzustandSonstiges.Length != 0)
            {
                string[] wurzelzustände = letzte_kontrolle.wurzelzustandSonstiges.Split(new Char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var wurzelzustand in wurzelzustände)
                {
                    listview_wurzelzustand.Items.Add(wurzelzustand.Trim());
                }
            }

            if (letzte_kontrolle.verkehrssicher==true)
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

            if (letzte_kontrolle.maßnahmenIDs != null)
            {
                string[] maßnahmenIDs = letzte_kontrolle.maßnahmenIDs.Split(new Char[] { ' ' });
                foreach (var id in maßnahmenIDs)
                {
                    if (id.Trim() != "")
                    {
                        switch (Convert.ToUInt32(id))
                        {
                            case 1: checkbox_maßnahmen_1.IsChecked = true;
                                break;
                            case 2: checkbox_maßnahmen_2.IsChecked = true;
                                break;
                            case 3: checkbox_maßnahmen_3.IsChecked = true;
                                break;
                            case 4: checkbox_maßnahmen_4.IsChecked = true;
                                break;
                            case 5: checkbox_maßnahmen_5.IsChecked = true;
                                break;
                            case 6: checkbox_maßnahmen_6.IsChecked = true;
                                break;
                            case 7: checkbox_maßnahmen_7.IsChecked = true;
                                break;
                            case 8: checkbox_maßnahmen_8.IsChecked = true;
                                break;
                            case 9: checkbox_maßnahmen_9.IsChecked = true;
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            if ((letzte_kontrolle.maßnahmenSonstiges != "")&&(letzte_kontrolle.maßnahmenSonstiges!=null))
            {
                checkbox_maßnahmen_sonstiges.IsChecked = true;
                textbox_maßnahmen_sonstiges.Visibility = Visibility.Visible;
                textbox_maßnahmen_sonstiges.Text = letzte_kontrolle.wurzelzustandSonstiges;
            }

            List<AusführenBis> list_ausführen_bis = connection_to_arbeitsDB.Query<AusführenBis>("SELECT * FROM tabAusfuehrenBis WHERE id=?", letzte_kontrolle.ausführenBisIDs);
            if (list_ausführen_bis.Count!=0)
            {
                combo_ausführen_bis.SelectedItem = list_ausführen_bis.ElementAt(0).name;
            }
        }

        //Funktionen zum Bearbeiten der Baumzustände
        private void header_kronenzustand_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (stackpanel_kronenzustand.Visibility==Visibility.Collapsed)
            {
                stackpanel_kronenzustand.Visibility = Visibility.Visible;
            }
            else
            {
                stackpanel_kronenzustand.Visibility = Visibility.Collapsed;
            }
        }
        private async void button_kronenzustand_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            if (autotext_kronenzustand.Text.Length!=0)
            {
                if (listview_kronenzustand.Items.Contains(autotext_kronenzustand.Text)||(checkbox_kronenzustand_4.IsChecked==true&&autotext_kronenzustand.Text=="Trockenholz")||(checkbox_kronenzustand_3.IsChecked==true&&autotext_kronenzustand.Text.Contains("Lichtraumprofil")))
                {
                    MessageDialog message_kronenzustand_vorhanden = new MessageDialog("Der eingegebene Wert ist bereits in der Liste vorhanden, oder in einer der Checkboxen angekreuzt.");
                    await message_kronenzustand_vorhanden.ShowAsync();
                }
                else
                {
                    List<Kronenzustand> kronenzustand_exists = connection_to_arbeitsDB.Query<Kronenzustand>("SELECT * FROM tabKronenzustand WHERE name=?", autotext_kronenzustand.Text);

                    if (kronenzustand_exists.Count != 0)
                    {
                        //Es gibt bereits einen Eintrag in der Datenbank für den eingegebenen Kronenzustand, nominaler Fall
                        listview_kronenzustand.Items.Add(autotext_kronenzustand.Text);
                    }
                    else
                    {
                        MessageDialog message_kronenzustand_hinzufügen = new MessageDialog("Der eingegebene Kronenzustand ist noch nicht in der Datenbank gespeichert. Soll dieser Wert jetzt gespeichert und der Liste hinzugefügt werden?");

                        message_kronenzustand_hinzufügen.Commands.Add(new UICommand("Ja"));
                        message_kronenzustand_hinzufügen.Commands.Add(new UICommand("Nein"));

                        var command = await message_kronenzustand_hinzufügen.ShowAsync();

                        switch(command.Label)
                        {
                            case "Ja":                            
                                listview_kronenzustand.Items.Add(autotext_kronenzustand.Text.Trim());
                                Kronenzustand kronenzustand = new Kronenzustand();
                                kronenzustand.name = autotext_kronenzustand.Text.Trim();
                                connection_to_arbeitsDB.Insert(kronenzustand);
                                break;
                            case "Nein":
                                break;
                        }

                    } 
                }
                                
            }          
        }
        private void listview_kronenzustand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listview_kronenzustand.SelectedItem==null)
            {
                button_kronenzustand_entfernen.Visibility = Visibility.Collapsed;
            }
            else
            {
                button_kronenzustand_entfernen.Visibility = Visibility.Visible;
            }
        }
        private async void button_kronenzustand_entfernen_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog message_kronenzustand_entfernen = new MessageDialog("Woraus soll das Element entfernt werden?");
            message_kronenzustand_entfernen.Commands.Add(new UICommand("Aus Datenbank"));
            message_kronenzustand_entfernen.Commands.Add(new UICommand("Nur aus Liste"));
            var command=await message_kronenzustand_entfernen.ShowAsync();

            switch (command.Label)
            {
                case "Aus Datenbank":
                    
                    Kronenzustand kronenzustand = new Kronenzustand();
                    List<Kronenzustand> list_kronenzustand = connection_to_arbeitsDB.Query<Kronenzustand>("SELECT * FROM tabKronenzustand WHERE name=?",listview_kronenzustand.SelectedItem);
                    if (list_kronenzustand.Count!=0)
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
        private void autotext_kronenzustand_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<Kronenzustand> list_kronenzustände = connection_to_arbeitsDB.Table<Kronenzustand>().ToList();
                List<string> gefilterte_kronenzustände = new List<string>();

                string benutzereingabe = autotext_kronenzustand.Text;

                foreach (var kronenzustand in list_kronenzustände)
                {
                    if (kronenzustand.name.StartsWith(benutzereingabe)==true)
                    {
                        gefilterte_kronenzustände.Add(kronenzustand.name);
                    }
                }

                gefilterte_kronenzustände.Sort();
                autotext_kronenzustand.ItemsSource = gefilterte_kronenzustände;
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
            if (list_kronenzustände_string.Count!=0)
            {
                list_kronenzustände_string.Sort();
                autotext_kronenzustand.ItemsSource = list_kronenzustände_string;
                autotext_kronenzustand.IsSuggestionListOpen = true;
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
        private async void button_stammzustand_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            if (autotext_stammzustand.Text.Length != 0)
            {
                if (listview_stammzustand.Items.Contains(autotext_stammzustand.Text))
                {
                    MessageDialog message_stammzustand_vorhanden = new MessageDialog("Der eingegebene Wert ist bereits in der Liste vorhanden, oder in einer der Checkboxen angekreuzt.");
                    await message_stammzustand_vorhanden.ShowAsync();
                }
                else
                {
                    List<Stammzustand> stammzustand_exists = connection_to_arbeitsDB.Query<Stammzustand>("SELECT * FROM tabStammzustand WHERE name=?", autotext_stammzustand.Text);

                    if (stammzustand_exists.Count != 0)
                    {
                        //Es gibt bereits einen Eintrag in der Datenbank für den eingegebenen Stammzustand, nominaler Fall
                        listview_stammzustand.Items.Add(autotext_stammzustand.Text);
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
                                break;
                            case "Nein":
                                break;
                        }

                    }
                }

            }          
        }
        private void listview_stammzustand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listview_stammzustand.SelectedItem == null)
            {
                button_stammzustand_entfernen.Visibility = Visibility.Collapsed;
            }
            else
            {
                button_stammzustand_entfernen.Visibility = Visibility.Visible;
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
        private void autotext_stammzustand_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<Stammzustand> list_kronenzustände = connection_to_arbeitsDB.Table<Stammzustand>().ToList();
                List<string> gefilterte_kronenzustände = new List<string>();

                string benutzereingabe = autotext_stammzustand.Text;

                foreach (var stammzustand in list_kronenzustände)
                {
                    if (stammzustand.name.StartsWith(benutzereingabe) == true)
                    {
                        gefilterte_kronenzustände.Add(stammzustand.name);
                    }
                }

                gefilterte_kronenzustände.Sort();
                autotext_stammzustand.ItemsSource = gefilterte_kronenzustände;
            }
        }
        private void autotext_stammzustand_GotFocus(object sender, RoutedEventArgs e)
        {
            List<Stammzustand> list_kronenzustände = connection_to_arbeitsDB.Table<Stammzustand>().ToList();
            List<string> list_kronenzustände_string = new List<string>();

            foreach (var stammzustand in list_kronenzustände)
            {
                list_kronenzustände_string.Add(stammzustand.name);
            }
            if (list_kronenzustände_string.Count != 0)
            {
                list_kronenzustände_string.Sort();
                autotext_stammzustand.ItemsSource = list_kronenzustände_string;
                autotext_stammzustand.IsSuggestionListOpen = true;
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
        private async void button_wurzelzustand_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            if (autotext_wurzelzustand.Text.Length != 0)
            {
                if (listview_wurzelzustand.Items.Contains(autotext_wurzelzustand.Text))
                {
                    MessageDialog message_wurzelzustand_vorhanden = new MessageDialog("Der eingegebene Wert ist bereits in der Liste vorhanden, oder in einer der Checkboxen angekreuzt.");
                    await message_wurzelzustand_vorhanden.ShowAsync();
                }
                else
                {
                    List<Wurzelzustand> wurzelzustand_exists = connection_to_arbeitsDB.Query<Wurzelzustand>("SELECT * FROM tabWurzelzustand WHERE name=?", autotext_wurzelzustand.Text);

                    if (wurzelzustand_exists.Count != 0)
                    {
                        //Es gibt bereits einen Eintrag in der Datenbank für den eingegebenen Wurzelzustand, nominaler Fall
                        listview_wurzelzustand.Items.Add(autotext_wurzelzustand.Text);
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
                                break;
                            case "Nein":
                                break;
                        }

                    }
                }

            }          
        }
        private void listview_wurzelzustand_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (listview_wurzelzustand.SelectedItem == null)
            {
                button_wurzelzustand_entfernen.Visibility = Visibility.Collapsed;
            }
            else
            {
                button_wurzelzustand_entfernen.Visibility = Visibility.Visible;
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
        private void autotext_wurzelzustand_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<Wurzelzustand> list_kronenzustände = connection_to_arbeitsDB.Table<Wurzelzustand>().ToList();
                List<string> gefilterte_kronenzustände = new List<string>();

                string benutzereingabe = autotext_wurzelzustand.Text;

                foreach (var wurzelzustand in list_kronenzustände)
                {
                    if (wurzelzustand.name.StartsWith(benutzereingabe) == true)
                    {
                        gefilterte_kronenzustände.Add(wurzelzustand.name);
                    }
                }

                gefilterte_kronenzustände.Sort();
                autotext_wurzelzustand.ItemsSource = gefilterte_kronenzustände;
            }
        }
        private void autotext_wurzelzustand_GotFocus(object sender, RoutedEventArgs e)
        {
            List<Wurzelzustand> list_kronenzustände = connection_to_arbeitsDB.Table<Wurzelzustand>().ToList();
            List<string> list_kronenzustände_string = new List<string>();

            foreach (var wurzelzustand in list_kronenzustände)
            {
                list_kronenzustände_string.Add(wurzelzustand.name);
            }
            if (list_kronenzustände_string.Count != 0)
            {
                list_kronenzustände_string.Sort();
                autotext_wurzelzustand.ItemsSource = list_kronenzustände_string;
                autotext_wurzelzustand.IsSuggestionListOpen = true;
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


        //Kartenstuff
        private void  button_karte_wählen_Click(object sender, RoutedEventArgs e)
        {
            //FileOpenPicker openPicker = new FileOpenPicker();
            //openPicker.ViewMode = PickerViewMode.Thumbnail;
            //openPicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
            //openPicker.FileTypeFilter.Add(".pdf");

            //var test_file = (await KnownFolders.RemovableDevices.GetFoldersAsync()).FirstOrDefault().CreateFileAsync("test.pdf");

            
            

        }



































        

    }
}
