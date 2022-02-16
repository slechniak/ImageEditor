using Microsoft.AspNetCore.Components;
using PracaInz04.Client.Services;

namespace PracaInz04.Client.BaseComponents
{
    public class SharedBaseComponent : ComponentBase
    {
        [Inject]
        protected StateService SService { get; set; }
        //[Inject]
        //protected ILocalStorageService LocalStorage { get; set; }

        
    }
}
