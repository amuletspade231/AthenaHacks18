using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Media.Capture;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using Windows.System.Display;
using Windows.Storage.Streams;
using Windows.Graphics.Display;
using Windows.Media.MediaProperties;
using Windows.Graphics.Imaging;
using Windows.Media;
using Microsoft.ProjectOxford.Face;
using Microsoft.ProjectOxford.Face.Contract;
using Microsoft.ProjectOxford.Common.Contract;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace CameraCapture
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaCapture mediaCapture;
        bool isPreviewing;
        DisplayRequest displayRequest = new DisplayRequest();

        public MainPage()
        {
            this.InitializeComponent();

            Application.Current.Suspending += Application_Suspending;
        }

        //-----------Helper Functions------------

        //Depending on your app's scenario, you may want to call this from the OnNavigatedTo event handler that is called when the page is loaded
        //or wait and launch the preview in response to UI events
        private async Task StartPreviewAsync()
        {
            try
            {

                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();

                displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
            catch (UnauthorizedAccessException)
            {
                // This will be thrown if the user denied access to the camera in privacy settings
                // ShowMessageToUser("The app was denied access to the camera");
                var messageDialog = new MessageDialog("The app was denied access to the camera.");
                return;
            }

            try
            {
                PreviewControl.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
                isPreviewing = true;
            }
            catch (System.IO.FileLoadException)
            {
                mediaCapture.CaptureDeviceExclusiveControlStatusChanged += _mediaCapture_CaptureDeviceExclusiveControlStatusChanged;
            }

        }

        private async void _mediaCapture_CaptureDeviceExclusiveControlStatusChanged(MediaCapture sender, MediaCaptureDeviceExclusiveControlStatusChangedEventArgs args)
        {
            if (args.Status == MediaCaptureDeviceExclusiveControlStatus.SharedReadOnlyAvailable)
            {
                var messageDialog = new MessageDialog("The camera preview can't be displayed because another app has exclusive access");
            }
            else if (args.Status == MediaCaptureDeviceExclusiveControlStatus.ExclusiveControlAvailable && !isPreviewing)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    await StartPreviewAsync();
                });
            }
        }

        private async Task CleanupCameraAsync()
        {
            if (mediaCapture != null)
            {
                if (isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PreviewControl.Source = null;
                    if (displayRequest != null)
                    {
                        displayRequest.RequestRelease();
                    }

                    mediaCapture.Dispose();
                    mediaCapture = null;
                });
            }

        }

        protected async override void OnNavigatedFrom(NavigationEventArgs e)
        {
            await CleanupCameraAsync();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            await StartPreviewAsync();
        }

        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            // Handle global application events only if this page is active
            if (Frame.CurrentSourcePageType == typeof(MainPage))
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                await CleanupCameraAsync();
                deferral.Complete();
            }
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            FaceServiceClient fClient = new FaceServiceClient("7fd36de2f576487aaad929010b131480", "https://westcentralus.api.cognitive.microsoft.com/face/v1.0");
            
            using (var captureStream = new InMemoryRandomAccessStream())
            {
                await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), captureStream);
                captureStream.Seek(0);

                var faces = await fClient.DetectAsync(captureStream.AsStream(), returnFaceLandmarks: true);

                if (faces.Length > 0)
                {
                    // Prepare to draw rectangles around the faces.

                    Face face = faces[0];
                    var polygon1 = new Polygon();
                    polygon1.Stroke = new SolidColorBrush(Windows.UI.Colors.Red);
                    polygon1.StrokeThickness = 4;

                    var points = new PointCollection();
                    points.Add(new Windows.Foundation.Point(face.FaceRectangle.Left, face.FaceRectangle.Top));
                    points.Add(new Windows.Foundation.Point(face.FaceRectangle.Left, face.FaceRectangle.Top + face.FaceRectangle.Height));
                    points.Add(new Windows.Foundation.Point(face.FaceRectangle.Left + face.FaceRectangle.Width, face.FaceRectangle.Top + face.FaceRectangle.Height));
                    points.Add(new Windows.Foundation.Point(face.FaceRectangle.Left + face.FaceRectangle.Width, face.FaceRectangle.Top));
                    polygon1.Points = points;

                    // When you create a XAML element in code, you have to add
                    // it to the XAML visual tree. This example assumes you have
                    // a panel named 'layoutRoot' in your XAML file, like this:
                    // <Grid x:Name="layoutRoot>
                    layoutRoot.Children.Add(polygon1);
                }

                };
        }
    }
}
