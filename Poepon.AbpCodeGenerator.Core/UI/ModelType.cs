using System;
using EnvDTE;
using Poepon.AbpCodeGenerator.Core.Utils;

namespace Poepon.AbpCodeGenerator.Core.UI
{
    /// <summary>
    /// Wrapper around CodeType for allowing string values.
    /// </summary>
    public class ModelType
    {
        public ModelType(CodeType codeType)
        {
            if (codeType == null)
            {
                throw new ArgumentNullException("codeType");
            }

            CodeType = codeType;
            TypeName = codeType.FullName;
            DisplayName = (codeType.Namespace != null && !String.IsNullOrWhiteSpace(codeType.Namespace.FullName))
                              ? String.Format("{0} ({1})", codeType.Name, codeType.Namespace.FullName)
                              : codeType.Name;
            ShortName = codeType.Name;
            CName = VmUtils.getCName(codeType);
        }

        public ModelType(string typeName)
        {
            CodeType = null;
            TypeName = typeName;
            DisplayName = typeName;
            CName = null;
        }

        

        public CodeType CodeType { get; set; }

        public string TypeName { get; set; }

        public string DisplayName { get; set; }

        public string CName { get; set; }

        public string ShortName { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }

    }
}
