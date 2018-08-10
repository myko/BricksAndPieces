using System;
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

        private void TextBox_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                (sender as TextBox).GetBindingExpression(TextBox.TextProperty).UpdateSource();

                vm.OnRefresh();
            }
        }
    }

    public class MainWindowViewModel: ViewModelKit.ViewModelBase
    {
        private HttpClientHandler handler;
        private HttpClient client;

        public ObservableCollection<Element> Elements { get; set; }

        public string ProductId { get; set; }
        public Element Product { get; set; }

        public DelegateCommand RefreshCommand { get; }
        public DelegateCommand AddElementCommand { get; }
        public DelegateCommand RemoveElementCommand { get; }
        public DelegateCommand RemoveSpecificElementCommand { get; }
        public DelegateCommand AddSpecificElementCommand { get; }

        public DelegateCommand FindProductCommand { get; }

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
            Elements.Add(new Element() { ElementId = elementId.ToString() });
        }

        public void OnFindProduct()
        {
            FindProduct(ProductId);
        }

        private async Task Refresh()
        {
            foreach (var element in Elements)
            {
                if (string.IsNullOrWhiteSpace(element.ElementId))
                    continue;

                try
                {
                    var result = await client.GetStringAsync($"https://www.lego.com/sv-SE/service/rpservice/getitemordesign?itemordesignnumber={element.ElementId}&isSalesFlow=true");
                    var product = JsonConvert.DeserializeObject<ResultJson>(result);

                    element.Image = null;

                    if (product.Bricks.Count > 0)
                        element.Description = product.Bricks[0].ItemDescr;

                    var removedBricks = element.Bricks.Where(b => !product.Bricks.Any(pb => pb.ItemNo == b.DesignId)).ToList();
                    foreach (var brick in removedBricks)
                    {
                        element.Bricks.Remove(brick);
                    }

                    foreach (var brickJson in product.Bricks)
                    {
                        var brick = element.Bricks.SingleOrDefault(b => b.DesignId == brickJson.ItemNo);
                        if (brick == null)
                        {
                            element.Bricks.Add(new Brick
                            {
                                DesignId = brickJson.ItemNo,
                                Color = brickJson.ColourDescr,
                                Quantity = new ChangingValue(brickJson.SQty),
                                Price = new ChangingValue(brickJson.Price)
                            });

                            if (element.Image == null)
                                element.Image = new BitmapImage(new Uri(product.ImageBaseUrl + brickJson.Asset));
                        }
                        else
                        {
                            brick.Quantity.Change(brickJson.SQty);
                            brick.Price.Change(brickJson.Price);
                        }
                    }

                    
                }
                catch (Exception)
                {
                    element.Description = "Element not found";
                }
            }
        }

        private async Task FindProduct(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId))
                return;

            try
            {
                Product = null;

                var result = await client.GetStringAsync($"https://www.lego.com/sv-SE/service/rpservice/getproduct?productnumber={productId}&isSalesFlow=true");
                var resultJson = JsonConvert.DeserializeObject<ResultJson>(result);

                Product = new Element();

                Product.Description = resultJson.Product.ProductName;
                Product.Image = new BitmapImage(new Uri(resultJson.Product.Asset));

                foreach (var brickJson in resultJson.Bricks)
                {
                    Product.Bricks.Add(new Brick
                    {
                        DesignId = brickJson.ItemNo,
                        Description = brickJson.ItemDescr,
                        Color = brickJson.ColourDescr,
                        Quantity = new ChangingValue(brickJson.SQty),
                        Price = new ChangingValue(brickJson.Price),
                        Image = new BitmapImage(new Uri(resultJson.ImageBaseUrl + brickJson.Asset)),
                    });
                }
            }
            catch (Exception)
            {
                Product = new Element();
                Product.Description = "Set not found.";
            }
        }
    }
}
