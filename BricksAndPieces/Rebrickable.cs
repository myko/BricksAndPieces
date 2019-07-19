using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using ViewModelKit;

namespace BricksAndPieces
{
    public class Rebrickable : ViewModelBase
    {
        private HttpClientHandler handler;
        private HttpClient client;
        private bool cancelRequested;

        private List<KeyValuePair<string, string>> categories;
        private List<KeyValuePair<string, string>> colors;

        public string ColorId { get; set; }
        public string CategoryId { get; set; }
        public string SearchText { get; set; }

        public DelegateCommand SearchCommand { get; set; }
        public DelegateCommand CancelSearchCommand { get; set; }
        public DelegateCommand FindElementColorsCommand { get; set; }

        public ObservableCollection<Element> Elements { get; set; } = new ObservableCollection<Element>();

        public List<KeyValuePair<string, string>> Categories
        {
            get
            {
                if (categories == null)
                    LoadCategories();

                return categories;
            }
        }

        public List<KeyValuePair<string, string>> Colors
        {
            get
            {
                if (colors == null)
                    LoadColors();

                return colors;
            }
        }

        public Rebrickable()
        {
            handler = new HttpClientHandler() { UseCookies = false };
            client = new HttpClient(handler);
        }

        public void OnSearch()
        {
            Search(SearchText);
        }

        public void OnCancelSearch()
        {
            if (SearchCommand.IsEnabled == false)
            {
                CancelSearchCommand.IsEnabled = false;
                cancelRequested = true;
            }
        }

        public void OnFindElementColors(object element)
        {
            FindElementColors(element as Element);
        }

        private async Task LoadCategories()
        {
            categories = new List<KeyValuePair<string, string>>();
            categories.Add(new KeyValuePair<string, string>("", "Any"));

            try
            {
                var next = RebrickableUrl("part_categories", $"");
                while (next != null)
                {
                    var result = await client.GetStringAsync(next);
                    var resultJson = JsonConvert.DeserializeObject<RebrickableResultJson<RebrickableCategoryJson>>(result);

                    next = resultJson.next;

                    foreach (var categoryJson in resultJson.results)
                    {
                        categories.Add(new KeyValuePair<string, string>(categoryJson.id, $"{categoryJson.id} {categoryJson.name} ({categoryJson.part_count} parts)"));
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        private async Task LoadColors()
        {
            colors = new List<KeyValuePair<string, string>>();
            colors.Add(new KeyValuePair<string, string>("", "Any"));
            colors.Add(new KeyValuePair<string, string>("1", "Blue"));

            try
            {
                var next = RebrickableUrl("colors", $"");
                while (next != null)
                {
                    var result = await client.GetStringAsync(next);
                    var resultJson = JsonConvert.DeserializeObject<RebrickableResultJson<RebrickableColorJson>>(result);

                    next = resultJson.next;

                    foreach (var colorJson in resultJson.results)
                    {
                        colors.Add(new KeyValuePair<string, string>(colorJson.id, $"{colorJson.id} {colorJson.name}"));
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        private async Task Search(string searchText)
        {
            try
            {
                SearchCommand.IsEnabled = false;

                Elements.Clear();

                var searchUrl = System.Web.HttpUtility.UrlEncode(searchText);

                var next = RebrickableUrl("parts", $"&inc_part_details=1&part_cat_id={CategoryId}&color_id={ColorId}&search={searchUrl}");
                while (next != null)
                {
                    if (cancelRequested)
                        break;

                    var result = await client.GetStringAsync(next);
                    var resultJson = JsonConvert.DeserializeObject<RebrickableResultJson<RebrickablePartJson>>(result);

                    next = resultJson.next;

                    foreach (var brickJson in resultJson.results)
                    {
                        var element = new Element
                        {
                            ElementId = brickJson.part_num,
                            Description = brickJson.name,
                            FromYear = brickJson.year_from,
                            ToYear = brickJson.year_to,
                        };

                        if (element.FromYear == "0")
                            element.FromYear = "";
                        if (element.ToYear == "0")
                            element.ToYear = "";

                        if (!string.IsNullOrWhiteSpace(brickJson.part_img_url))
                            element.Image = new BitmapImage(new Uri(brickJson.part_img_url));

                        element.HasNoColors = true;

                        Elements.Add(element);
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                cancelRequested = false;
                CancelSearchCommand.IsEnabled = true;
                SearchCommand.IsEnabled = true;
            }
        }

        private async Task FindElementColors(Element element)
        {
            try
            {
                FindElementColorsCommand.IsEnabled = false;

                element.Bricks.Clear();

                var next = RebrickableUrl($"parts/{element.ElementId}/colors", $"");
                while (next != null)
                {
                    var result = await client.GetStringAsync(next);
                    var resultJson = JsonConvert.DeserializeObject<RebrickableResultJson<RebrickablePartColorJson>>(result);

                    next = resultJson.next;

                    foreach (var brickJson in resultJson.results)
                    {
                        if (brickJson.elements.Count > 0)
                        {
                            foreach (var elementId in brickJson.elements)
                            {
                                if (int.TryParse(elementId, out int designId))
                                {
                                    var brick = new Brick
                                    {
                                        DesignId = designId,
                                        Description = element.Description,
                                        Color = brickJson.color_name,
                                        AppearsIn = $"{brickJson.num_set_parts} parts in {brickJson.num_sets} sets",
                                    };

                                    element.Bricks.Add(brick);
                                    element.HasNoColors = false;
                                }
                            }
                        }
                        else
                        {
                            var brick = new Brick
                            {
                                DesignId = -1,
                                Description = element.Description,
                                Color = brickJson.color_name,
                                AppearsIn = $"{brickJson.num_set_parts} parts in {brickJson.num_sets} sets",
                            };

                            element.Bricks.Add(brick);
                            element.HasNoColors = false;
                        }
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                FindElementColorsCommand.IsEnabled = true;
            }
        }

        private string RebrickableUrl(string url, string parameters)
        {
            return $"https://rebrickable.com/api/v3/lego/{url}?key=29d6f8667f6a4349f9c7ac8f4cd864b2&page_size=100{parameters}";
        }
    }
}
