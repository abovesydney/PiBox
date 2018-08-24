using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PiBox.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FlightRecord : Page
    {

        public String recordSelect { get; set; }

        public RootObject ro;
        
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
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            recordSelect = e.Parameter.ToString();
            _BackButton.IsEnabled = this.Frame.CanGoBack;
            DoTheMain();
        }
        #endregion NAVIGATION

        #region MAIN

        public async void DoTheMain()
        {

            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore
                
            };

            using (var client = new HttpClient())
            {
                string url = "http://www.abovesydney.net:8080/VirtualRadar/aircraftlist.json?fIcoQ=" + recordSelect;
                //string url = "http://www.abovesydney.net/test.json";
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    //Decode JSON to list here

                    var jsonString = await response.Content.ReadAsStringAsync();
                    ro = JsonConvert.DeserializeObject<RootObject>(jsonString,jsonSettings);

                    //RootObject rootObject = new RootObject();
                    AcList acList = new AcList();
                    //FlightList.ItemsSource = ro.acList;

                    client.Dispose();
                }
                else
                {
                    Debug.WriteLine("ERROR");
                    OnHTTPFail();
                }

                async void OnHTTPFail()
                {
                    var messageDialog = new MessageDialog("Error getting JSON file.");
                    await messageDialog.ShowAsync();
                    On_BackRequested();

                }

                #endregion

                #region GENERATE
                //Populate fields

                _tbICAO.Text = ro.acList[0].Icao;

                string _OPERATOR = ro.acList[0].Op;
                if (_OPERATOR == null)
                {
                    _OPERATOR = "UNKNOWN";
                }
                _tbOP.Text = _OPERATOR;

                string _REG = ro.acList[0].Reg;
                if (_REG == null)
                {
                    _REG = "UNKNOWN";
                }
                _tbREG.Text = _REG;

                if (ro.acList[0].Alt < 50)
                {
                    _tbALT.Text = "GND";
                }
                else
                _tbALT.Text = ro.acList[0].Alt.ToString() + "m";
                string _CALLSIGN = ro.acList[0].Call;
                if (_CALLSIGN == null)
                {
                    _CALLSIGN = "UNKNOWN";
                }
                _tbCALLSIGN.Text = _CALLSIGN;
                double _MAPLAT = ro.acList[0].Lat;
                double _MAPLON = ro.acList[0].Long;
                if (_MAPLON == 0)
                {
                    _MAPLAT = -33.9399;
                    _MAPLON = 151.1753;
                }

                float _knSPEED = ro.acList[0].Spd;
                double _kmSPEED = _knSPEED * 1.825;
                string _kSPEED = _kmSPEED.ToString();
                Speed.Text = _kSPEED + "km/h";


                int flightALT = ro.acList[0].Alt;
                int mapALT = 10;


                if (flightALT < 500)
                {
                    //Zoom map, low flight or ground
                    mapALT = 15;
                }
                else if (flightALT > 500 && flightALT < 5000)
                {
                    //Mid zoom
                    mapALT = 12;
                }
                else if (flightALT > 5000)
                {
                    //Zoom out, high altitude
                    mapALT = 7;
                }

                _BigLogo.Source = _BigLogo.Source = new BitmapImage(new Uri("ms-appx:///Assets/logos/biglogos/" + ro.acList[0].OpIcao + ".png", UriKind.Absolute));

                //Render Map
                BasicGeoposition planePosition = new BasicGeoposition() { Latitude = _MAPLAT, Longitude = _MAPLON };
                Geopoint planeCenter = new Geopoint(planePosition);
                MapIcon planeLOC = new MapIcon { Location = planeCenter, NormalizedAnchorPoint = new Point(0.5, 1.0), Title = _CALLSIGN, ZIndex = 0 };
                _locationMap.MapElements.Add(planeLOC);
                _locationMap.Center = planeCenter;
                _locationMap.ZoomLevel = mapALT;



            }
        }
        #endregion


        public FlightRecord()
        {
            this.InitializeComponent();
        }

    }
}