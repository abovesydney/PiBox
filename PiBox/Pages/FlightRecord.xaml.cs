using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using Windows.ApplicationModel;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace PiBox.Pages
{
    public sealed partial class FlightRecord : Page
    {
        //AcList acList = new AcList();
        

        private int basetime;
        public string fullURL;
        public string listString;
        public string recordString;
        public string RecordSelect { get; set; }
        public RootObject Ro;
        public AcList acList;
        private DispatcherTimer timer;
        //BasicGeoposition planePosition = new BasicGeoposition() { Latitude = 0.0, Longitude = 0.0 };
        //Geopoint planeCenter => new Geopoint(planePosition);
        //MapIcon planeLOC => new MapIcon { Location = planeCenter, NormalizedAnchorPoint = new Point(0.5, 1.0), Title = "?", ZIndex = 0 };

        public FlightRecord()
        {
            this.InitializeComponent();
            InitTimer();
        }

        private void InitTimer()
        {
            timer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 1) };
            timer.Tick += Timer_Tick;
            basetime = 5;
            timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            basetime = basetime - 1;
            Debug.WriteLine(basetime.ToString());
            if (basetime == 0)
            {
                timer.Stop();
                basetime = 5;
                RunningUpdate();
                timer.Start();
            }
        }

        public async void ReadRecord()
        {
            _loading.IsActive = true;
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            using (var client = new HttpClient())
            {
                //string url = "http://" + jsonloc + "/aircraftlist.json?fIcoQ=" + RecordSelect;
                //string url = recordURL + "?fIcoQ=" + RecordSelect;
                //string url = recordURL + "/aircraftlist.json?fIcoQ=" + RecordSelect;
                //string url = "http://www.abovesydney.net:8080/VirtualRadar/aircraftlist.json?fIcoQ=" + RecordSelect;
                //testing string url = "http://www.abovesydney.net/test.json";
                //string url = "http://www.abovesydney.net:8080/VirtualRadar/aircraftlist.json?fIcoQ=";
                fullURL = recordString + RecordSelect;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync(fullURL);

                if (response.IsSuccessStatusCode)

                {
                    //Decode JSON to list here

                    var jsonString = await response.Content.ReadAsStringAsync();
                    Ro = JsonConvert.DeserializeObject<RootObject>(jsonString, jsonSettings);

                    //RootObject rootObject = new RootObject();
                    //AcList acList = new AcList();
                    //FlightList.ItemsSource = Ro.acList;
                    client.Dispose();
                    SanityCheck();
                }
                else
                {
                    Debug.WriteLine("ERROR");
                    OnHttpFail();
                }

                async void OnHttpFail()
                {
                    var messageDialog = new MessageDialog("Error getting JSON file.") { Title = "Connection Failed!" };
                    timer.Stop();
                    await messageDialog.ShowAsync();
                    On_BackRequested();
                }

                //Check if record is still valid
                
            }
        }

        async void SanityCheck()
        {
            try
            {
                string _ICAO = Ro.acList[0].Icao;
                _tbICAO.Text = _ICAO;
                DoEverything();
            }
            //Oh shit, it's not! 
            catch (ArgumentOutOfRangeException e)
            {
                Debug.WriteLine(e);
                var messageDialog = new MessageDialog("Invalid ICAO. Flight out of range?") { Title = "ERROR" };
                await messageDialog.ShowAsync();
                On_BackRequested();
            }

            //Populate fields
            void DoEverything()
            {
                string _OPERATOR = Ro.acList[0].Op;
                if (_OPERATOR == null)
                {
                    _OPERATOR = "UNKNOWN / PRIVATE";
                }
                _tbOP.Text = _OPERATOR;
                
                string _REG = Ro.acList[0].Reg;
                if (_REG == null)
                {
                    _REG = "UNKNOWN";
                }
                _tbREG.Text = _REG;

                if (Ro.acList[0].Alt < 50)
                {
                    _tbALT.Text = "GND";
                }
                else
                    _tbALT.Text = Ro.acList[0].Alt.ToString() + "ft";


                string _CALLSIGN = Ro.acList[0].Call;

                if (_CALLSIGN == null)
                {
                    _CALLSIGN = "UNKNOWN";
                }
                _tbCALLSIGN.Text = _CALLSIGN;

                _airline.Text = _OPERATOR;
                _flightnumber.Text = _CALLSIGN;

                _date.Text = "PosUpdated: " + Ro.acList[0].LastPosUpdate.ToLocalTime().ToString("HH:mm:ss");

                double _MAPLAT = Ro.acList[0].Lat;
                double _MAPLON = Ro.acList[0].Long;

                if (_MAPLON == 0)
                {
                    _MAPLAT = -33.9399;
                    _MAPLON = 151.1753;
                }

                float _knSPEED = Ro.acList[0].Spd;
                int _rndSpd = Convert.ToInt32(_knSPEED);
                //long _kmSPEED = _knSPEED * 1.825;
                string _kSPEED = _rndSpd.ToString();
                Speed.Text = "Speed: " + _kSPEED + "kmh";
                int flightALT = Ro.acList[0].Alt;
                int mapALT = 10;

                _loading.IsActive = true;

                if (flightALT == 500 || flightALT < 500)
                {
                    //Zoom map, low flight or ground
                    mapALT = 15;
                }
                else if (flightALT > 500 && flightALT < 10000)
                {
                    //Mid zoom
                    mapALT = 13;
                }
                else if (flightALT > 10000)
                {
                    //Zoom out, high altitude
                    mapALT = 9;
                }

                if (Ro.acList[0].Vsi > 0)
                {
                    // Up arrow
                    _arrow.Visibility = Visibility.Visible;
                    _arrow.Source = _arrow.Source = new BitmapImage(new Uri("ms-appx:///Assets/images/ui/up-arrow.png", UriKind.Absolute));
                }
                else if (Ro.acList[0].Vsi < 0)
                {
                    //Down Arrow
                    _arrow.Visibility = Visibility.Visible;
                    _arrow.Source = _arrow.Source = new BitmapImage(new Uri("ms-appx:///Assets/images/ui/down-arrow.png", UriKind.Absolute));
                }
                else if (Ro.acList[0].Vsi == 0)
                {
                    //DO NOTHING?
                }

                string _couFlagstr = Ro.acList[0].Cou;
                _couFlagImg.Source = _couFlagImg.Source = new BitmapImage(new Uri("ms-appx:///Assets/images/flags/" + _couFlagstr + ".png", UriKind.Absolute));
                _tboxCountry.Text = Ro.acList[0].Cou;

                _BigLogo.Source = _BigLogo.Source = new BitmapImage(new Uri("ms-appx:///Assets/images/logos/tails/" + Ro.acList[0].OpIcao + ".png", UriKind.Absolute));
                
                //Render Map
                _locationMap.Children.Clear();
                BasicGeoposition planePosition = new BasicGeoposition() { Latitude = _MAPLAT, Longitude = _MAPLON };
                Geopoint planeCenter = new Geopoint(planePosition);
                MapIcon planeLOC = new MapIcon { Location = planeCenter, NormalizedAnchorPoint = new Point(0.5, 1.0), Title = _CALLSIGN, ZIndex = 0 };
                _locationMap.MapElements.Clear();
                _locationMap.MapElements.Add(planeLOC);
                _locationMap.Center = planeCenter;
                _locationMap.ZoomLevel = mapALT;
                _loading.IsActive = false;

                //Arrive / Depart
                if (Ro.acList[0].From == null)
                {
                    _tbDeparting.Text = "UNKNOWN";
                }
                else
                    _tbDeparting.Text = Ro.acList[0].From;

                if (Ro.acList[0].To == null)
                {
                    _tbArriving.Text = "UNKNOWN";
                }
                else
                    _tbArriving.Text = Ro.acList[0].To;

                //Are we multi-stop?
                try
                {
                    string _VIA = Ro.acList[0].Stops[0];
                    _tbVia.Text = _VIA;
                }
                //NOPE!
                catch (NullReferenceException)
                {
                    Debug.WriteLine("**CATCH!**  We are direct...");
                    _tbVia.Text = "DIRECT";
                }

                string major = Package.Current.Id.Version.Major.ToString();
                string minor = Package.Current.Id.Version.Major.ToString();
                string build = Package.Current.Id.Version.Build.ToString();
                string revision = Package.Current.Id.Version.Revision.ToString();

                _version.Text = "v " + major + "." + minor + "." + build + "." + revision;
            }
        }

        private async void RunningUpdate()
        {
            _loading.IsActive = true;
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Include,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };

            using (var client = new HttpClient())
            {
                //string url = "http://www.abovesydney.net:8080/VirtualRadar/aircraftlist.json?fIcoQ=" + RecordSelect;
                //testing string url = "http://www.abovesydney.net/test.json";
                //string url = "http://www.abovesydney.net:8080/VirtualRadar/aircraftlist.json?fIcoQ=";
                string url = recordString + RecordSelect;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)

                {
                    //Decode JSON to list here

                    var jsonString = await response.Content.ReadAsStringAsync();
                    Ro = JsonConvert.DeserializeObject<RootObject>(jsonString, jsonSettings);

                    //RootObject rootObject = new RootObject();
                    AcList acList = new AcList();
                    //FlightList.ItemsSource = Ro.acList;
                    client.Dispose();
                    SanityReCheck();
                }
                else
                {
                    Debug.WriteLine("ERROR");
                    OnHttpFail();
                }

                async void OnHttpFail()
                {
                    var messageDialog = new MessageDialog("Error getting JSON file.") { Title = "Connection Failed!" };
                    timer.Stop();
                    await messageDialog.ShowAsync();
                    On_BackRequested();
                }

                //Check if record is still valid
                void SanityReCheck()
                {
                    try
                    {
                        string _ICAO = Ro.acList[0].Icao;
                        _tbICAO.Text = _ICAO;
                        DoSmallUpdate();
                    }
                    //Oh shit, it's not!
                    catch (ArgumentOutOfRangeException e)
                    {
                        Debug.WriteLine(e);
                        //var messageDialog = new MessageDialog("Signal Lost. Flight has gone out of range!") { Title = "ERROR" };
                        timer.Stop();
                        _tbStatus.Foreground = new SolidColorBrush(Colors.Red);
                        _tbStatus.Text = "NO SIGNAL";
                        _loading.IsActive = false;
                        //await messageDialog.ShowAsync();
                        //On_BackRequested();
                    }

                    void DoSmallUpdate()
                    {
                        //Debug.WriteLine(Ro.acList[0].Cou);
                        _tbStatus.Foreground = new SolidColorBrush(Colors.Green);
                        _tbStatus.Text = "RECEIVING";
                        string _CALLSIGN = Ro.acList[0].Call;

                        if (_CALLSIGN == null)
                        {
                            _CALLSIGN = "UNKNOWN";
                        }
                        _tbCALLSIGN.Text = _CALLSIGN;

                        float _knSPEED = Ro.acList[0].Spd;
                        int _rndSpd = Convert.ToInt32(_knSPEED);
                        //long _kmSPEED = _knSPEED * 1.825;
                        string _kSPEED = _rndSpd.ToString();
                        Speed.Text = "Speed: " + _kSPEED + "kmh";

                        double _MAPLAT = Ro.acList[0].Lat;
                        double _MAPLON = Ro.acList[0].Long;
                        int flightALT = Ro.acList[0].Alt;

                        if (_MAPLON == 0)
                        {
                            _MAPLAT = -33.9399;
                            _MAPLON = 151.1753;
                        }

                        if (Ro.acList[0].Alt < 50)
                        {
                            _tbALT.Text = "GND";
                        }
                        else
                            _tbALT.Text = Ro.acList[0].Alt.ToString() + "ft";

                        BasicGeoposition planePosition = new BasicGeoposition() { Latitude = _MAPLAT, Longitude = _MAPLON };
                        Geopoint planeCenter = new Geopoint(planePosition);
                        MapIcon planeLOC = new MapIcon { Location = planeCenter, NormalizedAnchorPoint = new Point(0.5, 1.0), Title = _CALLSIGN, ZIndex = 0 };

                        _locationMap.MapElements.Clear();
                        _locationMap.MapElements.Add(planeLOC);
                        _locationMap.Center = planeCenter;
                        //_loading.IsActive = false;

                        _date.Text = "PosUpdated: " + Ro.acList[0].LastPosUpdate.ToLocalTime().ToString("HH:mm:ss");
                    }
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //Populate settings from local storage
            _BackButton.IsEnabled = Frame.CanGoBack;
            RecordSelect = e.Parameter.ToString();
            {
                ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
                if (localSettings.Values["recordURL"] != null)
                {
                    listString = localSettings.Values["listURL"].ToString();
                    recordString = localSettings.Values["recordURL"].ToString();
                }
                else
                {
                    Debug.WriteLine("Go and do the settings, idiot!");
                    OnSettingsFail();
                }
                ReadRecord();
            }
        }

        public async void OnSettingsFail()
        {
            var messageDialog = new MessageDialog("aircraftlist.json NOT set!") { Title = "Settings Error!" };
            timer.Stop();
            await messageDialog.ShowAsync();
            On_BackRequested();
        }

        private void _backClick(object sender, TappedRoutedEventArgs e)
        {
            On_BackRequested();
        }

        private bool On_BackRequested()
        {
            if (this.Frame.CanGoBack)
            {
                timer.Stop();
                this.Frame.GoBack();
                return true;
            }
            return false;
        }
    }
    }

