using Microsoft.AspNetCore.Components;

namespace PracaInz04.Client.BaseComponents
{
    public class ImageBaseComponent : ComponentBase
    {
        public string Image { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            Image = "image.png";
        }
    }
}
