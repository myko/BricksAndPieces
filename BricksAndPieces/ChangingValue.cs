namespace BricksAndPieces
{
    public class ChangingValue: ViewModelKit.ViewModelBase
    {
        public double Value { get; private set; }
        public bool IsEnabled { get; set; } = true;
        public bool IsDisabled => !IsEnabled;

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
