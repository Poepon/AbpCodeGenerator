﻿using System;
using EnvDTE;

namespace Poepon.AbpCodeGenerator.Core.UI
{
    /// <summary>
    /// Wrapper around CodeType for allowing string values.
    /// </summary>
    public class MethodType
    {
        public MethodType(CodeFunction codeType)
        {
            if (codeType == null)
            {
                throw new ArgumentNullException("codeType");
            }

            CodeType = codeType;
            TypeName = codeType.FullName;
            ShortName = codeType.Name;

            DisplayName = ShortName;
        }

        public MethodType(string typeName)
        {
            CodeType = null;
            TypeName = typeName;
            DisplayName = typeName;
        }



        public CodeFunction CodeType { get; set; }

        public string TypeName { get; set; }

        public string DisplayName { get; set; }

        public string ShortName { get; set; }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
