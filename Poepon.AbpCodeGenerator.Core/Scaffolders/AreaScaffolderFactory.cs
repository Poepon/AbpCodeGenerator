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
                displayName: "[ABP]���Area�е���ɾ�Ĳ�",
                description: "��Web��Ŀ��Area��������Ӧ����ɾ�Ĳ����",
                author: "��",
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