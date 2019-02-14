

namespace Cake.XmlDoc.Checker.Dto
{
    public abstract class BaseDto
    {
        public string Name { get; set; }

        public string XmlDescription { get; set; }

        public virtual bool HasErrors { get { return string.IsNullOrEmpty(XmlDescription); } }
    }       
}
