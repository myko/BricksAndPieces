﻿using System;
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

            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12;

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
            Properties.Settings.Default.CountryCode = vm.CountryCode;
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

        public string CountryCode { get; set; } = "SE";

        public DelegateCommand RefreshCommand { get; }
        public DelegateCommand AddElementCommand { get; }
        public DelegateCommand RemoveElementCommand { get; }
        public DelegateCommand RemoveSpecificElementCommand { get; }
        public DelegateCommand AddSpecificElementCommand { get; }

        public DelegateCommand FindProductCommand { get; }

        public Rebrickable Rebrickable { get; }

        public MainWindowViewModel()
        {
            if (Properties.Settings.Default.NeedSettingUpgrade)
            {
                Properties.Settings.Default.Upgrade();
                Properties.Settings.Default.NeedSettingUpgrade = false;
            }

            Elements = new ObservableCollection<Element>();
            foreach (var elementId in Properties.Settings.Default.Elements)
            {
                Elements.Add(new Element { ElementId = elementId });
            }

            CountryCode = Properties.Settings.Default.CountryCode;

            Rebrickable = new Rebrickable();

            handler = new HttpClientHandler() { UseCookies = false };
            client = new HttpClient(handler);
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
            SetClientHeaders();

            foreach (var element in Elements)
            {
                if (string.IsNullOrWhiteSpace(element.ElementId))
                    continue;

                try
                {
                    var result = await client.GetStringAsync($"https://bricksandpieces.services.lego.com/api/v1/bricks/items/{element.ElementId}?country=SE&orderType=buy");
                    var product = JsonConvert.DeserializeObject<BapResultJson>(result);

                    if (product.bricks.Count > 0)
                        element.Description = product.bricks[0].description;

                    var removedBricks = element.Bricks.Where(b => !product.bricks.Any(pb => pb.itemNumber == b.DesignId)).ToList();
                    foreach (var brick in removedBricks)
                    {
                        element.Bricks.Remove(brick);
                    }

                    element.Image = null;

                    if (product.bricks.Count == 0)
                        element.Description = "Element not found";

                    foreach (var brickJson in product.bricks)
                    {
                        if (element.Image == null)
                        {
                            try
                            {
                                element.Image = new BitmapImage(new Uri(brickJson.imageUrl));
                            }
                            catch { }
                        }                            

                        var brick = element.Bricks.SingleOrDefault(b => b.DesignId == brickJson.itemNumber);
                        if (brick == null)
                        {
                            element.Bricks.Add(new Brick
                            {
                                DesignId = brickJson.itemNumber,
                                Color = brickJson.colorFamily,
                                Quantity = new ChangingValue(brickJson.itemQuantity) { IsEnabled = !brickJson.isSoldOut },
                                Price = new ChangingValue(brickJson.price.amount),
                            });
                        }
                        else
                        {
                            brick.Quantity.Change(brickJson.itemQuantity);
                            brick.Price.Change(brickJson.price.amount);
                        }
                    }
                }
                catch (Exception ex)
                {
                    element.Description = "Error: " + ex.Message;
                }
            }
        }

        private async Task FindProduct(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId))
                return;

            SetClientHeaders();

            try
            {
                Product = null;

                var result = await client.GetStringAsync($"https://bricksandpieces.services.lego.com/api/v1/bricks/product/{productId}?country=SE&orderType=buy");
                var resultJson = JsonConvert.DeserializeObject<BapResultJson>(result);

                Product = new Element();

                Product.Description = productId; // resultJson.Product.ProductName;
                //Product.Image = new BitmapImage(new Uri(resultJson.Product.Asset));

                foreach (var brickJson in resultJson.bricks)
                {
                    Product.Bricks.Add(new Brick
                    {
                        DesignId = brickJson.itemNumber,
                        Description = brickJson.description,
                        Color = brickJson.colorFamily,
                        Quantity = new ChangingValue(brickJson.itemQuantity) { IsEnabled = !brickJson.isSoldOut },
                        Price = new ChangingValue(brickJson.price.amount),
                        Image = new BitmapImage(new Uri(brickJson.imageUrl)),
                    });
                }
            }
            catch (Exception)
            {
                Product = new Element();
                Product.Description = "Set not found.";
            }
        }

        private void SetClientHeaders()
        {
            client.DefaultRequestHeaders.Remove("cookie");
            client.DefaultRequestHeaders.Add("cookie", "csAgeAndCountry={\"age\":\"22\",\"countrycode\":\"" + CountryCode + "\"}");

            client.DefaultRequestHeaders.Remove("x-api-key");
            client.DefaultRequestHeaders.Add("x-api-key", "saVSCq0hpuxYV48mrXMGfdKnMY1oUs3s");

            client.DefaultRequestHeaders.Remove("upgrade-insecure-requests");
            client.DefaultRequestHeaders.Add("upgrade-insecure-requests", "1");
        }
    }
}
