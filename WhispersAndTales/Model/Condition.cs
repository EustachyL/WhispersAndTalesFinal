using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WhispersAndTales.Dictionary;
using WhispersAndTales.Model.Interfaces;
using WhispersAndTales.FileBuilders;

namespace WhispersAndTales.Model
{
    public class Condition : IHasProperties
    {
        public string Tag { get; set; }
        public Type Type => typeof(Condition);
        public Dictionary<string, Property> Properties { get; set; } = new();

        private Func<ITaged, bool> ConditionType { get; }
        public string ExpectedValue { get; set; }

        public Condition() { }
        public bool CheckCondition(ITaged taggedObject)
        {
            return ConditionType(taggedObject);
        }
        public bool CheckTag(ITaged taggedObject)
        {
            return taggedObject.Tag == ExpectedValue;
        }

        public bool CheckProperty(ITaged taggedObject)
        {
            try
            {
                if (taggedObject is IHasProperties hasProperties &&
                    hasProperties.Properties.TryGetValue((string)Properties["PropertyName"].Value, out var property))
                {
                    return property.Value == ExpectedValue;
                }

            }
            catch (Exception ex)
            {
                return false;
            }

            return false;
        }
    }
}
