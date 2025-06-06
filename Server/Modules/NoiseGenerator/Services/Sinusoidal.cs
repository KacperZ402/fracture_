﻿namespace Fracture.Server.Modules.NoiseGenerator.Services;

public class Sinusoidal
{
    // So, you might be wondering what the f is this?
    // This is a custom noise generator that uses sinusoidal functions.
    // It's a bit more complex than Perlin noise,
    // but it's worth it, generated noise is more "smooth" - ideal for biomes.
    // You can use this to generate temperature, humidity, etc.
    // The code is written in Python, but I've translated it to C#.
    // So if you find more elegant way to write this, please do so.

    private static (float, float) RotateCoordinates(float x, float y, float angle)
    {
        // Rotate the coordinates.
        // This one is usefull as sometimes noise can produce artifacts like diagonal lines.
        // By rotating the coordinates, you can remove those artifacts.
        var cosAngle = (float)Math.Cos(angle);
        var sinAngle = (float)Math.Sin(angle);
        var rotatedX = x * cosAngle - y * sinAngle;
        var rotatedY = x * sinAngle + y * cosAngle;
        return (rotatedX, rotatedY);
    }

    private static float Fade(float t, float x, float y, float n)
    {
        // Fade function for smoother borders. It can and will be changed.
        var distance = Math.Abs(Math.Sqrt(Math.Sqrt(Math.Pow(x, 4) + Math.Pow(y, 4))));
        var maxDistance = Math.Sqrt(Math.Pow(n, 4));
        var fadeFactor = 1 - distance / maxDistance;
        return t * (float)fadeFactor;
    }

    public static float Layer(
        float x,
        float y,
        float frequency,
        float amplitude,
        float offset,
        Func<float, float, float, float, float> customFunction
    )
    {
        // This is the main function that generates the noise.
        // Proceed with caution, this is a bit complex.
        // x, y - coordinates
        // frequency - frequency of the wave
        // amplitude - amplitude of the wave
        // offset - offset of the wave
        // customFunction - custom function that generates the wave

        return Fade(
            amplitude * customFunction(x, y, frequency, offset),
            x - offset,
            y - offset,
            2.5f
        );
    }

    public static float CustomNoise(
        float x,
        float y,
        (float freq, float amp, float offs, float ang)[] layers,
        float lacunarity,
        Func<float, float, float, float, float> customFunction
    )
    {
        float total = 0;
        foreach (var (frequency, amplitude, offset, angle) in layers)
        {
            var (rotatedX, rotatedY) = RotateCoordinates(x, y, angle);
            total +=
                Layer(
                    rotatedX + offset,
                    rotatedY + offset,
                    frequency,
                    amplitude,
                    offset,
                    customFunction
                ) / lacunarity;
        }

        return total;
    }

    public static float[,] GenerateNoiseMap(
        int size,
        float min,
        float max,
        (float freq, float amp, float offs, float ang)[] layers,
        float lacunarity,
        Func<float, float, float, float, float> customFunction
    )
    {
        var map = new float[size, size];
        var minValue = float.MaxValue;
        var maxValue = float.MinValue;

        for (var i = 0; i < size; i++)
        // Generate the noise map and find the min and max values
        for (var j = 0; j < size; j++)
        {
            var noiseValue = CustomNoise(i, j, layers, lacunarity, customFunction);
            map[i, j] = noiseValue;
            if (noiseValue < minValue)
                minValue = noiseValue;
            if (noiseValue > maxValue)
                maxValue = noiseValue;
        }

        for (var i = 0; i < size; i++)
        // Normalize the noise map to the range [min, max]
        for (var j = 0; j < size; j++)
            map[i, j] = min + (map[i, j] - minValue) / (maxValue - minValue) * (max - min);

        return map;
    }

    /*
     * Example usage:
     * 1. Create a list of layers with frequencies, amplitudes, offsets, angles, functions.
     * 1.1 You can use the provided custom functions or create your own. It is the best to use the provided ones.
     * 2. Generate the noise map.
     * 3. Normalize the noise map.
     *
     * code:
     * var layers = new[]
     * {
     *  (0.8f, 20f, 0f, 1f, CustomFunction1), // Frequency, amplitude, offset, angle, custom function
     *  (1f, 20f, 1f, 2f, CustomFunction2), // angles are in radians
     *  (1.2f, 20f, 2f, 3f, CustomFunction3), // offset is the offset of the wave, also known as phase
     *  (1.4f, 20f, 3f, 4f, CustomFunction4),
     *  (1.6f, 20f, 4f, 5f, CustomFunction5),
     * };
     *
     * automatic layer generation:
        var random = new Random();
        var customFunctions = CustomFunctions.GetCustomFunctions();
        var layers = new List<(float, float, float, float, Func<float, float, float, float, float>)>();

        for (int i = 0; i < 8; i++)
        {
            float frequency = (float)(0.8 * (i + 1) + random.NextDouble() * (i + 1 - 0.8 * (i + 1)));
            float amplitude = 20;
            float offset = (float)(random.NextDouble() * 2);
            float angle = (float)(random.NextDouble() * 2 * Math.PI);
            var customFunction = customFunctions[random.Next(customFunctions.Length)];

            layers.Add((frequency, amplitude, offset, angle, customFunction));
        }
     */
}
