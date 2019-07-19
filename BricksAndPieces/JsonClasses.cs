using System.Collections.Generic;

namespace BricksAndPieces
{
    public class BapResultJson
    {
        public BapProductJson Product { get; set; }
        public List<BapBrickJson> Bricks { get; set; } = new List<BapBrickJson>();
        public string ImageBaseUrl { get; set; }
    }
    
    public class BapProductJson
    {
        public string ProductName { get; set; }
        public string Asset { get; set; }
    }

    public class BapBrickJson
    {
        public int ItemNo { get; set; }
        public string ItemDescr { get; set; }
        public double SQty { get; set; }
        public string ColourDescr { get; set; }
        public string Asset { get; set; }
        public double Price { get; set; }
        public string PriceWithTaxStr { get; set; }
    }

    public class RebrickableResultJson<T>
    {
        public int count { get; set; }
        public string next { get; set; }
        public string previous { get; set; }
        public List<T> results { get; set; }
    }

    public class RebrickablePartJson
    {
        public string part_num { get; set; }
        public string name { get; set; }
        public string part_cat_id { get; set; }
        public string part_img_url { get; set; }
        public string year_from { get; set; }
        public string year_to { get; set; }
    }

    public class RebrickablePartColorJson
    {
        public string color_id { get; set; }
        public string color_name { get; set; }
        public string num_sets { get; set; }
        public string num_set_parts { get; set; }
        public string part_img_url { get; set; }
        public List<string> elements { get; set; }
    }

    public class RebrickableCategoryJson
    {
        public string id { get; set; }
        public string name { get; set; }
        public int part_count { get; set; }
    }

    public class RebrickableColorJson
    {
        public string id { get; set; }
        public string name { get; set; }
        public string rgb { get; set; }
        public bool is_trans { get; set; }
    }
}
