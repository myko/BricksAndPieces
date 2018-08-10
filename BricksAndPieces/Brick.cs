namespace BricksAndPieces
{
    public class Brick : ViewModelKit.ViewModelBase
    {
        public int DesignId { get; set; }
        public string Color { get; set; }
        public ChangingValue Quantity { get; set; }
        public ChangingValue Price { get; set; }
    }

    public class ChangingValue: ViewModelKit.ViewModelBase
    {
        public double Value { get; private set; }

        public bool HasIncreased { get; private set; }
        public bool HasDecreased { get; private set; }

        public ChangingValue(double value)
        {
            Value = value;
        }

        public void Change(double newValue)
        {
            HasIncreased = newValue > Value;
            HasDecreased = newValue < Value;
            Value = newValue;
        }
    }
}
