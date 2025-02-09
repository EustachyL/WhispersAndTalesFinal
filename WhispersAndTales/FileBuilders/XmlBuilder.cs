using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.Model;
using WhispersAndTales.Model.Interfaces;
using WhispersAndTales.Services;

namespace WhispersAndTales.FileBuilders
{
    public class XmlBuilder<T> : IXmlSubElementParser<T> where T : BaseObject, new()
    {
        private readonly List<IXmlSubElementParser> _parsers = [];
        private T _instance;
        private bool _hasErrors;
        public XmlBuilder()
        {
            _instance = new T();
            _hasErrors = false;
        }
        public void AddParser(IXmlSubElementParser parser)
        {
            _parsers.Add(parser);
        }
        public void Parse(XElement element)
        {
            _instance = new T();
            _hasErrors = false;
            try
            {
                foreach (var parser in _parsers)
                {
                    parser.Parse(element);
                }
            }
            catch (Exception ex)
            {
                Log.Write($"Error while parsing {element.Name}: {ex.Message}");
                _hasErrors = true;
            }

        }
        public T Build()
        {
            if (_hasErrors)
            {
                return default;
            }
            foreach (var parser in _parsers)
            {
                parser.ApplyTo(_instance);
            }
            return _instance;
        }
    }
}

