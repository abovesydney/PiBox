﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.System.Threading;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PiBox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private DispatcherTimer timer;
        private int basetime;
        static Random rnd = new Random();

        public MainPage()
        {
            //GC.Collect(2);          
            this.InitializeComponent();
            timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += timer_Tick;
            basetime = 30;
            timer.Start();
            SetUpTails();
            DoTheMain();
        }

        void timer_Tick(object sender, object e)
        {
            basetime = basetime - 1;
            Debug.WriteLine (basetime.ToString());
            _progressBar.Value = basetime;
            if (basetime == 0)
            {
                timer.Stop();
                basetime = 30;
                DoTheMain();
                timer.Start();
                
            }
        }

        public async void SetUpTails()
        {
            StorageFolder appInstalledFolder = Package.Current.InstalledLocation;
            StorageFolder assets = await appInstalledFolder.GetFolderAsync(@"Assets\images\logos\tails");
            IReadOnlyList<StorageFile> files = await assets.GetFilesAsync();
            int tailFrnt = rnd.Next(files.Count);
            int tailMidl = rnd.Next(files.Count);
            int tailRear = rnd.Next(files.Count);
            string Frnturi = "ms-appx:///Assets/images/logos/tails/" + files[tailFrnt].Name.ToString();
            string Midlturi = "ms-appx:///Assets/images/logos/tails/" + files[tailMidl].Name.ToString();
            string Rearuri = "ms-appx:///Assets/images/logos/tails/" + files[tailRear].Name.ToString();
            _headerLogoFront.Source = new BitmapImage(new Uri(Frnturi, UriKind.Absolute));
            _headerLogoMid.Source = new BitmapImage(new Uri(Midlturi, UriKind.Absolute));
            _headerLogoBack.Source = new BitmapImage(new Uri(Rearuri, UriKind.Absolute));
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
                    var response = await client.GetAsync("http://www.abovesydney.net:8080/VirtualRadar/aircraftlist.json?fIcoSN=7CF");

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
            timer.Stop();
            var _SelectedLine = (AcList)FlightList.SelectedItem;
            var _SelectedFlight = _SelectedLine.Icao.ToString();
            //Debug.WriteLine(_SelectedFlight);
            Frame.Navigate(typeof(Pages.FlightRecord), _SelectedFlight, new DrillInNavigationTransitionInfo());
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
        private void _backClick(object sender, TappedRoutedEventArgs e)
        {
            On_BackRequested();
        }
    }
}
