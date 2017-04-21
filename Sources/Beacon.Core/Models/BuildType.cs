using System;
using System.Security.Policy;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Beacon.Core.Models
{
    internal class BuildType
    {
        public static BuildType FromXml(string rawXml)
        {
            var document = XDocument.Parse(rawXml);
            XElement buildTypeElement = document.Element("buildType");

            return new BuildType
            {
                Id = buildTypeElement.Attribute("id").Value,
                IsPaused = buildTypeElement.GetAttributeOrDefault("paused", false),
                IsUnstable = IsMarkedAsUnstable(buildTypeElement)
            };
        }

        private static bool IsMarkedAsUnstable(XElement buildTypeElement)
        {
            XElement propertyElement = buildTypeElement.XPathSelectElement(@"/buildType/parameters/property[@name='system.BuildState']/type");
            if (propertyElement == null)
            {
                propertyElement = buildTypeElement.XPathSelectElement(@"/buildType/parameters/property[@name='BuildState']/type");
            }

            if (propertyElement != null)
            {
                return propertyElement.GetAttributeOrDefault("rawValue", "")
                    .Equals("unstable", StringComparison.InvariantCultureIgnoreCase);
            }
            
            return false;
        }

        public string Id { get; set; }

        public bool IsPaused { get; set; }

        public bool IsUnstable { get; set; }
    }

    public static class XElementExtensions
    {
        public static T GetAttributeOrDefault<T>(this XElement element, string name, T defaultValue)
        {
            var attribute = element.Attribute(name);
            string stringValue = attribute?.Value;
            return (stringValue != null) ? (T) Convert.ChangeType(stringValue, typeof(T)) : defaultValue;
        }
    }
}