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
        
        // add ImageData
        public async Task<string> AddImageDataToIDb(byte[] imageArray, string imageName, string imageType)
        {
            int maxKey = await IndexedDbContext.GetMaxKey<int, ImageData>();
            ImageData imageData = CreateImageData(imageArray, imageName, imageType, maxKey + 1);
            Console.WriteLine($"AddImageDataToIDb ImageArray.Length: {imageData.ImageArray.Length}");
            var addedResult = await IndexedDbContext.AddItems<ImageData>(new List<ImageData> { imageData });

            return addedResult;
        }

        //should be in idbmodels class?
        public ImageData CreateImageData(byte[] imageArr, string fileName, string imageType, int maxKey)
        {
            var item = new ImageData
            {
                ImageId = maxKey,
                ImageName = fileName,
                ImageArray = imageArr,
                ImageType = imageType
            };
            return item;
        }

        // add ImageResized
        public async Task<string> AddImageResizedToIDb(byte[] imageArray, string imageName, string imageType)
        {
            // shouldnt it be ImageResized?
            //int maxKey = await IndexedDbContext.GetMaxKey<int, ImageData>();
            int maxKey = await IndexedDbContext.GetMaxKey<int, ImageResized>();
            ImageResized imageResized = CreateImageResized(imageArray, imageName, imageType, maxKey + 1);
            Console.WriteLine($"AddImageResizedToIDb ImageArray.Length: {imageResized.ImageArray.Length}");
            var addedResult = await IndexedDbContext.AddItems<ImageResized>(new List<ImageResized> { imageResized });

            return addedResult;
        }

        //should be in idbmodels class?
        public ImageResized CreateImageResized(byte[] imageArr, string fileName, string imageType, int maxKey)
        {
            var item = new ImageResized
            {
                ImageId = maxKey,
                ImageName = fileName,
                ImageArray = ImageProc.ResizeSkiaDefaut(imageArr),
                ImageType = imageType
            };
            return item;
        }

        // should be an idbcontext extension + not used yet
        public async Task<List<TEntity>> GetByIndex<TKey, TEntity>(TKey indexValue, string dbIndex)
        {
            return await IndexedDbContext.GetByIndex<TKey, TEntity>(indexValue, indexValue, dbIndex, false);
        }
        public async Task<ImageResized> FetchImageResized(string imageName)
        {
            var result = await IndexedDbContext.GetByIndex<string, ImageResized>(imageName, null, ImageNameIndex, false);
            //var result = await this.GetByIndex<string, ImageResized>(imageName, ImageNameIndex);
            return result.First();
        }

        public async Task<ImageData> FetchImageData(string imageName)
        {
            var result = await IndexedDbContext.GetByIndex<string, ImageData>(imageName, null, ImageNameIndex, false);
            //var result = await this.GetByIndex<string, ImageResized>(imageName, ImageNameIndex);
            return result.First();
        }

        public async Task UpdateIDb(SKBitmap sKBitmap, ImageData imageData, ImageResized imageResized)
        {
            byte[] sKBitmapArray = ImageProc.SKBitmapToArray(sKBitmap);
            imageData.ImageArray = sKBitmapArray;
            var openDbResult = await IndexedDbContext.OpenIndexedDb();
            var result = await IndexedDbContext.UpdateItems<ImageData>(
                                new List<ImageData>() { imageData });

            imageResized.ImageArray = ImageProc.ResizeSkiaDefaut(sKBitmap);

            var result2 = await IndexedDbContext.UpdateItems<ImageResized>(
                                new List<ImageResized>() { imageResized });
        }
    }
}
