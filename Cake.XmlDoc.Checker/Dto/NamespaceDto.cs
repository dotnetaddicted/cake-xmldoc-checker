using System.Collections.Generic;
using System.Linq;

namespace Cake.XmlDoc.Checker.Dto
{  
    public class NamespaceDto : BaseDto
    {
        public List<TypeDto> Types { get; set; }

        public override bool HasErrors { get { return base.HasErrors || Types.Any(_ => _.HasErrors); } }

        public int ErrorCount
        {
            get
            {
                return (string.IsNullOrEmpty(XmlDescription) ? 1 : 0) + Types.Sum(_ => _.ErrorCount);
            }
        }

        public NamespaceDto()
        {
            Types = new List<TypeDto>();
        }
    }
}
