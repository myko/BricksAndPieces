using System.Windows.Media.Imaging;
using System.Collections.ObjectModel;

namespace BricksAndPieces
{
    public class Element : ViewModelKit.ViewModelBase
    {
        public string ElementId { get; set; }
        public ObservableCollection<Brick> Bricks { get; set; } = new ObservableCollection<Brick>();

        public string Description { get; set; }
        public BitmapImage Image { get; set; }
    }
}
