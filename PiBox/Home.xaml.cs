﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PiBox
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Home : Page
    {

        DispatcherTimer Timer = new DispatcherTimer();

        public Home()
        {
            ApplicationDataContainer savedSettings = ApplicationData.Current.LocalSettings;
            var _SETvrsserver = savedSettings.Values["#VRSSERVER"];

            if (_SETvrsserver == null)
            {
                //DO A SETUP THING?
                savedSettings.Values["#VRSSERVER"] = "http://www.abovesydney.net:8080/VirtualRadar/aircraftlist.json";
            }



            InitializeComponent();
            DataContext = this;
            Timer.Tick += Timer_Tick;
            Timer.Interval = new TimeSpan(0, 0, 1);
            Timer.Start();
            _date.Text = DateTime.Now.ToShortDateString();
            GetWeather();
        }

        private void Timer_Tick(object sender, object e)
        {
            _clock.Text = DateTime.Now.ToString("h:mm:ss tt");
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

        private async void _navFlights(object sender, TappedRoutedEventArgs e)
        {
            ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values["IsFlightsSet"] != null)
            {
                Frame.Navigate(typeof(MainPage), new DrillInNavigationTransitionInfo()); ;
            }
            else
            {
                var messageDialog = new MessageDialog("Aircraftlist.json not set!");
                await messageDialog.ShowAsync();
            }
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

        private void _navSettings(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(Pages.Settings), new DrillInNavigationTransitionInfo());
        }

        private void _navOld(object sender, TappedRoutedEventArgs e)
        {
            Frame.Navigate(typeof(Pages.oldmain), new DrillInNavigationTransitionInfo());
        }
    }
}