using PracaInz04.Client.ImageProcessingClasses;
using SkiaSharp;
using static PracaInz04.Client.IndexedDbClasses.IndexedDBModels;

namespace PracaInz04.Client.IndexedDbClasses 
{
    public class IndexedDbManager
    {
        public string ImageNameIndex { get; set; } = "imageName";
        public IndexedDbContext IndexedDbContext { get; set; }
        public ImageProcessing ImageProc { get; set; }
        public IndexedDbManager(IndexedDbContext iDbContext, ImageProcessing imageProc)
        {
            IndexedDbContext = iDbContext;
            ImageProc = imageProc;
        }
        
        public async Task<List<TEntity>> GetByIndex<TKey, TEntity>(TKey indexValue, string dbIndex)
        {
            return await IndexedDbContext.GetByIndex<TKey, TEntity>(indexValue, indexValue, dbIndex, false);
        }

        public async Task<int> AddImageToIDb(byte[] imageArray, string imageName)
        {
            string CurrentDate = DateTime.Now.ToString();
            int maxKey = await IndexedDbContext.GetMaxKey<int, ImageOriginal2>();
            SKBitmap bitmap = SKBitmap.Decode(imageArray);
            ImageOriginal2 imageOriginal2 = new ImageOriginal2()
            {
                Id = maxKey + 1,
                Array = imageArray,
                Name = imageName,
                Width = bitmap.Width,
                Height = bitmap.Height,
                Date = CurrentDate
            };
            var addedResult = await IndexedDbContext.AddItems<ImageOriginal2>(new List<ImageOriginal2> { imageOriginal2 });

            byte[] resizedArray = ImageProc.ResizeSkiaDefaut(bitmap);
            SKBitmap resizedBitmap = SKBitmap.Decode(resizedArray);
            ImageResized2 imageResized2= new ImageResized2()
            {
                Id = maxKey + 1,
                Array = resizedArray,
                Name = imageName,
                Width = resizedBitmap.Width,
                Height = resizedBitmap.Height,
                OriginalWidth = bitmap.Width,
                OriginalHeight = bitmap.Height,
                OriginalSize = imageArray.Length,
                Date = CurrentDate
            };
            var addedResult2 = await IndexedDbContext.AddItems<ImageResized2>(new List<ImageResized2> { imageResized2 });

            return maxKey + 1;
        }

        public async Task DeleteImageFromIDb(int imageId)
        {
            var deleteResult = await IndexedDbContext.DeleteByKey<int, ImageResized2>(imageId);
            var deleteResult2 = await IndexedDbContext.DeleteByKey<int, ImageOriginal2>(imageId);
        }

        public async Task<ImageResized2> FetchImageResized2(int imageId)
        {
            var result = await IndexedDbContext.GetByKey<int, ImageResized2>(imageId);
            return result;
        }

        public async Task<ImageOriginal2> FetchImageOriginal2(int imageId)
        {
            var result = await IndexedDbContext.GetByKey<int, ImageOriginal2>(imageId);
            return result;
        }

        public async Task UpdateIDb2(SKBitmap sKBitmap, int imageId, string imageName)
        {
            string CurrentDate = DateTime.Now.ToString();
            byte[] sKBitmapArray = ImageProc.SKBitmapToArray(sKBitmap);
            var imageOriginal2 = new ImageOriginal2()
            {
                Id = imageId,
                Array = sKBitmapArray,
                Name = imageName,
                Width = sKBitmap.Width,
                Height = sKBitmap.Height,
                Date = CurrentDate
            };
            byte[] resizedArray = ImageProc.ResizeSkiaDefaut(sKBitmap);
            SKBitmap resizedBitmap = SKBitmap.Decode(resizedArray);
            var imageResized2 = new ImageResized2()
            {
                Id = imageId,
                Array = resizedArray,
                Name = imageName,
                Width = resizedBitmap.Width,
                Height = resizedBitmap.Height,
                OriginalWidth = sKBitmap.Width,
                OriginalHeight = sKBitmap.Height,
                OriginalSize = sKBitmapArray.Length,
                Date = CurrentDate
            };
            var openDbResult = await IndexedDbContext.OpenIndexedDb();
            var result = await IndexedDbContext.UpdateItems<ImageOriginal2>(
                                new List<ImageOriginal2>() { imageOriginal2 });
            var result2 = await IndexedDbContext.UpdateItems<ImageResized2>(
                                new List<ImageResized2>() { imageResized2 });
        }
    }
}
