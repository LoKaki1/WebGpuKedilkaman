using System;

public class HeightMapGenerator
{
    /// <summary>
    /// Generates a height map from a grayscale image.
    /// The image is first processed using a bilateral filter to reduce noise
    /// (while preserving edges) and an unsharp mask to enhance edge contrast.
    /// Then, bilinear interpolation is used to resample the image,
    /// and a scale factor is applied to convert intensity to height.
    /// </summary>
    /// <param name="image">2D array of bytes representing the grayscale image</param>
    /// <param name="scaleFactor">Scale factor to convert intensity to height</param>
    /// <param name="outputWidth">Width of the output height map</param>
    /// <param name="outputHeight">Height of the output height map</param>
    /// <returns>2D array of floats representing the height map</returns>
    public static float[,] GenerateHeightMap(byte[,] image, float scaleFactor, int outputWidth, int outputHeight)
    {
        // Step 1: Apply bilateral filter to reduce noise while preserving edges.
        byte[,] denoisedImage = ApplyBilateralFilter(image, diameter: 5, sigmaColor: 25.0, sigmaSpace: 5.0);

        // Step 2: Apply unsharp masking to enhance height differences.
        byte[,] sharpenedImage = ApplyUnsharpMask(denoisedImage, amount: 1.5);

        int inputHeight = sharpenedImage.GetLength(0);
        int inputWidth = sharpenedImage.GetLength(1);
        float[,] heightMap = new float[outputHeight, outputWidth];

        // Calculate scaling ratios for bilinear interpolation.
        float xRatio = (inputWidth - 1) / (float)(outputWidth - 1);
        float yRatio = (inputHeight - 1) / (float)(outputHeight - 1);

        for (int j = 0; j < outputHeight; j++)
        {
            float y = j * yRatio;
            for (int i = 0; i < outputWidth; i++)
            {
                float x = i * xRatio;
                // Use bilinear interpolation on the processed image.
                float interpolatedValue = BilinearInterpolate(sharpenedImage, x, y);
                heightMap[j, i] = interpolatedValue * scaleFactor;
            }
        }

        return heightMap;
    }

    /// <summary>
    /// Applies a bilateral filter to a grayscale image.
    /// This filter smooths the image while preserving edges.
    /// </summary>
    /// <param name="image">2D byte array of the input image</param>
    /// <param name="diameter">Diameter of the filter kernel (should be odd)</param>
    /// <param name="sigmaColor">Filter sigma in the intensity (color) domain</param>
    /// <param name="sigmaSpace">Filter sigma in the coordinate space</param>
    /// <returns>A new 2D byte array representing the filtered image</returns>
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

                // Iterate over the kernel window.
                for (int k = -half; k <= half; k++)
                {
                    for (int l = -half; l <= half; l++)
                    {
                        int ni = i + k;
                        int nj = j + l;

                        // Skip out-of-bounds indices.
                        if (ni < 0 || ni >= height || nj < 0 || nj >= width)
                            continue;

                        double neighborVal = image[ni, nj];
                        double spatialDistSq = k * k + l * l;
                        double intensityDiff = neighborVal - centerVal;
                        double intensityDistSq = intensityDiff * intensityDiff;

                        // Compute the bilateral weight.
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
    /// Applies an unsharp mask to a grayscale image.
    /// This enhances edges by subtracting a blurred version of the image from the original.
    /// </summary>
    /// <param name="image">2D byte array of the input image</param>
    /// <param name="amount">Amount of sharpening (typical values between 1.0 and 2.0)</param>
    /// <returns>A new 2D byte array representing the sharpened image</returns>
    public static byte[,] ApplyUnsharpMask(byte[,] image, double amount)
    {
        // Use a simple 3x3 Gaussian blur as the basis for unsharp masking.
        byte[,] blurred = ApplyGaussianBlur(image);
        int height = image.GetLength(0);
        int width = image.GetLength(1);
        byte[,] result = new byte[height, width];

        for (int i = 0; i < height; i++)
        {
            for (int j = 0; j < width; j++)
            {
                // Enhance edges: original + amount * (original - blurred)
                int val = image[i, j] + (int)(amount * (image[i, j] - blurred[i, j]));
                result[i, j] = (byte)Math.Clamp(val, 0, 255);
            }
        }
        return result;
    }

    /// <summary>
    /// Applies a simple 3x3 Gaussian blur to a grayscale image.
    /// Kernel used: [1 2 1; 2 4 2; 1 2 1] / 16.
    /// </summary>
    /// <param name="image">2D byte array of the input image</param>
    /// <returns>A new 2D byte array representing the blurred image</returns>
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
                        // Clamp indices to image boundaries.
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
    /// Performs bilinear interpolation on a grayscale image represented as a 2D byte array.
    /// </summary>
    /// <param name="image">2D byte array of the image</param>
    /// <param name="x">Floating point x coordinate in the input image space</param>
    /// <param name="y">Floating point y coordinate in the input image space</param>
    /// <returns>Interpolated pixel value as a float</returns>
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

    // Example usage:
    public static void Main()
    {
        // Example: Create a dummy 5x5 grayscale image.
        byte[,] grayscaleImage = new byte[5, 5]
        {
            { 10,  20,  30,  40,  50 },
            { 15,  25,  35,  45,  55 },
            { 20,  30,  40,  50,  60 },
            { 25,  35,  45,  55,  65 },
            { 30,  40,  50,  60,  70 }
        };

        // Define output dimensions and scale factor.
        int outputWidth = 5;
        int outputHeight = 5;
        float scaleFactor = 13.25f;

        float[,] heightMap = GenerateHeightMap(grayscaleImage, scaleFactor, outputWidth, outputHeight);

        // Print the resulting height map.
        for (int i = 0; i < outputHeight; i++)
        {
            for (int j = 0; j < outputWidth; j++)
            {
                Console.Write($"{heightMap[i, j]:F2}\t");
            }
            Console.WriteLine();
        }
    }
}