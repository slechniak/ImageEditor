using DnetIndexedDb;
using DnetIndexedDb.Models;
using Microsoft.JSInterop;
using DnetIndexedDb.Fluent;

namespace PracaInz04.Client.IndexedDbClasses
{
    public class IndexedDBModels
    {
        public class ImageCommon
        {
            [IndexDbKey(AutoIncrement = true)]
            public int? Id { get; set; }
            [IndexDbIndex]
            public string Name { get; set; }
            public byte[] Array { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
            public string Date { get; set; }
        }

        public class ImageResized2 : ImageCommon 
        {
            public int OriginalWidth { get; set; }
            public int OriginalHeight { get; set; }
            public int OriginalSize { get; set; }
        }

        public class ImageOriginal2 : ImageCommon { }


        public static IndexedDbDatabaseModel GetIndexedDbDatabaseModelAttributeBased()
        {
            var indexedDbDatabaseModel = new IndexedDbDatabaseModel()
                .WithName("TestAttributes")
                .WithVersion(1);

            indexedDbDatabaseModel.AddStore<ImageResized2>();
            indexedDbDatabaseModel.AddStore<ImageOriginal2>();

            return indexedDbDatabaseModel;
        }

        public class IndexedDbContext : IndexedDbInterop
        {
            public IndexedDbContext(IJSRuntime jsRuntime, IndexedDbOptions<IndexedDbContext> options) : base(jsRuntime, options)
            {

            }
        }
    }
}
