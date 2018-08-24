using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PiBox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            GC.Collect(2);
            DoTheMain();
            this.InitializeComponent();
        }

        public async void DoTheMain()
        {

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
                    var response = await client.GetAsync("http://www.abovesydney.net:8080/VirtualRadar/aircraftlist.json");

                    if (response.IsSuccessStatusCode)
                    {
                        //Decode JSON to list here
                        var jsonString = await response.Content.ReadAsStringAsync();
                        RootObject ro = JsonConvert.DeserializeObject<RootObject>(jsonString);

                        RootObject rootObject = new RootObject();
                        AcList acList = new AcList();
                        FlightList.ItemsSource = ro.acList;
                        
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

        private void _DoRetry(object sender, TappedRoutedEventArgs e)
        {
            DoTheMain();
        }

        private void _pickFlight(object sender, SelectionChangedEventArgs e)
        {
            var _SelectedLine = (AcList)FlightList.SelectedItem;
            var _SelectedFlight = _SelectedLine.Icao.ToString();
            Debug.WriteLine(_SelectedFlight);
            Frame.Navigate(typeof(Pages.FlightRecord), _SelectedFlight, new DrillInNavigationTransitionInfo());
        }
    }
}
