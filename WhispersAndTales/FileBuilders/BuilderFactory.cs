using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WhispersAndTales.Model.Interfaces;
using WhispersAndTales.FileBuilders;

namespace WhispersAndTales.FileBuilders
{
    public static class XmlBuilderFactory
    {
        public static IXmlSubElementParser<BaseObject> CreateBuilder<T>() where T : BaseObject, new()
        {
            return new XmlBuilder<T>();
        }
    }
}
