using Newtonsoft.Json;
using Ookii.Dialogs.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using WeatherApp.Commands;
using WeatherApp.Models;
using WeatherApp.Services;

namespace WeatherApp.ViewModels
{
    public class ApplicationViewModel : BaseViewModel
    {
        #region Membres -----------------------------------------------------------------------------------------------------

        private BaseViewModel currentViewModel;
        private List<BaseViewModel> viewModels;
        private TemperatureViewModel tvm;
        private OpenWeatherService ows;
        private string filename;
        private string openFilename;
        private string saveFilename;
        private string fileContent;

        private ObservableCollection<TemperatureModel> tempList = new ObservableCollection<TemperatureModel>();

        private VistaSaveFileDialog saveFileDialog;
        private VistaOpenFileDialog openFileDialog;

        #endregion


        #region Propriétés ----------------------------------------------------------------------------------------------------

        public BaseViewModel CurrentViewModel
        {
            get { return currentViewModel; }
            set {
                currentViewModel = value;
                OnPropertyChanged();
            }
        }


        public ObservableCollection<TemperatureModel> TempList
        {
            get { return tempList; }
            set
            {
                tempList = value;
                OnPropertyChanged();
            }
        }


        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
            }
        }

        public string OpenFilename
        {
            get { return openFilename; }
            set
            {
                openFilename = value;
                OnPropertyChanged();
            }
        }

        public string SaveFilename
        {
            get { return saveFilename; }
            set
            {
                saveFilename = value;
                OnPropertyChanged();
            }
        }

        public string FileContent
        {
            get { return fileContent; }
            set
            {
                fileContent = value;
                OnPropertyChanged();
            }
        }


        public DelegateCommand<string> ChangePageCommand { get; set; }
        public DelegateCommand<string> ExportCommand { get; set; }
        public DelegateCommand<string> ImportCommand { get; set; }
        public DelegateCommand<string> LanguageCommand { get; set; }


        public List<BaseViewModel> ViewModels
        {
            get {
                if (viewModels == null)
                    viewModels = new List<BaseViewModel>();
                return viewModels;
            }
        }
        #endregion


        #region Constructeur -----------------------------------------------------------------------------------------------
        public ApplicationViewModel()
        {
            ChangePageCommand = new DelegateCommand<string>(ChangePage);
            ExportCommand = new DelegateCommand<string>(Export, CanExport);
            ImportCommand = new DelegateCommand<string>(Import);
            LanguageCommand = new  DelegateCommand<string>(ChangeLanguage);

            initViewModels();

            CurrentViewModel = ViewModels[0];

        }

        #endregion


        #region Méthodes Base --------------------------------------------------------------------------------------------------
        void initViewModels()
        {
            /// TemperatureViewModel setup
            tvm = new TemperatureViewModel();

            string apiKey = "";

            if (Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") == "DEVELOPMENT")
            {
                apiKey = AppConfiguration.GetValue("OWApiKey");
            }

            if (string.IsNullOrEmpty(Properties.Settings.Default.apiKey) && apiKey == "")
            {
                tvm.RawText = "Aucune clé API, veuillez la configurer";
            } else
            {
                if (apiKey == "")
                    apiKey = Properties.Settings.Default.apiKey;

                ows = new OpenWeatherService(apiKey);
            }

            tvm.SetTemperatureService(ows);
            ViewModels.Add(tvm);

            var cvm = new ConfigurationViewModel();
            ViewModels.Add(cvm);
        }


        private void ChangePage(string pageName)
        {
            if (CurrentViewModel is ConfigurationViewModel)
            {
                ows.SetApiKey(Properties.Settings.Default.apiKey);

                var vm = (TemperatureViewModel)ViewModels.FirstOrDefault(x => x.Name == typeof(TemperatureViewModel).Name);
                if (vm.TemperatureService == null)
                    vm.SetTemperatureService(ows);
            }

            CurrentViewModel = ViewModels.FirstOrDefault(x => x.Name == pageName);
        }

        #endregion


        #region Methodes Export / Import ----------------------------------------------------------------------------------
        private bool CanExport(string obj)
        {
            ///TODO a modif pour mettre false qqu part ici
            if (tvm.Temperatures != null) return true;
            else return false;
        }

        private void Export(string obj)
        {
            //ouvrir fichier
            if (saveFileDialog == null)
            {
                saveFileDialog = new VistaSaveFileDialog();
                saveFileDialog.Filter = "Json file|*.json|All files|*.*";
                saveFileDialog.DefaultExt = "json";
            }

            //ecrire dedans
            if (saveFileDialog.ShowDialog() == true)
            {
                SaveFilename = saveFileDialog.FileName;
                saveToFile();
            }

        }


        //++++++++++++++++++++++++++++++++++++++++++++++++++++++++

        private void Import(string obj)
        {
            if (openFileDialog == null)
            {
                openFileDialog = new VistaOpenFileDialog();
                openFileDialog.Filter = "Json file|*.json|All files|*.*";
                openFileDialog.DefaultExt = "json";
            }

            if (openFileDialog.ShowDialog() == true)
            {
                OpenFilename = openFileDialog.FileName;
                openFromFile();
            }


        }
        #endregion


        #region Methodes File ----------------------------------------------------------------------------------------------

        private void openFromFile()
        {
            using (var sr = new StreamReader(OpenFilename))
            {
                //FileContent = "-- FileContent --" + Environment.NewLine;
                FileContent += sr.ReadToEnd();

                if (FileContent != "")
                {
                   
                   tvm.Temperatures = JsonConvert.DeserializeObject<ObservableCollection<TemperatureModel>>(FileContent);
                    //tvm.RawText = string.Join(Environment.NewLine, tvm.Temperatures);
                    FileContent = "";
                   
                    
                }

            }
        }


        private void saveToFile()
        {
            var resultat = JsonConvert.SerializeObject(tvm.Temperatures, Formatting.Indented);

            using (var tw = new StreamWriter(SaveFilename, false))
            {
                tw.WriteLine(resultat);
                tw.Close();
            }

        }


        

        #endregion


        #region Internationnalisation -------------------------------------------------------------------------------------

        private void ChangeLanguage(string language)
        {
            MessageBoxResult result = MessageBox.Show($"{ Properties.Resources.Msg_Restart}", "My App", MessageBoxButton.YesNoCancel);
            switch (result)
            {
                case MessageBoxResult.Yes:
                    Properties.Settings.Default.Language = language;
                    Properties.Settings.Default.Save();
                    Restart();
                    break;

                case MessageBoxResult.No:
                    MessageBox.Show($"{ Properties.Resources.Msg_RestartLater}");
                    Properties.Settings.Default.Language = language;
                    Properties.Settings.Default.Save();
                    break;

                case MessageBoxResult.Cancel:
                    MessageBox.Show($"{ Properties.Resources.Msg_Nevermind}");
                    break;
            }
        }


        void Restart()
        {
            var filename = Application.ResourceAssembly.Location;
            var newFile = Path.ChangeExtension(filename, ".exe");
            Process.Start(newFile);
            Application.Current.Shutdown();
        }
        #endregion

    }
}
