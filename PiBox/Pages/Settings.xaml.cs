using Newtonsoft.Json;
using System;
using System.Net.Http;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace PiBox.Pages
{

    public sealed partial class Settings : Page
    {
        ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public RootObject Ro;
        public string adsbLAT;
        public string adsbLON;
        public double adsbDST;
        public string listURL;
        public string recordURL;
        public string airportName;
        public string domain;
        public string totalAc = "0";

        public Settings()
        {
            InitializeComponent();
            //Check for and fill saved settings
            if (localSettings.Values["ADSBSet"] != null)
            {
                serverToggle.IsOn = true;
                listURL = localSettings.Values["listURL"].ToString();
                recordURL = localSettings.Values["recordURL"].ToString();
                airportName = localSettings.Values["airportName"].ToString();
                _tboxAPName.Text = airportName;
                _tboxLat.Text = localSettings.Values["LAT"].ToString();
                _tboxLon.Text = localSettings.Values["LON"].ToString();
                _tboxAPName.Text = localSettings.Values["airportName"].ToString();
                _slidDst.Value = Convert.ToDouble(localSettings.Values["DIST"]);
            }

            else if (localSettings.Values["VRSSet"] != null)
            {
                serverToggle.IsOn = false;
                domain = localSettings.Values["domain"].ToString();
                recordURL = localSettings.Values["recordURL"].ToString();
                airportName = localSettings.Values["airportName"].ToString();
                _tboxAPName.Text = airportName;
                _tboxVRS.Text = domain;
            }
        }

        private void Server_switch(object sender, RoutedEventArgs e)
        {
            ToggleSwitch toggleSwitch = sender as ToggleSwitch;
            if (toggleSwitch != null)
            {
                if (toggleSwitch.IsOn == true)
                {
                    //This means ADSB Exchange
                    //_tboxAPName.IsEnabled = true;
                    _tboxVRS.IsEnabled = false;
                    _tboxLat.IsEnabled = true;
                    _tboxLon.IsEnabled = true;
                    _slidDst.IsEnabled = true;
                    _buttSetnReadVRS.IsEnabled = false;
                    _buttSetnReadADSB.IsEnabled = true;
                    //progress1.Visibility = Visibility.
                }
                else
                {
                    //This means local VRS
                    _tboxVRS.IsEnabled = true;
                    //_tboxAPName.IsEnabled = false;
                    _tboxLat.IsEnabled = false;
                    _tboxLon.IsEnabled = false;
                    _slidDst.IsEnabled = false;
                    _buttSetnReadVRS.IsEnabled = true;
                    _buttSetnReadADSB.IsEnabled = false;

                }
            }
        }

        private async void SetnReadOW(object sender, TappedRoutedEventArgs e)
        {
            //ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
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
                    var OWString = await response.Content.ReadAsStringAsync();
                    weatherJSon.Rootobject ro = JsonConvert.DeserializeObject<weatherJSon.Rootobject>(OWString);
                    client.Dispose();
                    _tbOWResult.Text = ro.name;
                }
                else
                {
                    _tbOWResult.Text = "Uhhhh!";
                }

            }

        }

        private void SetnReadADSB(object sender, TappedRoutedEventArgs e)
        {

            airportName = _tboxAPName.Text;
            adsbLAT = _tboxLat.Text;
            adsbLON = _tboxLon.Text;
            adsbDST = _slidDst.Value;
            _test1.Text = _tboxLat.Text;
            _test2.Text = _tboxLon.Text;
            _test3.Text = _slidDst.Value.ToString() + "km";
            string listURL = "https://public-api.adsbexchange.com/VirtualRadar/AircraftList.json?lat=" + adsbLAT + "&lng=" + adsbLON + "&fDstL=0&fDstU=" + adsbDST;
            string recordURL = "https://public-api.adsbexchange.com/VirtualRadar/AircraftList.json?fIcoQ=";

            localSettings.Values["listURL"] = listURL;
            localSettings.Values["recordURL"] = recordURL;
            localSettings.Values["airportName"] = airportName;
            localSettings.Values["LAT"] = adsbLAT;
            localSettings.Values["LON"] = adsbLON;
            localSettings.Values["DIST"] = adsbDST;
            localSettings.Values["ADSBSet"] = 1;
            _test4.Text = listURL;
            _test5.Text = recordURL;
        }

        private async void SetnReadJSON(object sender, RoutedEventArgs e)
        {
            //ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            localSettings.Values["jsonloc"] = _tboxVRS.Text;
            _tbResult.Text = localSettings.Values["jsonloc"].ToString();

            using (var client = new HttpClient())
            {
                string domain = _tboxVRS.Text;
                string url = "http://" + domain + "/VirtualRadar/aircraftlist.json";
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode && _tboxAPName.Text != "")

                {
                    var vrsString = await response.Content.ReadAsStringAsync();
                    Ro = JsonConvert.DeserializeObject<RootObject>(vrsString);
                    totalAc = Ro.totalAc.ToString();
                    _test6.Text = totalAc;
                    string listURL = "http://" + domain + "/VirtualRadar/aircraftlist.json?fIcoSN=7CF";
                    string recordURL = "http://" + domain + "/VirtualRadar/aircraftlist.json?fIcoQ=";
                    _tbResult.Text = localSettings.Values["jsonloc"].ToString() + " looks OK!!";
                    airportName = _tboxAPName.Text;
                    localSettings.Values["domain"] = domain;
                    localSettings.Values["airportName"] = airportName;
                    localSettings.Values["listURL"] = listURL;
                    localSettings.Values["recordURL"] = recordURL;
                    localSettings.Values["VRSSet"] = 1;
                    _test4.Text = listURL;
                    _test5.Text = recordURL;
                }

                else
                {
                    _tbResult.Text = "NOPE!";
                }

            }
        }

        private void Dbgset(object sender, TappedRoutedEventArgs e)
        {
            _tboxOWKey.Text = "1316159b5cc951e41bafa169cb346185";
            _tboxOWLoc.Text = "2158626";
            _tboxVRS.Text = "www.abovesydney.net:8080";
        }

        private void _backClick(object sender, TappedRoutedEventArgs e)
        {
            On_BackRequested();
        }

        private bool On_BackRequested()
        {
            if (this.Frame.CanGoBack)
            {
                this.Frame.GoBack();
                return true;
            }
            return false;
        }
    }
}
