using Newtonsoft.Json;
using System;
using System.Net.Http;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;

namespace PiBox
{
    public sealed partial class Home : Page
    {
        private DispatcherTimer Timer = new DispatcherTimer();

        public Home()
        {
            InitializeComponent();
            DataContext = this;
            Timer.Tick += Timer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();
            _date.Text = DateTime.Now.ToShortDateString();
            GetWeather();
        }

        private async void _buttReset(object sender, TappedRoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            await ApplicationData.Current.ClearAsync();
            _tbReset.Text = "RESET!!";
        }

        private async void _navFlights(object sender, TappedRoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values["VRSSet"] != null || localSettings.Values["ADSBSet"] != null)
            {
                Frame.Navigate(typeof(Pages.FlightList), new DrillInNavigationTransitionInfo()); ;
            }
            else
            {
                var messageDialog = new MessageDialog("Aircraftlist.json not set!");
                await messageDialog.ShowAsync();
            }
        }

        private void _navSettings(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(Pages.Settings), new DrillInNavigationTransitionInfo());
        }

        private async void _navWeather(object sender, TappedRoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values["OWSet"] != null)
            {
                Frame.Navigate(typeof(Pages.Weather), new DrillInNavigationTransitionInfo()); ;
            }
            else
            {
                var messageDialog = new MessageDialog("Weather location not set!");
                await messageDialog.ShowAsync();
            }
        }

        private async void GetWeather()
        {
            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    var response = await client.GetAsync("https://api.openweathermap.org/data/2.5/weather?q=Marrickville,au&appid=1316159b5cc951e41bafa169cb346185&units=metric");

                    if (response.IsSuccessStatusCode)
                    {
                        //Decode JSON to list here
                        var jsonString = await response.Content.ReadAsStringAsync();
                        weatherJSon.Rootobject ro = JsonConvert.DeserializeObject<weatherJSon.Rootobject>(jsonString);

                        weatherJSon.Rootobject rootObject = new weatherJSon.Rootobject();
                        //AcList acList = new AcList();
                        //FlightList.ItemsSource = ro.acList;
                        string _weaName = ro.name;
                        string _weaTemp = ro.main.temp.ToString();
                        string _weaDesc = ro.weather[0].description;
                        string _weaMain = ro.weather[0].main;
                        _weather.Text = _weaTemp + "°C  " + _weaMain;
                    }
                    else
                    {
                        OnHTTPFail();
                    }
                }
                finally
                {
                    client.Dispose();
                }

                async void OnHTTPFail()
                {
                    var messageDialog = new MessageDialog("Error getting JSON file.");
                    await messageDialog.ShowAsync();
                }
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            _clock.Text = DateTime.Now.ToString("h:mm:ss tt");
        }
    }
}