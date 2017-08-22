
namespace ForeignExchange.ViewModels
{
    using ForeignExchange.Models;
    using GalaSoft.MvvmLight.Command;
    using Newtonsoft.Json;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Net.Http;
    using System.Windows.Input;
    using Xamarin.Forms;

    public class MainViewModel : INotifyPropertyChanged
    {
        #region Events
        public event PropertyChangedEventHandler PropertyChanged;
        #endregion

        #region Atributes

        bool _isRunning;
        string _result;
        bool _isEnable;
        ObservableCollection<Rate> _rates;


        #endregion

        #region Properties
        public string Amount
        {
            get;
            set;
        }

        public ObservableCollection<Rate> Rates
        {
            get
            {
                return _rates;
            }
            set
            {
                if (_rates != value)
                {
                    _rates = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(Rates)));
                }
            }
        }

        public Rate SourceRate { get; set; }

        public Rate TargetRate { get; set; }

        public bool IsRunning
        {
            get
            {
                return _isRunning;
            }
            set
            {
                if (_isRunning != value)
                {
                    _isRunning = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(IsRunning)));
                }
            }

        }

        public bool IsEnabled
        {
            get
            {
                return _isEnable;
            }
            set
            {
                if (_isEnable != value)
                {
                    _isEnable = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(IsEnabled)));
                }
            }
        }

        public string Result
        {
            get
            {
                return _result;
            }
            set
            {
                if (_result != value)
                {
                    _result = value;
                    PropertyChanged?.Invoke(
                        this,
                        new PropertyChangedEventArgs(nameof(Result)));
                }
            }
        }

        #endregion

        #region Constructor

        public MainViewModel()
        {
            LoadRates();
        }


        #region Methonds

        async void LoadRates()
        {
            IsRunning = true;
            Result = "Loading Rates...";

            try
            {
                var client = new HttpClient();
                client.BaseAddress = new Uri("http://apiexchangerates.azurewebsites.net");
                var controller = "api/Rates";
                var response = await client.GetAsync(controller);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    IsRunning = false;
                    Result = result;
                }

                var rates = JsonConvert.DeserializeObject<List<Rate>>(result);
                Rates = new ObservableCollection<Rate>(rates);

                IsRunning = false;
                Result = "Ready to convert!";

                IsEnabled = true;

            }
            catch (Exception ex)
            {

                IsRunning = false;
                Result = ex.Message;
            }

        }

        #endregion


        #endregion
        #region Commands

        public ICommand ConvertCommand
        {
            get
            {
                return new RelayCommand(Convert);
            }
        }

        async void Convert()
        {
            if (string.IsNullOrEmpty(Amount))
            {
                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "You must enter in a value in amount!",
                    "Accept");
                return;
            }

            decimal amount = 0;

            if (!decimal.TryParse(Amount, out amount))
            {

                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "You must enter in a numeric value in amount!",
                    "Accept");
                return;
            }

            if (SourceRate == null)
            {

                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "You must select a Source Rate!",
                    "Accept");
                return;
            }

            if (TargetRate == null)
            {

                await Application.Current.MainPage.DisplayAlert(
                    "Error",
                    "You must select a Target Rate!",
                    "Accept");
                return;
            }

            var amountConverted = amount / (decimal)SourceRate.TaxRate * (decimal)TargetRate.TaxRate;

            Result = string.Format("{0} {1:C2} = {2} {3:C2}", 
                SourceRate.Code, 
                amount, 
                TargetRate.Code, 
                amountConverted);
        }

        #endregion
    }
}
