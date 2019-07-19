using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;
using System;
using System.Windows.Media;

namespace BricksAndPieces
{
    public class Element : ViewModelKit.ViewModelBase
    {
        public string ElementId { get; set; }
        public ObservableCollection<Brick> Bricks { get; set; } = new ObservableCollection<Brick>();

        public bool HasNoColors { get; set; }

        public string Description { get; set; }
        public string FromYear { get; set; }
        public string ToYear { get; set; }
        public ImageSource Image { get; set; }
    }
}
