using System;
using ImageMagick;

namespace HeightMapProcessor
{
    class Program
    {
        static void Main(string[] args)
        {
            // Input and output file names.
            string inputFile = "input.jp2";
            string outputFile = "output.tiff";

            // Parameters for cropping and scaling.
            int cropWidth = 512;
            int cropHeight = 512;
            float scaleFactor = 13.25f;

            // Load the JP2 image.
            using (var image = new MagickImage(inputFile))
            {
                // Ensure image is in grayscale.
                image.ColorSpace = ColorSpace.Gray;

                // Crop a 512x512 region (here, top-left is used; adjust as needed).
                image.Crop(new MagickGeometry(cropWidth, cropHeight, 0, 0));
                image.RePage(); // Remove virtual canvas info.

                // Get pixel data (for a grayscale8 image each pixel is one byte).
                byte[] pixelData = image.GetPixels().ToByteArray();
                byte[,] byteArray = new byte[cropHeight, cropWidth];
                for (int i = 0; i < cropHeight; i++)
                {
                    for (int j = 0; j < cropWidth; j++)
                    {
                        byteArray[i, j] = pixelData[i * cropWidth + j];
                    }
                }

                // Process the image to generate the height map.
                float[,] heightMap = HeightMapGenerator.GenerateHeightMap(byteArray, scaleFactor, cropWidth, cropHeight);

                // Convert the 2D height map array to a 1D float array.
                float[] floatData = new float[cropWidth * cropHeight];
                for (int i = 0; i < cropHeight; i++)
                {
                    for (int j = 0; j < cropWidth; j++)
                    {
                        floatData[i * cropWidth + j] = heightMap[i, j];
                    }
                }

                // Create a new MagickImage from the float array.
                // "I" channel means intensity and StorageType.Float allows storing floating-point values.
                var settings = new PixelReadSettings(cropWidth, cropHeight, "I", StorageType.Float);
                using (var heightMapImage = new MagickImage())
                {
                    heightMapImage.ReadPixels(floatData, settings);
                    heightMapImage.Format = MagickFormat.Tiff;
                    heightMapImage.Write(outputFile);
                }
            }
        }
    }

    public class HeightMapGenerator
    {
        /// <summary>
        /// Processes the input image by first applying bilateral filtering to reduce noise,
        /// then applying an unsharp mask to enhance height differences.
        /// Finally, bilinear interpolation is used to generate the output height map.
        /// </summary>
        public static float[,] GenerateHeightMap(byte[,] image, float scaleFactor, int outputWidth, int outputHeight)
        {
            // Step 1: Denoise with a bilateral filter (diameter 5, sigmaColor 25.0, sigmaSpace 5.0).
            byte[,] denoisedImage = ApplyBilateralFilter(image, diameter: 5, sigmaColor: 25.0, sigmaSpace: 5.0);
            // Step 2: Enhance edges using unsharp masking (amount 1.5).
            byte[,] sharpenedImage = ApplyUnsharpMask(denoisedImage, amount: 1.5);

            int inputHeight = sharpenedImage.GetLength(0);
            int inputWidth = sharpenedImage.GetLength(1);
            float[,] heightMap = new float[outputHeight, outputWidth];

            // Calculate ratios to map output pixels back into input space.
            float xRatio = (inputWidth - 1) / (float)(outputWidth - 1);
            float yRatio = (inputHeight - 1) / (float)(outputHeight - 1);

            for (int j = 0; j < outputHeight; j++)
            {
                float y = j * yRatio;
                for (int i = 0; i < outputWidth; i++)
                {
                    float x = i * xRatio;
                    float interpolatedValue = BilinearInterpolate(sharpenedImage, x, y);
                    heightMap[j, i] = interpolatedValue * scaleFactor;
                }
            }
            return heightMap;
        }

        /// <summary>
        /// Applies a bilateral filter to reduce noise while preserving edges.
        /// </summary>
        public static byte[,] ApplyBilateralFilter(byte[,] image, int diameter, double sigmaColor, double sigmaSpace)
        {
            int height = image.GetLength(0);
            int width = image.GetLength(1);
            byte[,] result = new byte[height, width];
            int half = diameter / 2;

            double twoSigmaColorSq = 2 * sigmaColor * sigmaColor;
            double twoSigmaSpaceSq = 2 * sigmaSpace * sigmaSpace;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    double weightSum = 0.0;
                    double pixelSum = 0.0;
                    double centerVal = image[i, j];

                    for (int k = -half; k <= half; k++)
                    {
                        for (int l = -half; l <= half; l++)
                        {
                            int ni = i + k;
                            int nj = j + l;
                            if (ni < 0 || ni >= height || nj < 0 || nj >= width)
                                continue;

                            double neighborVal = image[ni, nj];
                            double spatialDistSq = k * k + l * l;
                            double intensityDiff = neighborVal - centerVal;
                            double intensityDistSq = intensityDiff * intensityDiff;
                            double weight = Math.Exp(-((spatialDistSq / twoSigmaSpaceSq) + (intensityDistSq / twoSigmaColorSq)));
                            weightSum += weight;
                            pixelSum += neighborVal * weight;
                        }
                    }
                    result[i, j] = (byte)Math.Round(pixelSum / weightSum);
                }
            }
            return result;
        }

        /// <summary>
        /// Enhances edges by applying an unsharp mask.
        /// </summary>
        public static byte[,] ApplyUnsharpMask(byte[,] image, double amount)
        {
            byte[,] blurred = ApplyGaussianBlur(image);
            int height = image.GetLength(0);
            int width = image.GetLength(1);
            byte[,] result = new byte[height, width];

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int val = image[i, j] + (int)(amount * (image[i, j] - blurred[i, j]));
                    result[i, j] = (byte)Math.Clamp(val, 0, 255);
                }
            }
            return result;
        }

        /// <summary>
        /// Applies a simple 3x3 Gaussian blur using the kernel [1 2 1; 2 4 2; 1 2 1]/16.
        /// </summary>
        public static byte[,] ApplyGaussianBlur(byte[,] image)
        {
            int height = image.GetLength(0);
            int width = image.GetLength(1);
            byte[,] result = new byte[height, width];

            int[,] kernel = new int[3, 3]
            {
                {1, 2, 1},
                {2, 4, 2},
                {1, 2, 1}
            };
            int kernelSum = 16;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < width; j++)
                {
                    int sum = 0;
                    for (int ki = -1; ki <= 1; ki++)
                    {
                        for (int kj = -1; kj <= 1; kj++)
                        {
                            int ni = i + ki;
                            int nj = j + kj;
                            ni = Math.Clamp(ni, 0, height - 1);
                            nj = Math.Clamp(nj, 0, width - 1);
                            sum += image[ni, nj] * kernel[ki + 1, kj + 1];
                        }
                    }
                    result[i, j] = (byte)(sum / kernelSum);
                }
            }
            return result;
        }

        /// <summary>
        /// Performs bilinear interpolation on a grayscale image.
        /// </summary>
        public static float BilinearInterpolate(byte[,] image, float x, float y)
        {
            int width = image.GetLength(1);
            int height = image.GetLength(0);

            int x0 = (int)Math.Floor(x);
            int y0 = (int)Math.Floor(y);
            int x1 = Math.Min(x0 + 1, width - 1);
            int y1 = Math.Min(y0 + 1, height - 1);

            float dx = x - x0;
            float dy = y - y0;

            float topLeft = image[y0, x0];
            float topRight = image[y0, x1];
            float bottomLeft = image[y1, x0];
            float bottomRight = image[y1, x1];

            float top = topLeft * (1 - dx) + topRight * dx;
            float bottom = bottomLeft * (1 - dx) + bottomRight * dx;

            return top * (1 - dy) + bottom * dy;
        }
    }
}