

namespace Cake.XmlDoc.Checker
{
    public class CheckOptions
    {
        public string AssemblyPath { get; set; }

        public bool LogOnlyMissing { get; set; }

        public bool SkipDelegatesProcessing { get; set; }

        public CheckOptions()
        {
            LogOnlyMissing = true;
            SkipDelegatesProcessing = true;
        }
    }
}
