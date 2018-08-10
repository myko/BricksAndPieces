using System;
using System.Windows.Media;

namespace BricksAndPieces
{
    public class Brick : ViewModelKit.ViewModelBase
    {
        public int DesignId { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public ChangingValue Quantity { get; set; }
        public ChangingValue Price { get; set; }
        public ImageSource Image { get; set; }
    }
}
