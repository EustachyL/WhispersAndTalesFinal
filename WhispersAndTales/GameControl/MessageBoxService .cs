using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhispersAndTales.Services
{
    public class MessageBoxService
    {
        private readonly Page _page;

        public MessageBoxService(Page page)
        {
            _page = page;
        }

        public async void ShowError(string message)
        {
            await _page.DisplayAlert("Error", message, "OK");
        }

        public async void ShowInfo(string message)
        {
            await _page.DisplayAlert("Info", message, "OK");
        }
    }

}
