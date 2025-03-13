
//using Microsoft.Extensions.DependencyInjection;
//using Silk.NET.Maths;
//using Silk.NET.WebGPU;
//using Silk.NET.Windowing;
//using WebGpuKedilkaman;
//using WebGpuKedilkaman.Pipelines;

//var serviceCollection window without any api
//var windowOptions = WindowOptions.Default with
//{
//    API = GraphicsAPI.None,
//    Size = new Vector2D<int>(800, 600),
//    Title = "LoKaki GPU"
//};

//var window = Window.Create(windowOptions);
//window.Initialize();
//serviceCollection.AddSingleton(window);= new ServiceCollection();
//// Initialize the 
///using System;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Reflection.Emit;

class Program
{
    static void Main()
    {
        // Load the noisy image
        string inputPath = "image3.png";
        string outputPath = "enhanced_denoised_image.png";

        // Load the noisy image
        Mat noisyImage = CvInvoke.Imread(inputPath, ImreadModes.Color);
        Mat denoisedImage = new Mat();
        Mat sharpenedImage = new Mat();

        // Step 1: Apply stronger noise reduction
        CvInvoke.FastNlMeansDenoisingColored(noisyImage, denoisedImage, 25, 25, 7, 21);

        // Step 2: Apply a secondary pass with lower intensity to refine details
        CvInvoke.FastNlMeansDenoisingColored(denoisedImage, denoisedImage, 25, 25, 7, 21);
        // Convert to grayscale to detect dark areas
        Mat gray = new Mat();
        CvInvoke.CvtColor(denoisedImage, gray, ColorConversion.Bgr2Gray);

        // Threshold to create a mask of dark regions (adjust threshold as needed)
        Mat darkMask = new Mat();
        CvInvoke.Threshold(gray, darkMask, 50, 255, ThresholdType.BinaryInv); // Dark pixels are white in mask

        // Apply stronger denoising ONLY on dark areas
        Mat darkDenoised = new Mat();
        CvInvoke.FastNlMeansDenoisingColored(denoisedImage, darkDenoised, 30, 30, 7, 21);

        // Blend the denoised dark regions back into the original image
        darkDenoised.CopyTo(denoisedImage, darkMask);

        // Step 2: Apply Unsharp Masking (Sharpen the Image)
        Mat blurred = new Mat();
        CvInvoke.GaussianBlur(denoisedImage, blurred, new System.Drawing.Size(0, 0), 3);
        CvInvoke.AddWeighted(denoisedImage, 1.5, blurred, -0.5, 0, sharpenedImage);

        // Step 3: Apply CLAHE (Contrast Limited Adaptive Histogram Equalization)
        Mat labImage = new Mat();
        CvInvoke.CvtColor(sharpenedImage, labImage, ColorConversion.Bgr2Lab);

        // Split into LAB channels
        VectorOfMat labChannels = new VectorOfMat();
        CvInvoke.Split(labImage, labChannels);
        Mat lChannel = labChannels[0];

        // Apply CLAHE on the L channel
        Mat claheL = new Mat();
        CvInvoke.CLAHE(lChannel, 3.0, new System.Drawing.Size(8, 8), claheL);
        var mat = new List<Mat>();
        for (int i = 0; i < labChannels.Size; i++)
        {
            mat.Add(labChannels[i]);
        }
        mat[0] = claheL;
        VectorOfMat labChannels2 = new VectorOfMat(mat.ToArray());
        //// Replace the L-channel with the enhanced version
        //labChannels.Replace(0, claheL);

        // Merge LAB channels back and convert to BGR
        CvInvoke.Merge(labChannels2, labImage);
        CvInvoke.CvtColor(labImage, sharpenedImage, ColorConversion.Lab2Bgr);

        // Save the final enhanced image
        CvInvoke.Imwrite(outputPath, sharpenedImage);

        Console.WriteLine("Enhanced denoised image saved as " + outputPath);
    }
}

// Initialize the WebGPU API
// var webGPUAPI = WebGPU.GetApi();
// serviceCollection.AddSingleton(webGPUAPI);
// var descriptor = new InstanceDescriptor();

// unsafe
// {
//     var instance = webGPUAPI.CreateInstance(ref descriptor);
//     // We can't 
//     serviceCollection.AddSingleton();
// }


// Engine engine = new Engine();

// UnlitRenderPipeline unlitRenderPipeline = new UnlitRenderPipeline(engine);

// engine.OnInitialize += () =>
// {
//     unlitRenderPipeline.Initialize();
// };
// engine.OnRender += () =>
// {
//     unlitRenderPipeline.Render();
// };

// engine.Initialize();
// engine.Dispose();