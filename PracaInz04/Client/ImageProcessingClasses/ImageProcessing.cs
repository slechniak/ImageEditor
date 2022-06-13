using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PracaInz04.Client.Services;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SkiaSharp;
using System;
using System.Collections.Generic;
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
			Contrast
		}

		StateService SService { get; set; }
		public IJSRuntime JS { get; set; }
		public int LongerResized { get; set; } = 1000;

		byte minValue, maxValue;
		byte[] histogram;
		SKBitmap bitmap2;
		byte[] pixelValues;

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
				Console.WriteLine($"{image.Width} -> {width}");
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
				Console.WriteLine($"ResizeSkiaCommon resized w, h: {resultBitmap.Width}, {resultBitmap.Height}");
				return SKBitmapToArray(resultBitmap);
			}
			else
			{
				Console.WriteLine($"ResizeSkiaCommon NOT resized w, h: {sourceBitmap.Width}, {sourceBitmap.Height}");
				return SKBitmapToArray(sourceBitmap); 
			}
		}

		public SKBitmap ResizeSKBitmap(SKBitmap bitmap, int? width = null, int? height = null)
		{
			var resultInfo = GetResizedImageInfo(bitmap, width, height);
			Console.WriteLine($"resultInfo: w: {resultInfo.Width}, h: {resultInfo.Height}");
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
			if (SService.newHistogram)
			{
				//SKColor[] pixels = bitmap.Pixels;
				pixelsH = new float[pixels.Length];
				pixelsS = new float[pixels.Length];
				pixelsL = new float[pixels.Length];
				for (int i = 0; i < pixels.Length; i++)
				{
					//pixels[i].ToHsl(out pixelsH[i], out pixelsS[i], out pixelsL[i]);
					pixels[i].ToHsv(out pixelsH[i], out pixelsS[i], out pixelsL[i]);
				}
				minL = pixelsL.Min();
				maxL = pixelsL.Max();
				Console.WriteLine($"minL: {minL}, maxL: {maxL}");
				SService.newHistogram = false;
			}
			float oldL, newL;

			float oldMin = minL;
			float oldMax = maxL;
			float newMin = minL - factor;
			float newMax = maxL + factor;
			Console.WriteLine($"oldMin: {oldMin}, oldMax: {oldMax}");
			Console.WriteLine($"newMin: {newMin}, newMax: {newMax}, factor: {factor}");
			// dla ujemnych wartosci factor
			newMin = Math.Min(newMin, newMax);
			newMax = Math.Max(newMax, newMin);
			for (int i = 0; i < pixelsL.Length; i++)
			{
				oldL = pixelsL[i];
				newL = ((oldL - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
				newL = Math.Clamp(newL, 0f, 100f);
				//pixels[i] = SKColor.FromHsl(pixelsH[i], pixelsS[i], newL);
				pixels[i] = SKColor.FromHsv(pixelsH[i], pixelsS[i], newL);
				if (i % 10000 == 0)
					Console.WriteLine($"oldL: {oldL}, newL: {newL}");
			}
			result.Pixels = pixels;
			return result;
		}

		public SKBitmap FilterContrastGrayscale(SKBitmap bitmap, int factor)
		{
			//bitmap2 = bitmap;
			if (SService.newHistogram)
			{
				bitmap2 = FilterGrayscale(bitmap);
				pixelValues = bitmap2.Pixels.Select(x => x.Red).ToArray();
			}
			byte[] table = ContrastTable2(factor, pixelValues);
			ShowTable(table, "contrast table");
			return ApplyFilter(bitmap2, SKColorFilter.CreateTable(
				null, table, table, table));
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

		public byte[] ContrastTable2(int factor, byte[] pixels)
		{
			if (SService.newHistogram)
			{
				minValue = pixels.Min();
				maxValue = pixels.Max();
				histogram = GetHistogram2(pixels);
				SService.newHistogram = false;
				Console.WriteLine($"min:{minValue}, max:{maxValue}");
				ShowTable(histogram, "histogram");
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
			// dla ujemnych factor
			newMin = Math.Min(newMin, newMax);
			newMax = Math.Max(newMax, newMin);

			for (int i = (int)oldMin; i <= oldMax; i++)
			{
				float newValue = ((i - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
				table[i] = (byte)Math.Clamp(newValue, 0, 255);
			}
			
			return table;
		}

		public byte[] GetHistogram2(byte[] pixels)
		{
			byte[] histogram = new byte[256];
			for (int i = 0; i < pixels.Length; i++)
			{
				histogram[pixels[i]] += 1;
			}
			return histogram;
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

		public void ShowTable(byte[] a, string title)
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
