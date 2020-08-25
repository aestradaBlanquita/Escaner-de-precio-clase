using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace Escaner_de_precio_clase
{
    [Activity(Label = "Activity2", ScreenOrientation = ScreenOrientation.Landscape)]
    public class Activity2 : Activity
    {
        TextView loyaltyID;
        TextView puntosDisponible;
        TextView arribaDePuntos;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            string loyalInfo = Intent.GetStringExtra("loyalID");

            Console.WriteLine("LOYALINFO: " + loyalInfo);

            SetContentView(Resource.Layout.layout_displayLoyal);

            try
            {
                string url = "http://192.168.102.79/StoreImages/loyaltypoints.php?loyalid=" + loyalInfo;

                WebClient client = new WebClient();
                string downloadString = client.DownloadString(url);
                var item = JsonConvert.DeserializeObject<Example>(downloadString);
                string nombreDelCliente = item.firstname;
                string loyaltyId = item.loyal;
                int puntosDisponibles = item.availablePoints;

                string puntosDisponiblesString = puntosDisponibles.ToString();

                displayLoyalty(nombreDelCliente, loyaltyId, puntosDisponiblesString);

                await Task.Delay(5000);
                sendToMain();
            }
            catch (Exception)
            {

                Context context = Application.Context;
                string text = "No se encontro informacion, verifica tu NUMERO RECOMPENSA BLANQUITA";
                ToastLength duration = ToastLength.Long;

                var toast = Toast.MakeText(context, text, duration);
                toast.Show();

                sendToMain();
            }

        }


        void displayLoyalty(string nombreCliente, string loyalty, string puntos)
        {
            loyaltyID = FindViewById<TextView>(Resource.Id.texviewID);
            loyaltyID.Text = "LOYALTY ID: " + loyalty;
            puntosDisponible = FindViewById<TextView>(Resource.Id.textViewPuntos);
            arribaDePuntos = FindViewById<TextView>(Resource.Id.textViewPuntosLabel);
            arribaDePuntos.Text = nombreCliente;
            puntosDisponible.Text = "PUNTOS: " + puntos;

        }

        void sendToMain()
        {
            var first = new Intent(this, typeof(MainActivity));
            StartActivity(first);
        }

        public override void OnBackPressed()
        {
            sendToMain();
        }
    }

    [System.Serializable]
    public class Datum
    {
        public string id;
        public string firstname;
        public string loyal;
        public int availablePoints;
    }

    [System.Serializable]
    public class ConversionType
    {
        public string type;
        public string conversionSymbol;
    }

    [System.Serializable]
    public class Example
    {
        public string id;
        public string firstname;
        public string loyal;
        public int availablePoints;
    }
}