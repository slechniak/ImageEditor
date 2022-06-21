using Microsoft.AspNetCore.Components;
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
			EqualizeHistogram,
			StretchHistogram
		}

		StateService SService { get; set; }
		public IJSRuntime JS { get; set; }
		public int LongerResized { get; set; } = 1000;

		byte minValue, maxValue;
		//byte[] histogram;
		//int[] histogram;
		SKBitmap bitmap2;
		//byte[] pixelValues;
		IEnumerable<byte> pixelValues;

		float[] pixelsH, pixelsS, pixelsL;
		float minL, maxL;

		public ImageProcessing(IJSRuntime jSRuntime, StateService sService)
		{
			JS = jSRuntime;
			SService = sService;
		}

		public byte[] ResizeImageSharp(byte[] imageArray, int width = 500)
		{
			// rotate on upload - comment was in ResizeSkia byte[]
			//using(Image image = Image.Load(imageArray, out var imageFormat))
			//{
			//	image.Mutate(x => x.AutoOrient());
			//	using (var ms = new MemoryStream())
			//	{
			//		image.Save(ms, imageFormat);
			//		imageArray = ms.ToArray();
			//	}
			//}
			//

			byte[] imageResult = new byte[] { };
			using (Image image = Image.Load(imageArray, out var imageFormat))
			{
				image.Mutate(x => x.AutoOrient());
				//int height = (image.Height * width) / image.Width;
				//image.Mutate(x => x.Resize(width, height).AutoOrient());
				//Console.WriteLine($"{image.Width} -> {width}");
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
			//int height = (sourceBitmap.Height * width) / sourceBitmap.Width;
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
				//Console.WriteLine($"ResizeSkiaCommon resized w, h: {resultBitmap.Width}, {resultBitmap.Height}");
				return SKBitmapToArray(resultBitmap);
			}
			else
			{
				//Console.WriteLine($"ResizeSkiaCommon NOT resized w, h: {sourceBitmap.Width}, {sourceBitmap.Height}");
				return SKBitmapToArray(sourceBitmap); 
			}
		}

		public SKBitmap ResizeSKBitmap(SKBitmap bitmap, int? width = null, int? height = null)
		{
			var resultInfo = GetResizedImageInfo(bitmap, width, height);
			//Console.WriteLine($"resultInfo: w: {resultInfo.Width}, h: {resultInfo.Height}");
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
			//imgSrc = await JS.InvokeAsync<string>("URL.createObjectURL", imageArray);
			imgSrc = await JS.InvokeAsync<string>("createObjectURLFromBA", imageArray);
			return imgSrc;
		}

		public byte[] SKBitmapToArray(SKBitmap bitmap)
		{
			var skData = bitmap.Encode(SKEncodedImageFormat.Png, 100);
			var bitmapArray = skData.ToArray();
			return bitmapArray;
		}

		public SKBitmap ApplyFilter(SKBitmap bitmap, SKColorFilter colorFilter)
        {
			SKBitmap result = new SKBitmap(bitmap.Info);
			using (SKCanvas canvas = new SKCanvas(result))
			{
				SKPaint paint = new SKPaint()
				{
					ColorFilter = colorFilter
				};
				canvas.DrawBitmap(bitmap, result.Info.Rect, paint);
			}
			return result;
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
			//float[] result = BrightnessMatrix(level);
			float[] result = ColorMatrix(shift, channel);
			ShowMatrix(result);
			return ApplyFilter(bitmap, SKColorFilter.CreateColorMatrix(
				result));
		}

		public SKBitmap ChangeColorMulti(SKBitmap bitmap, int shift, bool isRed, bool isGreen, bool isBlue)
		{
			//float[] result = BrightnessMatrix(level);
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
				//SKColor[] pixels = bitmap.Pixels;
				pixelsH = new float[pixels.Length];
				pixelsS = new float[pixels.Length];
				pixelsL = new float[pixels.Length];
				for (int i = 0; i < pixels.Length; i++)
				{
					pixels[i].ToHsl(out pixelsH[i], out pixelsS[i], out pixelsL[i]);
                    //pixels[i].ToHsv(out pixelsH[i], out pixelsS[i], out pixelsL[i]);
                }
				minL = pixelsL.Min();
				maxL = pixelsL.Max();
				//Console.WriteLine($"minL: {minL}, maxL: {maxL}");
				SService.newBitmap = false;
			}
			float oldL, newL;

			float oldMin = minL;
			float oldMax = maxL;
			float newMin = minL - factor;
			float newMax = maxL + factor;
			//Console.WriteLine($"oldMin: {oldMin}, oldMax: {oldMax}");
			//Console.WriteLine($"newMin: {newMin}, newMax: {newMax}, factor: {factor}");
			// dla ujemnych wartosci factor
			newMin = Math.Min(newMin, newMax);
			newMax = Math.Max(newMax, newMin);
			float newRange = newMax - newMin;
			float offset = (100 - newRange) / 2f;
			for (int i = 0; i < pixelsL.Length; i++)
			{
				oldL = pixelsL[i];
				//newL = ((oldL - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
				newL = ((oldL - oldMin) / (oldMax - oldMin)) * newRange + offset;
				newL = Math.Clamp(newL, 0f, 100f);
				pixels[i] = SKColor.FromHsl(pixelsH[i], pixelsS[i], newL);
                //pixels[i] = SKColor.FromHsv(pixelsH[i], pixelsS[i], newL);
                //if (i % 10000 == 0)
                //	Console.WriteLine($"oldL: {oldL}, newL: {newL}");
            }
			result.Pixels = pixels;
			return result;
		}

		public SKBitmap FilterContrastGrayscale(SKBitmap bitmap, int factor)
		{
			//bitmap2 = bitmap;
			if (SService.newBitmap)
			{
				bitmap2 = FilterGrayscale(bitmap);
                //pixelValues = bitmap2.Pixels.Select(x => x.Red).ToArray();
                pixelValues = bitmap2.Pixels.Select(x => x.Red);
			}
			//byte[] table = ContrastTable1(factor, pixelValues);
			//byte[] table = ContrastTable2(factor, pixelValues);
			//byte[] table = ContrastTable3(factor, pixelValues);
			byte[] table = ContrastTable4(factor, pixelValues);
            //ShowTable(table.Select(x => (int)x).ToArray(), "contrast table");
            return ApplyFilter(bitmap2, SKColorFilter.CreateTable(
				null, table, table, table));
		}

		public SKBitmap EqualizeHistogram(SKBitmap bitmap)
        {
			bitmap2 = FilterGrayscale(bitmap);
			IEnumerable<byte> pixelList = bitmap2.Pixels.Select(x => x.Red);
			byte[] table = EqualizedHistogramTable(pixelList);
			//ShowTable(table.Select(x => (int)x).ToArray(), "EqualizeHistogram table");
			return ApplyFilter(bitmap2, SKColorFilter.CreateTable(
				null, table, table, table));
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

		public float[] ColorMatrix(int shift, SKColorChannel channel)
		{
			// fifth column needs values from 0-1, shift should be (-255)-255 
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
			// fifth column needs values from 0-1, shift should be (-255)-255 
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

		// casts to (oldmin-f ; oldmax+f)
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
			// dla ujemnych wartosci factor
			newMin = Math.Min(newMin, newMax);
			newMax = Math.Max(newMax, newMin);
			float newRange = newMax - newMin;

			for (int i = (int)oldMin; i <= oldMax; i++)
			{
				//float newValue = ((i - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
				double newValue = Math.Round(((i - oldMin) / (oldMax - oldMin)) * newRange + newMin);
				table[i] = (byte)Math.Clamp(newValue, 0, 255);
			}

			return table;
		}

		// centers destination range around 128
		public byte[] ContrastTable2(int factor, IEnumerable<byte> pixels)
		{
			if (SService.newBitmap)
			{
				minValue = pixels.Min();
				maxValue = pixels.Max();
				//histogram = GetHistogram2(pixels);
				SService.newBitmap = false;
				//Console.WriteLine($"min:{minValue}, max:{maxValue}");
				//ShowTable(histogram, "histogram");
			}
			//byte lower = (byte)(minValue - factor);
			//byte upper = (byte)(maxValue + factor);
			byte[] table = new byte[256];
            for (int i = 0; i < table.Length; i++)
            {
				table[i] = (byte)i;
            }

			float oldMin = minValue;
			float oldMax = maxValue;
			float newMin = minValue - factor;
			float newMax = maxValue + factor;
			// dla ujemnych wartosci factor
			newMin = Math.Min(newMin, newMax);
			newMax = Math.Max(newMax, newMin);
			float newRange = newMax - newMin;
			float offset = (255 - newRange) / 2f;
			// dla ujemnych factor
			newMin = Math.Min(newMin, newMax);
			newMax = Math.Max(newMax, newMin);
			

			for (int i = (int)oldMin; i <= oldMax; i++)
			{
				//float newValue = ((i - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
				float newValue = ((i - oldMin) / (oldMax - oldMin)) * newRange + offset;
				table[i] = (byte)Math.Clamp(newValue, 0, 255);
			}
			
			return table;
		}

		// casts to max(oldmin-f,0):min(oldmax+f,255)
		public byte[] ContrastTable3(int factor, IEnumerable<byte> pixels)
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
			float newMin = Math.Max(minValue - factor, 0);
			float newMax = Math.Min(maxValue + factor, 255);
			// dla ujemnych wartosci factor
			newMin = Math.Min(newMin, newMax);
			newMax = Math.Max(newMax, newMin);
			float newRange = newMax - newMin;

			for (int i = (int)oldMin; i <= oldMax; i++)
			{
				//float newValue = ((i - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
				double newValue = Math.Round(((i - oldMin) / (oldMax - oldMin)) * newRange + newMin);
				//table[i] = (byte)Math.Clamp(newValue, 0, 255);
				table[i] = (byte)newValue;
			}

			return table;
		}

		// should be percentiles 5,95 instead of oldmin, oldmax
		// casts to max(oldmin-f,0):min(oldmax+f,255), then if (newmin==0 && newmax==255) casts to (oldmin-f ; oldmax+f)
		public byte[] ContrastTable4(int factor, IEnumerable<byte> pixels)
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
			if (newMin >= 0 || newMax <= 255)
			{
				newMin = Math.Max(newMin, 0);
				newMax = Math.Min(newMax, 255);
			}
			// dla ujemnych wartosci factor
			newMin = Math.Min(newMin, newMax);
			newMax = Math.Max(newMax, newMin);
			float newRange = newMax - newMin;

			for (int i = (int)oldMin; i <= oldMax; i++)
			{
				//float newValue = ((i - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
				double newValue = Math.Round(((i - oldMin) / (oldMax - oldMin)) * newRange + newMin);
				table[i] = (byte)Math.Clamp(newValue, 0, 255);
            }

			return table;
		}

		//percentiles
		// fivepercent = 0.05*pixels.length;
		// for(int i=0; i<histogram.length; i++)
		// {
		//	fivepercent -= histogram[i];
		//  if(fivepercent<=0)
		//  percentile05 = i;
		// }
		//
		// for(int i=histogram.length-1; i>=0; i--)
		// {
		//	fivepercent -= histogram[i];
		//  if(fivepercent<=0)
		//  percentile95 = i;
		// }

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

			//Stopwatch stopwatch = new Stopwatch();
			//stopwatch.Start();

			int[] array = new int[256];
			// fastest - around 110ms for cat01 500.jpg red (List<byte> pixelsList)
			foreach (var pixel in pixelsList)
			{
				array[pixel]++;
			}
			return array;
			// fast - around 210ms for cat01 500.jpg red (IEnumerable<byte> pixels)
			//foreach(var pixel in pixels)
			//{
			//    array[pixel]++;
			//}
			// VERY slow, both
			//for (int i = 0; i < pixels.Count(); i++)
			//{
			//    Console.WriteLine(i);
			//    array[pixels.ElementAt(i)]++;
			//    array[pixelsList[i]]++;
			//}
			//stopwatch.Stop();
			//Console.WriteLine("array2: {0} ms", stopwatch.ElapsedMilliseconds);
		}

		//public byte ByteRange(int number)
		//      {
		//	return (byte)Math.Clamp(number, 0, 255);
		//      }

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

		public void ShowMatrix(float[] a, string title = "matrix title")
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
            Console.WriteLine(s);
		}

		public void ShowTable(int[] a, string title)
		{
			string s = $"{title}:\n";
			for (int i = 0; i < a.Length; i+=1)
			{
				s += $"{i}>{a[i]}, ";
			}
			Console.WriteLine(s);
		}
	}
}
