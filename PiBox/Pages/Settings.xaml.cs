using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PiBox.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public RootObject Ro;
        public Settings()
        {
            this.InitializeComponent();
        }

        private async void SetnReadJSON(object sender, RoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["jsonloc"] = _tboxJSON.Text;
            _tbResult.Text = localSettings.Values["jsonloc"].ToString();

            using (var client = new HttpClient())
            {
                string jsonloc = localSettings.Values["jsonloc"].ToString();
                string url = "http://" + jsonloc + "/aircraftlist.json";
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)

                {
                    _tbResult.Text = localSettings.Values["jsonloc"].ToString() + " looks OK!!";
                    localSettings.Values["IsFlightsSet"] = 1;
                }
                else
                {
                    _tbResult.Text = "NOPE!";
                }
            }
        }

        #region NAVIGATION
        private bool On_BackRequested()
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
                return true;
            }
            return false;
        }
        private void _backClick(object sender, TappedRoutedEventArgs e)
        {
            On_BackRequested();
        }
        #endregion

        private async void _buttReset(object sender, TappedRoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            await ApplicationData.Current.ClearAsync();
            _tbResult.Text = "RESET!!";
        }
        private async void SetnReadOW(object sender, TappedRoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["OWKey"] = _tboxOWKey.Text;
            localSettings.Values["OWLoc"] = _tboxOWLoc.Text;
            localSettings.Values["OWSet"] = 1;

            using (var client = new HttpClient())
            {
                string apikey = localSettings.Values["OWkey"].ToString();
                string wealoc = localSettings.Values["OWLoc"].ToString();
                string url = "https://api.openweathermap.org/data/2.5/weather?id=" + wealoc + "&appid=" + apikey + "&units=metric&lang=en";
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)

                {
                    //Decode JSON to list here

                    var jsonString = await response.Content.ReadAsStringAsync();
                    weatherJSon.Rootobject ro = JsonConvert.DeserializeObject<weatherJSon.Rootobject>(jsonString);

                    //RootObject rootObject = new RootObject();
                    AcList acList = new AcList();
                    //FlightList.ItemsSource = Ro.acList;
                    client.Dispose();
                    _tbOWResult.Text = ro.name;
                }
                else
                {
                    _tbOWResult.Text = "Uhhhh!";
                }

            }

        }

        private void dbgset(object sender, TappedRoutedEventArgs e)
        {
            _tboxOWKey.Text = "1316159b5cc951e41bafa169cb346185";
            _tboxOWLoc.Text = "2158626";
            _tboxJSON.Text = "www.abovesydney.net:8080/VirtualRadar";
        }
    }
}
