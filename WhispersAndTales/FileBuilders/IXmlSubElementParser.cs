using System.Xml.Linq;

namespace WhispersAndTales.FileBuilders
{
    public interface IXmlSubElementParser
    {
        void Parse(XElement element); // Logika przetwarzania XML
        void ApplyTo<T>(T instance); // Zastosowanie danych do obiektu
    }
}