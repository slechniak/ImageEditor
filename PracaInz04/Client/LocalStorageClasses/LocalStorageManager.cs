using Blazored.LocalStorage;
using PracaInz04.Client.Services;

namespace PracaInz04.Client.LocalStorageClasses
{
    public class LocalStorageManager
    {
        public ILocalStorageService LStorage{ get; set; }
        public StateService SService { get; set; }
        public string CountKey { get; set; } = "count";
        public string ImageNameKey { get; set; } = "imageName";
        //public Dictionary<string, string> keys = new(){
        //{"count", "count"},
        //{"imageName", "imageName"}
        //};

        public LocalStorageManager(ILocalStorageService ls, StateService sservice)
        {
            LStorage = ls;
            SService = sservice;
        }

        public async Task SaveImageName(string imageName)
        {
            await LStorage.SetItemAsync(ImageNameKey, imageName);
        }

        public async Task<string> GetImageName()
        {
            return await LStorage.GetItemAsync<string>(ImageNameKey);
        }

        //counter
        public async Task GetFromLocalStorage()
        {
            int? countValue;
            countValue = await LStorage.GetItemAsync<int>(CountKey);
            if (countValue is not null)
            {
                SService.Count = (int)countValue;
            }
            //Console.WriteLine($"GetFromLocalStorage, Count = {Count}");
        }

        public async Task SaveToLocalStorage()
        {
            await LStorage.SetItemAsync(CountKey, SService.Count);
        }
    }
}
