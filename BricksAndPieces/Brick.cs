using System;
using System.Windows.Media;

namespace BricksAndPieces
{
    public class Brick : ViewModelKit.ViewModelBase
    {
        public int DesignId { get; set; }
        public string Description { get; set; }
        public string Color { get; set; }
        public string FromYear { get; set; }
        public string ToYear { get; set; }
        public string AppearsIn { get; set; }
        public ChangingValue Quantity { get; set; } = new ChangingValue(0);
        public ChangingValue Price { get; set; } = new ChangingValue(0);
        public ImageSource Image { get; set; }
    }
}
