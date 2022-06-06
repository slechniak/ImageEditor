using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
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
			Binary
		}

		public IJSRuntime JS { get; set; }
		public int LongerResized { get; set; } = 1000;
		
		public ImageProcessing(IJSRuntime jSRuntime)
		{
			JS = jSRuntime;
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

		public SKBitmap FilterBitmap(SKBitmap bitmap, FilterType filterType, params object[] args)
        {
			switch(filterType)
            {
				case FilterType.Grayscale:
					return FilterGrayscale(bitmap);
				case FilterType.Binary:
					return FilterBinary(bitmap, (int)args[0]);
				default:
					return bitmap;
            }
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

		public byte[] BinaryTable(int treshold)
		{
			byte[] table = new byte[256];
            for (int i = 0; i < treshold; i++)
            {
                table[i] = 0;
            }
			for (int i = treshold; i < 256; i++)
			{
				table[i] = 255;
			}

			return table;
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

		//public SKBitmap FilterGrayscale1(SKBitmap bitmap)
		//{
		//	SKBitmap result = new SKBitmap(bitmap.Info);
		//	using (SKCanvas canvas = new SKCanvas(result))
		//	{
		//		SKPaint paint = new SKPaint()
		//		{
		//			ColorFilter = SKColorFilter.CreateColorMatrix(GrayscaleMatrix())
		//	};
		//		canvas.DrawBitmap(bitmap, result.Info.Rect, paint);
		//	}
		//	return result;
		//}

		//public SKBitmap FilterBinary1(SKBitmap bitmap)
		//{
		//	SKBitmap result = new SKBitmap(bitmap.Info);
		//	using (SKCanvas canvas = new SKCanvas(result))
		//	{
		//		SKPaint paint = new SKPaint()
		//		{
		//			ColorFilter = SKColorFilter.CreateTable(BinaryTable(128))
		//		};
		//		SKBitmap grayscaleBitmap = FilterGrayscale(bitmap);
		//		canvas.DrawBitmap(grayscaleBitmap, result.Info.Rect, paint);
		//	}
		//	return result;
		//}
	}
}
