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
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PiBox.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MapTest : Page
    {
        public MapTest()
        {
            this.InitializeComponent();
        }

        public RootObject Ro;

        private async void MapRun()
        {
            using (var client = new HttpClient())
            {
                //string url = "http://www.abovesydney.net:8080/VirtualRadar/aircraftlist.json?fIcoQ=" + RecordSelect;
                string url = "http://www.abovesydney.net/test.json";
                //string url = "http://www.abovesydney.net:8080/VirtualRadar/aircraftlist.json?fIcoQ=";
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)

                {
                    //Decode JSON to list here

                    var jsonString = await response.Content.ReadAsStringAsync();
                    Ro = JsonConvert.DeserializeObject<RootObject>(jsonString);

                    //RootObject rootObject = new RootObject();
                    AcList acList = new AcList();
                    //FlightList.ItemsSource = Ro.acList;
                    client.Dispose();
                    //SanityCheck();
                }
                else
                {
                    Debug.WriteLine("ERROR");
                    //OnHttpFail();
                }
                RootObject rootObject = new RootObject();
                AcList rootObject = new AcList();
                List<BasicGeoposition> dotList = new List<BasicGeoposition>();
                foreach (var item in rootObject)
                {
                    dotList.Add(new BasicGeoposition()
                    {
                        Latitude = Ro.acList[].
                        Longitude = item.Position.Longitude
                    });
                }

                //Example of one point
                //PosList.Add(new BasicGeoposition()
                //{
                //  Latitude = 52.46479093,
                //  Longitude = 16.91743341
                //});

                MapPolyline line = new MapPolyline();
                line.StrokeColor = Colors.Red;
                line.StrokeThickness = 5;
                line.Path = new Geopath(PosList);

                myMap.MapElements.Add(line);


                //Render Map
                BasicGeoposition planePosition = new BasicGeoposition() { Latitude = -33.9343, Longitude = 151.1658 };
                Geopoint planeCenter = new Geopoint(planePosition);
                MapIcon planeLOC = new MapIcon { Location = planeCenter, NormalizedAnchorPoint = new Point(0.5, 1.0), Title = "TEST" , ZIndex = 0 };
                _locationMap.MapElements.Add(planeLOC);
                _locationMap.Center = planeCenter;
                _locationMap.ZoomLevel = 10;
                //_loading.IsActive = false;

                MapPolyline mapLines = new MapPolyline();


                //async void OnHttpFail()
                //{
                //    var messageDialog = new MessageDialog("Error getting JSON file.") { Title = "Connection Failed!" };
                //    await messageDialog.ShowAsync();
                //    On_BackRequested();

            }
            }
    }
}
