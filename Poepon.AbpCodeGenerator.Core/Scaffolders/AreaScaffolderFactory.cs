using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.AspNet.Scaffolding;

namespace Poepon.AbpCodeGenerator.Core.Scaffolders
{
    [Export(typeof(CodeGeneratorFactory))]
    public class AreaScaffolderFactory : CodeGeneratorFactory
    {
        public AreaScaffolderFactory()
            : base(CreateCodeGeneratorInformation())
        {

        }

        public override ICodeGenerator CreateInstance(CodeGenerationContext context)
        {
            return new AreaScaffolder(context, Information);
        }

        // We support CSharp WAPs targetting at least .Net Framework 4.5 or above.
        // We DON'T currently support VB
        public override bool IsSupported(CodeGenerationContext context)
        {
            return true;
        }

        private static CodeGeneratorInformation CreateCodeGeneratorInformation()
        {
            return new CodeGeneratorInformation(
                displayName: "[ABP]添加Area中的增删改查",
                description: "在Web项目的Area中生成相应的增删改查代码",
                author: "彭波",
                version: new Version(0, 1, 0, 0),
                id: "ABPScaffolder",
                icon: ToImageSource(Resources.Application),
                gestures: new[] { "ABP" },
                categories: new[] { "ABP", Categories.Common, Categories.MvcArea }
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