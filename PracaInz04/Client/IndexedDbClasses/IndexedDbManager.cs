using static PracaInz04.Client.IndexedDbClasses.IndexedDBModels;
using PracaInz04.Client.ImageProcessingClasses;

namespace PracaInz04.Client.IndexedDbClasses
{
    public class IndexedDbManager
    {
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
            ImageData imageData = GetImageData(imageArray, imageName, imageType, maxKey + 1);
            var addedResult = await IndexedDbContext.AddItems<ImageData>(new List<ImageData> { imageData });

            return addedResult;
        }

        public ImageData GetImageData(byte[] imageArr, string fileName, string imageType, int maxKey)
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
            ImageResized imageResized = GetImageResized(imageArray, imageName, imageType, maxKey + 1);
            var addedResult = await IndexedDbContext.AddItems<ImageResized>(new List<ImageResized> { imageResized });

            return addedResult;
        }

        public ImageResized GetImageResized(byte[] imageArr, string fileName, string imageType, int maxKey)
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
    }
}
