using System;
using System.IO;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components.Forms;
using PracaInz04.Client.Controls;
using SkiaSharp;

namespace PracaInz04.Client.Services
{
    public class StateService
    {
        private SKBitmap _bitmap;
        public SKBitmap bitmap 
        {
            get 
            { 
                return _bitmap;
            }
            set
            {
                _bitmap = value;
                newBitmap = true;
                RedrawHistogram?.Invoke();
            }
        }

        public SKBitmap originalBitmap { get; set; }

        public int Count { get; set; }
        public string countKey = "count";

        
        public bool newBitmap = false;

        public ILocalStorageService localStorage;

        public event Action OnChange;

        public event Func<Task> RedrawHistogram;

        public bool isRed = false;
        public bool isGreen = false;
        public bool isBlue = false;

        public StateService(ILocalStorageService ls)
        {
            localStorage = ls;
        }

        public void SubscribeToRedrawHistogram(Func<Task> handler)
        {
            RedrawHistogram = null;
            RedrawHistogram += handler;
        }

        public void NotifyStateChanged()
        {
            OnChange?.Invoke();
        }

        public async Task Progress(IModalService Modal, Func<Task> method, string message = "Progress...")
        {
            var options = new ModalOptions()
            {
                HideCloseButton = true
            };
            var formModal = Modal.Show<ProgressComponent>(message, options);
            await Task.Delay(1);
            await method();
            formModal.Close();
            await Task.Delay(1);
        }

    }
}
