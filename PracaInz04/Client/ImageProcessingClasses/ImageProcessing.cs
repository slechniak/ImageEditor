﻿using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PracaInz04.Client.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PracaInz04.Client.ImageProcessingClasses
{
    public class ImageProcessing
    {
		public enum FilterType
		{
			Grayscale,
			Binary,
			Brightness,
			Contrast,
			Negative,
			EqualizeHistogram,
			StretchHistogram,
			Average,
			Sharpen,
			Gaussian,
			EdgeDetect,
			Emboss,
			SobelHorizontal,
			SobelVertical,
			Inferno,
			Turbo,
			Median,
			Erosion,
			Dilation,
			Opacity,
			Custom
		}

		StateService SService { get; set; }
		public IJSRuntime JS { get; set; }
		public int LongerResized { get; set; } = 1000;

		byte minValue, maxValue;
		SKBitmap bitmap2;
		IEnumerable<byte> pixelValues;
		int[] histogramArray;

		float[] pixelsH, pixelsS, pixelsL;
		float minL, maxL;

		int[,] indexTable;
		SKColor[] pixelTable;

		public ImageProcessing(IJSRuntime jSRuntime, StateService sService)
		{
			JS = jSRuntime;
			SService = sService;
		}

		public byte[] ResizeImageSharp(byte[] imageArray, int width = 500)
		{
			byte[] imageResult = new byte[] { };
			using (Image image = Image.Load(imageArray, out var imageFormat))
			{
				image.Mutate(x => x.AutoOrient());
				image.Mutate(x => x.Resize(width, 0));
				using (var ms = new MemoryStream())
				{
					image.Save(ms, imageFormat);
					imageResult = ms.ToArray();
				}
			}
			return imageResult;
		}

		public byte[] ResizeSkiaDefaut(byte[] imageArray)
		{
			SKBitmap sourceBitmap = SKBitmap.Decode(imageArray);
			return ResizeSkiaCommon(sourceBitmap);
		}

		public byte[] ResizeSkiaDefaut(SKBitmap sourceBitmap)
		{
			return ResizeSkiaCommon(sourceBitmap);
		}

		public byte[] ResizeSkiaCommon(SKBitmap sourceBitmap)
		{
			int longer = Math.Max(sourceBitmap.Width, sourceBitmap.Height);
			int shorter = Math.Min(sourceBitmap.Width, sourceBitmap.Height);

			if (longer > LongerResized)
			{
				float ratio = (float)LongerResized / longer;

				int resizedShorter = (int)Math.Round(ratio * shorter);
				int resizedWidth, resizedHeight;
				if (sourceBitmap.Width > sourceBitmap.Height)
				{
					resizedWidth = LongerResized;
					resizedHeight = resizedShorter;
				}
				else
				{
					resizedHeight = LongerResized;
					resizedWidth = resizedShorter;
				}
				var resultInfo = new SKImageInfo(resizedWidth, resizedHeight);
				SKBitmap resultBitmap = sourceBitmap.Resize(resultInfo, SKFilterQuality.High);
				return SKBitmapToArray(resultBitmap);
			}
			else
			{
				return SKBitmapToArray(sourceBitmap); 
			}
		}

		public SKBitmap ResizeSKBitmap(SKBitmap bitmap, int? width = null, int? height = null)
		{
			var resultInfo = GetResizedImageInfo(bitmap, width, height);
			SKBitmap resultBitmap = bitmap.Resize(resultInfo, SKFilterQuality.High);
			return resultBitmap;
		}

		public SKImageInfo GetResizedImageInfo(SKBitmap bitmap, int? width = null, int? height = null)
        {
			float ratio;
			if (height == null && width == null)
			{
				width = bitmap.Width;
				height = bitmap.Height;
			}
			else if (width == null)
            {
				ratio = (float)height / (float)bitmap.Height;
				width = (int)(ratio * (float)bitmap.Width);
			}
			else if (height == null)
			{
				ratio = (float)width / (float)bitmap.Width;
				height = (int)(ratio * (float)bitmap.Height);
			}

			return new SKImageInfo((int)width, (int)height);
		}

		public byte[] RotateSkia(byte[] imageArray)
		{
			byte[] imageResult = new byte[] { };
			using SKBitmap bitmap = SKBitmap.Decode(imageArray);
			using SKBitmap rotatedBitmap = new SKBitmap(bitmap.Height, bitmap.Width);
			using (SKCanvas canvas = new SKCanvas(rotatedBitmap))
			{
				canvas.Clear();
				canvas.Translate(bitmap.Height, 0);
				canvas.RotateDegrees(90);
				canvas.DrawBitmap(bitmap, new SKPoint());
			}
			var skData = rotatedBitmap.Encode(SKEncodedImageFormat.Png, 100);
			imageResult = skData.ToArray();

			return imageResult;
		}

		public async Task<string> GetImageURL(byte[] imageArray)
		{
			string imgSrc = string.Empty;
			imgSrc = await JS.InvokeAsync<string>("createObjectURLFromBA", imageArray);
			return imgSrc;
		}

		public byte[] SKBitmapToArray(SKBitmap bitmap)
		{
			var skData = bitmap.Encode(SKEncodedImageFormat.Png, 100);
			var bitmapArray = skData.ToArray();
			return bitmapArray;
		}

		public SKBitmap ApplyFilter(SKBitmap bitmap, SKColorFilter colorFilter = null, SKImageFilter imageFilter = null)
        {
			SKBitmap result = new SKBitmap(bitmap.Info);
			using (SKCanvas canvas = new SKCanvas(result))
			{
				SKPaint paint = new SKPaint()
				{
					ColorFilter = colorFilter,
					ImageFilter = imageFilter
				};
				canvas.DrawBitmap(bitmap, result.Info.Rect, paint);
			}
			return result;
		}

		public float[] IdentityKernel()
        {
			return new float[]
			{
				0,0,0,
				0,1,0,
				0,0,0
			};
		}

		public SKBitmap FilterErode(SKBitmap bitmap, int radius)
		{
			if (SService.newBitmap)
			{
				bitmap2 = bitmap;
				SService.newBitmap = false;
			}
			return ApplyFilter(bitmap2, imageFilter: SKImageFilter.CreateErode(radius, radius));
		}

		public SKBitmap FilterDilate(SKBitmap bitmap, int radius)
		{
			if (SService.newBitmap)
			{
				bitmap2 = bitmap;
				SService.newBitmap = false;
			}
			return ApplyFilter(bitmap2, imageFilter: SKImageFilter.CreateDilate(radius, radius));
		}

		public SKBitmap FilterOpacity(SKBitmap bitmap, int level)
		{
			float[] result = OpacityMatrix(level);
			ShowMatrix(result);
			return ApplyFilter(bitmap, SKColorFilter.CreateColorMatrix(
				result));
		}

		public float Gauss(int x, int y, float sigma)
        {
			return (float)(1 / (2d * Math.PI * sigma * sigma) * 
				Math.Pow(Math.E,
				-((x*x+y*y) / (2d * sigma * sigma))));
        }

		public SKBitmap FilterGaussian2(SKBitmap bitmap, int kernelSize, float sigma)
		{
			float[] kernel = GaussianKernel2(kernelSize, sigma);
			ShowKernel(kernel, "gaussian2 krenel");
			if (SService.newBitmap)
			{
				bitmap2 = bitmap;
				SService.newBitmap = false;
			}
			int offset = (int)Math.Floor(kernelSize / 2f);
			return ApplyFilter(bitmap2, imageFilter: SKImageFilter.CreateMatrixConvolution(
				new SKSizeI(kernelSize, kernelSize), kernel, gain: 1, bias: 0,
				kernelOffset: new SKPointI(offset, offset), SKShaderTileMode.Clamp, convolveAlpha: false));
		}

		public float[] GaussianKernel2(int kernelSize, float sigma)
		{
			float[] kernel = new float[kernelSize * kernelSize];
			int k = (kernelSize - 1) / 2;

			int n = 0;
            for (int i = 0; i < kernelSize; i++)
            {
				for (int j = 0; j < kernelSize; j++)
				{
					kernel[n] = Gauss(i - k, j - k, sigma);
					n++;
				}
			}
			
			float sum = kernel.Sum();
			for (int i = 0; i < kernel.Length; i++)
			{
				kernel[i] /= sum;
			}

			return kernel;
		}

		public void ShowKernel(float[] kernel, string title = "kernel:\n")
		{
			string s = title + $" ({kernel.Length}):\n";
			int counter = 1;
			int kernelSize = (int)Math.Round(Math.Sqrt(kernel.Length)); ;
			for (int i = 0; i < kernel.Length; i++)
			{
				s += $"{kernel[i].ToString("F4")}, ";
				if (counter == kernelSize)
				{
					s += "\n";
					counter = 0;
				}
				counter++;
			}
			//Console.WriteLine(s);
		}

		public SKBitmap FilterAverage(SKBitmap bitmap, int kernelSize)
		{
			float[] kernel = AverageKernel(kernelSize);
			if (SService.newBitmap)
			{
				bitmap2 = bitmap;
				SService.newBitmap = false;
			}
			int offset = (int)Math.Floor(kernelSize / 2f);
			return ApplyFilter(bitmap2, imageFilter: SKImageFilter.CreateMatrixConvolution(
				new SKSizeI(kernelSize, kernelSize), kernel, gain: 1, bias: 0,
				kernelOffset: new SKPointI(offset, offset), SKShaderTileMode.Clamp, convolveAlpha: false));
		}

		public float[] AverageKernel(int kernelSize)
		{
			int kernelLength = kernelSize * kernelSize;
			float[] kernel = new float[kernelLength];
			float value = 1f / (kernelLength);
			//Console.WriteLine($"value: {value} kernelSize: {kernelSize}");
			for (int i = 0; i < kernel.Length; i++)
			{
				kernel[i] = value;
			}
			return kernel;
		}

		public SKBitmap FilterCustom(SKBitmap bitmap, float[] customKernel)
		{
			float[] kernel = customKernel;
			if (SService.newBitmap)
			{
				bitmap2 = bitmap;
				SService.newBitmap = false;
			}
			int kernelSize = (int)Math.Round(Math.Sqrt(kernel.Length));
			int offset = (int)Math.Floor(kernelSize / 2f);
			return ApplyFilter(bitmap2, imageFilter: SKImageFilter.CreateMatrixConvolution(
				new SKSizeI(kernelSize, kernelSize), kernel, gain: 1, bias: 0,
				kernelOffset: new SKPointI(offset, offset), SKShaderTileMode.Clamp, convolveAlpha: false));
		}

		public SKBitmap FilterEmboss(SKBitmap bitmap)
		{
			float[] kernel = EmbossKernel();
			if (SService.newBitmap)
			{
				bitmap2 = bitmap;
				SService.newBitmap = false;
			}
			int kernelSize = (int)Math.Round(Math.Sqrt(kernel.Length));
			int offset = (int)Math.Floor(kernelSize / 2f);
			return ApplyFilter(bitmap2, imageFilter: SKImageFilter.CreateMatrixConvolution(
				new SKSizeI(kernelSize, kernelSize), kernel, gain: 1, bias: 0,
				kernelOffset: new SKPointI(offset, offset), SKShaderTileMode.Clamp, convolveAlpha: false));
		}

		public float[] EmbossKernel()
		{
			float[] kernel = new float[]
			{
				-2, -1, 0,
				-1, 1, 1,
				0, 1, 2
			};
			return kernel;
		}

		public SKBitmap FilterSharpen(SKBitmap bitmap)
		{
			float[] kernel = SharpenKernel();
			if (SService.newBitmap)
			{
				bitmap2 = bitmap;
				SService.newBitmap = false;
			}
			int kernelSize = (int)Math.Round(Math.Sqrt(kernel.Length));
			int offset = (int)Math.Floor(kernelSize / 2f);
			return ApplyFilter(bitmap2, imageFilter: SKImageFilter.CreateMatrixConvolution(
				new SKSizeI(kernelSize, kernelSize), kernel, gain: 1, bias: 0, 
				kernelOffset: new SKPointI(offset,offset), SKShaderTileMode.Clamp, convolveAlpha: false));
		}

		public float[] SharpenKernel()
        {
			float[] kernel = new float[]
            {
				0, -1, 0,
				-1, 5, -1,
				0, -1, 0
            };
			return kernel;
        }

		public SKBitmap FilterSobelVertical(SKBitmap bitmap)
		{
			float[] kernel = SobelVerticalKernel();
			if (SService.newBitmap)
			{
				bitmap2 = bitmap;
				SService.newBitmap = false;
			}
			int kernelSize = (int)Math.Round(Math.Sqrt(kernel.Length));
			int offset = (int)Math.Floor(kernelSize / 2f);
			return ApplyFilter(bitmap2, imageFilter: SKImageFilter.CreateMatrixConvolution(
				new SKSizeI(kernelSize, kernelSize), kernel, gain: 1, bias: 0,
				kernelOffset: new SKPointI(offset, offset), SKShaderTileMode.Clamp, convolveAlpha: false));
		}

		public SKBitmap FilterSobelHorizontal(SKBitmap bitmap)
		{
			float[] kernel = SobelHorizontalKernel();
			if (SService.newBitmap)
			{
				bitmap2 = bitmap;
				SService.newBitmap = false;
			}
			int kernelSize = (int)Math.Round(Math.Sqrt(kernel.Length));
			int offset = (int)Math.Floor(kernelSize / 2f);
			return ApplyFilter(bitmap2, imageFilter: SKImageFilter.CreateMatrixConvolution(
				new SKSizeI(kernelSize, kernelSize), kernel, gain: 1, bias: 0,
				kernelOffset: new SKPointI(offset, offset), SKShaderTileMode.Clamp, convolveAlpha: false));
		}

		public float[] SobelHorizontalKernel()
		{
			float[] kernel = new float[]
			{
				-1, 0, 1,
				-2, 0, 2,
				-1, 0, 1
			};
			return kernel;
		}

		public float[] SobelVerticalKernel()
		{
			float[] kernel = new float[]
			{
				-1, -2, -1,
				0, 0, 0,
				1, 2, 1
			};
			return kernel;
		}

		public SKBitmap FilterEdgeDetect(SKBitmap bitmap)
		{
			float[] kernel = EdgeDetectKernel();
			if (SService.newBitmap)
			{
				bitmap2 = bitmap;
				SService.newBitmap = false;
			}
			int kernelSize = (int)Math.Round(Math.Sqrt(kernel.Length));
			int offset = (int)Math.Floor(kernelSize / 2f);
			return ApplyFilter(bitmap2, imageFilter: SKImageFilter.CreateMatrixConvolution(
				new SKSizeI(kernelSize, kernelSize), kernel, gain: 1, bias: 0,
				kernelOffset: new SKPointI(offset, offset), SKShaderTileMode.Clamp, convolveAlpha: false));
		}

		public float[] EdgeDetectKernel()
		{
			float[] kernel = new float[]
			{
				0, 1, 0,
				1, -4, 1,
				0, 1, 0
			};
			return kernel;
		}

		public SKBitmap FilterBinary(SKBitmap bitmap, int treshold)
		{
			byte[] table = BinaryTable(treshold);
			SKBitmap grayscaleBitmap = FilterGrayscale(bitmap);
			return ApplyFilter(grayscaleBitmap, SKColorFilter.CreateTable(
				null, table, table, table));
		}

		public SKBitmap FilterGrayscale(SKBitmap bitmap)
		{
			return ApplyFilter(bitmap, SKColorFilter.CreateColorMatrix(
				GrayscaleMatrix()));
		}

		public SKBitmap FilterBrightness(SKBitmap bitmap, int level)
		{
			float[] result = BrightnessMatrix(level);
			ShowMatrix(result);
			return ApplyFilter(bitmap, SKColorFilter.CreateColorMatrix(
				result));
		}

		public SKBitmap ChangeColor(SKBitmap bitmap, int shift, SKColorChannel channel)
		{
			float[] result = ColorMatrix(shift, channel);
			ShowMatrix(result);
			return ApplyFilter(bitmap, SKColorFilter.CreateColorMatrix(
				result));
		}

		public SKBitmap ChangeColorMulti(SKBitmap bitmap, int shift, bool isRed, bool isGreen, bool isBlue)
		{
			float[] result = ColorMatrixMulti(shift, isRed, isGreen, isBlue);
			ShowMatrix(result);
			return ApplyFilter(bitmap, SKColorFilter.CreateColorMatrix(
				result));
		}

		public SKBitmap FilterContrast(SKBitmap bitmap, int factor, bool inColor = true)
        {
			if (inColor)
				return FilterContrastColor(bitmap, factor);
			else
				return FilterContrastGrayscale(bitmap, factor);
		}

		public SKBitmap FilterContrastColor(SKBitmap bitmap, int factor)
		{
			SKBitmap result = new SKBitmap(bitmap.Info);
			SKColor[] pixels = bitmap.Pixels;
			if (SService.newBitmap)
			{
				pixelsH = new float[pixels.Length];
				pixelsS = new float[pixels.Length];
				pixelsL = new float[pixels.Length];
				for (int i = 0; i < pixels.Length; i++)
				{
					pixels[i].ToHsl(out pixelsH[i], out pixelsS[i], out pixelsL[i]);
                }
				minL = pixelsL.Min();
				maxL = pixelsL.Max();
				SService.newBitmap = false;
			}
			float oldL, newL;

			float oldMin = minL;
			float oldMax = maxL;
			float newMin = minL - factor;
			float newMax = maxL + factor;
			newMin = Math.Min(newMin, newMax);
			newMax = Math.Max(newMax, newMin);
			float newRange = newMax - newMin;
			float offset = (100 - newRange) / 2f;
			for (int i = 0; i < pixelsL.Length; i++)
			{
				oldL = pixelsL[i];
				newL = ((oldL - oldMin) / (oldMax - oldMin)) * newRange + newMin;
				newL = Math.Clamp(newL, 0f, 100f);
				pixels[i] = SKColor.FromHsl(pixelsH[i], pixelsS[i], newL);
            }
			result.Pixels = pixels;
			return result;
		}

		public SKBitmap FilterNegative(SKBitmap bitmap)
		{
			byte[] table = NegativeTable();
			return ApplyFilter(bitmap, SKColorFilter.CreateTable(
				null, table, table, table));
		}

		public SKBitmap FilterInferno(SKBitmap bitmap)
		{
			bitmap2 = FilterGrayscale(bitmap);
			return ApplyFilter(bitmap2, SKColorFilter.CreateTable(
				null, InfernoTable(SKColorChannel.R),
				InfernoTable(SKColorChannel.G),
				InfernoTable(SKColorChannel.B)));
		}

		public SKBitmap FilterTurbo(SKBitmap bitmap)
		{
			bitmap2 = FilterGrayscale(bitmap);
			return ApplyFilter(bitmap2, SKColorFilter.CreateTable(
				null, TurboTable(SKColorChannel.R),
				TurboTable(SKColorChannel.G),
				TurboTable(SKColorChannel.B)));
		}

		public SKBitmap FilterContrastGrayscale(SKBitmap bitmap, int factor)
		{
			if (SService.newBitmap)
			{
				bitmap2 = FilterGrayscale(bitmap);
				pixelValues = bitmap2.Pixels.Select(x => x.Red).ToArray();
			}
			byte[] table = ContrastTable1(factor, pixelValues);
            return ApplyFilter(bitmap2, SKColorFilter.CreateTable(
				null, table, table, table));
		}

		public static void AddSorted<T>(List<T> list, T item) where T : IComparable<T>
		{
			if (list.Count == 0)
			{
				list.Add(item);
				return;
			}
			if (list[list.Count - 1].CompareTo(item) <= 0)
			{
				list.Add(item);
				return;
			}
			if (list[0].CompareTo(item) >= 0)
			{
				list.Insert(0, item);
				return;
			}
			int index = list.BinarySearch(item);
			if (index < 0)
				index = ~index;
			list.Insert(index, item);
		}

		private int[,] GetIndexTable(int width, int height)
        {
			int[,] table = new int[width, height];
			int index = 0;
            for (int i = 0; i < height; i++)
            {
				for (int j = 0; j < width; j++)
                {
					table[j, i] = index;
					index++;
				}
            }
			return table;
        }

		private SKColor[] GetNeighbourhood(SKPointI start, SKPointI end, int windowLength)
		{
			SKColor[] array = new SKColor[windowLength];
			int index = 0;
			for (int i = start.X; i <= end.X; i++)
			{
				for (int j = start.Y; j <= end.Y; j++)
				{
					array[index] = pixelTable[indexTable[i, j]];
					index++;
				}
			}
			return array;
		}

		private (List<byte> windowR, List<byte> windowG, List<byte> windowB, List<SKColor> toRemove) GetWindowSorted
			(SKPointI start, SKPointI end)
		{
			var windowR = new List<byte>();
			var windowG = new List<byte>();
			var windowB = new List<byte>();
			var toRemove = new List<SKColor>();
			var pixel = new SKColor();

			for (int i = start.X; i <= end.X; i++)
			{
				for (int j = start.Y; j <= end.Y; j++)
				{
					pixel = pixelTable[indexTable[start.X, j]];
					AddSorted(windowR, pixel.Red);
					AddSorted(windowG, pixel.Green);
					AddSorted(windowB, pixel.Blue);
					toRemove.Add(pixel);
				}
			}
			return (windowR, windowG, windowB, toRemove);
		}

		public SKBitmap FilterMedian(SKBitmap bitmap, int kernelSize)
		{
			if (SService.newBitmap)
			{
				bitmap2 = bitmap;
				indexTable = GetIndexTable(bitmap2.Width, bitmap2.Height);
				pixelTable = bitmap2.Pixels;
				SService.newBitmap = false;
			}

			Stopwatch stopwatch = new Stopwatch();
			stopwatch.Start();
			//Console.WriteLine("Start: result");

			SKBitmap result = new SKBitmap(bitmap2.Info);
			SKColor[] resultPixels = new SKColor[pixelTable.Length];
			SKPointI start = new SKPointI();
			SKPointI end = new SKPointI();
			int k = (int)Math.Floor(kernelSize / 2f);
			int windowLength = kernelSize * kernelSize;
			int medianIndex = (int)Math.Round(windowLength / 2f);
			List<byte> windowR;
			List<byte> windowG;
			List<byte> windowB;
			List<SKColor> toRemove;
			SKColor pixel;

			start = new SKPointI(0, 0);
			end = new SKPointI(2 * k, 2 * k);

			for (int j = k; j < bitmap2.Height - k; j++)
			{
				(windowR, windowG, windowB, toRemove) = GetWindowSorted(start, end);
				resultPixels[indexTable[k, j]] = 
					new SKColor(windowR[medianIndex], windowG[medianIndex], windowB[medianIndex]);
				start.X++;
				end.X++;

				for (int i = k + 1; i < bitmap2.Width - k; i++)
				{
                    for (int n = 1; n <= kernelSize; n++)
                    {
						pixel = toRemove[0];
						windowR.Remove(pixel.Red);
						windowG.Remove(pixel.Green);
						windowB.Remove(pixel.Blue);
						toRemove.RemoveAt(0);
                    }

                    for (int n = start.Y; n <= end.Y; n++)
                    {
						pixel = pixelTable[indexTable[end.X, n]];
                        AddSorted(windowR, pixel.Red);
						AddSorted(windowG, pixel.Green);
						AddSorted(windowB, pixel.Blue);
						toRemove.Add(pixel);
					}

					resultPixels[indexTable[i, j]] =
					new SKColor(windowR[medianIndex], windowG[medianIndex], windowB[medianIndex]);

					start.X++;
					end.X++;
				}

				start.X = 0;
				end.X = 2 * k;
				start.Y++;
				end.Y++;
			}
			result.Pixels = resultPixels;

			stopwatch.Stop();
			//Console.WriteLine("Stop: result: {0} ms", stopwatch.ElapsedMilliseconds);

			return result;
		}

		public SKBitmap EqualizeHistogram(SKBitmap bitmap)
        {
			bitmap2 = FilterGrayscale(bitmap);
			IEnumerable<byte> pixelList = bitmap2.Pixels.Select(x => x.Red);
			byte[] table = EqualizedHistogramTable(pixelList);
			return ApplyFilter(bitmap2, SKColorFilter.CreateTable(
				null, table, table, table));
		}

		public SKBitmap StretchHistogram(SKBitmap bitmap, int cutoff)
		{
			if (SService.newBitmap)
			{
				bitmap2 = FilterGrayscale(bitmap);
				pixelValues = bitmap2.Pixels.Select(x => x.Red);
				histogramArray = GetHistogramArray(pixelValues);
				SService.newBitmap = false;
			}
			byte[] table = StretchedHistogramTable(histogramArray, cutoff);
			return ApplyFilter(bitmap2, SKColorFilter.CreateTable(
				null, table, table, table));
		}

		public byte[] StretchedHistogramTable(int[] histogram, int cutoff)
		{
			int pixelsLength = pixelValues.Count();
            byte centileMin = Percentile(histogram, cutoff);
            byte centileMax = Percentile(histogram, 100 - cutoff);
            //Console.WriteLine($"centileMin: {centileMin}, centileMax: {centileMax}");

            byte[] table = new byte[256];
			for (int i = 0; i < centileMin; i++)
			{
				table[i] = 0;
			}
			for (int i = centileMax + 1; i < table.Length; i++)
			{
				table[i] = 255;
			}
			for (int i = centileMin; i <= centileMax; i++)
			{
				table[i] = (byte)Math.Round((float)(i - centileMin) / (centileMax - centileMin) * 255);
			}
			return table;
		}

		public byte[] EqualizedHistogramTable(IEnumerable<byte> pixels)
        {
			int pixelsLength = pixels.Count();
			int[] histogram = GetHistogramArray(pixels);
			int[] cdf = GetCDF(histogram);
			float cdfMin = cdf.Min();

			byte[] table = new byte[256];
            for (int i = 0; i < table.Length; i++)
            {
				table[i] = (byte)Math.Round((cdf[i] - cdfMin) / (pixelsLength - cdfMin) * 255);
            }
			return table;
        }

		public int[] GetCDF(int[] histogram)
		{
			int[] array = new int[256];
			histogram.CopyTo(array, 0);
            for (int i = 1; i < histogram.Length; i++)
            {
				array[i] += array[i - 1];
            }
			return array;
		}

		public float[] GrayscaleMatrix()
        {
			float[] matrix = new float[]
			{
				0.21f, 0.72f, 0.07f, 0, 0,
				0.21f, 0.72f, 0.07f, 0, 0,
				0.21f, 0.72f, 0.07f, 0, 0,
				0,     0,     0,     1, 0
			};
			return matrix;
		}

		public float[] BrightnessMatrix(int level)
		{
			float level2 = (float)level / 255;
			float[] offsetMatrix = new float[]
			{
				0, 0, 0, 0, level2,
				0, 0, 0, 0, level2,
				0, 0, 0, 0, level2,
				0, 0, 0, 0, 0
			};
			return AddArrays(offsetMatrix, IdentityMatrix());
		}

		public float[] OpacityMatrix(int level)
		{
			float level2 = (float)level / 255;
			float[] offsetMatrix = new float[]
			{
				0, 0, 0, 0, 0,
				0, 0, 0, 0, 0,
				0, 0, 0, 0, 0,
				0, 0, 0, 0, level2
			};
			return AddArrays(offsetMatrix, IdentityMatrix());
		}

		public float[] ColorMatrix(int shift, SKColorChannel channel)
		{
			float shift2 = (float)shift / 255;
			float[] shiftMatrix = new float[]
			{
				0, 0, 0, 0, channel == SKColorChannel.R ? shift2 : 0,
				0, 0, 0, 0, channel == SKColorChannel.G ? shift2 : 0,
				0, 0, 0, 0, channel == SKColorChannel.B ? shift2 : 0,
				0, 0, 0, 0, 0
			};
			return AddArrays(shiftMatrix, IdentityMatrix());
		}

		public float[] ColorMatrixMulti(int shift, bool isRed, bool isGreen, bool isBlue)
		{
			float shift2 = (float)shift / 255;
			float[] shiftMatrix = new float[]
			{
				0, 0, 0, 0, isRed ? shift2 : 0,
				0, 0, 0, 0, isGreen ? shift2 : 0,
				0, 0, 0, 0, isBlue ? shift2 : 0,
				0, 0, 0, 0, 0
			};
			return AddArrays(shiftMatrix, IdentityMatrix());
		}

		public byte[] NegativeTable()
		{
			byte[] table = new byte[256];
			for (int i = 0; i < table.Length; i++)
			{
				table[i] = (byte)(255 - i);
			}
			return table;
		}

		public byte[] BinaryTable(int treshold)
		{
			byte[] table = new byte[256];
            for (int i = 0; i <= treshold; i++)
            {
                table[i] = 0;
            }
			for (int i = treshold + 1; i < 256; i++)
			{
				table[i] = 255;
			}

			return table;
		}

		public byte[] ContrastTable1(int factor, IEnumerable<byte> pixels)
		{
			if (SService.newBitmap)
			{
				minValue = pixels.Min();
				maxValue = pixels.Max();
				SService.newBitmap = false;
			}
			byte[] table = new byte[256];
			for (int i = 0; i < table.Length; i++)
			{
				table[i] = (byte)i;
			}

			float oldMin = minValue;
			float oldMax = maxValue;
			float newMin = minValue - factor;
			float newMax = maxValue + factor;
			float newRange = newMax - newMin;

			for (int i = (int)oldMin; i <= oldMax; i++)
			{
				double newValue = Math.Round(((i - oldMin) / (oldMax - oldMin)) * newRange + newMin);
				table[i] = (byte)Math.Clamp(newValue, 0, 255);
			}

			return table;
		}

		public byte Percentile(int[] histogram, int percent)
        {
			byte result = 0;
			int percentofLength = (int)Math.Round(percent / 100f * pixelValues.Count());
            for (int i = 0; i < histogram.Length; i++)
            {
				percentofLength -= histogram[i];
				if (percentofLength <= 0)
				{
					result = (byte)i;
					break;
				}
            }
			return result;
		}

		public int[] GetHistogram2(byte[] pixels)
		{
			int[] histogram = new int[256];
			for (int i = 0; i < pixels.Length; i++)
			{
				histogram[pixels[i]] += 1;
			}
			return histogram;
		}

		public int[] GetHistogramArray(IEnumerable<byte> pixels)
		{
			List<byte> pixelsList = pixels.ToList();

			int[] array = new int[256];
			foreach (var pixel in pixelsList)
			{
				array[pixel]++;
			}
			return array;
		}

		public float[] IdentityMatrix()
        {
			return new float[]
			{
				1, 0, 0, 0, 0,
				0, 1, 0, 0, 0,
				0, 0, 1, 0, 0,
				0, 0, 0, 1, 0
			};
		}

		public float[] AddArrays(float[] a, float[] b)
		{
			return a.Zip(b, (x, y) => x + y).ToArray();
		}

		public byte[] InfernoTable(SKColorChannel channel)
        {
			switch(channel)
            {
				case SKColorChannel.R:
					return new byte[256] { 
						0, 1, 1, 1, 2, 2, 2, 3, 4, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 16, 17, 18, 20,
						21, 22, 24, 25, 27, 28, 30, 31, 33, 35, 36, 38, 40, 41, 43, 45, 47, 49, 50, 52,
						54, 56, 57, 59, 61, 62, 64, 66, 68, 69, 71, 73, 74, 76, 77, 79, 81, 82, 84, 85,
						87, 89, 90, 92, 93, 95, 97, 98, 100, 101, 103, 105, 106, 108, 109, 111, 113, 114,
						116, 117, 119, 120, 122, 124, 125, 127, 128, 130, 132, 133, 135, 136, 138, 140,
						141, 143, 144, 146, 147, 149, 151, 152, 154, 155, 157, 159, 160, 162, 163, 165,
						166, 168, 169, 171, 173, 174, 176, 177, 179, 180, 182, 183, 185, 186, 188, 189,
						191, 192, 193, 195, 196, 198, 199, 200, 202, 203, 204, 206, 207, 208, 210, 211,
						212, 213, 215, 216, 217, 218, 219, 221, 222, 223, 224, 225, 226, 227, 228, 229,
						230, 231, 232, 233, 234, 235, 235, 236, 237, 238, 239, 239, 240, 241, 241, 242,
						243, 243, 244, 245, 245, 246, 246, 247, 247, 248, 248, 248, 249, 249, 249, 250,
						250, 250, 251, 251, 251, 251, 251, 252, 252, 252, 252, 252, 252, 252, 252, 252,
						252, 252, 252, 251, 251, 251, 251, 251, 250, 250, 250, 250, 249, 249, 249, 248,
						248, 247, 247, 246, 246, 245, 245, 244, 244, 244, 243, 243, 242, 242, 242, 241,
						241, 241, 241, 242, 242, 243, 243, 244, 245, 246, 248, 249, 250, 252};
				case SKColorChannel.G:
					return new byte[256] { 0, 0, 1, 1, 1, 2, 2, 2, 3, 3, 4, 4, 5, 5, 6, 7, 7, 8, 8, 9, 9,
						10, 10, 11, 11, 11, 12, 12, 12, 12, 12, 12, 12, 12, 12, 12, 11, 11, 11, 11, 10,
						10, 10, 10, 9, 9, 9, 9, 9, 9, 10, 10, 10, 10, 11, 11, 12, 12, 13, 13, 14, 14,
						15, 15, 16, 16, 17, 18, 18, 19, 19, 20, 21, 21, 22, 22, 23, 24, 24, 25, 25,
						26, 26, 27, 28, 28, 29, 29, 30, 30, 31, 32, 32, 33, 33, 34, 34, 35, 35, 36,
						37, 37, 38, 38, 39, 39, 40, 41, 41, 42, 42, 43, 44, 44, 45, 46, 46, 47, 48,
						48, 49, 50, 50, 51, 52, 53, 53, 54, 55, 56, 57, 58, 58, 59, 60, 61, 62, 63,
						64, 65, 66, 67, 68, 69, 70, 71, 72, 74, 75, 76, 77, 78, 80, 81, 82, 83, 85,
						86, 87, 89, 90, 92, 93, 94, 96, 97, 99, 100, 102, 103, 105, 106, 108, 110,
						111, 113, 115, 116, 118, 120, 121, 123, 125, 126, 128, 130, 132, 133, 135,
						137, 139, 140, 142, 144, 146, 148, 150, 151, 153, 155, 157, 159, 161, 163,
						165, 166, 168, 170, 172, 174, 176, 178, 180, 182, 184, 186, 188, 190, 192,
						194, 196, 198, 199, 201, 203, 205, 207, 209, 211, 213, 215, 217, 219, 221,
						223, 225, 227, 229, 230, 232, 234, 236, 237, 239, 241, 242, 244, 245, 246,
						248, 249, 250, 251, 252, 253, 255};
				case SKColorChannel.B:
					return new byte[256] { 
						4, 5, 6, 8, 10, 12, 14, 16, 18, 20, 23, 25, 27, 29, 31, 34, 36, 38, 41, 43, 45,
						48, 50, 52, 55, 57, 60, 62, 65, 67, 69, 72, 74, 76, 79, 81, 83, 85, 87, 89, 91,
						92, 94, 95, 97, 98, 99, 100, 101, 102, 103, 104, 104, 105, 106, 106, 107, 107,
						108, 108, 108, 109, 109, 109, 110, 110, 110, 110, 110, 110, 110, 110, 110, 110,
						110, 110, 110, 110, 110, 110, 110, 110, 110, 110, 109, 109, 109, 109, 109, 108,
						108, 108, 107, 107, 107, 106, 106, 105, 105, 105, 104, 104, 103, 103, 102, 102,
						101, 100, 100, 99, 99, 98, 97, 96, 96, 95, 94, 94, 93, 92, 91, 90, 90, 89, 88,
						87, 86, 85, 84, 83, 82, 81, 80, 79, 78, 77, 76, 75, 74, 73, 72, 71, 70, 69, 68,
						67, 66, 65, 63, 62, 61, 60, 59, 58, 56, 55, 54, 53, 52, 51, 49, 48, 47, 46, 45,
						43, 42, 41, 40, 38, 37, 36, 35, 33, 32, 31, 29, 28, 27, 25, 24, 23, 21, 20, 19,
						18, 16, 15, 14, 12, 11, 10, 9, 8, 7, 7, 6, 6, 6, 6, 7, 7, 8, 9, 10, 12, 13, 15,
						17, 18, 20, 22, 24, 26, 29, 31, 33, 35, 38, 40, 42, 45, 47, 50, 53, 55, 58, 61,
						64, 67, 70, 73, 76, 79, 83, 86, 90, 93, 97, 101, 105, 109, 113, 117, 121, 125,
						130, 134, 138, 142, 146, 150, 154, 157, 161, 164};
				default: 
					return new byte[256];
			}
        }

		public byte[] TurboTable(SKColorChannel channel)
		{
			byte[,] turboTable = new byte[256, 3] { { 48, 18, 59 }, { 50, 21, 67 }, { 51, 24, 74 }, { 52, 27, 81 }, { 53, 30, 88 }, { 54, 33, 95 }, { 55, 36, 102 }, { 56, 39, 109 }, { 57, 42, 115 }, { 58, 45, 121 }, { 59, 47, 128 }, { 60, 50, 134 }, { 61, 53, 139 }, { 62, 56, 145 }, { 63, 59, 151 }, { 63, 62, 156 }, { 64, 64, 162 }, { 65, 67, 167 }, { 65, 70, 172 }, { 66, 73, 177 }, { 66, 75, 181 }, { 67, 78, 186 }, { 68, 81, 191 }, { 68, 84, 195 }, { 68, 86, 199 }, { 69, 89, 203 }, { 69, 92, 207 }, { 69, 94, 211 }, { 70, 97, 214 }, { 70, 100, 218 }, { 70, 102, 221 }, { 70, 105, 224 }, { 70, 107, 227 }, { 71, 110, 230 }, { 71, 113, 233 }, { 71, 115, 235 }, { 71, 118, 238 }, { 71, 120, 240 }, { 71, 123, 242 }, { 70, 125, 244 }, { 70, 128, 246 }, { 70, 130, 248 }, { 70, 133, 250 }, { 70, 135, 251 }, { 69, 138, 252 }, { 69, 140, 253 }, { 68, 143, 254 }, { 67, 145, 254 }, { 66, 148, 255 }, { 65, 150, 255 }, { 64, 153, 255 }, { 62, 155, 254 }, { 61, 158, 254 }, { 59, 160, 253 }, { 58, 163, 252 }, { 56, 165, 251 }, { 55, 168, 250 }, { 53, 171, 248 }, { 51, 173, 247 }, { 49, 175, 245 }, { 47, 178, 244 }, { 46, 180, 242 }, { 44, 183, 240 }, { 42, 185, 238 }, { 40, 188, 235 }, { 39, 190, 233 }, { 37, 192, 231 }, { 35, 195, 228 }, { 34, 197, 226 }, { 32, 199, 223 }, { 31, 201, 221 }, { 30, 203, 218 }, { 28, 205, 216 }, { 27, 208, 213 }, { 26, 210, 210 }, { 26, 212, 208 }, { 25, 213, 205 }, { 24, 215, 202 }, { 24, 217, 200 }, { 24, 219, 197 }, { 24, 221, 194 }, { 24, 222, 192 }, { 24, 224, 189 }, { 25, 226, 187 }, { 25, 227, 185 }, { 26, 228, 182 }, { 28, 230, 180 }, { 29, 231, 178 }, { 31, 233, 175 }, { 32, 234, 172 }, { 34, 235, 170 }, { 37, 236, 167 }, { 39, 238, 164 }, { 42, 239, 161 }, { 44, 240, 158 }, { 47, 241, 155 }, { 50, 242, 152 }, { 53, 243, 148 }, { 56, 244, 145 }, { 60, 245, 142 }, { 63, 246, 138 }, { 67, 247, 135 }, { 70, 248, 132 }, { 74, 248, 128 }, { 78, 249, 125 }, { 82, 250, 122 }, { 85, 250, 118 }, { 89, 251, 115 }, { 93, 252, 111 }, { 97, 252, 108 }, { 101, 253, 105 }, { 105, 253, 102 }, { 109, 254, 98 }, { 113, 254, 95 }, { 117, 254, 92 }, { 121, 254, 89 }, { 125, 255, 86 }, { 128, 255, 83 }, { 132, 255, 81 }, { 136, 255, 78 }, { 139, 255, 75 }, { 143, 255, 73 }, { 146, 255, 71 }, { 150, 254, 68 }, { 153, 254, 66 }, { 156, 254, 64 }, { 159, 253, 63 }, { 161, 253, 61 }, { 164, 252, 60 }, { 167, 252, 58 }, { 169, 251, 57 }, { 172, 251, 56 }, { 175, 250, 55 }, { 177, 249, 54 }, { 180, 248, 54 }, { 183, 247, 53 }, { 185, 246, 53 }, { 188, 245, 52 }, { 190, 244, 52 }, { 193, 243, 52 }, { 195, 241, 52 }, { 198, 240, 52 }, { 200, 239, 52 }, { 203, 237, 52 }, { 205, 236, 52 }, { 208, 234, 52 }, { 210, 233, 53 }, { 212, 231, 53 }, { 215, 229, 53 }, { 217, 228, 54 }, { 219, 226, 54 }, { 221, 224, 55 }, { 223, 223, 55 }, { 225, 221, 55 }, { 227, 219, 56 }, { 229, 217, 56 }, { 231, 215, 57 }, { 233, 213, 57 }, { 235, 211, 57 }, { 236, 209, 58 }, { 238, 207, 58 }, { 239, 205, 58 }, { 241, 203, 58 }, { 242, 201, 58 }, { 244, 199, 58 }, { 245, 197, 58 }, { 246, 195, 58 }, { 247, 193, 58 }, { 248, 190, 57 }, { 249, 188, 57 }, { 250, 186, 57 }, { 251, 184, 56 }, { 251, 182, 55 }, { 252, 179, 54 }, { 252, 177, 54 }, { 253, 174, 53 }, { 253, 172, 52 }, { 254, 169, 51 }, { 254, 167, 50 }, { 254, 164, 49 }, { 254, 161, 48 }, { 254, 158, 47 }, { 254, 155, 45 }, { 254, 153, 44 }, { 254, 150, 43 }, { 254, 147, 42 }, { 254, 144, 41 }, { 253, 141, 39 }, { 253, 138, 38 }, { 252, 135, 37 }, { 252, 132, 35 }, { 251, 129, 34 }, { 251, 126, 33 }, { 250, 123, 31 }, { 249, 120, 30 }, { 249, 117, 29 }, { 248, 114, 28 }, { 247, 111, 26 }, { 246, 108, 25 }, { 245, 105, 24 }, { 244, 102, 23 }, { 243, 99, 21 }, { 242, 96, 20 }, { 241, 93, 19 }, { 240, 91, 18 }, { 239, 88, 17 }, { 237, 85, 16 }, { 236, 83, 15 }, { 235, 80, 14 }, { 234, 78, 13 }, { 232, 75, 12 }, { 231, 73, 12 }, { 229, 71, 11 }, { 228, 69, 10 }, { 226, 67, 10 }, { 225, 65, 9 }, { 223, 63, 8 }, { 221, 61, 8 }, { 220, 59, 7 }, { 218, 57, 7 }, { 216, 55, 6 }, { 214, 53, 6 }, { 212, 51, 5 }, { 210, 49, 5 }, { 208, 47, 5 }, { 206, 45, 4 }, { 204, 43, 4 }, { 202, 42, 4 }, { 200, 40, 3 }, { 197, 38, 3 }, { 195, 37, 3 }, { 193, 35, 2 }, { 190, 33, 2 }, { 188, 32, 2 }, { 185, 30, 2 }, { 183, 29, 2 }, { 180, 27, 1 }, { 178, 26, 1 }, { 175, 24, 1 }, { 172, 23, 1 }, { 169, 22, 1 }, { 167, 20, 1 }, { 164, 19, 1 }, { 161, 18, 1 }, { 158, 16, 1 }, { 155, 15, 1 }, { 152, 14, 1 }, { 149, 13, 1 }, { 146, 11, 1 }, { 142, 10, 1 }, { 139, 9, 2 }, { 136, 8, 2 }, { 133, 7, 2 }, { 129, 6, 2 }, { 126, 5, 2 }, { 122, 4, 3 } };
			byte[] table = new byte[256];
			switch (channel)
			{
				case SKColorChannel.R:
					for (int i = 0; i < 256; i++)
					{
						table[i] = turboTable[i, 0];
					}
					break;
				case SKColorChannel.G:
					for (int i = 0; i < 256; i++)
					{
						table[i] = turboTable[i, 1];
					}
					break;
				case SKColorChannel.B:
					for (int i = 0; i < 256; i++)
					{
						table[i] = turboTable[i, 2];
					}
					break;
			}
			return table;
		}
		
		public void ShowMatrix(float[] a, string title = "matrix: ")
		{
			string s = "";
			int counter = 1;
            for (int i = 0; i < a.Length; i++)
            {
				s += $"{a[i]}, ";
				if (counter == 5)
				{ 
					s += "\n";
					counter = 0;
				}
				counter++;
			}
            //Console.WriteLine(s);
		}

		public void ShowTable(int[] a, string title)
		{
			string s = $"{title}:\n";
			for (int i = 0; i < a.Length; i+=1)
			{
				s += $"{i}>{a[i]}, ";
			}
			//Console.WriteLine(s);
		}
	}
}
