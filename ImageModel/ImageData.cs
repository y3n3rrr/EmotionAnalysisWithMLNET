using Microsoft.ML.Data;

namespace EmotionAnalysisWithMLNET.ImageModel
{
    public class ImageData
    {
        [LoadColumn(0)] //  Microsoft.ML.Data;
        public string ImagePath;

        [LoadColumn(1)]
        public string Label;
    }
}