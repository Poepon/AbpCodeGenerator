using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Input;
using EnvDTE;
using Microsoft.AspNet.Scaffolding;
using Poepon.AbpCodeGenerator.Core.UI;
using Poepon.AbpCodeGenerator.Core.Utils;

namespace Poepon.AbpCodeGenerator.Core.Scaffolders
{
    // 此类包含基架生成的所有步骤:
    // 1) ShowUIAndValidate() - 显示一个Visual Studio的对话框用于设置生成参数
    // 2) Validate() - 确认提取的Model   validates the model collected from the dialog
    // 3) GenerateCode() - 根据模板生成代码文件 if all goes well, generates the scaffolding output from the templates
    public class AreaScaffolder : CodeGenerator
    {

        private GeneratorAreaViewModel _moduleViewModel;

        internal AreaScaffolder(CodeGenerationContext context, CodeGeneratorInformation information)
            : base(context, information)
        {

        }

        public override bool ShowUIAndValidate()
        {
            _moduleViewModel = new GeneratorAreaViewModel(Context);

            GeneratorAreaDialog window = new GeneratorAreaDialog(_moduleViewModel);
            bool? isOk = window.ShowModal();

            if (isOk == true)
            {
                Validate();
            }
            return (isOk == true);
        }


        // Validates the model returned by the Visual Studio dialog.
        // We always force a Visual Studio build so we have a model
        private void Validate()
        {
            if (_moduleViewModel.ModelType == null)
            {
                throw new InvalidOperationException("请选择一个有效的实体类。");
            }

            if (_moduleViewModel.DtoClass == null)
            {
                throw new InvalidOperationException("未找到实体类对应的Dto类。");
            }

            if (_moduleViewModel.ItemClass == null)
            {
                throw new InvalidOperationException("未找到实体类对应的QueryDto类。");
            }

            if (string.IsNullOrWhiteSpace(_moduleViewModel.FunctionName))
            {
                throw new InvalidOperationException("请填写功能中文名称");
            }


        }

        public override void GenerateCode()
        {
            if (_moduleViewModel == null)
            {
                throw new InvalidOperationException("需要先调用ShowUIAndValidate方法。");
            }

            Cursor currentCursor = Mouse.OverrideCursor;
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;

                var project = Context.ActiveProject;
                var entity = _moduleViewModel.ModelType.CodeType;
                var entityName = entity.Name;
                var entityNamespace = entity.Namespace.FullName;
                var entityFolerName = ProjectItemUtils.GetModuleName(entityNamespace);
                var puralEntityName = VmUtils.ToPlural(entityName);
                var projectNamespace = ProjectItemUtils.GetProjectName(entityNamespace);
                var overwrite = _moduleViewModel.OverwriteFiles;
                Dictionary<string, object> templateParams = new Dictionary<string, object>(){
                {"AppName", projectNamespace}
                , {"EntityNamespace", entityNamespace}
                , {"EntityFolerName",entityFolerName}
                , {"PluralEntityName",puralEntityName}
                , {"EntityName", entityName}
                , {"DtoNamespace", _moduleViewModel.DtoNamespace}
                , {"FunctionName", _moduleViewModel.FunctionName}
                , {"DtoMetaTable", _moduleViewModel.DtoClassMetadataViewModel.DataModel}
                , {"ItemMetaTable", _moduleViewModel.ItemClassMetadataViewModel.DataModel}
            };

                var templates = new[]
                 {
                    @"{AppName}.Web\Areas\{Module}\Controllers\{PluralEntityName}Controller.cs",
                    @"{AppName}.Web\Areas\{Module}\Models\{PluralEntityName}\CreateOrEdit{Entity}ModalViewModel.cs",
                    @"{AppName}.Web\Areas\{Module}\Views\{PluralEntityName}\_CreateOrEditModal.cshtml",
                    @"{AppName}.Web\Areas\{Module}\Views\{PluralEntityName}\Index.cshtml",
                    @"{AppName}.Web\wwwroot\view-resources\Areas\{Module}\Views\{PluralEntityName}\_CreateOrEditModal.js",
                    @"{AppName}.Web\wwwroot\view-resources\Areas\{Module}\Views\{PluralEntityName}\Index.js",
                    @"{AppName}.Web\wwwroot\view-resources\Areas\{Module}\Views\{PluralEntityName}\index.less",
                    @"{AppName}.Common\{PluralEntityName}PageNames.cs",
                };
                foreach (var template in templates)
                {
                    string outputPath = Path.GetFileNameWithoutExtension(Path.Combine(@"_GeneratedCode\",
                        template.Replace("{AppName}", projectNamespace)
                            .Replace("{Module}", "Admin")
                            .Replace("{PluralEntityName}", puralEntityName)
                            .Replace("{Entity}", entityName)));

                    string templatePath = Path.Combine(template);

                    AddFileFromTemplate(project, outputPath, templatePath, templateParams, !overwrite);
                }
            }
            finally
            {
                Mouse.OverrideCursor = currentCursor;
            }
        }

        protected new bool AddFileFromTemplate(Project project, string outputPath, string templateName,
            IDictionary<string, object> templateParameters, bool skipIfExists)
        {
            if (project == null)
                throw new ArgumentNullException("project");
            if (templateParameters == null)
                throw new ArgumentNullException("templateParameters");
            var service1 = this.ServiceProvider.GetService<ICodeGeneratorFilesLocator>();
            var service2 = this.ServiceProvider.GetService<ICodeGeneratorActionsService>();
            string filePath = Path.GetFileNameWithoutExtension(templateName);
            string fileExtension = Path.GetExtension(templateName);
            string textTemplatePath = service1.GetTextTemplatePath(filePath, this.TemplateFolders, fileExtension);
            string str = Path.GetDirectoryName(project.DTE.Solution.FullName).TrimEnd('\\') + "\\";

            if (textTemplatePath.StartsWith(str, StringComparison.OrdinalIgnoreCase))
            {
                AddTelemetryData(this.Context, "MSInternal_CoreInfo", "IsCustomTemplate", true);
            }

            return service2.AddFileFromTemplate(project, outputPath, textTemplatePath, templateParameters, skipIfExists);
        }

        public static void AddTelemetryData(CodeGenerationContext context, string producerId, string key, object value)
        {
            if (context == null)
                throw new ArgumentNullException("context");
            if (producerId == null)
                throw new ArgumentNullException("producerId");
            if (key == null)
                throw new ArgumentNullException("key");
            if (value == null)
                throw new ArgumentNullException("value");
            GetItems(context, producerId)[key] = value;
        }

        private static Dictionary<string, object> GetItems(CodeGenerationContext context, string producerId)
        {
            Dictionary<string, object> property;
            if (!context.Items.TryGetProperty(producerId, out property))
            {
                property = new Dictionary<string, object>();
                context.Items.AddProperty(producerId, property);
            }
            return property;
        }


        private void WriteLog(string str)
        {
            File.AppendAllText("D:\\Scaffolder.log.txt", str + "\r\n");
        }



        #region function library


        // Called to ensure that the project was compiled successfully
        private Type GetReflectionType(string typeName)
        {
            return GetService<IReflectedTypesService>().GetType(Context.ActiveProject, typeName);
        }

        private TService GetService<TService>() where TService : class
        {
            return (TService)ServiceProvider.GetService(typeof(TService));
        }


        // Returns the relative path of the folder selected in Visual Studio or an empty 
        // string if no folder is selected.
        protected string GetSelectionRelativePath()
        {
            return Context.ActiveProjectItem == null ? String.Empty : ProjectItemUtils.GetProjectRelativePath(Context.ActiveProjectItem);
        }

        #endregion


    }
}
