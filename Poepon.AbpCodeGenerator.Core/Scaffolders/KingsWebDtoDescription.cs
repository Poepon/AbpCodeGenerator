using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.AspNet.Scaffolding;

namespace Poepon.AbpCodeGenerator.Core.Scaffolders
{
    public class KingsWebDtoDescription : CodeGeneratorFactory
    {
        public KingsWebDtoDescription()
            : base(CreateCodeGeneratorInformation())
        {

        }
        public override ICodeGenerator CreateInstance(CodeGenerationContext context)
        {
            return new DtoScaffolder(context, Information);
        }

        // We support CSharp WAPs targetting at least .Net Framework 4.5 or above.
        // We DON'T currently support VB
        public override bool IsSupported(CodeGenerationContext codeGenerationContext)
        {
            if (ProjectLanguage.CSharp.Equals(codeGenerationContext.ActiveProject.GetCodeLanguage()))
            {
                FrameworkName targetFramework = codeGenerationContext.ActiveProject.GetTargetFramework();
                return (targetFramework != null) &&
                        String.Equals(".NetFramework", targetFramework.Identifier, StringComparison.OrdinalIgnoreCase) &&
                        targetFramework.Version >= new Version(4, 5);
            }

            return false;
        }

        private static CodeGeneratorInformation CreateCodeGeneratorInformation()
        {
            return new CodeGeneratorInformation(
                displayName: "获取Dto文档",
                description: "前端人员获取Dto文档",
                author: "寒飞",
                version: new Version(0, 1, 0, 0),
                id: "ABPScaffolder",
                icon: ToImageSource(Resources.Application),
                gestures: new[] { "YMC" },
                categories: new[] { "YMC", Categories.Common, Categories.Other }
            );
        }

        /// <summary>
        /// Helper method to convert Icon to Imagesource.
        /// </summary>
        /// <param name="icon">Icon</param>
        /// <returns>Imagesource</returns>
        public static ImageSource ToImageSource(Icon icon)
        {
            ImageSource imageSource = Imaging.CreateBitmapSourceFromHIcon(
                icon.Handle,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            return imageSource;
        }
    }
}
