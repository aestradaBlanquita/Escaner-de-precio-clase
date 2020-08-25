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
    [Activity(Label = "Activity3", ScreenOrientation = ScreenOrientation.Landscape)]
    public class Activity3 : Activity
    {
        TextView productoView;
        TextView precioView;
        TextView medida;
        ImageView imagenProducto;
        string categoria;
        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.layout_precio);

            //ActionBar.Hide();
             
            string barcode = Intent.GetStringExtra("codigo");

            getItemData(barcode);

            await Task.Delay(2500);
            sendToMain();
        }

        public void getItemData(string barcode)
        {
            string url = "http://192.168.102.79/StoreImages/itemCode.php?code=" + barcode;

            WebClient client = new WebClient();
            string downloadString = client.DownloadString(url);
            var item = JsonConvert.DeserializeObject<Examples>(downloadString);
            string precio = item.precio;
            string producto = item.name;
            string codigo = item.code;
            string categoriaData = item.categoria;
            string uomCode = item.uom;

            if (producto == "")
            {
                Context context = Application.Context;
                string text = "No se encontro producto";
                ToastLength duration = ToastLength.Long;

                var toast = Toast.MakeText(context, text, duration);
                toast.Show();

                sendToMain();
            }

            productoView = FindViewById<TextView>(Resource.Id.textViewNombreProductos);
            precioView = FindViewById<TextView>(Resource.Id.textViewPrecio);
            medida = FindViewById<TextView>(Resource.Id.textViewMedida);

            productoView.Text = producto;
            precioView.Text = "$" + precio;
            medida.Text = "MEDIDA: " + uomCode;
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
    public class Datums
    {
        public string name;
        public string precio;
        public string code;
        public string image;
        public string categoria;
        public string uom;
    }

    [System.Serializable]
    public class ConversionTypes
    {
        public string type;
        public string conversionSymbol;
    }

    [System.Serializable]
    public class Examples
    {
        public string name;
        public string precio;
        public string code;
        public string image;
        public string uom;
        public string categoria;
    }
}
