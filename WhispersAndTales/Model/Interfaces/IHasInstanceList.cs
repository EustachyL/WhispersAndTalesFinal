using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WhispersAndTales.Model.Interfaces
{
    public interface IHasInstanceList<T>
    {
        List<T> GetInstanceList();
    }
}
