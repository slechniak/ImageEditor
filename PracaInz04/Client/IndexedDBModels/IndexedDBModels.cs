using DnetIndexedDb;
using DnetIndexedDb.Models;
using Microsoft.JSInterop;
using DnetIndexedDb.Fluent;

namespace PracaInz04.Client.IndexedDBModels
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

        public static IndexedDbDatabaseModel GetGridColumnDatabaseModelAttributeBased()
        {
            var indexedDbDatabaseModel = new IndexedDbDatabaseModel()
                .WithName("TestAttributes")
                .WithVersion(1);

            indexedDbDatabaseModel.AddStore<ImageDto>();
            indexedDbDatabaseModel.AddStore<ImageInfo>();

            return indexedDbDatabaseModel;
        }

        public class GridColumnDataIndexedDb2 : IndexedDbInterop
        {
            public GridColumnDataIndexedDb2(IJSRuntime jsRuntime, IndexedDbOptions<GridColumnDataIndexedDb2> options) : base(jsRuntime, options)
            {

            }
        }
    }
}
