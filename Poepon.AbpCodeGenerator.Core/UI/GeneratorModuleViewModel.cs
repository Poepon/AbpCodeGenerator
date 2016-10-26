using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using EnvDTE;
using EnvDTE80;
using Microsoft.AspNet.Scaffolding;

namespace Poepon.AbpCodeGenerator.Core.UI
{
    internal class GeneratorModuleViewModel : ViewModel<GeneratorModuleViewModel>
    {
        //public ModelMetadataViewModel ModelMetadataVM { get; set; }

        private MetadataSettingViewModel _dtoClassMetadataViewModel;

        private MetadataSettingViewModel _itemClassMetadataViewModel;

        private ObservableCollection<ModelType> _modelTypeCollection;

        private readonly CodeGenerationContext _context;

        public GeneratorModuleViewModel(CodeGenerationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }

            _context = context;
            _GenerateDtos = true;
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
                            {
                                CreateMetadataViewModel();
                            }
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
                            //SaveDesignData();
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

        #endregion


        private ModelType _modelType;

        public ModelType ModelType
        {
            get { return _modelType; }
            set
            {
                Validate();

                if (value == _modelType)
                {
                    return;
                }

                _modelType = value;

                OnPropertyChanged();


                if (!string.IsNullOrEmpty(_modelType.CName))
                {
                    FunctionName = _modelType.CName;
                    OnPropertyChanged(m => m.FunctionName);
                }

                //_ProgramTitle = _modelType.ShortName;
                //OnPropertyChanged(m => m.ProgramTitle);

                //_ViewPrefix = _modelType.ShortName;
                //OnPropertyChanged(m => m.ViewPrefix);
            }
        }


        private string _modelTypeName;

        public string ModelTypeName
        {
            get { return _modelTypeName; }
            set
            {
                Validate();

                if (value == _modelTypeName)
                {
                    return;
                }

                _modelTypeName = value;
                if (ModelType != null)
                {
                    if (ModelType.DisplayName.StartsWith(_modelTypeName, StringComparison.Ordinal))
                    {
                        _modelTypeName = ModelType.DisplayName;
                    }
                    else
                    {
                        ModelType = null;
                    }
                }
                OnPropertyChanged();
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
            }
        }

        private bool _GenerateDtos;
        public bool GenerateDtos
        {
            get { return _GenerateDtos; }
            set
            {
                Validate();

                if (value == _GenerateDtos)
                {
                    return;
                }

                _GenerateDtos = value;
                OnPropertyChanged();
            }
        }


        private bool _overwriteFiles;

        public bool OverwriteFiles
        {
            get { return _overwriteFiles; }
            set
            {
                Validate();

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
                Validate();

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
                Validate();

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
            if (DtoClassMetadataViewModel == null || DtoClassMetadataViewModel.CodeType != ModelType.CodeType)
                DtoClassMetadataViewModel = new MetadataSettingViewModel(ModelType.CodeType);
            //ItemClassMetadataViewModel = new MetadataSettingViewModel(ModelType.CodeType);
        }

        //private void SaveDesignData()
        //{
        //    StorageMan<MetaTableInfo> sm = new StorageMan<MetaTableInfo>(this.MethodTypeName, SaveFolderPath);
        //    sm.Save(this.QueryFormViewModel.DataModel);

        //    sm.ModelName = ModelType.ShortName;
        //    sm.Save(this.ResultListViewModel.DataModel);
        //}

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
                                        .Where(IsEntityClass)
                                        .OrderBy(x => x.Name)
                                        .Select(codeType => new ModelType(codeType));
                    _modelTypeCollection = new ObservableCollection<ModelType>(modelTypes);
                    
                }
                return _modelTypeCollection;
            }
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
                (codeType.IsDerivedType("Abp.Domain.Entities.Entity")
                ||codeType.IsDerivedType("Abp.Domain.Entities.IEntity")
                || codeType.IsDerivedType(strBaseType[0])
                || codeType.IsDerivedType(strBaseType[1])
                || codeType.IsDerivedType(strBaseType[2])
                || codeType.IsDerivedType(strBaseType[3])
                || codeType.IsDerivedType(strBaseType[4])
                || codeType.IsDerivedType(strBaseType[5])
                || codeType.IsDerivedType("Abp.Domain.Entities.ICreationAudited"));
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
                }
            }

        }

        private TService GetService<TService>() where TService : class
        {
            return (TService)_context.ServiceProvider.GetService(typeof(TService));
        }
    }
}
