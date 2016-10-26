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
    public class DtoScaffolder: CodeGenerator
    {
        private GeneratorModuleViewModel _moduleViewModel;
        internal DtoScaffolder(CodeGenerationContext context, CodeGeneratorInformation information)
            : base(context, information)
        {

        }

        public override bool ShowUIAndValidate()
        {
            _moduleViewModel = new GeneratorModuleViewModel(Context);

            GeneratorModuleDialog window = new GeneratorModuleDialog(_moduleViewModel);
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
            CodeType modelType = _moduleViewModel.ModelType.CodeType;

            if (modelType == null)
            {
                throw new InvalidOperationException("请选择一个有效的Dto类。");
            }

            var visualStudioUtils = new VisualStudioUtils();
            visualStudioUtils.BuildProject(Context.ActiveProject);


            Type reflectedModelType = GetReflectionType(modelType.FullName);
            if (reflectedModelType == null)
            {
                throw new InvalidOperationException("不能加载的Dto类型。如果项目没有编译，请编译后重试。");
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

                generateCode();
            }
            finally
            {
                Mouse.OverrideCursor = currentCursor;
            }
        }


        private void generateCode()
        {
            var project = Context.ActiveProject;
            var entity = _moduleViewModel.ModelType.CodeType;
            var entityName = entity.Name;
            var projectNamespace = project.GetDefaultNamespace();
            var entityNamespace = entity.Namespace.FullName;
            var moduleNamespace = getModuleNamespace(entityNamespace);
            var moduleName = getModuleName(moduleNamespace);
            var functionName = _moduleViewModel.FunctionName;
            var isDisplayOrderable = IsDisplayOrderable(entity);
            var overwrite = _moduleViewModel.OverwriteFiles;

            Dictionary<string, object> templateParams = new Dictionary<string, object>(){
                {"ProjectNamespace", projectNamespace}
                , {"EntityNamespace", entityNamespace}
                , {"ModuleNamespace", moduleNamespace}
                , {"ModuleName", moduleName}
                , {"EntityName", entityName}
                , {"FunctionName", functionName}
                , {"DtoMetaTable", _moduleViewModel.DtoClassMetadataViewModel.DataModel}
                , {"IsDisplayOrderable", isDisplayOrderable}
            };

            var templates = new[] {
                @"Application\{FunctionFolderName}\Dtos\{Entity}Dto"
                , @"Application\{FunctionFolderName}\Dtos\{Entity}QueryDto"
                , @"Application\{FunctionFolderName}\Dtos\Get{Entity}QueryInput"
                , @"Application\{FunctionFolderName}\I{Entity}AppService"
                , @"Application\{FunctionFolderName}\{Entity}AppService"
                , @"Application\{FunctionFolderName}\_{Entity}Permissions"
                , @"Domain\{FunctionFolderName}\Repositories\I{Entity}Repository"
                , @"Repository\{FunctionFolderName}\Repositories\{Entity}Repository"
            };

            foreach (var template in templates)
            {
                string outputPath = Path.Combine(@"_GeneratedCode\" + moduleName, template.Replace("{Entity}", entityName));
               
                string templatePath = Path.Combine(@"Module", template);
                //WriteLog("outputPath:" + outputPath);
                //WriteLog("templatePath:" + templatePath);
                AddFileFromTemplate(project, outputPath, templatePath, templateParams, !overwrite);
            }

        }

        private void WriteLog(string str)
        {
            File.AppendAllText("D:\\Scaffolder.log.txt", str + "\r\n");
        }

        private bool IsDisplayOrderable(CodeType entity)
        {
            return entity.IsDerivedType("Abp.Domain.Entities.IDisplayOrderable");
        }

        private string getModuleNamespace(string entityNamespace)
        {
            var list = entityNamespace.Split('.').ToList();
            if (entityNamespace.Contains("YMC.Entities."))  //YMC.Entities.Content  
            {
                return "YMC.ECCentral." + list[2];
            }
            if (list.Contains("Domain"))  //命名空间包含Domain时， 去除Domain以后的
            {
                int index = list.IndexOf("Domain");
                if (index > 0)
                {
                    list.RemoveRange(index, list.Count - index);
                }
            }
            return string.Join(".", list);
        }

        private string getModuleName(string moduleNamespace)
        {
            return moduleNamespace.Split('.').Last();
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
