using PracaInz04.Client.ImageProcessingClasses;
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
            int maxKey = await IndexedDbContext.GetMaxKey<int, ImageData>();
            ImageResized imageResized = CreateImageResized(imageArray, imageName, imageType, maxKey + 1);
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
                ImageArray = ImageProc.ResizeSkia(imageArr),
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
    }
}
