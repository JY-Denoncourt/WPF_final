using System;
using WeatherApp.Commands;

namespace WeatherApp.ViewModels
{
    public class ConfigurationViewModel : BaseViewModel
    {
        private string apiKey;        

        public string ApiKey { 
            get => apiKey;
            set
            {
                apiKey = value;
                OnPropertyChanged();
            }
        }

        public DelegateCommand<string> SaveConfigurationCommand { get; set; }


        public ConfigurationViewModel()
        {
            Name = GetType().Name;

            ApiKey = GetApiKey();

            SaveConfigurationCommand = new DelegateCommand<string>(SaveConfiguration);
        }

        private void SaveConfiguration(string obj)
        {
            Properties.Settings.Default.apiKey = ApiKey;
            Properties.Settings.Default.Save();
        }

        private string GetApiKey()
        {
            return Properties.Settings.Default.apiKey;
        }

    }
}
