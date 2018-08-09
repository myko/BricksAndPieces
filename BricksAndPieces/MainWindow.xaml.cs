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
using ViewModelKit;

namespace BricksAndPieces
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowViewModel vm;

        public MainWindow()
        {
            InitializeComponent();

            vm = new MainWindowViewModel();

            DataContext = vm;
        }
        
        protected override void OnClosing(CancelEventArgs e)
        {
            Properties.Settings.Default.Elements.Clear();
            foreach (var element in vm.Elements)
            {
                Properties.Settings.Default.Elements.Add(element.ElementId);
            }
            Properties.Settings.Default.Save();

            base.OnClosing(e);
        }
    }

    public class MainWindowViewModel: ViewModelKit.ViewModelBase
    {
        private HttpClientHandler handler;
        private HttpClient client;
        private WebClient webClient;

        public ObservableCollection<Element> Elements { get; set; }

        public DelegateCommand RefreshCommand { get; }
        public DelegateCommand AddElementCommand { get; }
        public DelegateCommand RemoveElementCommand { get; }
        public DelegateCommand RemoveSpecificElementCommand { get; }
        public DelegateCommand AddSpecificElementCommand { get; }

        public MainWindowViewModel()
        {
            Elements = new ObservableCollection<Element>();
            foreach (var elementId in Properties.Settings.Default.Elements)
            {
                Elements.Add(new Element { ElementId = elementId });
            }

            handler = new HttpClientHandler() { UseCookies = false };
            client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("cookie", "csAgeAndCountry={\"age\":\"22\",\"countrycode\":\"SE\"}");

            webClient = new WebClient();
        }

        public void OnRefresh()
        {
            Refresh();
        }

        public void OnAddElement()
        {
            Elements.Add(new Element());
        }

        public void OnRemoveElement()
        {
            var cvs = CollectionViewSource.GetDefaultView(Elements);
            Elements.RemoveAt(cvs.CurrentPosition);
        }

        public void OnRemoveSpecificElement(object element)
        {
            Elements.Remove(element as Element);
        }

        public void OnAddSpecificElement(object elementId)
        {
            Elements.Add(new Element() { ElementId = elementId as string });
        }

        private async Task Refresh()
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
                        element.Bricks.Add(new Brick { DesignId = brick.ItemNo.ToString(), Color = brick.ColourDescr, Quantity = brick.SQty, Asset = brick.Asset });
                    }

                    element.Image = new BitmapImage(new Uri(product.ImageBaseUrl + element.Bricks[0].Asset));
                }
                catch (Exception)
                {
                    element.Description = "Element not found";
                }
            }
        }
    }

    public class Element : ViewModelKit.ViewModelBase
    {
        public string ElementId { get; set; }
        public ObservableCollection<Brick> Bricks { get; set; } = new ObservableCollection<Brick>();

        public string Description { get; set; }
        public BitmapImage Image { get; set; }
    }

    public class Brick : ViewModelKit.ViewModelBase
    {
        public string DesignId { get; set; }
        public string Color { get; set; }
        public double Quantity { get; set; }
        public string Asset { get; set; }
    }

    public class ResultJson
    {
        public List<BrickJson> Bricks { get; set; } = new List<BrickJson>();
        public string ImageBaseUrl { get; set; }
    }

    public class BrickJson
    {
        public int ItemNo { get; set; }
        public string ItemDescr { get; set; }
        public double SQty { get; set; }
        public string ColourDescr { get; set; }
        public string Asset { get; set; }
    }
}
