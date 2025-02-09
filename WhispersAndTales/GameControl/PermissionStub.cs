using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhispersAndTales.Services
{
    public class PermissionStub : IPermission
    {
        public Task<bool> CheckAndRequestExternalStoragePermission()
        {
            return Task.Run(() => true);
        }
    }
}
