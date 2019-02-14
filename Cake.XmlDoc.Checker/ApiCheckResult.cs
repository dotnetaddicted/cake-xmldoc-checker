using Cake.XmlDoc.Checker.Dto;
using System.Collections.Generic;
using System.Linq;

namespace Cake.XmlDoc.Checker
{
    public class ApiCheckResult
    {
        public string AssemblyName { get; set; }
        public List<NamespaceDto> Namespaces { get; set; }

        public bool HasErrors { get { return Namespaces.Any(_ => _.HasErrors); } }

        public int ErrorCount
        {
            get
            {
                return Namespaces.Sum(_ => _.ErrorCount);
            }
        }

        public ApiCheckResult()
        {
            Namespaces = new List<NamespaceDto>();
        }
    }
}
