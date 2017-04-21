using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Beacon.Core.Models
{
    internal class BuildCollection : IEnumerable<Build>
    {
        private Build[] builds;

        public bool IsEmpty => builds.Length == 0;

        public static BuildCollection FromXml(string rawXml)
        {
            var document = XDocument.Parse(rawXml);

            Build[] builds = document
                .XPathSelectElements("/builds/build")
                .Select(Build.FromXml)
                .ToArray();

            return new BuildCollection
            {
                builds = builds
            };
        }
        IEnumerator<Build> IEnumerable<Build>.GetEnumerator()
        {
            return ((IEnumerable<Build>)builds).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<Build>)builds).GetEnumerator();
        }
    }
}
