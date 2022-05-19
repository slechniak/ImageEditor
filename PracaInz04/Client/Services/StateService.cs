using System;
using System.IO;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Forms;
using SkiaSharp;

namespace PracaInz04.Client.Services
{
    public class StateService
    {
        public byte[] ImageArray { get; set; }
        public string ImageName { get; set; }
        public SKBitmap bitmap { get; set; }

        // old
        public int Count { get; set; }
        public string countKey = "count";

        public IBrowserFile imageFile { get; set; }
        public Stream imageStream { get; set; }
        public byte[] imageArray { get; set; }
        public string imgUrl { get; set; } = string.Empty;
        //public Image imageResult { get; set; }

        public ILocalStorageService localStorage;

        public event Action OnChange;

        public StateService(ILocalStorageService ls)
        {
            localStorage = ls;
        }

        public void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }

        //public async Task GetFromLocalStorage()
        //{
        //    int? countValue;
        //    countValue = await localStorage.GetItemAsync<int>(countKey);
        //    if (countValue is not null)
        //    {
        //        Count = (int)countValue;
        //    }
        //    //Console.WriteLine($"GetFromLocalStorage, Count = {Count}");
        //}

        //public async Task SaveToLocalStorage()
        //{
        //    await localStorage.SetItemAsync(countKey, Count);
        //}

        //public async Task LoadImage()
        //{
        //    imageResult = Image.Load(imageArray);
        //}
    }
}
