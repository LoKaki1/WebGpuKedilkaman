using System;

namespace WebGpuKedilkaman;

public class KillMe
{
    void SmoothBlackDots(byte[] dem, int width, int height)
    {
        byte[] temp = new byte[dem.Length];

        for (int y = 1; y < height - 1; y++)
        {
            for (int x = 1; x < width - 1; x++)
            {
                int sum = 0;
                int count = 0;

                for (int ky = -1; ky <= 1; ky++)
                {
                    for (int kx = -1; kx <= 1; kx++)
                    {
                        int index = (y + ky) * width + (x + kx);
                        sum += dem[index];
                        count++;
                    }
                }

                int avg = sum / count;
                int idx = y * width + x;

                // Ensure we don't make black areas too bright
                temp[idx] = (byte)(dem[idx] < 128 ? Math.Min(dem[idx], avg) : avg);
            }
        }

        Buffer.BlockCopy(temp, 0, dem, 0, dem.Length);
    }
}
