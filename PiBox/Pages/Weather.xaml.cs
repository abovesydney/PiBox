using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PiBox.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Weather : Page
    {

        public string apikey;
        public string wealoc;

        public Weather()
        {
            this.InitializeComponent();

            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                if (localSettings.Values["OWSet"] != null)
                {
                    wealoc = localSettings.Values["OWLoc"].ToString();
                    apikey = localSettings.Values["OWKey"].ToString();
                    DoTheMain();
                }
                else
                {
                    Debug.WriteLine("Go and do the settings, idiot!");
                }
            }

            DoTheMain();
        }

        public async void DoTheMain()
        {
            //GC.Collect(2);

            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            using (var client = new HttpClient())
            {
                try
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                    //var response = await client.GetAsync("https://api.openweathermap.org/data/2.5/weather?q=Marrickville,au&appid=1316159b5cc951e41bafa169cb346185&units=metric&lang=en");
                    var response = await client.GetAsync("https://api.openweathermap.org/data/2.5/weather?id=" + wealoc + "&appid=" + apikey + "&units=metric&lang=en");

                    if (response.IsSuccessStatusCode)
                    {
                        //Decode JSON to list here
                        var jsonString = await response.Content.ReadAsStringAsync();
                        weatherJSon.Rootobject ro = JsonConvert.DeserializeObject<weatherJSon.Rootobject>(jsonString);

                        //weatherJSon.Rootobject rootObject = new weatherJSon.Rootobject();
                        //AcList acList = new AcList();
                        //FlightList.ItemsSource = ro.acList;
                        string _weaName = ro.name;
                        string _weaTemp = ro.main.temp.ToString();
                        string _weaDesc = ro.weather[0].description;
                        string _weaPress = ro.main.pressure.ToString();
                        string _weaWinDir = ro.wind.deg.ToString();
                        string _weaWinSpd = ro.wind.speed.ToString();
                        string _weaHumid = ro.main.humidity.ToString();
                        string _weaIcon = "ms-appx:///Assets/images/weather/" +  ro.weather[0].icon + ".png";
                        _TBbigTemp.Text = _weaTemp + "°C ";
                        _TBbigwords.Text = _weaDesc;
                        _TBbiglocation.Text = _weaName;
                        _TBPressure.Text = _weaPress + "hPa";
                        _TBWDir.Text = _weaWinDir + "°";
                        _TBWSpd.Text = _weaWinSpd + "m/sec";
                        _TBHumid.Text = _weaHumid + "%";
                        _imgWeather.Source = new BitmapImage(new Uri(_weaIcon, UriKind.Absolute));

                    }
                    else
                    {
                        Debug.WriteLine("ERROR");
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
    }
}
