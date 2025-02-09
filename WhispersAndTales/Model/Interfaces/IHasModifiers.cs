using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhispersAndTales.Model;

namespace WhispersAndTales.Model.Interfaces
{
    public interface IHasModifiers
    {
        List<Modifier> Modifiers { get; set; }
    }
}
