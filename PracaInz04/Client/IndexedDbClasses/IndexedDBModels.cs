using DnetIndexedDb;
using DnetIndexedDb.Models;
using Microsoft.JSInterop;
using DnetIndexedDb.Fluent;

namespace PracaInz04.Client.IndexedDbClasses
{
    public class IndexedDBModels
    {
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

        public static IndexedDbDatabaseModel GetIndexedDbDatabaseModelAttributeBased()
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
    }
}
