using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

using Windows.Storage;
using Windows.UI.Popups;
// Die Vorlage "Leere Anwendung" ist unter http://go.microsoft.com/fwlink/?LinkId=234227 dokumentiert.
#if WINDOWS_PHONE_APP
using Windows.Phone.UI.Input;
#endif

namespace Baumkontrollen
{
    /// <summary>
    /// Stellt das anwendungsspezifische Verhalten bereit, um die Standardanwendungsklasse zu ergänzen.
    /// </summary>
    public sealed partial class App : Application
    {
#if WINDOWS_PHONE_APP
        private TransitionCollection transitions;
#endif

        DatenbankVerwalter dbVerwalter = new DatenbankVerwalter();

        private StorageFile _errorFile;
        public StorageFile ErrorFile { get { return _errorFile; } set { _errorFile = value; } }

        /// <summary>
        /// Initialisiert das Singletonanwendungsobjekt. Dies ist die erste Zeile von erstelltem Code
        /// und daher das logische Äquivalent von main() bzw. WinMain().
        /// </summary>
        public App()
        {
            
            this.InitializeComponent();
            this.Suspending += this.OnSuspending;
            createErrorFile();
            this.UnhandledException+=App_UnhandledException;

#if WINDOWS_PHONE_APP
           //HardwareButtons.BackPressed += HardwareButtons_BackPressed;
#endif

            //dbVerwalter.deleteAllDB();
            dbVerwalter.createProjektDB();
            dbVerwalter.createBenutzerDB();
            dbVerwalter.createBaumartDB();
        }

        private async void App_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
             if (e.Exception.GetType() == typeof(System.ArgumentException)) 
             {
                 await writeErrorMessage(string.Format("UnhandledException - Continue - {0}", e.Exception.ToString()));
                 e.Handled = true; // Keep Running the app 
             } 
             else 
             {

                 Task t = writeErrorMessage(string.Format("UnhandledException - Exit - {0}", e.Exception.ToString()));
                 t.Wait(3000); // Give the application 3 seconds to write to the log file. Should be enough time. 
                                
                 e.Handled = false; 
                 
             }

             
        }

        private async void createErrorFile()
        {

            StorageFolder local = Windows.Storage.ApplicationData.Current.LocalFolder;
            ErrorFile = await local.CreateFileAsync("ErrorFile.txt", CreationCollisionOption.OpenIfExists);

        }

        public async Task writeErrorMessage(string strMessage) 
        { 
            if (ErrorFile != null) 
            { 
                try 
                { 
                    // Run asynchronously 
                    await Windows.Storage.FileIO.AppendTextAsync(ErrorFile, string.Format("{0} - {1}\r\n", DateTime.Now.ToLocalTime().ToString(), strMessage));
                } 
                catch (Exception) 
                {
                    // If another option is available to the app to log error(i.e. Azure Mobile Service, etc...) then try that here 
                } 
            } 
        }

        public async void copyErrorFile_to_SD()
        {
            
            //Zugriff auf die SD-Karte erstellen
            StorageFolder sdCard = (await KnownFolders.RemovableDevices.GetFoldersAsync()).FirstOrDefault();
            //Ordner ProBaumkontrollen erstellen
            StorageFolder dataFolder_on_sd = await sdCard.CreateFolderAsync("ProBaumkontrollen_ErrorLog", CreationCollisionOption.OpenIfExists);

            StorageFile errorFile_intern = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync("ErrorFile.txt");
            
            if (errorFile_intern!=null)
            {
                await errorFile_intern.CopyAsync(dataFolder_on_sd, errorFile_intern.Name, NameCollisionOption.ReplaceExisting);
            }          
        }

        /// <summary>
        /// Wird aufgerufen, wenn die Anwendung durch den Endbenutzer normal gestartet wird.  Weitere Einstiegspunkte
        /// werden verwendet, wenn die Anwendung zum Öffnen einer bestimmten Datei, zum Anzeigen
        /// von Suchergebnissen usw. gestartet wird.
        /// </summary>
        /// <param name="e">Details über Startanforderung und -prozess.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
#if DEBUG
            if (System.Diagnostics.Debugger.IsAttached)
            {
                this.DebugSettings.EnableFrameRateCounter = true;
            }


#endif


            Frame rootFrame = Window.Current.Content as Frame;

            // App-Initialisierung nicht wiederholen, wenn das Fenster bereits Inhalte enthält.
            // Nur sicherstellen, dass das Fenster aktiv ist.
            if (rootFrame == null)
            {
                // Einen Rahmen erstellen, der als Navigationskontext fungiert und zum Parameter der ersten Seite navigieren
                rootFrame = new Frame();

                // TODO: diesen Wert auf eine Cachegröße ändern, die für Ihre Anwendung geeignet ist
                rootFrame.CacheSize = 1;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    // TODO: Zustand von zuvor angehaltener Anwendung laden
                }

                // Den Rahmen im aktuellen Fenster platzieren
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
#if WINDOWS_PHONE_APP
                // Entfernt die Drehkreuznavigation für den Start.
                if (rootFrame.ContentTransitions != null)
                {
                    this.transitions = new TransitionCollection();
                    foreach (var c in rootFrame.ContentTransitions)
                    {
                        this.transitions.Add(c);
                    }
                }

                rootFrame.ContentTransitions = null;
                rootFrame.Navigated += this.RootFrame_FirstNavigated;
#endif

                // Wenn der Navigationsstapel nicht wiederhergestellt wird, zur ersten Seite navigieren
                // und die neue Seite konfigurieren, indem die erforderlichen Informationen als Navigationsparameter
                // übergeben werden
                if (!rootFrame.Navigate(typeof(MainPage), e.Arguments))
                {
                    throw new Exception("Failed to create initial page");
                }
            }

            // Sicherstellen, dass das aktuelle Fenster aktiv ist
            Window.Current.Activate();

            //Kopiert die interne Error Log Datei auf die Speicherkarte
            //copyErrorFile_to_SD();
        }

#if WINDOWS_PHONE_APP
        /// <summary>
        /// Stellt die Inhaltsübergänge nach dem Start der App wieder her.
        /// </summary>
        /// <param name="sender">Das Objekt, an das der Handler angefügt wird.</param>
        /// <param name="e">Details zum Navigationsereignis.</param>
        private void RootFrame_FirstNavigated(object sender, NavigationEventArgs e)
        {
            var rootFrame = sender as Frame;
            rootFrame.ContentTransitions = this.transitions ?? new TransitionCollection() { new NavigationThemeTransition() };
            rootFrame.Navigated -= this.RootFrame_FirstNavigated;
        }

        /// <summary>
        /// Behandelt bei einer Windows Phone Anwendung den Back-Button
        /// </summary>
        /// <param name="sender">Das Objekt, an das der Handler angefügt wird.</param>
        /// <param name="e">Details zum BackPressed-Ereignis.</param>
        //void HardwareButtons_BackPressed(object sender, BackPressedEventArgs e)
        //{
        //     Frame rootFrame = Window.Current.Content as Frame;
    
        //     if (rootFrame != null && rootFrame.CanGoBack)
        //     {
                
        //        e.Handled = true;
        //        rootFrame.GoBack();
        //    }
        //}
#endif

        
        /// <summary>
        /// Wird aufgerufen, wenn die Ausführung der Anwendung angehalten wird.  Der Anwendungszustand wird gespeichert,
        /// ohne zu wissen, ob die Anwendung beendet oder fortgesetzt wird und die Speicherinhalte dabei
        /// unbeschädigt bleiben.
        /// </summary>
        /// <param name="sender">Die Quelle der Anhalteanforderung.</param>
        /// <param name="e">Details zur Anhalteanforderung.</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            // TODO: Anwendungszustand speichern und alle Hintergrundaktivitäten beenden
            deferral.Complete();
        }    
    }
}