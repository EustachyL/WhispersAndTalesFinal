
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.FileBuilders;

namespace WhispersAndTales.Model.Interfaces
{
    public interface ITaged
    {
        string Tag { get; set; }
        Type Type { get; }
        IXmlSubElementParser<BaseObject> BuildFromElement(XElement rootElement);
    }
}
