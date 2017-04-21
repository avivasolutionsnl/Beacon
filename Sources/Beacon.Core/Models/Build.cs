using System;
using System.Xml.Linq;

namespace Beacon.Core.Models
{
    internal class Build
    {
        public static Build FromXml(XElement element)
        {
            return new Build
            {
                Id = long.Parse(element.Attribute("id").Value),
                BranchName = element.Attribute("branchName").Value,
                IsRunning = !element.Attribute("state").Value.Equals("finished", StringComparison.InvariantCultureIgnoreCase),
                IsSuccessful = element.Attribute("status").Value.Equals("success", StringComparison.CurrentCultureIgnoreCase)
            };
        }

        public bool IsSuccessful { get; set; }

        public bool IsRunning { get; set; }

        public long Id { get; set; }

        public string BranchName { get; set; }
    }
}