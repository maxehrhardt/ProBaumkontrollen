using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using SQLite;
using Windows.Storage;
using System.Threading.Tasks;

using Baumkontrollen.Hilfstabellen;
using System.IO;
using Windows.UI.Popups;


namespace Baumkontrollen
{
    public class DatenbankVerwalter
    {
        string dbFileEnding = ".sqlite";
        static StorageFolder dataFolder;
        static StorageFolder arbeitsDBFolder;

        public DatenbankVerwalter()
        {
            createWorkfolder();
        }


        private async void createWorkfolder()
        {
            if (dataFolder==null)
            {
                dataFolder=await ApplicationData.Current.LocalFolder.CreateFolderAsync("Datenbanken",CreationCollisionOption.OpenIfExists);
                arbeitsDBFolder = await dataFolder.CreateFolderAsync("ArbeitsDatenbanken", CreationCollisionOption.OpenIfExists);
            }
           
        }

        /// <summary>
        /// Prüft ob eine Datebank im Local App Folder vorhanden ist.
        /// </summary>
        /// <param name="dbName">Name der Datenbank die gesucht werden soll. Die Angabe erfolgt ohne Endung.</param>
        public async Task<bool> checkForDB(string dbName)
        {
            
            bool dbExist = true;
            try
            {
                StorageFile dbfile = await ApplicationData.Current.LocalFolder.GetFileAsync(dbName+dbFileEnding);
               
            }
            catch (Exception)
            {
                dbExist = false;
            }
            return dbExist;
        }

        /// <summary>
        /// Erstellt die Projekt-Datenbank im Local App Folder.
        /// </summary>
        public void createProjektDB()
        {
            
            SQLiteConnection connection = new SQLiteConnection(dataFolder.Path+"\\Projekte" + dbFileEnding);

            //Auskommentieren wenn die Tabelle "Projekt" zurückgesetzt werden soll
            //connection.DropTable<Projekt>();

            connection.CreateTable<Projekt>();
            connection.Close();

        }

        public void createBenutzerDB()
        {
            SQLiteConnection connection = new SQLiteConnection(dataFolder.Path + "\\Benutzer" + dbFileEnding);

            //Auskommentieren wenn die Tabelle "Benutzer" zurückgesetzt werden soll
            //connection.DropTable<Projekt>();

            connection.CreateTable<Benutzer>();
            connection.Close();
        }

        public void createBaumartDB()
        {

            SQLiteConnection connection = new SQLiteConnection(dataFolder.Path + "\\Baumart" + dbFileEnding);
            connection.CreateTable<Baumart>();
            connection.Close();
        }

        //Erstellt die Datenbank die die Bäume und Kontrollen enthält
        public void createArbeitsDB(string projektname)
        {
            SQLiteConnection connection_to_arbeitsDB = new SQLiteConnection(arbeitsDBFolder.Path + "\\" + projektname + dbFileEnding);

            //Festlegen der Versionsnummer der DB
            connection_to_arbeitsDB.CreateTable<DBVersion>();
            DBVersion dbVersion = new DBVersion
            {
                id = 1,
                version = "1.1"
            };
            connection_to_arbeitsDB.Insert(dbVersion);

            connection_to_arbeitsDB.CreateTable<Baum>();
            connection_to_arbeitsDB.CreateTable<Kontrolle>();

            connection_to_arbeitsDB.CreateTable<Baumart>();
            //Werte müssen aus der Phone internen DB übertragen werden

            connection_to_arbeitsDB.CreateTable<Straße>();

            ////////
            ///Tabelle Baumhöhenbereiche erstellen
            ////////
            connection_to_arbeitsDB.CreateTable<Baumhöhenbereiche>();
            Baumhöhenbereiche baumhöhenbereich = new Baumhöhenbereiche
            {
                id = 1,
                name = "0-5"
            };
            connection_to_arbeitsDB.Insert(baumhöhenbereich);

            baumhöhenbereich.id = 2;
            baumhöhenbereich.name = "5-10";
            connection_to_arbeitsDB.Insert(baumhöhenbereich);

            baumhöhenbereich.id = 3;
            baumhöhenbereich.name = "10-15";
            connection_to_arbeitsDB.Insert(baumhöhenbereich);

            baumhöhenbereich.id = 4;
            baumhöhenbereich.name = "15-20";
            connection_to_arbeitsDB.Insert(baumhöhenbereich);

            baumhöhenbereich.id = 5;
            baumhöhenbereich.name = "20-25";
            connection_to_arbeitsDB.Insert(baumhöhenbereich);

            baumhöhenbereich.id = 6;
            baumhöhenbereich.name = "25-30";
            connection_to_arbeitsDB.Insert(baumhöhenbereich);

            baumhöhenbereich.id = 7;
            baumhöhenbereich.name = "30-35";
            connection_to_arbeitsDB.Insert(baumhöhenbereich);

            baumhöhenbereich.id = 8;
            baumhöhenbereich.name = ">35";
            connection_to_arbeitsDB.Insert(baumhöhenbereich);

            ////////
            ///Tabelle Entwicklungsphase erstellen
            ////////
            connection_to_arbeitsDB.CreateTable<Entwicklungsphase>();
            Entwicklungsphase entwicklungsphase = new Entwicklungsphase 
            { 
                id=1,
                name="Jugendphase"
            };
            connection_to_arbeitsDB.Insert(entwicklungsphase);

            entwicklungsphase.id = 2;
            entwicklungsphase.name = "Reifephase";
            connection_to_arbeitsDB.Insert(entwicklungsphase);

            entwicklungsphase.id = 3;
            entwicklungsphase.name = "Alterungsphase";
            connection_to_arbeitsDB.Insert(entwicklungsphase);

            ////////
            ///Tabelle Kronenzustand erstellen
            ////////
            connection_to_arbeitsDB.CreateTable<Kronenzustand>();
            //Kronenzustand kronenzustand = new Kronenzustand
            //{
            //    id=1,
            //    name = "Ungünstiger Kronenaufbau"
            //};
            //connection_to_arbeitsDB.Insert(kronenzustand);

            //kronenzustand.id = 2;
            //kronenzustand.name = "Starker Konkurrenzdruck";
            //connection_to_arbeitsDB.Insert(kronenzustand);
            //Kronenzustand kronenzustand = new Kronenzustand();
            //kronenzustand.id = 3;
            //kronenzustand.name = "Verkehrsgefährdendes Astwerk/Lichtraumprofil";
            //connection_to_arbeitsDB.Insert(kronenzustand);

            //kronenzustand.id = 4;
            //kronenzustand.name = "Trockenholz";
            //connection_to_arbeitsDB.Insert(kronenzustand);

            //kronenzustand.id = 5;
            //kronenzustand.name = "Schadhafter Leittrieb";
            //connection_to_arbeitsDB.Insert(kronenzustand);

            //kronenzustand.id = 6;
            //kronenzustand.name = "Reibende Äste";
            //connection_to_arbeitsDB.Insert(kronenzustand);

            //kronenzustand.id = 7;
            //kronenzustand.name = "Unterdurchschnittliche Belaubung";
            //connection_to_arbeitsDB.Insert(kronenzustand);

            //kronenzustand.id = 8;
            //kronenzustand.name = "Vorzeitiger Laubfall";
            //connection_to_arbeitsDB.Insert(kronenzustand);

            //kronenzustand.id = 9;
            //kronenzustand.name = "Blattverfärbungen/Nekrosen";
            //connection_to_arbeitsDB.Insert(kronenzustand);

            //kronenzustand.id = 10;
            //kronenzustand.name = "Spitzendürre";
            //connection_to_arbeitsDB.Insert(kronenzustand);

            //kronenzustand.id = 11;
            //kronenzustand.name = "Insektenbefall/biotische Schäden";
            //connection_to_arbeitsDB.Insert(kronenzustand);

            //kronenzustand.id = 12;
            //kronenzustand.name = "Mangelerscheinung";
            //connection_to_arbeitsDB.Insert(kronenzustand);

            //kronenzustand.id = 13;
            //kronenzustand.name = "Sonstiges";
            //connection_to_arbeitsDB.Insert(kronenzustand);

            ////////
            ///Tabelle Maßnahmen erstellen
            ////////
            connection_to_arbeitsDB.CreateTable<Maßnahmen>();
            //Maßnahmen maßnahmen = new Maßnahmen
            //{
            //    id=1,
            //    name="Fällung"
            //};
            //connection_to_arbeitsDB.Insert(maßnahmen);

            //maßnahmen.id = 2;
            //maßnahmen.name = "Totholz schneiden";
            //connection_to_arbeitsDB.Insert(maßnahmen);

            //maßnahmen.id = 3;
            //maßnahmen.name = "Krone einkürzen 5%";
            //connection_to_arbeitsDB.Insert(maßnahmen);

            //maßnahmen.id = 4;
            //maßnahmen.name = "Krone einkürzen 10%";
            //connection_to_arbeitsDB.Insert(maßnahmen);

            //maßnahmen.id = 5;
            //maßnahmen.name = "Krone einkürzen 20%";
            //connection_to_arbeitsDB.Insert(maßnahmen);

            //maßnahmen.id = 6;
            //maßnahmen.name = "Krone einkürzen 25%";
            //connection_to_arbeitsDB.Insert(maßnahmen);

            //maßnahmen.id = 7;
            //maßnahmen.name = "Kronenpflege";
            //connection_to_arbeitsDB.Insert(maßnahmen);

            //maßnahmen.id = 8;
            //maßnahmen.name = "Kronenauslichtung";
            //connection_to_arbeitsDB.Insert(maßnahmen);

            //maßnahmen.id = 9;
            //maßnahmen.name = "Lichtraumprofil schneiden";
            //connection_to_arbeitsDB.Insert(maßnahmen);



            //maßnahmen.id = 10;
            //maßnahmen.name = "Sonstiges";
            //connection_to_arbeitsDB.Insert(maßnahmen);

            ////////
            ///Tabelle Schädigungsgrad erstellen
            ////////
            connection_to_arbeitsDB.CreateTable<Schädigungsgrad>();
            Schädigungsgrad schädigungsgrad = new Schädigungsgrad
            {
                id=1,
                name="Gesund"
            };
            connection_to_arbeitsDB.Insert(schädigungsgrad);

            schädigungsgrad.id = 2;
            schädigungsgrad.name = "Leicht geschädigt";
            connection_to_arbeitsDB.Insert(schädigungsgrad);

            schädigungsgrad.id = 3;
            schädigungsgrad.name = "Stärker geschädigt";
            connection_to_arbeitsDB.Insert(schädigungsgrad);

            ////////
            ///Tabelle Stammfußzustand erstellen
            ////////

            //connection_to_arbeitsDB.CreateTable<Stammfußzustand>();
            //Stammfußzustand stammfußzustand = new Stammfußzustand
            //{
            //    id=1,
            //    name="Würgewurzeln"
            //};
            //connection_to_arbeitsDB.Insert(stammfußzustand);


            //stammfußzustand.id = 2;
            //stammfußzustand.name = "Höhlungen";
            //connection_to_arbeitsDB.Insert(stammfußzustand);

            //stammfußzustand.id = 3;
            //stammfußzustand.name = "Pilzbefall";
            //connection_to_arbeitsDB.Insert(stammfußzustand);

            //stammfußzustand.id = 4;
            //stammfußzustand.name = "Rindenschäden";
            //connection_to_arbeitsDB.Insert(stammfußzustand);

            //stammfußzustand.id = 5;
            //stammfußzustand.name = "Risse";
            //connection_to_arbeitsDB.Insert(stammfußzustand);

            //stammfußzustand.id = 6;
            //stammfußzustand.name = "Stammfußverbreiterung";
            //connection_to_arbeitsDB.Insert(stammfußzustand);

            //stammfußzustand.id = 7;
            //stammfußzustand.name = "Stockaustriebe";
            //connection_to_arbeitsDB.Insert(stammfußzustand);

            //stammfußzustand.id = 8;
            //stammfußzustand.name = "Wuchsanomalien";
            //connection_to_arbeitsDB.Insert(stammfußzustand);

            //stammfußzustand.id = 9;
            //stammfußzustand.name = "Sonstiges";
            //connection_to_arbeitsDB.Insert(stammfußzustand);


            ////////
            ///Tabelle Stammzustand erstellen
            ////////

            connection_to_arbeitsDB.CreateTable<Stammzustand>();

            //Stammzustand stammzustand = new Stammzustand
            //{
            //    id = 1,
            //    name = "Anfahrschäden"
            //};
            //connection_to_arbeitsDB.Insert(stammzustand);


            //stammzustand.id = 2;
            //stammzustand.name = "Astungswunden";
            //connection_to_arbeitsDB.Insert(stammzustand);

            //stammzustand.id = 3;
            //stammzustand.name = "Baumfremder Bewuchs";
            //connection_to_arbeitsDB.Insert(stammzustand);

            //stammzustand.id = 4;
            //stammzustand.name = "Fäulen";
            //connection_to_arbeitsDB.Insert(stammzustand);

            //stammzustand.id = 5;
            //stammzustand.name = "Pilzbefall";
            //connection_to_arbeitsDB.Insert(stammzustand);

            //stammzustand.id = 6;
            //stammzustand.name = "Bedeutende Neigung";
            //connection_to_arbeitsDB.Insert(stammzustand);

            //stammzustand.id = 7;
            //stammzustand.name = "Sonstiges";
            //connection_to_arbeitsDB.Insert(stammzustand);


            ////////
            ///Tabelle Vitalitätsstufe erstellen
            ////////
            //connection_to_arbeitsDB.CreateTable<Vitalitätsstufe>();

            //Vitalitätsstufe vitalitätsstufe = new Vitalitätsstufe
            //{
            //    id=1,
            //    name="gut"
            //};
            //connection_to_arbeitsDB.Insert(vitalitätsstufe);

            //vitalitätsstufe.id = 2;
            //vitalitätsstufe.name = "mittel";
            //connection_to_arbeitsDB.Insert(vitalitätsstufe);


            //vitalitätsstufe.id = 3;
            //vitalitätsstufe.name = "schlecht";
            //connection_to_arbeitsDB.Insert(vitalitätsstufe);

            //vitalitätsstufe.id = 4;
            //vitalitätsstufe.name = "abgestorben";
            //connection_to_arbeitsDB.Insert(vitalitätsstufe);

            ////////
            ///Tabelle Wurzelzustand erstellen
            ////////
            connection_to_arbeitsDB.CreateTable<Wurzelzustand>();
            //Wurzelzustand wurzelzustand = new Wurzelzustand
            //{
            //    id=1,
            //    name = "Bodenaufwölbungen"
            //};
            //connection_to_arbeitsDB.Insert(wurzelzustand);

            //wurzelzustand.id = 2;
            //wurzelzustand.name = "Bodenrisse";
            //connection_to_arbeitsDB.Insert(wurzelzustand);

            //wurzelzustand.id = 3;
            //wurzelzustand.name = "Pilzbefall";
            //connection_to_arbeitsDB.Insert(wurzelzustand);

            //wurzelzustand.id = 4;
            //wurzelzustand.name = "Baugruben";
            //connection_to_arbeitsDB.Insert(wurzelzustand);

            //wurzelzustand.id = 5;
            //wurzelzustand.name = "Bodenauf- oder Abtrag";
            //connection_to_arbeitsDB.Insert(wurzelzustand);

            //wurzelzustand.id = 6;
            //wurzelzustand.name = "Bodenverdichtung";
            //connection_to_arbeitsDB.Insert(wurzelzustand);

            //wurzelzustand.id = 7;
            //wurzelzustand.name = "Versiegelungen";
            //connection_to_arbeitsDB.Insert(wurzelzustand);

            //wurzelzustand.id = 8;
            //wurzelzustand.name = "Freistellung";
            //connection_to_arbeitsDB.Insert(wurzelzustand);

            //wurzelzustand.id = 9;
            //wurzelzustand.name = "Sonstiges";
            //connection_to_arbeitsDB.Insert(wurzelzustand);

            ////////
            ///Tabelle AusführenBis erstellen
            ////////
            connection_to_arbeitsDB.CreateTable<AusführenBis>();
            AusführenBis ausführenBis = new AusführenBis
            {
                id=1,
                name="Sofort"
            };
            connection_to_arbeitsDB.Insert(ausführenBis);

            ausführenBis.id = 2;
            ausführenBis.name = "Zeitnah";
            connection_to_arbeitsDB.Insert(ausführenBis);

            ausführenBis.id = 3;
            ausführenBis.name = "Bis zur nächsten Kontrolle";
            connection_to_arbeitsDB.Insert(ausführenBis);


            connection_to_arbeitsDB.Close();
        }

        public SQLiteConnection connectToProjektDB()
        {
            return new SQLiteConnection(dataFolder.Path + "\\Projekte" + dbFileEnding);
        }

        public SQLiteConnection connectToBenutzerDB()
        {
            return new SQLiteConnection(dataFolder.Path + "\\Benutzer" + dbFileEnding);
        }

        public SQLiteConnection connectToBaumartDB()
        {
            return new SQLiteConnection(dataFolder.Path + "\\Baumart" + dbFileEnding);
        }

        public SQLiteConnection connectToArbeitsDB(string aktives_projekt)
        {
            return new SQLiteConnection(arbeitsDBFolder.Path + "\\" + aktives_projekt + dbFileEnding);
        }


        public async void deleteArbeitsDB(string projektname)
        {
            StorageFile dbfile;

            dbfile = await arbeitsDBFolder.GetFileAsync(projektname + dbFileEnding);
            try
            {
                await dbfile.DeleteAsync(StorageDeleteOption.Default);
            }
            catch (Exception ex)
            {
                string message = ex.Message;
                throw;
            }           
        }

        public async void deleteAllDB()
        {
            
            var file_to_delete = await dataFolder.GetFileAsync("Projekte"+dbFileEnding);
            await file_to_delete.DeleteAsync();
            file_to_delete = await dataFolder.GetFileAsync("Benutzer" + dbFileEnding);
            await file_to_delete.DeleteAsync();
            file_to_delete = await dataFolder.GetFileAsync("Baumart" + dbFileEnding);
            await file_to_delete.DeleteAsync();


            var files_to_delete = await arbeitsDBFolder.GetFilesAsync();
            foreach (var file in files_to_delete)
            {
                await file.DeleteAsync();
            }
            

        }

        public async void copy_arbeitsDB_to_SD()
        {
            //Zugriff auf die SD-Karte erstellen
            StorageFolder sdCard = (await KnownFolders.RemovableDevices.GetFoldersAsync()).FirstOrDefault();
            //Ordner ProBaumkontrollen erstellen
            StorageFolder dataFolder_on_sd = await sdCard.CreateFolderAsync("ProBaumkontrollen", CreationCollisionOption.OpenIfExists);


            var list_arbeitsDB = await arbeitsDBFolder.GetFilesAsync();

            foreach (var file in list_arbeitsDB)
            {

                if (file.FileType==".sqlite")
                {
                    await file.CopyAsync(dataFolder_on_sd, file.Name, NameCollisionOption.ReplaceExisting);
                }
                else
                {
                    MessageDialog message = new MessageDialog("Datei mit Endung: " + file.FileType + " wurde versucht zu kopieren.");
                    await message.ShowAsync();
                    //throw new System.ArgumentException("File ending while saving was: "+file.FileType);
                }
                
            }

        }

        //Kopiert eine Datei vom internen speicher in den internen Speiceher
        public async void copy_arbeitsDB_to_internal(string projektname)
        {
            //Zugriff auf die SD-Karte erstellen
            StorageFolder sdCard = (await KnownFolders.RemovableDevices.GetFoldersAsync()).FirstOrDefault();
            //Ordner ProBaumkontrollen erstellen
            StorageFolder dataFolder_on_sd = await sdCard.CreateFolderAsync("ProBaumkontrollen", CreationCollisionOption.OpenIfExists);

            var file_arbeitsDB = await dataFolder_on_sd.GetFileAsync(projektname + dbFileEnding);

            await file_arbeitsDB.CopyAsync(arbeitsDBFolder, file_arbeitsDB.Name, NameCollisionOption.ReplaceExisting);

            SQLiteConnection connection_to_projekteDB = connectToProjektDB();

            List<Projekt> list_projekt_vorhanden = connection_to_projekteDB.Query<Projekt>("SELECT * FROM tabProjekt WHERE Name=?",projektname);

            if (list_projekt_vorhanden.Count>0)
            {
                //Irgendwas machen, dass die Beiden Datenbanken zusammenführt
            }
            else
            {
                Projekt projekt = new Projekt();
                projekt.Name = projektname;
                connection_to_projekteDB.Insert(projekt);
                
            }

            connection_to_projekteDB.Close();
        }
    }
}
