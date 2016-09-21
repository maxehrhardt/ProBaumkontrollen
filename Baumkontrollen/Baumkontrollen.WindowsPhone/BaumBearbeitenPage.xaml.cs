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
using Windows.UI.Popups;
using Windows.UI;
using Baumkontrollen.Hilfstabellen;
using Windows.Phone.UI.Input;
// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkID=390556 dokumentiert.

namespace Baumkontrollen
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class BaumBearbeitenPage : Page
    {
        DatenbankVerwalter dbVerwalter = new DatenbankVerwalter();
        SQLiteConnection connection_to_projekteDB;
        SQLiteConnection connection_to_arbeitsDB;
        SQLiteConnection connection_to_baumartDB;


        bool verkehrssicher = false;

        Baum bearbeiteter_baum;
        Kontrolle bearbeitete_kontrolle;
        
        public BaumBearbeitenPage()
        {           
            HardwareButtons.BackPressed += HardwareButtons_BackPressed;
            this.InitializeComponent();
        }

        /// <summary>
        /// Wird aufgerufen, wenn diese Seite in einem Frame angezeigt werden soll.
        /// </summary>
        /// <param name="e">Ereignisdaten, die beschreiben, wie diese Seite erreicht wurde.
        /// Dieser Parameter wird normalerweise zum Konfigurieren der Seite verwendet.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {            
            connection_to_projekteDB = dbVerwalter.connectToProjektDB();

            List<Projekt> list_aktives_projekt = connection_to_projekteDB.Query<Projekt>("SELECT * FROM tabProjekt WHERE Aktiv=?", true);

            if (list_aktives_projekt.Count!=0)
            {
                connection_to_arbeitsDB = dbVerwalter.connectToArbeitsDB(list_aktives_projekt.ElementAt(0).Name);                              
            }
            else
            {
                MessageDialog error_message = new MessageDialog("Fehler beim verbinden mit der Datenbank des aktuellen Projektes.");
                await error_message.ShowAsync();
                this.Frame.Navigate(typeof(AufgenommeneBäumePage));
            }


            ListView_bäume_item item = e.Parameter as ListView_bäume_item;

            List<Baum> list_bearbeiteter_baum = connection_to_arbeitsDB.Query<Baum>("SELECT * FROM tabBaeume WHERE id=?", item.baumID);

            if (list_bearbeiteter_baum.Count!=0)
            {
                bearbeiteter_baum = list_bearbeiteter_baum.ElementAt(0);
            }
            else
            {
                MessageDialog error_message = new MessageDialog("Fehler beim Finden des ausgewählten Baumes!");
                await error_message.ShowAsync();
                this.Frame.Navigate(typeof(AufgenommeneBäumePage));
            }


            //Dieser Code würde entfallen wenn alle Kontrollen geladen werden
            List<Kontrolle> list_kontrollen = connection_to_arbeitsDB.Query<Kontrolle>("SELECT * FROM tabKontrolle WHERE baumID=?", item.baumID);
            if (list_kontrollen.Count!=0)
            {
                list_kontrollen = list_kontrollen.OrderBy(x => x.kontrolldatum).ToList();
                bearbeitete_kontrolle = list_kontrollen.ElementAt(0);
            }
            else
            {
                MessageDialog error_message = new MessageDialog("Fehler beim Finden der zugehörigen Kontrolle!");
                await error_message.ShowAsync();
                this.Frame.Navigate(typeof(AufgenommeneBäumePage));
            }

            connection_to_baumartDB = dbVerwalter.connectToBaumartDB();

            update_values();


            //for (int i = 1; i < 2; i++)
            //{
            //    PivotItem kontrolle_pivotItem = new PivotItem();
            //    kontrolle_pivotItem.Header = "Kontrolle " + Convert.ToString(i);
            //    pivot_baum_bearbeiten.Items.Add(kontrolle_pivotItem);
            //}
            
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            HardwareButtons.BackPressed -= HardwareButtons_BackPressed;
        }

        private async void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        {

            e.Handled = true;
            MessageDialog message = new MessageDialog("Sie sind dabei die Seite zu verlassen!"+"\r\n"+"Nicht gespeicherte Eingaben werden verworfen."+"\r\n"+"Soll die Seite verlassen werden?");

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

        private void update_values()
        {

            textbox_benutzer.Text = bearbeiteter_baum.benutzer;
            textbox_projekt.Text = bearbeiteter_baum.projekt;

            List<Straße> list_straßen = connection_to_arbeitsDB.Table<Straße>().ToList();
            foreach (var straße in list_straßen)
            {
                combo_straße.Items.Add(straße.name);
            }
            List<Straße> ausgewählte_straße = connection_to_arbeitsDB.Query<Straße>("SELECT * FROM tabStrassen WHERE id=?", bearbeiteter_baum.straßeId);
            if (ausgewählte_straße.Count!=0)
            {
                combo_straße.SelectedItem = ausgewählte_straße.ElementAt(0).name;
            }

            textbox_baumNr.Text = Convert.ToString(bearbeiteter_baum.baumNr);

            textbox_plakettenNr.Text = Convert.ToString(bearbeiteter_baum.plakettenNr);

            update_autotext_baumart();
            List<Baumart> ausgewählte_baumart = connection_to_baumartDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE id=?", bearbeiteter_baum.baumartId);
            if (ausgewählte_baumart.Count!=0)
            {
                autotext_baumart_deutsch.Text = ausgewählte_baumart.ElementAt(0).NameDeutsch;
                autotext_baumart_botanisch.Text = ausgewählte_baumart.ElementAt(0).NameBotanisch;
            }

            List<Entwicklungsphase> list_entwicklungsphasen = connection_to_arbeitsDB.Table<Entwicklungsphase>().ToList();
            foreach (var entwicklungsphase in list_entwicklungsphasen)
            {
                combo_entwicklungsphase.Items.Add(entwicklungsphase.name);
            }
            List<Entwicklungsphase> ausgewählte_entwicklungsphase=connection_to_arbeitsDB.Query<Entwicklungsphase>("SELECT * FROM tabEntwicklungsphase WHERE id=?",bearbeitete_kontrolle.entwicklungsphaseID);
            if (ausgewählte_entwicklungsphase.Count!=0)
            {
                combo_entwicklungsphase.SelectedItem = ausgewählte_entwicklungsphase.ElementAt(0).name;
            }

            textbox_baumerstelldatum.Text = bearbeiteter_baum.erstelldatum;

            textbox_kontrollintervall.Text = Convert.ToString(bearbeitete_kontrolle.kontrollintervall);

            datepicker_kontrolldatum.Date = Convert.ToDateTime(bearbeitete_kontrolle.kontrolldatum);

            List<Baumhöhenbereiche> list_baumhöhenbereiche = connection_to_arbeitsDB.Table<Baumhöhenbereiche>().ToList();
            foreach (var baumhöhenbereich in list_baumhöhenbereiche)
            {
                combo_baumhöhenbereich.Items.Add(baumhöhenbereich.name);
            }
            if (bearbeitete_kontrolle.baumhöhe_bereichIDs!=0)
            {
                List<Baumhöhenbereiche> ausgewählter_baumhöhenbereich = connection_to_arbeitsDB.Query<Baumhöhenbereiche>("SELECT * FROM tabBaumhoehenbereiche WHERE id=?", bearbeitete_kontrolle.baumhöhe_bereichIDs);
                combo_baumhöhenbereich.SelectedItem = ausgewählter_baumhöhenbereich.ElementAt(0).name;
            }

            if (Convert.ToString(bearbeitete_kontrolle.baumhöhe)=="0")
            {
                textbox_baumhöhe.Text = "";
            }
            else
            {
                textbox_baumhöhe.Text = Convert.ToString(bearbeitete_kontrolle.baumhöhe);
            }

            if (Convert.ToString(bearbeitete_kontrolle.kronendurchmesser)=="0")
            {
                textbox_kronendurchmesser.Text = "";
            }
            else
            {
                textbox_kronendurchmesser.Text = Convert.ToString(bearbeitete_kontrolle.kronendurchmesser);
            }

            if (Convert.ToString(bearbeitete_kontrolle.stammdurchmesser)=="0")
            {
                textbox_stammdurchmesser.Text = "";
            }
            else
            {
                textbox_stammdurchmesser.Text = Convert.ToString(bearbeitete_kontrolle.stammdurchmesser);
            }

            if (Convert.ToString(bearbeitete_kontrolle.stammanzahl)=="0")
            {
                textbox_stammanzahl.Text = "";
            }
            else
            {
                textbox_stammanzahl.Text = Convert.ToString(bearbeitete_kontrolle.stammanzahl);
            }
            
            List<Schädigungsgrad> list_schädigungsgrad = connection_to_arbeitsDB.Table<Schädigungsgrad>().ToList();
            foreach (var schädigungsgrad in list_schädigungsgrad)
            {
                combo_schädigungsgrad.Items.Add(schädigungsgrad.name);
            }
            if (bearbeitete_kontrolle.schädigungsgradID!=0)
            {
                List<Schädigungsgrad> ausgewählter_schädigungsgrad = connection_to_arbeitsDB.Query<Schädigungsgrad>("SELECT * FROM tabSchaedigungsgrad WHERE id=?", bearbeitete_kontrolle.schädigungsgradID);
                combo_schädigungsgrad.SelectedItem = ausgewählter_schädigungsgrad.ElementAt(0).name;
            }

            //if (bearbeitete_kontrolle.kronenzustandIDs!=null)
            //{
            //    string[] kronenzustandIDs = bearbeitete_kontrolle.kronenzustandIDs.Split(new Char[] { ' ' });
            //    foreach (var id in kronenzustandIDs)
            //    {
            //        if (id.Trim() != "")
            //        {                                            
            //            switch (Convert.ToInt32(id))
            //            {
            //                case 1:
            //                    checkbox_kronenzustand_1.IsChecked = true;
            //                    break;
            //                case 2:
            //                    checkbox_kronenzustand_2.IsChecked = true;
            //                    break;
            //                case 3:
            //                    checkbox_kronenzustand_3.IsChecked = true;
            //                    break;
            //                case 4:
            //                    checkbox_kronenzustand_4.IsChecked = true;
            //                    break;
            //                case 5:
            //                    checkbox_kronenzustand_5.IsChecked = true;
            //                    break;
            //                case 6:
            //                    checkbox_kronenzustand_6.IsChecked = true;
            //                    break;
            //                case 7:
            //                    checkbox_kronenzustand_7.IsChecked = true;
            //                    break;
            //                case 8:
            //                    checkbox_kronenzustand_8.IsChecked = true;
            //                    break;
            //                case 9:
            //                    checkbox_kronenzustand_9.IsChecked = true;
            //                    break;
            //                case 10:
            //                    checkbox_kronenzustand_10.IsChecked = true;
            //                    break;
            //                case 11:
            //                    checkbox_kronenzustand_11.IsChecked = true;
            //                    break;
            //                case 12:
            //                    checkbox_kronenzustand_12.IsChecked = true;
            //                    break;
            //                default:
            //                    break;
            //            }
            //        }
            //    }
            //}
            if (bearbeitete_kontrolle.kronenzustandSonstiges.Length!=0)
            {
                //checkbox_kronenzustand_sonstiges.IsChecked = true;
                textbox_kronenzustand_sonstiges.Text = bearbeitete_kontrolle.kronenzustandSonstiges;
            }

            //if (bearbeitete_kontrolle.stammzustandIDs != null)
            //{
            //    string[] stammzustandIDs = bearbeitete_kontrolle.stammzustandIDs.Split(new Char[] { ' ' });
            //    foreach (var id in stammzustandIDs)
            //    {
            //        if (id.Trim() != "")
            //        {
            //            switch (Convert.ToInt32(id))
            //            {
            //                case 1:
            //                    checkbox_stammzustand_1.IsChecked = true;
            //                    break;
            //                case 2:
            //                    checkbox_stammzustand_2.IsChecked = true;
            //                    break;
            //                case 3:
            //                    checkbox_stammzustand_3.IsChecked = true;
            //                    break;
            //                case 4:
            //                    checkbox_stammzustand_4.IsChecked = true;
            //                    break;
            //                case 5:
            //                    checkbox_stammzustand_5.IsChecked = true;
            //                    break;
            //                case 6:
            //                    checkbox_stammzustand_6.IsChecked = true;
            //                    break;
            //                default:
            //                    break;
            //            }
            //        }
            //    }
            //}
            if (bearbeitete_kontrolle.stammzustandSonstiges.Length != 0)
            {
                //checkbox_stammzustand_sonstiges.IsChecked = true;
                textbox_stammzustand_sonstiges.Text = bearbeitete_kontrolle.stammzustandSonstiges;
            }

            //if (bearbeitete_kontrolle.stammfußzustandIDs != null)
            //{
            //    string[] stammfußzustandIDs = bearbeitete_kontrolle.stammfußzustandIDs.Split(new Char[] { ' ' });
            //    foreach (var id in stammfußzustandIDs)
            //    {
            //        if (id.Trim() != "")
            //        {
            //            switch (Convert.ToInt32(id))
            //            {
            //                case 1:
            //                    checkbox_stammfußzustand_1.IsChecked = true;
            //                    break;
            //                case 2:
            //                    checkbox_stammfußzustand_2.IsChecked = true;
            //                    break;
            //                case 3:
            //                    checkbox_stammfußzustand_3.IsChecked = true;
            //                    break;
            //                case 4:
            //                    checkbox_stammfußzustand_4.IsChecked = true;
            //                    break;
            //                case 5:
            //                    checkbox_stammfußzustand_5.IsChecked = true;
            //                    break;
            //                case 6:
            //                    checkbox_stammfußzustand_6.IsChecked = true;
            //                    break;
            //                case 7:
            //                    checkbox_stammfußzustand_7.IsChecked = true;
            //                    break;
            //                case 8:
            //                    checkbox_stammfußzustand_8.IsChecked = true;
            //                    break;
            //                default:
            //                    break;
            //            }
            //        }
            //    }
            //}
            //if (bearbeitete_kontrolle.stammfußzustandSonstiges!= null)
            //{
            //    checkbox_stammfußzustand_sonstiges.IsChecked = true;
            //    textbox_stammfußzustand_sonstiges.Text = bearbeitete_kontrolle.stammfußzustandSonstiges;
            //}

            //if (bearbeitete_kontrolle.wurzelzustandIDs != null)
            //{
            //    string[] wurzelzustandIDs = bearbeitete_kontrolle.wurzelzustandIDs.Split(new Char[] { ' ' });
            //    foreach (var id in wurzelzustandIDs)
            //    {
            //        if (id.Trim() != "")
            //        {
            //            switch (Convert.ToInt32(id))
            //            {
            //                case 1:
            //                    checkbox_wurzelzustand_1.IsChecked = true;
            //                    break;
            //                case 2:
            //                    checkbox_wurzelzustand_2.IsChecked = true;
            //                    break;
            //                case 3:
            //                    checkbox_wurzelzustand_3.IsChecked = true;
            //                    break;
            //                case 4:
            //                    checkbox_wurzelzustand_4.IsChecked = true;
            //                    break;
            //                case 5:
            //                    checkbox_wurzelzustand_5.IsChecked = true;
            //                    break;
            //                case 6:
            //                    checkbox_wurzelzustand_6.IsChecked = true;
            //                    break;
            //                case 7:
            //                    checkbox_wurzelzustand_7.IsChecked = true;
            //                    break;
            //                case 8:
            //                    checkbox_wurzelzustand_8.IsChecked = true;
            //                    break;
            //                default:
            //                    break;
            //            }
            //        }
            //    }
            //}
            if (bearbeitete_kontrolle.wurzelzustandSonstiges.Length != 0)
            {
                //checkbox_wurzelzustand_sonstiges.IsChecked = true;
                textbox_wurzelzustand_sonstiges.Text = bearbeitete_kontrolle.wurzelzustandSonstiges;
            }

            if (bearbeitete_kontrolle.verkehrssicher==true)
            {
                verkehrssicher = true;
                togglebutton_verkehrssicherheit.Content = "Verkehrssicher: Ja";
                togglebutton_verkehrssicherheit.Background = new SolidColorBrush(Colors.Green);
            }
            else
            {
                verkehrssicher = false;
                togglebutton_verkehrssicherheit.Content = "Verkehrssicher: Nein";
                togglebutton_verkehrssicherheit.Background = new SolidColorBrush(Colors.Red);
            }

            //if (bearbeitete_kontrolle.maßnahmenIDs != null)
            //{
            //    string[] maßnahmenIDs = bearbeitete_kontrolle.maßnahmenIDs.Split(new Char[] { ' ' });
            //    foreach (var id in maßnahmenIDs)
            //    {
            //        if (id.Trim() != "")
            //        {
            //            switch (Convert.ToInt32(id))
            //            {
            //                case 1:
            //                    checkbox_maßnahmen_1.IsChecked = true;
            //                    break;
            //                case 2:
            //                    checkbox_maßnahmen_2.IsChecked = true;
            //                    break;
            //                case 3:
            //                    checkbox_maßnahmen_3.IsChecked = true;
            //                    break;
            //                case 4:
            //                    checkbox_maßnahmen_4.IsChecked = true;
            //                    break;
            //                case 5:
            //                    checkbox_maßnahmen_5.IsChecked = true;
            //                    break;
            //                case 6:
            //                    checkbox_maßnahmen_6.IsChecked = true;
            //                    break;
            //                case 7:
            //                    checkbox_maßnahmen_7.IsChecked = true;
            //                    break;
            //                case 8:
            //                    checkbox_maßnahmen_8.IsChecked = true;
            //                    break;
            //                case 9:
            //                    checkbox_maßnahmen_9.IsChecked = true;
            //                    break;
            //                default:
            //                    break;
            //            }
            //        }
            //    }
            //}
            if (bearbeitete_kontrolle.maßnahmenSonstiges.Length!=0)
            {
                //checkbox_maßnahmen_sonstiges.IsChecked = true;
                textbox_maßnahmen_sonstiges.Text = bearbeitete_kontrolle.maßnahmenSonstiges;
            }


            List<AusführenBis> list_ausführen_bis = connection_to_arbeitsDB.Table<AusführenBis>().ToList();

            foreach (var ausführen_bis in list_ausführen_bis)
            {
                combo_ausführen_bis.Items.Add(ausführen_bis.name);
            }
            if (bearbeitete_kontrolle.ausführenBisIDs!=0)
            {
                List<AusführenBis> ausgewählter_ausführen_bis = connection_to_arbeitsDB.Query<AusführenBis>("SELECT * FROM tabAusfuehrenBis WHERE id=?", bearbeitete_kontrolle.ausführenBisIDs);
                combo_ausführen_bis.SelectedItem = ausgewählter_ausführen_bis.ElementAt(0).name;
            }
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

        //Hinzufüge einer neuen Baumart

        private void button_baumart_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            if (autotext_baumart_deutsch.Text != "")
            {
                textbox_baumart_deutsch.Text = autotext_baumart_deutsch.Text;
            }
            if (autotext_baumart_botanisch.Text != "")
            {
                textbox_baumart_botanisch.Text = autotext_baumart_botanisch.Text;
            }

            autotext_baumart_deutsch.Visibility = Visibility.Collapsed;
            autotext_baumart_botanisch.Visibility = Visibility.Collapsed;
            button_baumart_hinzufügen.Visibility = Visibility.Collapsed;
            border_baumart_hinzufügen.Visibility = Visibility.Visible;
        }

        private async void button_baumart_hinzufügen_bestätigen_Click(object sender, RoutedEventArgs e)
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

                if (connection_to_baumartDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE NameBotanisch=? AND NameDeutsch=?", baumart.NameBotanisch, baumart.NameDeutsch).Count() == 0)
                {
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
                else
                {
                    MessageDialog message = new MessageDialog("Diese Baumart ist bereits in der Datenbank vorhanden!");
                    await message.ShowAsync();
                }
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
                if (baumart.NameBotanisch != "")
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

            if (liste_baumarten_botanisch.Count != 0)
            {
                liste_baumarten_botanisch.Sort();
                autotext_baumart_botanisch.ItemsSource = liste_baumarten_botanisch;
                autotext_baumart_botanisch.UpdateLayout();
            }

        }

        //Aktualisieren der Baumarten-Autosuggestbox bei Benutzereingabe
        private void autotext_baumart_deutsch_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                List<Baumart> alle_baumarten = connection_to_baumartDB.Table<Baumart>().ToList();
                List<string> gefilterte_baumarten = new List<string>();

                string benutzereingabe = autotext_baumart_deutsch.Text;

                foreach (var baumart in alle_baumarten)
                {
                    if (baumart.NameDeutsch.StartsWith(benutzereingabe) == true)
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
                List<Baumart> alle_baumarten = connection_to_baumartDB.Table<Baumart>().ToList();
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

        //Verhalten des Verkehrssicherheit Buttons

        private void togglebutton_verkehrssicherheit_Click(object sender, RoutedEventArgs e)
        {
            if (verkehrssicher==true)
            {
                verkehrssicher = false;
                togglebutton_verkehrssicherheit.Content = "Verkehrssicher: Nein";
                togglebutton_verkehrssicherheit.Background = new SolidColorBrush(Colors.Red);
            }
            else
            {
                verkehrssicher = true;
                togglebutton_verkehrssicherheit.Content = "Verkehrssicher: Ja";
                togglebutton_verkehrssicherheit.Background = new SolidColorBrush(Colors.Green);
            }


        }





        //Bottom Appbar Button
        private async void button_speichern_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog message_speichern_bestätigung = new MessageDialog("");
            if (auf_gültige_eingabe_testen() == true)
            {

                //////
                //Baum erstellen
                /////
                Baum baum = new Baum();
                baum.id = bearbeiteter_baum.id;
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
                }

                baum.baumNr = Convert.ToInt32(textbox_baumNr.Text);


                if (textbox_plakettenNr.Text != "")
                {
                    baum.plakettenNr = Convert.ToInt32(textbox_plakettenNr.Text);
                }

                if (autotext_baumart_deutsch.Text != "")
                {
                    List<Baumart> list_baum_in_baumart_db = connection_to_baumartDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE NameDeutsch=?", autotext_baumart_deutsch.Text);
                    List<Baumart> list_ausgewählte_baumart = connection_to_arbeitsDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE NameDeutsch=?", autotext_baumart_deutsch.Text);
                    if (list_ausgewählte_baumart.Count == 0 && list_baum_in_baumart_db.Count != 0)
                    {
                        //Wenn der Baum in der internen Datenbank ist, aber nicht in der Tabelle der ArbeitsDB muss dieser erst hinzugefügt werden
                        connection_to_arbeitsDB.Insert(list_baum_in_baumart_db.ElementAt(0));
                        list_ausgewählte_baumart = connection_to_arbeitsDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE NameDeutsch=?", autotext_baumart_deutsch.Text);
                        Baumart ausgewählte_baumart = list_ausgewählte_baumart.ElementAt(0);
                        baum.baumartId = ausgewählte_baumart.ID;
                    }
                    else if (list_ausgewählte_baumart.Count != 0)
                    {
                        Baumart ausgewählte_baumart = list_ausgewählte_baumart.ElementAt(0);
                        baum.baumartId = ausgewählte_baumart.ID;
                    }
                    else
                    {
                        //Error log
                    }

                }
                else
                {
                    if (autotext_baumart_botanisch.Text != "")
                    {
                        List<Baumart> list_baum_in_baumart_db = connection_to_baumartDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE NameDeutsch=?", autotext_baumart_botanisch.Text);
                        List<Baumart> list_ausgewählte_baumart = connection_to_arbeitsDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE NameBotanisch=?", autotext_baumart_botanisch.Text);
                        if (list_ausgewählte_baumart.Count == 0 && list_baum_in_baumart_db.Count != 0)
                        {
                            //Wenn der Baum in der internen Datenbank ist, aber nicht in der Tabelle der ArbeitsDB muss dieser erst hinzugefügt werden
                            connection_to_arbeitsDB.Insert(list_baum_in_baumart_db.ElementAt(0));
                            list_ausgewählte_baumart = connection_to_arbeitsDB.Query<Baumart>("SELECT * FROM tabBaumart WHERE NameBotanisch=?", autotext_baumart_botanisch.Text);
                            Baumart ausgewählte_baumart = list_ausgewählte_baumart.ElementAt(0);
                            baum.baumartId = ausgewählte_baumart.ID;
                        }
                        else if (list_ausgewählte_baumart.Count != 0)
                        {
                            Baumart ausgewählte_baumart = list_ausgewählte_baumart.ElementAt(0);
                            baum.baumartId = ausgewählte_baumart.ID;
                        }
                        else
                        {
                            //Error log
                        }
                    }
                }

                baum.erstelldatum = textbox_baumerstelldatum.Text;
                if (baumdaten_bearbeitet_testen(baum,bearbeiteter_baum)==true)
                {

                    List<Baum> baum_bereits_gespeichert = connection_to_arbeitsDB.Query<Baum>("SELECT * FROM tabBaeume WHERE baumNr=? AND straßeId=?", baum.baumNr, baum.straßeId);
                    if (baum_bereits_gespeichert.ElementAt(0).id == bearbeiteter_baum.id)
                    {
                        MessageDialog message_baum_speichern_bestätigen = new MessageDialog("Sollen die Baumdaten wirklich aktualisiert werden?\r\n");


                        message_baum_speichern_bestätigen.Commands.Add(new UICommand("Ja"));
                        message_baum_speichern_bestätigen.Commands.Add(new UICommand("Nein"));

                        var command = await message_baum_speichern_bestätigen.ShowAsync();
                        switch (command.Label)
                        {
                            case "Ja":
                                //baum.id = bearbeiteter_baum.id;
                                connection_to_arbeitsDB.Update(baum);
                                message_speichern_bestätigung.Content = "Die Baumdaten wurden erfolgreich aktualisiert.\r\n";
                                break;
                            case "Nein":
                                //baum.id = bearbeiteter_baum.id;
                                message_speichern_bestätigung.Content = "Die Baumdaten wurden nicht veränddert.\r\n";
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        MessageDialog message_baum_update = new MessageDialog("Es ist bereits ein Baum mit der angegebenen Baumnummer in der Straße gespeichert." + "\r\n" + "Um diesen zu bearbeiten, wählen sie ihn aus der Liste der aufgenommenen Bäume aus." + "\r\n" + "Am aktuellen Baum wurden keine Veränderungen vorgenommen.");

                        await message_baum_update.ShowAsync();

                        return;
                    }
                }
                else
                {
                    message_speichern_bestätigung.Content = "Die Baumdaten wurden nicht verändert.\r\n";
                }



                //////
                //Kontrolle erstellen
                /////
                Kontrolle kontrolle = new Kontrolle();

                kontrolle.baumID = baum.id;
                
                kontrolle.kontrolldatum =  datepicker_kontrolldatum.Date.ToString("dd.M.yyyy");

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
                if (textbox_kronenzustand_sonstiges.Text.Length>0)
                {
                    kontrolle.kronenzustandSonstiges = textbox_kronenzustand_sonstiges.Text;
                }

                //////
                //Stammzustand
                /////
                if (textbox_stammzustand_sonstiges.Text.Length>0)
                {
                    kontrolle.stammzustandSonstiges = textbox_stammzustand_sonstiges.Text;
                }
                
                       
                //////
                //Wurzelzustand
                /////
                if (textbox_wurzelzustand_sonstiges.Text.Length>0)
                {
                    kontrolle.wurzelzustandSonstiges = textbox_wurzelzustand_sonstiges.Text;
                }
                
                

                //////
                //Verkehrssicherheit
                /////


                if (verkehrssicher == true)
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
                //if (checkbox_maßnahmen_1.IsChecked == true)
                //{
                //    kontrolle.maßnahmenIDs = "1 ";
                //}
                //if (checkbox_maßnahmen_2.IsChecked == true)
                //{
                //    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "2 ";
                //}
                //if (checkbox_maßnahmen_3.IsChecked == true)
                //{
                //    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "3 ";
                //}
                //if (checkbox_maßnahmen_4.IsChecked == true)
                //{
                //    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "4 ";
                //}
                //if (checkbox_maßnahmen_5.IsChecked == true)
                //{
                //    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "5 ";
                //}
                //if (checkbox_maßnahmen_6.IsChecked == true)
                //{
                //    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "6 ";
                //}
                //if (checkbox_maßnahmen_7.IsChecked == true)
                //{
                //    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "7 ";
                //}
                //if (checkbox_maßnahmen_8.IsChecked == true)
                //{
                //    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "8 ";
                //}
                //if (checkbox_maßnahmen_9.IsChecked == true)
                //{
                //    kontrolle.maßnahmenIDs = kontrolle.maßnahmenIDs + "9 ";
                //}

                //if (checkbox_maßnahmen_sonstiges.IsChecked == true)
                //{
                //    kontrolle.maßnahmenSonstiges = textbox_maßnahmen_sonstiges.Text;
                //}
                if (textbox_maßnahmen_sonstiges.Text.Length!=0)
                {
                    kontrolle.maßnahmenSonstiges = textbox_maßnahmen_sonstiges.Text;
                }

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

                if (kontrolle_bearbeitet_testen(kontrolle,bearbeitete_kontrolle)==true)
                {
                    MessageDialog message_kontrolle_speichern_bestätigen = new MessageDialog("Die Kontrolle wurde bearbeitet." + "\r\n" + "Soll die aktuelle Kontrolle mit den neuen Werten überschrieben werden?");

                    message_kontrolle_speichern_bestätigen.Commands.Add(new UICommand("Ja"));
                    message_kontrolle_speichern_bestätigen.Commands.Add(new UICommand("Nein"));

                    var command=await message_kontrolle_speichern_bestätigen.ShowAsync();

                    switch(command.Label)
                    {
                        case "Ja":
                            kontrolle.id = bearbeitete_kontrolle.id;
                            connection_to_arbeitsDB.Update(kontrolle);
                            message_speichern_bestätigung.Content = message_speichern_bestätigung.Content + "Die Kontrolle wurde aktualisiert.\r\n";
                            break;
                        case "Nein":
                            message_speichern_bestätigung.Content = message_speichern_bestätigung.Content + "An der Kontrolle wurde keine Veränderung vorgenommen.\r\n";
                            break;

                        default:
                            break;
                    }
                                
                }
                else
                {
                    message_speichern_bestätigung.Content = message_speichern_bestätigung.Content + "An der Kontrolle wurde keine Veränderung vorgenommen.\r\n";
                }
               
                await message_speichern_bestätigung.ShowAsync();


                this.Frame.Navigate(typeof(AufgenommeneBäumePage));
            }
            else
            {
                //Eingabe ungültig
                icon_baum_speichern_fehler.Visibility = Visibility.Visible;                
                textbox_baum_speichern_fehler.Visibility = Visibility.Visible;

                Flyout.ShowAttachedFlyout((FrameworkElement)sender);
            }            
        }


        private bool auf_gültige_eingabe_testen()
        {
            bool eingabe_gültig = true;

            if (combo_straße.SelectedItem == null)
            {
                eingabe_gültig = false;
                textbox_fehler_keine_straße.Visibility = Visibility.Visible;
            }
            else
            {
                textbox_fehler_keine_straße.Visibility = Visibility.Collapsed;
            }


            if ((textbox_baumNr.Text == "") && (combo_baumhöhenbereich.SelectedItem == null))
            {
                eingabe_gültig = false;
                textbox_fehler_keine_baumNr.Visibility = Visibility.Visible;
            }
            else
            {
                textbox_fehler_keine_baumNr.Visibility = Visibility.Collapsed;
            }

            if (autotext_baumart_deutsch.Text == "" && autotext_baumart_botanisch.Text=="")
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

            if (textbox_stammanzahl.Text=="")
            {
                eingabe_gültig = false;
                textbox_fehler_keine_stammanzahl.Visibility = Visibility.Visible;
            }
            else
            {
                textbox_fehler_keine_stammanzahl.Visibility = Visibility.Collapsed;
            }

            if (verkehrssicher==null)
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

        private bool kontrolle_bearbeitet_testen(Kontrolle aktuelle_kontrolle, Kontrolle ursprüngliche_kontrolle)
        {
            bool kontrolle_bearbeitet = false;
            if (aktuelle_kontrolle.kontrollintervall!=ursprüngliche_kontrolle.kontrollintervall)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.entwicklungsphaseID!=ursprüngliche_kontrolle.entwicklungsphaseID)
            {
                kontrolle_bearbeitet=true;
            }
            if (aktuelle_kontrolle.vitalitätsstufeID != ursprüngliche_kontrolle.vitalitätsstufeID)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.schädigungsgradID != ursprüngliche_kontrolle.schädigungsgradID)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.baumhöhe_bereichIDs != ursprüngliche_kontrolle.baumhöhe_bereichIDs)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.baumhöhe != ursprüngliche_kontrolle.baumhöhe)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.kronendurchmesser != ursprüngliche_kontrolle.kronendurchmesser)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.stammdurchmesser != ursprüngliche_kontrolle.stammdurchmesser)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.stammanzahl != ursprüngliche_kontrolle.stammanzahl)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.kronenzustandIDs != ursprüngliche_kontrolle.kronenzustandIDs)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.kronenzustandSonstiges != ursprüngliche_kontrolle.kronenzustandSonstiges)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.stammzustandIDs != ursprüngliche_kontrolle.stammzustandIDs)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.stammzustandSonstiges != ursprüngliche_kontrolle.stammzustandSonstiges)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.stammfußzustandIDs != ursprüngliche_kontrolle.stammfußzustandIDs)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.stammfußzustandSonstiges != ursprüngliche_kontrolle.stammfußzustandSonstiges)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.wurzelzustandIDs != ursprüngliche_kontrolle.wurzelzustandIDs)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.wurzelzustandSonstiges != ursprüngliche_kontrolle.wurzelzustandSonstiges)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.verkehrssicher != ursprüngliche_kontrolle.verkehrssicher)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.maßnahmenIDs != ursprüngliche_kontrolle.maßnahmenIDs)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.maßnahmenSonstiges != ursprüngliche_kontrolle.maßnahmenSonstiges)
            {
                kontrolle_bearbeitet = true;
            }
            if (aktuelle_kontrolle.ausführenBisIDs != ursprüngliche_kontrolle.ausführenBisIDs)
            {
                kontrolle_bearbeitet = true;
            }
     
            return kontrolle_bearbeitet;
        }

        private bool baumdaten_bearbeitet_testen(Baum aktuelle_baumdaten,Baum ursprüngliche_baumdaten)
        {
            bool baumdaten_bearbeitet = false;

            if (aktuelle_baumdaten.straßeId!=ursprüngliche_baumdaten.straßeId)
            {
                baumdaten_bearbeitet = true;
            }
            if (aktuelle_baumdaten.baumNr != ursprüngliche_baumdaten.baumNr)
            {
                baumdaten_bearbeitet = true;
            }
            if (aktuelle_baumdaten.plakettenNr != ursprüngliche_baumdaten.plakettenNr)
            {
                baumdaten_bearbeitet = true;
            }
            if (aktuelle_baumdaten.baumartId != ursprüngliche_baumdaten.baumartId)
            {
                baumdaten_bearbeitet = true;
            }
            return baumdaten_bearbeitet;
        }

        private async void button_abbrechen_Click(object sender, RoutedEventArgs e)
        {
            MessageDialog message_abbrechen_bestätigen = new MessageDialog("Soll die Eingabe wirklich abgebrochen werden? Die Eingaben werden dabei verworfen!");

            message_abbrechen_bestätigen.Commands.Add(new UICommand("Ja"));
            message_abbrechen_bestätigen.Commands.Add(new UICommand("Nein"));

            var command = await message_abbrechen_bestätigen.ShowAsync();

            switch (command.Label)
            {
                case "Ja":
                    update_values();
                    break;
                case "Nein":
                    break;
                default:
                    break;
            }
        }









    }
}
