using System;


namespace Poepon.AbpCodeGenerator.Core.UI
{
    /// <summary>
    /// Interaction logic for WebFormsScaffolderDialog.xaml
    /// </summary>
    internal partial class GeneratorAreaDialog : VSPlatformDialogWindow
    {
        public GeneratorAreaDialog(GeneratorAreaViewModel viewModel)
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
