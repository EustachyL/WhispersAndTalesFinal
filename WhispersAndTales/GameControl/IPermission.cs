using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhispersAndTales.Services
{
    public interface IPermission
    {
        Task<bool> CheckAndRequestExternalStoragePermission();
    }
}
