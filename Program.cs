using EmotionAnalysisWithMLNET.Helper;
using EmotionAnalysisWithMLNET.ImageModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace EmotionAnalysisWithMLNET
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            string assetsPath = GetAbsolutePath(@"../../../assets");
            var inceptionPb = DownloadTansorFlowModel(Path.Combine(assetsPath, "tensorflow-pretrained-models")); //download and get path of tensorflow model

            var imageClassifierZip = Path.Combine(assetsPath, "outputs", "imageClassifier.zip");
            string imagesDownloadFolderPath = Path.Combine(assetsPath, "inputs", "images");
            string testimagesDownloadFolderPath = Path.Combine(assetsPath, "inputs", "TestImages");

            DownloadImageSet(imagesDownloadFolderPath, "emotion_train_samples.zip");

            //This method is optinal, just speed up training process. you may want to skip this, but its extremely usefull for production env
            EditImages(imagesDownloadFolderPath);
            EditImages(testimagesDownloadFolderPath); //reformat target test images as well

            Console.WriteLine($"Images folder: {imagesDownloadFolderPath}");

            IEnumerable<ImageData> allImages = LoadImagesFromDirectory(folder: imagesDownloadFolderPath, useFolderNameasLabel: true);
            IEnumerable<ImageData> testImages = LoadImagesFromDirectory(folder: testimagesDownloadFolderPath);

            try
            {
                var modelBuilder = new ModelBuilder(inceptionPb, imageClassifierZip);

                modelBuilder.BuildAndTrain(allImages, testImages);
            }
            catch (Exception ex)
            {
                ConsoleHelper.ConsoleWriteException(ex.ToString());
            }

            ConsoleHelper.ConsolePressAnyKey();
        }

        private static string DownloadTansorFlowModel(string folder)
        {
            string fileName = "inception_v3_2016_08_28_frozen.pb";
            string fullPath = Path.Combine(folder, fileName);
            string fileToBeExtracted = $"{fullPath}.tar.gz";
            if (!File.Exists(fileToBeExtracted))
            {
                fileName = $"{fileName}.tar.gz";
                string downloadUrl = $"https://storage.googleapis.com/download.tensorflow.org/models/inception_v3_2016_08_28_frozen.pb.tar.gz";
                AppHelper.Download(downloadUrl, folder, fileName); // Downloading tensorflow model from google server
                AppHelper.ExtractTarGz(Path.Combine(folder, fileName), folder); // Extract files same directory
            }
            return fullPath;
        }

        public static void EditImages(string folder)
        {
            var lists = Directory.GetDirectories(folder);

            foreach (var item in lists)
            {
                var files = Directory.GetFiles(item, "*",
                    searchOption: SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    if (CheckImageScale(file) || ((Path.GetExtension(file) != ".jpeg") && (Path.GetExtension(file) != ".jpg") && (Path.GetExtension(file) != ".PNG") && (Path.GetExtension(file) != ".png")))
                        continue;
                    ReFormatImages(file);
                }
            }
        }

        //check if target image size is 48x48
        public static bool CheckImageScale(string path)
        {
            Bitmap img = new Bitmap(path);
            return img.Height == 48 && img.Width == 48;
        }

        public static void ReFormatImages(string _path)
        {
            Bitmap target = null;
            using (Image targetImage = Image.FromFile(_path))
            {
                var imageD = AppHelper.ResizeImage(targetImage, 48, 48);
                target = AppHelper.TranformToGrayscale(imageD);
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
            File.Delete(_path);
            string fileName = Path.GetFileName(_path);
            string newFile = string.Format("{0}\\{1}", Directory.GetParent(_path), fileName);
            target.Save(newFile);
        }

        public static List<ImageData> LoadImagesFromDirectory(string folder, bool useFolderNameasLabel = true)
        {
            var lists = Directory.GetDirectories(folder);

            var result = new List<ImageData>();
            foreach (var item in lists)
            {
                var files = Directory.GetFiles(item, "*",
                    searchOption: SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    if ((Path.GetExtension(file) != ".jpeg") && (Path.GetExtension(file) != ".jpg") && (Path.GetExtension(file) != ".png") && (Path.GetExtension(file) != ".PNG"))
                        continue;

                    var label = Path.GetFileName(file);
                    if (useFolderNameasLabel)
                        label = Directory.GetParent(file).Name;
                    else
                    {
                        for (int index = 0; index < label.Length; index++)
                        {
                            if (!char.IsLetter(label[index]))
                            {
                                label = label.Substring(0, index);
                                break;
                            }
                        }
                    }

                    result.Add(new ImageData()
                    {
                        ImagePath = file,
                        Label = label
                    });

                    //yield return new ImageData()
                    //{
                    //    ImagePath = file,
                    //    Label = label
                    //};
                }
            }

            return result;
        }

        public static void DownloadImageSet(string imagesDownloadFolder, string fileName)
        {
            // Download a set of images to teach the network about the new classes

            //SMALL FLOWERS IMAGESET (200 files
            if (!File.Exists(Path.Combine(imagesDownloadFolder, fileName)))
            {
                string url = $"https://srv-store2.gofile.io/download/JWR6fB/{fileName}";
                AppHelper.Download(url, imagesDownloadFolder, fileName);
                AppHelper.UnZip(Path.Combine(imagesDownloadFolder, fileName), imagesDownloadFolder);
            }
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName;

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }
    }
}