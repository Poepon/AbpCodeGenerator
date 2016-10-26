using System;
using Poepon.AbpCodeGenerator.Core.UI;


namespace Poepon.AbpCodeGenerator.Core.UI
{
    /// <summary>
    /// Interaction logic for WebFormsScaffolderDialog.xaml
    /// </summary>
    internal partial class GeneratorModuleDialog : VSPlatformDialogWindow
    {
        public GeneratorModuleDialog(GeneratorModuleViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel");
            }
            
            InitializeComponent();
            
            //viewModel.PromptForNewDataContextTypeName += model =>
            //{
            //    var dialog = new NewDataContextDialog(model);
            //    var result = dialog.ShowModal();
            //    model.Canceled = !result.HasValue || !result.Value;
            //};

            viewModel.Close += result => DialogResult = result;

            DataContext = viewModel;
        }
    }
}
