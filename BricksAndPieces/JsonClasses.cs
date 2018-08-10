using System.Collections.Generic;

namespace BricksAndPieces
{
    public class ResultJson
    {
        public ProductJson Product { get; set; }
        public List<BrickJson> Bricks { get; set; } = new List<BrickJson>();
        public string ImageBaseUrl { get; set; }
    }
    
    public class ProductJson
    {
        public string ProductName { get; set; }
        public string Asset { get; set; }
    }

    public class BrickJson
    {
        public int ItemNo { get; set; }
        public string ItemDescr { get; set; }
        public double SQty { get; set; }
        public string ColourDescr { get; set; }
        public string Asset { get; set; }
        public double Price { get; set; }
        public string PriceWithTaxStr { get; set; }
    }
}
