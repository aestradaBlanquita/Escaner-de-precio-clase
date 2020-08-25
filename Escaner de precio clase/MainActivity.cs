using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System;
using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Symbol.XamarinEMDK;
using Symbol.XamarinEMDK.Barcode;
using System.Collections.Generic;
using Android.Content;
using System.Timers;
using System.Threading.Tasks;
using Android.Telephony;
using System.Net.Http;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net;

namespace Escaner_de_precio_clase
{
    [Activity(Label = "@string/app_name", ScreenOrientation = ScreenOrientation.Landscape, MainLauncher = true)]

    public class MainActivity : AppCompatActivity, EMDKManager.IEMDKListener
    {
        private EMDKManager emdkManager = null;
        private BarcodeManager barcodeManager = null;
        private Scanner scanner = null;
        Android.App.AlertDialog alertDialog;


        Button buscarLoyal;
        string loyalId;
        ImageView imageView;

        int index;
        int[] images;


        public string ScannerStatus { get; set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.activity_main);

            getImagesOferta();

            buscarLoyal = (Button)FindViewById(Resource.Id.recompensa_btn);

            buscarLoyal.Click += dialogLoyal;

            EMDKResults results = EMDKManager.GetEMDKManager(Android.App.Application.Context, this);

            if (results.StatusCode != EMDKResults.STATUS_CODE.Success)
            {
                ScannerStatus = "Status: EMDKManager object creation failed ...";
            }
            else
            {
                ScannerStatus = "Status: EMDKManager object creation succeeded ...";
            }

            StartTimer();
        }

        private void dialogLoyal(object sender, EventArgs e)
        {
            DisplayRequestLoyalty();
        }

         private void hacerConsulta(object sender, EventArgs e)
         {
             if (string.IsNullOrEmpty(loyalId))
             {
                Context context = Application.Context;
                string text = "Ingresa tu numero Recompensa";
                ToastLength duration = ToastLength.Short;

                var toast = Toast.MakeText(context, text, duration);
                toast.Show();
            }
            else
            {
                 var second = new Intent(this, typeof(Activity2));
                 second.PutExtra("loyalID", loyalId);
                 StartActivity(second);
            }      
         }

        void redirectPrice(string barcode)
        {
            var third = new Intent(this, typeof(Activity3));
            third.PutExtra("codigo", barcode);
            StartActivity(third);
        }

        public void OnClosed()
        {
            ScannerStatus = "Status: EMDK Open failed unexpectedly. ";

            if (emdkManager != null)
            {
                emdkManager.Release();
                emdkManager = null;
            }
        }

        public void OnOpened(EMDKManager emdkManager)
        {
            ScannerStatus = "Status: EMDK Opened successfully ...";
            this.emdkManager = emdkManager;

            InitScanner();
        }

        protected override void OnResume()
        {
            base.OnResume();
            InitScanner();

        }

        protected override void OnPause()
        {
            base.OnPause();
            DeinitScanner();

        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (emdkManager != null)
            {
                emdkManager.Release();
                emdkManager = null;
            }
        }

        public void OnScanEvent(object sender, Scanner.DataEventArgs e)
        {
            ScanDataCollection scanDataCollection = e.P0;

            if ((scanDataCollection != null) && (scanDataCollection.Result == ScannerResults.Success))
            {
                IList<ScanDataCollection.ScanData> scanData = scanDataCollection.GetScanData();

                foreach (ScanDataCollection.ScanData data in scanData)
                {
                    try
                    {
                        string dataString = data.Data;

                        Console.WriteLine("ESCANER DATA:  " + dataString);

                        redirectPrice(dataString);
                    }
                    catch (Exception ex)
                    {

                        throw ex;
                    }

                }
            }
        }

        public void OnStatusEvent(object sender, Scanner.StatusEventArgs e)
        {
            // EMDK: The status will be returned on multiple cases. Check the state and take the action.
            StatusData.ScannerStates state = e.P0.State;

            if (state == StatusData.ScannerStates.Idle)
            {
                ScannerStatus = "Scanner is idle and ready to submit read.";
                try
                {
                    if (scanner.IsEnabled && !scanner.IsReadPending)
                    {
                        scanner.Read();
                    }
                }
                catch (ScannerException e1)
                {
                    ScannerStatus = e1.Message;
                }
            }
            if (state == StatusData.ScannerStates.Waiting)
            {
                ScannerStatus = "Waiting for Trigger Press to scan";
            }
            if (state == StatusData.ScannerStates.Scanning)
            {
                ScannerStatus = "Scanning in progress...";
            }
            if (state == StatusData.ScannerStates.Disabled)
            {
                ScannerStatus = "Scanner disabled";
            }
            if (state == StatusData.ScannerStates.Error)
            {
                ScannerStatus = "Error occurred during scanning";
            }
        }

        private void InitScanner()
        {
            if (emdkManager != null)
            {
                if (barcodeManager == null)
                {
                    try
                    {
                        // Get the feature object such as BarcodeManager object for accessing the feature.
                        barcodeManager = (BarcodeManager)emdkManager.GetInstance(EMDKManager.FEATURE_TYPE.Barcode);

                        scanner = barcodeManager.GetDevice(BarcodeManager.DeviceIdentifier.Default);

                        if (scanner != null)
                        {
                            // Attahch the Data Event handler to get the data callbacks.
                            scanner.Data += OnScanEvent;

                            // Attach Scanner Status Event to get the status callbacks.
                            scanner.Status += OnStatusEvent;

                            scanner.Enable();

                            // EMDK: Configure the scanner settings
                            ScannerConfig config = scanner.GetConfig();
                            config.SkipOnUnsupported = ScannerConfig.SkipOnUnSupported.None;
                            config.ScanParams.DecodeLEDFeedback = false;
                            config.ReaderParams.ReaderSpecific.ImagerSpecific.PickList = ScannerConfig.PickList.Enabled;
                            config.DecoderParams.Code39.Enabled = false;
                            config.DecoderParams.Code128.Enabled = true;
                            scanner.SetConfig(config);
                        }
                        else
                        {
                            // displayStatus("Failed to enable scanner.\n");
                        }
                    }
                    catch (ScannerException e)
                    {
                        // displayStatus("Error: " + e.Message);
                    }
                    catch (Exception ex)
                    {
                        // displayStatus("Error: " + ex.Message);
                    }
                }
            }
        }

        private void DeinitScanner()
        {
            if (emdkManager != null)
            {
                if (scanner != null)
                {
                    try
                    {
                        scanner.Data -= OnScanEvent;
                        scanner.Disable();
                    }
                    catch (ScannerException e)
                    {
                        // Log.Debug(this.Class.SimpleName, "Exception:" + e.Result.Description);
                    }
                }

                if (barcodeManager != null)
                {
                    emdkManager.Release(EMDKManager.FEATURE_TYPE.Barcode);
                }
                barcodeManager = null;
                scanner = null;
            }
        }

        public override void OnBackPressed()
        {
            Context context = Application.Context;
            string text = "NO PRESIONE OTRO BOTON";
            ToastLength duration = ToastLength.Short;

            var toast = Toast.MakeText(context, text, duration);
            toast.Show();
        }

        public async void StartTimer()
        {
           // await Task.Delay(120000); //2 minutes
                                      await Task.Delay(5000); //5 seconds
                                      //StartActivity(typeof(Activity4));

            //1.inflate the Customlayout
            View content = LayoutInflater.Inflate(Resource.Layout.slider_layout, null);

            loadImages();
            imageView = (ImageView)content.FindViewById(Resource.Id.imageView);
            index = 0;
            // Set the first image to imageview
            imageView.SetBackgroundResource(images[0]);
            setSlideShow(); // Make the timer to change image
            hideNavigationBar();// To hide navigation bar

            alertDialog = new Android.App.AlertDialog.Builder(this).Create();

            //4. set the view
            alertDialog.SetView(content);

            //5. show the dialog
            alertDialog.Show(); // This should be called before looking up for elements
        }

        void loadImages()
        {
            images = new int[]{
            Resource.Drawable.bancopel,//index= 0
            Resource.Drawable.banner_covid19_1,//index= 0
            };
        }

        void setSlideShow()
        {
            Timer timer;
            timer = new Timer();
            timer.Interval = 4000; // Interval to change image (4000 = 4 second per image)
            timer.Elapsed += (sender, e) =>
            {
                showNextImage();
            };
            timer.Start();
            timer.AutoReset = true;
        }

        void showNextImage()
        {
            index++;
            if (index == images.Length) { index = 0; }
            RunOnUiThread(() => imageView.SetBackgroundResource(images[index]));
        }

        void hideNavigationBar()
        {
            View decorView = Window.DecorView;
            var uiOptions = (int)decorView.SystemUiVisibility;
            var newUiOptions = uiOptions;
            newUiOptions |= (int)SystemUiFlags.HideNavigation;
            decorView.SystemUiVisibility = (StatusBarVisibility)newUiOptions;
        }

        void DisplayRequestLoyalty()
        {
            View content_layout = LayoutInflater.Inflate(Resource.Layout.loyalty_layout, null);

            var consultar = (Button)content_layout.FindViewById(Resource.Id.consultar);

            var loyaltyId = (EditText)content_layout.FindViewById(Resource.Id.loyalInput);

            alertDialog = new Android.App.AlertDialog.Builder(this).Create();

            //4. set the view
            alertDialog.SetView(content_layout);

            //5. show the dialog
            alertDialog.Show(); // This should be called before looking up for elements

            loyaltyId.KeyPress += (object sender, View.KeyEventArgs e) => {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    //add your logic here
                    loyalId = loyaltyId.Text;
                    var second = new Intent(this, typeof(Activity2));
                    second.PutExtra("loyalID", loyalId);
                    StartActivity(second);

                    e.Handled = true;
                }
            };

            consultar.Click += delegate
            {
                
                loyalId = loyaltyId.Text;
                var second = new Intent(this, typeof(Activity2));
                second.PutExtra("loyalID", loyalId);
                StartActivity(second);
            };
        }

        void getImagesOferta()
        {
            string url = "http://192.168.102.79/StoreImages/sendimages.php";
            WebClient client = new WebClient();
            string downloadString = client.DownloadString(url);
            //var item = JsonConvert.DeserializeObject<List<RetrieveMultipleResponse>(downloadString);
            //string codigoImagen = item.images;
            var des = (Exampless)Newtonsoft.Json.JsonConvert.DeserializeObject(response, typeof(Exampless));
            return des.data.Count.ToString();
        }
    }
    
    [System.Serializable]
    public class Datumss
    {
        public string images;
    }

    [System.Serializable]
    public class ConversionTypess
    {
        public string type;
        public string conversionSymbol;
    }

    [System.Serializable]
    public class Exampless
    {
        public string images;
    }
}

