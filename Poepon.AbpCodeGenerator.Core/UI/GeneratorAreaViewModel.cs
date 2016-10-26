using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using EnvDTE;
using Microsoft.AspNet.Scaffolding.EntityFramework;
using Microsoft.AspNet.Scaffolding;
using EnvDTE80;
using System.Xml;
using System.Text.RegularExpressions;
using System.Windows;
using Poepon.AbpCodeGenerator.Core.Models;

namespace Poepon.AbpCodeGenerator.Core.UI
{
    internal class GeneratorAreaViewModel : ViewModel<GeneratorAreaViewModel>
    {
        //public ModelMetadataViewModel ModelMetadataVM { get; set; }

        private MetadataSettingViewModel _dtoClassMetadataViewModel;

        private MetadataSettingViewModel _itemClassMetadataViewModel;

        private ObservableCollection<ModelType> _modelTypeCollection;

        private ObservableCollection<euControlType> _controlTypeCollection;

        //private ObservableCollection<ModelType> _dtoClassCollection;

        //private ObservableCollection<ModelType> _itemClassCollection;

        private readonly CodeGenerationContext _context;

        public GeneratorAreaViewModel(CodeGenerationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            _context = context;
        }

        #region Switch Tab / Button event

        private int CurrentStepIndex = 0;

        private ObservableCollection<Visibility> _StepVisibale
            = new ObservableCollection<Visibility>() { Visibility.Visible, Visibility.Collapsed, Visibility.Collapsed };

        public ObservableCollection<Visibility> StepVisibale
        {
            get
            {
                return _StepVisibale;
            }
        }

        public void ShowStep()
        {
            for (int x = 0; x < StepVisibale.Count; x++)
            {
                StepVisibale[x] = (x == CurrentStepIndex ? Visibility.Visible : Visibility.Collapsed);
            }
        }

        private DelegateCommand _NextStepCommand;
        public ICommand NextStepCommand
        {
            get
            {
                if (_NextStepCommand == null)
                {
                    _NextStepCommand = new DelegateCommand(_ =>
                    {
                        Validate(propertyName: null);
                        if (!HasErrors)
                        {
                            CurrentStepIndex += 1;
                            if (CurrentStepIndex == 1)
                                CreateMetadataViewModel();

                            ShowStep();
                        }
                    });
                }
                return _NextStepCommand;
            }
        }

        private DelegateCommand _BackStepCommand;
        public ICommand BackStepCommand
        {
            get
            {
                if (_BackStepCommand == null)
                {
                    _BackStepCommand = new DelegateCommand(_ =>
                    {
                        Validate(propertyName: null);
                        if (!HasErrors)
                        {
                            CurrentStepIndex -= 1;
                            ShowStep();
                        }
                    });
                }
                return _BackStepCommand;
            }
        }

        #endregion


        private DelegateCommand _okCommand;
        public ICommand OkCommand
        {
            get
            {
                if (_okCommand == null)
                {
                    _okCommand = new DelegateCommand(_ =>
                    {
                        Validate(propertyName: null);

                        if (!HasErrors)
                        {
                            OnClose(result: true);
                        }
                    });
                }

                return _okCommand;
            }
        }

        public event Action<bool> Close;

        private void OnClose(bool result)
        {
            if (Close != null)
            {
                Close(result);
            }
        }

        private ModelType _modelType;

        public ModelType ModelType
        {
            get { return _modelType; }
            set
            {
                if (value == _modelType)
                {
                    return;
                }

                _modelType = value;

                DtoClass = getDtoClass();
                ItemClass = getItemClass();

                OnPropertyChanged();

                if (!string.IsNullOrEmpty(_modelType.CName))
                {
                    FunctionName = _modelType.CName;
                    OnPropertyChanged(m => m.FunctionName);
                }

                Validate();
            }
        }



        private string _functionName;
        public string FunctionName
        {
            get { return _functionName ?? ""; }
            set
            {
                if (value == _functionName)
                {
                    return;
                }
                _functionName = value;
                OnPropertyChanged();
                Validate();
            }
        }


        private bool _overwriteFiles;

        public bool OverwriteFiles
        {
            get { return _overwriteFiles; }
            set
            {
                if (value == _overwriteFiles)
                {
                    return;
                }

                _overwriteFiles = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Dto类Meta
        /// </summary>
        public MetadataSettingViewModel DtoClassMetadataViewModel
        {
            get
            {
                return _dtoClassMetadataViewModel;
            }
            set
            {
                if (value == this._dtoClassMetadataViewModel)
                {
                    return;
                }

                this._dtoClassMetadataViewModel = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Item类Meta
        /// </summary>
        public MetadataSettingViewModel ItemClassMetadataViewModel
        {
            get
            {
                return _itemClassMetadataViewModel;
            }
            set
            {
                if (value == this._itemClassMetadataViewModel)
                {
                    return;
                }

                this._itemClassMetadataViewModel = value;
                OnPropertyChanged();
            }
        }

        private void CreateMetadataViewModel()
        {
            if (DtoClass != null && (DtoClassMetadataViewModel == null || DtoClassMetadataViewModel.CodeType != DtoClass.CodeType))
            {
                DtoClassMetadataViewModel = new MetadataSettingViewModel(DtoClass.CodeType, false);
            }
            if (ItemClass != null && (ItemClassMetadataViewModel == null || ItemClassMetadataViewModel.CodeType != ItemClass.CodeType))
            {
                ItemClassMetadataViewModel = new MetadataSettingViewModel(ItemClass.CodeType, false);
            }
        }

        //private void SaveDesignData()
        //{
        //    StorageMan<MetaTableInfo> sm = new StorageMan<MetaTableInfo>(this.MethodTypeName, SaveFolderPath);
        //    sm.Save(this.QueryFormViewModel.DataModel);

        //    sm.ModelName = ModelType.ShortName;
        //    sm.Save(this.ResultListViewModel.DataModel);
        //}

        private string SaveFolderPath
        {
            get
            {
                return Path.Combine(_context.ActiveProject.GetFullPath(), "CodeGen");
            }
        }

        public ObservableCollection<ModelType> ModelTypeCollection
        {
            get
            {
                if (_modelTypeCollection == null)
                {
                    ICodeTypeService codeTypeService = GetService<ICodeTypeService>();
                    Project project = _context.ActiveProject;


                    var modelTypes = codeTypeService
                                        .GetAllCodeTypes(project)
                                        .Where(codeType => IsEntityClass(codeType))
                                        .OrderBy(x => x.Name)
                                        .Select(codeType => new ModelType(codeType));
                    _modelTypeCollection = new ObservableCollection<ModelType>(modelTypes);
                }
                return _modelTypeCollection;
            }
        }

        public ObservableCollection<euControlType> ControlTypeCollection
        {
            get
            {
                if (_controlTypeCollection == null)
                {
                    _controlTypeCollection = new ObservableCollection<euControlType>();
                    _controlTypeCollection.Add(euControlType.Text);
                    _controlTypeCollection.Add(euControlType.DatePicker);
                    _controlTypeCollection.Add(euControlType.DateTimePicker);
                    _controlTypeCollection.Add(euControlType.DropdownList);
                    _controlTypeCollection.Add(euControlType.TextEditor);
                    _controlTypeCollection.Add(euControlType.Checkbox);
                }
                return _controlTypeCollection;
            }
        }

        public ModelType DtoClass { get; set; }

        public string DtoNamespace
        {
            get { return DtoClass.CodeType.Namespace.FullName; }
        }

        public ModelType ItemClass { get; set; }

        private ModelType getDtoClass()
        {
            if (ModelType == null) return null;
            ICodeTypeService codeTypeService = GetService<ICodeTypeService>();
            Project project = _context.ActiveProject;
            var dto = codeTypeService
                            .GetAllCodeTypes(project)
                            .Where(IsDtoClass)
                            .Select(codeType => new ModelType(codeType))
                            .FirstOrDefault();
            return dto;
        }

        private ModelType getItemClass()
        {
            if (ModelType == null) return null;
            ICodeTypeService codeTypeService = GetService<ICodeTypeService>();
            Project project = _context.ActiveProject;
            var cls = codeTypeService
                            .GetAllCodeTypes(project)
                            .Where(IsItemClass)
                            .Select(codeType => new ModelType(codeType))
                            .FirstOrDefault();
            return cls;
        }

        private bool IsEntityClass(CodeType codeType)
        {
            string[] strBaseType = {
                "Abp.Domain.Entities.IEntity<System.Guid>",
                "Abp.Domain.Entities.IEntity<System.Int32>",
                "Abp.Domain.Entities.IEntity<System.Int64>",
                "Abp.Domain.Entities.IEntity<System.DateTime>",
                "Abp.Domain.Entities.IEntity<System.Int16>",
                "Abp.Domain.Entities.IEntity<System.String>"
            };
            return !IsAbstract(codeType) && 
                (codeType.IsDerivedType("Abp.Domain.Entities.IEntity")
                || codeType.IsDerivedType(strBaseType[0])
                || codeType.IsDerivedType(strBaseType[1])
                || codeType.IsDerivedType(strBaseType[2])
                || codeType.IsDerivedType(strBaseType[3])
                || codeType.IsDerivedType(strBaseType[4])
                || codeType.IsDerivedType(strBaseType[5])
                || codeType.IsDerivedType("Abp.Domain.Entities.ICreationAudited"));
        }

        private bool IsDtoClass(CodeType codeType)
        {
            return codeType.Name.EndsWith("Dto", StringComparison.CurrentCulture);
        }

        private bool IsItemClass(CodeType codeType)
        {
            return codeType.Name.EndsWith("Input",StringComparison.CurrentCulture);
        }


        private bool IsAbstract(CodeType codeType)
        {
            CodeClass2 codeClass2 = codeType as CodeClass2;
            if (codeClass2 != null)
            {
                return codeClass2.IsAbstract;
            }
            else
            {
                return false;
            }
        }

        protected override void Validate([CallerMemberName]string propertyName = "")
        {
            string currentPropertyName;

            // ModelType
            currentPropertyName = PropertyName(m => m.ModelType);
            if (ShouldValidate(propertyName, currentPropertyName))
            {
                ClearError(currentPropertyName);
                if (ModelType == null)
                {
                    AddError(currentPropertyName, "请从列表中选择一个实体类");
                    return;
                }
                if (DtoClass == null)
                {
                    AddError(currentPropertyName, "未找到实体类对应的Dto类");
                }
                if (ItemClass == null)
                {
                    AddError(currentPropertyName, "未找到实体类对应的QueryDto类");
                }
            }

            currentPropertyName = PropertyName(m => m.FunctionName);
            if (ShouldValidate(propertyName, currentPropertyName))
            {
                ClearError(currentPropertyName);
                if (string.IsNullOrWhiteSpace(FunctionName))
                {
                    AddError(currentPropertyName, "请输入功能中文名称");
                }
            }
        }

        private TService GetService<TService>() where TService : class
        {
            return (TService)_context.ServiceProvider.GetService(typeof(TService));
        }
    }
}
