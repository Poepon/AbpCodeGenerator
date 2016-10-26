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
    public class ModuleScaffolderFactory : CodeGeneratorFactory
    {
        public ModuleScaffolderFactory()
            : base(CreateCodeGeneratorInformation())
        {

        }

        public override ICodeGenerator CreateInstance(CodeGenerationContext context)
        {
            return new ModuleScaffolder(context, Information);
        }

        public override bool IsSupported(CodeGenerationContext context)
        {
            return true;
        }

        private static CodeGeneratorInformation CreateCodeGeneratorInformation()
        {
            return new CodeGeneratorInformation(
                displayName: "[ABP]添加模块功能",
                description: "通过实体类，生成相应模块的Application和Repository代码",
                author: "彭波",
                version: new Version(0, 1, 0, 0),
                id: "ABPScaffolder",
                icon: ToImageSource(Resources.Application),
                gestures: new[] { "ABP" },
                categories: new[] { "ABP", Categories.Common, Categories.Other }
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