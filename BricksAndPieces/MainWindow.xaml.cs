using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Web;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json;

namespace BricksAndPieces
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClientHandler handler;
        private HttpClient client;

        public ObservableCollection<Element> Elements { get; set; }

        public MainWindow()
        {
            InitializeComponent();

            Elements = new ObservableCollection<Element>();
            foreach (var elementId in Properties.Settings.Default.Elements)
            {
                Elements.Add(new Element { ElementId = elementId });
            }

            handler = new HttpClientHandler() { UseCookies = false };
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("cookie", "csAgeAndCountry={\"age\":\"22\",\"countrycode\":\"SE\"}");

            DataContext = this;
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var element in Elements)
            {
                try
                {
                    var result = await client.GetStringAsync($"https://www.lego.com/sv-SE/service/rpservice/getitemordesign?itemordesignnumber={element.ElementId}&isSalesFlow=true");
                    var product = JsonConvert.DeserializeObject<ResultJson>(result);

                    if (product.Bricks.Count > 0)
                        element.Description = product.Bricks[0].ItemDescr;
                    element.Bricks.Clear();
                    foreach (var brick in product.Bricks)
                    {
                        element.Bricks.Add(new Brick { DesignId = brick.ItemNo.ToString(), Color = brick.ColourDescr, Quantity = brick.SQty });
                    }
                }
                catch (Exception)
                {
                    element.Description = "Element not found";
                }
            }
        }

        private void AddElementButton_Click(object sender, RoutedEventArgs e)
        {
            Elements.Add(new Element());
        }

        private void RemoveElementButton_Click(object sender, RoutedEventArgs e)
        {
            var cvs = CollectionViewSource.GetDefaultView(Elements);
            Elements.RemoveAt(cvs.CurrentPosition);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            Properties.Settings.Default.Elements.Clear();
            foreach (var element in Elements)
            {
                Properties.Settings.Default.Elements.Add(element.ElementId);
            }
            Properties.Settings.Default.Save();

            base.OnClosing(e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Elements.Add(new Element() { ElementId = (sender as Button).Content as string });
        }
    }

    public class Element : INotifyPropertyChanged
    {
        private string description;

        public string ElementId { get; set; }
        public ObservableCollection<Brick> Bricks { get; set; } = new ObservableCollection<Brick>();

        public string Description
        {
            get { return description; }
            set
            {
                description = value;
                OnPropertyChanged("Description");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class Brick : INotifyPropertyChanged
    {
        private string designId;
        private string color;
        private double quantity;
        
        public string DesignId
        {
            get { return designId; }
            set
            {
                designId = value;
                OnPropertyChanged("DesignId");
            }
        }

        public string Color
        {
            get { return color; }
            set
            {
                color = value;
                OnPropertyChanged("Color");
            }
        }

        public double Quantity
        {
            get { return quantity; }
            set
            {
                quantity = value;
                OnPropertyChanged("Quantity");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ResultJson
    {
        public List<BrickJson> Bricks { get; set; } = new List<BrickJson>();
    }

    public class BrickJson
    {
        public int ItemNo { get; set; }
        public string ItemDescr { get; set; }
        public double SQty { get; set; }
        public string ColourDescr { get; set; }
    }
}
