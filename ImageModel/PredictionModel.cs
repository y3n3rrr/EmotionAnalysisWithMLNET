namespace EmotionAnalysisWithMLNET.ImageModel
{
    public class PredictionModel
    {
        public float[] Score;

        public string PredictedLabelValue;
    }

    public class ImageWithLabelPrediction : PredictionModel
    {
        public ImageWithLabelPrediction(PredictionModel pred, string label)
        {
            Label = label;
            Score = pred.Score;
            PredictedLabelValue = pred.PredictedLabelValue;
        }

        public string Label;
    }

    public class ImagePredictionEx
    {
        public string ImagePath;
        public string Label;
        public string PredictedLabelValue;
        public float[] Score;
    }
}