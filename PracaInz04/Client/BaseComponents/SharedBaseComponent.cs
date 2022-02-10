using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using PracaInz04.Client.Services;
using Blazored.LocalStorage;
using System;

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
