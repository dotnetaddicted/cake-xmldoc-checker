using System.Collections.Generic;
using System.Linq;

namespace Cake.XmlDoc.Checker.Dto
{
    public class TypeDto : BaseDto
    {
        public List<ConstructorDto> Constructors { get; set; }
        public List<MethodDto> Methods { get; set; }
        public List<FieldDto> Fields { get; set; }
        public List<PropertyDto> Properties { get; set; }

        public override bool HasErrors
        {
            get
            {
                return base.HasErrors 
                    || Constructors.Any(_ => _.HasErrors)
                    || Methods.Any(_ => _.HasErrors)
                    || Fields.Any(_ => _.HasErrors)
                    || Properties.Any(_ => _.HasErrors);
            }
        }

        public int ErrorCount
        {
            get
            {               
                return (string.IsNullOrEmpty(XmlDescription) ? 1 : 0) +
                    + Constructors.Count(_ => _.HasErrors) 
                    + Methods.Count(_ => _.HasErrors)
                    + Fields.Count(_ => _.HasErrors)
                    + Properties.Count(_ => _.HasErrors);
            }
        }


        public TypeDto()
        {
            Constructors = new List<ConstructorDto>();
            Methods = new List<MethodDto>();
            Fields = new List<FieldDto>();
            Properties = new List<PropertyDto>();
        }
    }
}
