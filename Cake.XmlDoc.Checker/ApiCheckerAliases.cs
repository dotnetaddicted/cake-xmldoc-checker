using Cake.Core;
using Cake.Core.Annotations;
using Cake.Core.Diagnostics;

namespace Cake.XmlDoc.Checker
{
    [CakeAliasCategory("Cake.XmlDoc.Checker")]
    public static class XmlDocCheckerAliases
    {
        [CakeMethodAlias]
        public static ApiCheckResult CheckXmlDoc(this ICakeContext context, CheckOptions checkOptions)
        {                       
            context.Log.Write(Verbosity.Normal, LogLevel.Information, "Processing assembly from path '{0}'. Log only missing members '{1}'", checkOptions.AssemblyPath, checkOptions.LogOnlyMissing);

            ApiChecker.Log = context.Log;
            return ApiChecker.Check(checkOptions);
        }
    }
}
