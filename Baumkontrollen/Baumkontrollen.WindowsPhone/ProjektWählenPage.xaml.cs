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


// Die Elementvorlage "Leere Seite" ist unter http://go.microsoft.com/fwlink/?LinkID=390556 dokumentiert.

using SQLite;
using Windows.Phone.UI.Input;

namespace Baumkontrollen
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet werden kann oder auf die innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class ProjektWählenPage : Page
    {
        DatenbankVerwalter dbVerwalter= new DatenbankVerwalter();

        SQLiteConnection connection_to_projekteDB;
        SQLiteConnection connection_to_benutzerDB;

        public ProjektWählenPage()
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
            //Verbindung zu den Datenbanken: Benutzer und Projekt aufbauen
            connection_to_projekteDB = dbVerwalter.connectToProjektDB();
            connection_to_benutzerDB = dbVerwalter.connectToBenutzerDB();
            
            //Leeren der Anzeigeelemente für Benutzer und Projekte
            combo_projekt.Items.Clear();
            combo_benutzer.Items.Clear();


            //Auslesen aller Benutzer und Projekte in der jeweiligen Datenbank und hinzufügen dieser zu dem Anzeigeelement
            List<Projekt> liste_projekte = connection_to_projekteDB.Table<Projekt>().ToList();
            foreach (var Projekt in liste_projekte)
            {
                combo_projekt.Items.Add(Projekt.Name);
            }

            List<Benutzer> liste_benutzer = connection_to_benutzerDB.Table<Benutzer>().ToList();
            foreach (var Benutzer in liste_benutzer)
            {
                combo_benutzer.Items.Add(Benutzer.Name);
            }


            //Aktuelle Auswahl de Anzeigeelements auf das aktive Projekt bzw. Benutzer setzen
            List<Projekt> liste_aktive_Projekte = connection_to_projekteDB.Query<Projekt>("SELECT * FROM tabProjekt WHERE Aktiv=?",true);
            foreach (var Projekt in liste_aktive_Projekte)
            {
                combo_projekt.SelectedItem = Projekt.Name;
            }

            List<Benutzer> liste_aktive_benutzer = connection_to_benutzerDB.Query<Benutzer>("SELECT * FROM tabBenutzer WHERE Aktiv=?", true);
            foreach (var Benutzer in liste_aktive_benutzer)
            {
                combo_benutzer.SelectedItem = Benutzer.Name;
            } 
        }


        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            connection_to_projekteDB.Close();
            connection_to_benutzerDB.Close();
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

        //Ädert die Combobox in eine Textbox, um einen neuen Projektnamen einzugeben
        private void button_projekt_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            combo_projekt.Visibility = Visibility.Collapsed;
            textbox_projekt.Visibility = Visibility.Visible;
            button_projekt_hinzufügen_bestätigen.Visibility = Visibility.Visible;
            button_projekt_hinzufügen_abbrechen.Visibility = Visibility.Visible;

            textbox_projekt.Focus(FocusState.Programmatic);
        }

        private void button_benutzer_hinzufügen_Click(object sender, RoutedEventArgs e)
        {
            combo_benutzer.Visibility = Visibility.Collapsed;
            textbox_benutzer.Visibility = Visibility.Visible;
            button_benutzer_hinzufügen_bestätigen.Visibility = Visibility.Visible;
            button_benutzer_hinzufügen_abbrechen.Visibility = Visibility.Visible;

            textbox_benutzer.Focus(FocusState.Programmatic);
        }



        private void button_projekt_hinzufügen_abbrechen_Click(object sender, RoutedEventArgs e)
        {
            textbox_projekt.Visibility = Visibility.Collapsed;
            textbox_projekt.Text = "";
            button_projekt_hinzufügen_bestätigen.Visibility = Visibility.Collapsed;
            button_projekt_hinzufügen_abbrechen.Visibility = Visibility.Collapsed;
            combo_projekt.Visibility = Visibility.Visible;
        }

        private void button_benutzer_hinzufügen_abbrechen_Click(object sender, RoutedEventArgs e)
        {
            textbox_benutzer.Visibility = Visibility.Collapsed;
            textbox_benutzer.Text = "";
            button_benutzer_hinzufügen_bestätigen.Visibility = Visibility.Collapsed;
            button_benutzer_hinzufügen_abbrechen.Visibility = Visibility.Collapsed;
            combo_benutzer.Visibility = Visibility.Visible;
        }


        //Bestätigt den eingegebenen Projektnamen
        private void button_projekt_hinzufügen_bestätigen_Click(object sender, RoutedEventArgs e)
        {
            List<Projekt> liste_projekte_mit_selben_namen = connection_to_projekteDB.Query<Projekt>("SELECT * FROM tabProjekt WHERE Name=?", textbox_projekt.Text);

            if (textbox_projekt.Text != "")
            {
                //Textbox ist nicht leer
                if (liste_projekte_mit_selben_namen.Count==0)
                {
                    //Es ist kein Projekt mit dem selben Namen angelegt
                    Projekt newProjekt = new Projekt
                    {
                        Name = textbox_projekt.Text,
                        Aktiv = true

                    };

                    //Hinzufügen des neuen Projektes zur Projekt DB
                    connection_to_projekteDB.Insert(newProjekt);
                    combo_projekt.Items.Add(newProjekt.Name);
                    combo_projekt.SelectedItem = newProjekt.Name;

                    //Zurücksetzen des UI's
                    textbox_projekt.Visibility = Visibility.Collapsed;
                    textbox_projekt.Text = "";
                    button_projekt_hinzufügen_bestätigen.Visibility = Visibility.Collapsed;
                    button_projekt_hinzufügen_abbrechen.Visibility = Visibility.Collapsed;
                    combo_projekt.Visibility = Visibility.Visible; 

                    //Anlegen der Arbeits DB für das Projekt
                    dbVerwalter.createArbeitsDB(newProjekt.Name);

                    //Aktualisiieren der Baumartentabelle in der neuen DB
                    SQLiteConnection connection_to_arbeitsDB=dbVerwalter.connectToArbeitsDB(newProjekt.Name);
                    SQLiteConnection connection_to_baumartDB=dbVerwalter.connectToBaumartDB();

                    List<Baumart> list_alle_baumarten = connection_to_baumartDB.Table<Baumart>().ToList();

                    if (list_alle_baumarten.Count!=0)
                    {
                        foreach (var baumart in list_alle_baumarten)
                        {
                            connection_to_arbeitsDB.Insert(baumart);
                        }
                    }

                    connection_to_arbeitsDB.Close();
                    connection_to_baumartDB.Close();

                }
                else
                {
                    text_in_projektflyout_keine_eingabe.Visibility = Visibility.Collapsed;
                    text_in_projektflyout_projekt_doppelt.Visibility = Visibility.Visible;
                    texteingabe_projekt.Text = textbox_projekt.Text;                    
                    FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
                }
            }
            else
            {
                text_in_projektflyout_projekt_doppelt.Visibility = Visibility.Collapsed;
                text_in_projektflyout_keine_eingabe.Visibility = Visibility.Visible;
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
            }
        }

        private void button_benutzer_hinzufügen_bestätigen_Click(object sender, RoutedEventArgs e)
        {
            List<Benutzer> liste_benutzer_mit_selben_namen = connection_to_benutzerDB.Query<Benutzer>("SELECT * FROM tabBenutzer WHERE Name=?", textbox_benutzer.Text);

            if (textbox_benutzer.Text != "")
            {
                //Textbox ist nicht leer
                if (liste_benutzer_mit_selben_namen.Count == 0)
                {
                    //Es ist kein Projekt mit dem selben NAmen bereits angelegt
                    Benutzer newBenutzer = new Benutzer
                    {
                        Name = textbox_benutzer.Text,
                        Aktiv = true

                    };

                    connection_to_benutzerDB.Insert(newBenutzer);
                    combo_benutzer.Items.Add(newBenutzer.Name);
                    combo_benutzer.SelectedItem = newBenutzer.Name;

                    textbox_benutzer.Visibility = Visibility.Collapsed;
                    textbox_benutzer.Text = "";
                    button_benutzer_hinzufügen_bestätigen.Visibility = Visibility.Collapsed;
                    button_benutzer_hinzufügen_abbrechen.Visibility = Visibility.Collapsed;
                    combo_benutzer.Visibility = Visibility.Visible;
                }
                else
                {
                    text_in_benutzerflyout_keine_eingabe.Visibility = Visibility.Collapsed;
                    text_in_benutzerflyout_benutzer_doppelt.Visibility = Visibility.Visible;
                    texteingabe_benutzer.Text = textbox_benutzer.Text;
                    FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
                }
            }
            else
            {
                text_in_benutzerflyout_benutzer_doppelt.Visibility = Visibility.Collapsed;
                text_in_benutzerflyout_keine_eingabe.Visibility = Visibility.Visible;
                FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);                
            }
        }

        private void button_projekt_löschen_Click(object sender, RoutedEventArgs e)
        {
            if (combo_projekt.SelectedValue != null)
            {
                List<Projekt> liste_projekt_to_delete = connection_to_projekteDB.Query<Projekt>("SELECT * FROM tabProjekt WHERE Name=?", combo_projekt.SelectedItem.ToString());

                foreach (var projekt in liste_projekt_to_delete)
                {
                    connection_to_projekteDB.Delete<Projekt>(projekt.Name);
                }

                dbVerwalter.deleteArbeitsDB(combo_projekt.SelectedItem.ToString());
      
                combo_projekt.Items.Remove(combo_projekt.SelectedItem);

            } 
            
        }

        private void button_benutzer_löschen_Click(object sender, RoutedEventArgs e)
        {
            if (combo_benutzer.SelectedValue != null)
            {
                List<Benutzer> liste_benutzer_to_delete = connection_to_benutzerDB.Query<Benutzer>("SELECT * FROM tabBenutzer WHERE Name=?", combo_benutzer.SelectedItem.ToString());
                if (liste_benutzer_to_delete.Count!=0)
                {
                    foreach (var benutzer in liste_benutzer_to_delete)
                    {
                        connection_to_benutzerDB.Delete<Benutzer>(benutzer.Name);
                    }
                    combo_benutzer.Items.Remove(combo_benutzer.SelectedItem);
                }
            }    
        }



        private void combo_projekt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<Projekt> liste_zu_deaktivierende_projekte = connection_to_projekteDB.Table<Projekt>().ToList();
            foreach (var projekt in liste_zu_deaktivierende_projekte)
            {
                projekt.Aktiv = false;
                connection_to_projekteDB.Update(projekt);
            }

            if (combo_projekt.SelectedItem!=null)
            {
                List<Projekt> liste_zu_aktivierende_projekte = connection_to_projekteDB.Query<Projekt>("SELECT * FROM tabProjekt WHERE Name=?", combo_projekt.SelectedItem.ToString());

                foreach (var projekt in liste_zu_aktivierende_projekte)
                {
                    projekt.Aktiv = true;
                    connection_to_projekteDB.Update(projekt);
                }
            }
        }

        private void combo_benutzer_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            List<Benutzer> liste_zu_deaktivierende_benutzer = connection_to_benutzerDB.Table<Benutzer>().ToList();
            foreach (var benutzer in liste_zu_deaktivierende_benutzer)
            {
                benutzer.Aktiv = false;
                connection_to_benutzerDB.Update(benutzer);
            }

            if (combo_benutzer.SelectedItem != null)
            {
                List<Benutzer> liste_zu_aktivierende_benutzer = connection_to_benutzerDB.Query<Benutzer>("SELECT * FROM tabBenutzer WHERE Name=?", combo_benutzer.SelectedItem.ToString());

                foreach (var benutzer in liste_zu_aktivierende_benutzer)
                {
                    benutzer.Aktiv = true;
                    connection_to_benutzerDB.Update(benutzer);
                }
            }
        }














    }
}
