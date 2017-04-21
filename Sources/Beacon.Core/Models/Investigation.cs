using System;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Beacon.Core.Models
{
    internal class Investigation
    {
        public static Investigation FromXml(string rawXml)
        {
            BuildStatus? status = null;

            var document = XDocument.Parse(rawXml);
            foreach (XElement element in document.XPathSelectElements("/investigations/investigation"))
            {
                if (element.GetAttributeOrDefault("state", "").Equals("fixed", StringComparison.InvariantCultureIgnoreCase))
                {
                    Logger.Verbose("Investigation status detected as 'fixed'");
                    status = BuildStatus.Fixed;
                }

                if (element.GetAttributeOrDefault("state", "").Equals("taken", StringComparison.InvariantCultureIgnoreCase))
                {
                    Logger.Verbose("Investigation status detected as 'taken'");
                    status = BuildStatus.Investigating;
                }
            }

            return new Investigation
            {
                Status = status
            };
        }

        public BuildStatus? Status { get; set; }
    }
}