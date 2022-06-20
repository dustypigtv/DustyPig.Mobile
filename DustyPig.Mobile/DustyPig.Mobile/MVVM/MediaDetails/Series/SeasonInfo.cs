namespace DustyPig.Mobile.MVVM.MediaDetails.Series
{
    public class SeasonInfo
    {
        public SeasonInfo(int number)
        {
            Number = number;
            Text = $"Season {number}";
        }

        public int Number { get; set; }
        public string Text { get; set; }
    }
}
