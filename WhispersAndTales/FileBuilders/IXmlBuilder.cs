using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.Model.Interfaces;

namespace WhispersAndTales.FileBuilders
{
    public interface IXmlSubElementParser<out T> where T : BaseObject
    {
        T Build();
        void Parse(XElement element);
        void AddParser(IXmlSubElementParser parser);
    }
}
