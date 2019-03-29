using Microsoft.Azure.SpatialAnchors;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace ARFeebacksViewer
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CloudSpatialAnchorSession cloudAnchorSession = null;

        public MainPage()
        {
            this.InitializeComponent();
            InitSession();
        }

        void InitSession()
        {
            this.cloudAnchorSession = new CloudSpatialAnchorSession();
            this.cloudAnchorSession.Configuration.AccountId = "d86417ec-68a3-4f84-90b3-28721b923985";
            this.cloudAnchorSession.Configuration.AccountKey = "J4ByBZ81Kw5hpXIKbMrroK/QxvtkVDNaNfghADXU1rs=";
            this.cloudAnchorSession.Error += (s, e) => Debug.WriteLine(e.ErrorMessage);

            this.cloudAnchorSession.AnchorLocated += OnAnchorLocated;
            this.cloudAnchorSession.SessionUpdated += CloudAnchorSession_SessionUpdatedAsync;
            this.cloudAnchorSession.LocateAnchorsCompleted += OnLocateAnchorsCompleted;
        }

        private void CloudAnchorSession_SessionUpdatedAsync(object sender, SessionUpdatedEventArgs args)
        {
            Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal,
                () =>
                {
                    txtStatus.Text = args.Status.UserFeedback.ToString() + " " + args.Status.RecommendedForCreateProgress;
                });
            var watcher = this.cloudAnchorSession.CreateWatcher(
                new AnchorLocateCriteria()
                {
//                    Identifiers = new string[] { "1" },
                    BypassCache = true,
                    RequestedCategories = AnchorDataCategory.Spatial,
                    Strategy = LocateStrategy.AnyStrategy
                }
            );
        }

        void OnAnchorLocated(object sender, AnchorLocatedEventArgs args)
        {
            lstAnchors.Items.Add(args.Anchor.Identifier);
        }

        void OnLocateAnchorsCompleted(object sender, LocateAnchorsCompletedEventArgs args)
        {
            args.Watcher.Stop();
            this.cloudAnchorSession.Stop();
        }

        private void BtnLoad_Click(object sender, RoutedEventArgs e)
        {
            this.cloudAnchorSession.Start();

        }
    }
}
