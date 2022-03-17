using DnetIndexedDb;
using DnetIndexedDb.Models;
using Microsoft.JSInterop;
using DnetIndexedDb.Fluent;
using Microsoft.AspNetCore.Components;

namespace PracaInz04.Client.IndexedDBModels
{
    public class IndexedDBModels
    {
        [Inject]
        public static IndexedDbContext indexedDbContext { get; set; }
        //public class TableFieldDto
        //{
        //    [IndexDbKey(AutoIncrement = true)]
        //    public int? TableFieldId { get; set; }
        //    [IndexDbIndex]
        //    public string TableName { get; set; }
        //    [IndexDbIndex]
        //    public int Width { get; set; }
        //}
        public class ImageDto
        {
            [IndexDbKey(AutoIncrement = true)]
            public int? ImageId { get; set; }

            [IndexDbIndex]
            public string ImageName { get; set; }

            public byte[] ImageArray { get; set; }
            public string ImageType { get; set; }
        }

        public class ImageInfo
        {
            [IndexDbKey(AutoIncrement = true)]
            public int? ImageId { get; set; }

            [IndexDbIndex]
            public string ImageName { get; set; }

            public string  Thumbnail { get; set; }
        }

        public class ImageResized
        {
            [IndexDbKey(AutoIncrement = true)]
            public int? ImageId { get; set; }

            [IndexDbIndex]
            public string ImageName { get; set; }

            public byte[] ImageArray { get; set; }
            public string ImageType { get; set; }
        }

        //stores original image and additional info
        public class ImageData
        {
            [IndexDbKey(AutoIncrement = true)]
            public int? ImageId { get; set; }

            [IndexDbIndex]
            public string ImageName { get; set; }

            public byte[] ImageArray { get; set; }
            public string ImageType { get; set; }
        }

        public static IndexedDbDatabaseModel GetGridColumnDatabaseModelAttributeBased()
        {
            var indexedDbDatabaseModel = new IndexedDbDatabaseModel()
                .WithName("TestAttributes")
                .WithVersion(1);

            indexedDbDatabaseModel.AddStore<ImageDto>();
            indexedDbDatabaseModel.AddStore<ImageInfo>();
            indexedDbDatabaseModel.AddStore<ImageResized>();
            indexedDbDatabaseModel.AddStore<ImageData>();

            return indexedDbDatabaseModel;
        }

        //previously IndexedDbContext (Grid Column Data IndexedDb2)
        public class IndexedDbContext : IndexedDbInterop
        {
            public IndexedDbContext(IJSRuntime jsRuntime, IndexedDbOptions<IndexedDbContext> options) : base(jsRuntime, options)
            {

            }
        }

        // add ImageData
        public static async Task<string> AddImageDataToIDb(byte[] imageArray, string imageName, string imageType)
        {
            int maxKey = await indexedDbContext.GetMaxKey<int, ImageData>();
            ImageData imageData = GetImageData(imageArray, imageName, imageType, maxKey + 1);
            var addedResult = await indexedDbContext.AddItems<ImageData>(new List<ImageData> { imageData });

            return addedResult;
        }

        public static ImageData GetImageData(byte[] imageArr, string fileName, string imageType, int maxKey)
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
        public static async Task<string> AddImageResizedToIDb(byte[] imageArray, string imageName, string imageType)
        {
            int maxKey = await indexedDbContext.GetMaxKey<int, ImageData>();
            ImageResized imageResized = GetImageResized(imageArray, imageName, imageType, maxKey + 1);
            var addedResult = await indexedDbContext.AddItems<ImageResized>(new List<ImageResized> { imageResized });

            return addedResult;
        }

        public static ImageResized GetImageResized(byte[] imageArr, string fileName, string imageType, int maxKey)
        {
            var item = new ImageResized
            {
                ImageId = maxKey,
                ImageName = fileName,
                ImageArray = imageArr,
                ImageType = imageType
            };
            return item;
        }
    }
}
