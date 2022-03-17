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

namespace PracaInz04.Client.Services
{
    public class ImageProcessing
    {
		[Inject]
        public static IJSRuntime JS { get; private set; }

        public static byte[] ResizeImageSharp(byte[] imageArray, int width = 500)
		{
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

		public static byte[] ResizeSkia(byte[] imageArray)
		{
			// rotate on upload
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
			int width = 500;
			byte[] imageResult = new byte[] { };
			using (SKBitmap sourceBitmap = SKBitmap.Decode(imageArray))
			{
				int height = (sourceBitmap.Height * width) / sourceBitmap.Width;
				var resultInfo = new SKImageInfo(width, height);
				SKBitmap resultBitmap = sourceBitmap.Resize(resultInfo, SKFilterQuality.High);
				// v1
				//using(var ms = new MemoryStream())
				//{
				//	resultBitmap.Encode(ms, SKEncodedImageFormat.Png, 100);
				//	imageResult = ms.ToArray();
				//}
				// v2 - BEST
				var skData = resultBitmap.Encode(SKEncodedImageFormat.Png, 100);
				imageResult = skData.ToArray();
				//v3
				//using(SKImage skImage = SKImage.FromBitmap(resultBitmap))
				//{
				//	var skData = skImage.Encode(SKEncodedImageFormat.Png, 100);
				//	imageResult = skData.ToArray();
				//}
			}

			return imageResult;
		}

		public static byte[] RotateSkia(byte[] imageArray)
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

		public static async Task<string> GetImageURL(byte[] imageArray)
		{
			Console.WriteLine("start url");
			string imgSrc = string.Empty;
			//imgSrc = await JS.InvokeAsync<string>("URL.createObjectURL", imageArray);
			imgSrc = await JS.InvokeAsync<string>("createObjectURLFromBA", imageArray);
			Console.WriteLine("stop url");
			return imgSrc;
		}

		// slow
		public static string GetImageSrc(byte[] imageArray)
		{
			string imgSrc;
			using (Image image = Image.Load(imageArray, out var imageFormat))
			{
				imgSrc = image.ToBase64String(imageFormat);
				//imgSrc = $"data:{imageType};base64,{Convert.ToBase64String(imageArray)}";
			}
			return imgSrc;
		}

	}
}
