using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhispersAndTales.Dictionary;

namespace WhispersAndTales.Model.Interfaces
{
    public interface IHasProperties
    {
        Dictionary<string, Property> Properties { get; set; }
    }
}
