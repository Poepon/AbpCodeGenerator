using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using EnvDTE;
using Poepon.AbpCodeGenerator.Core.Models;
using Poepon.AbpCodeGenerator.Core.Utils;

namespace Poepon.AbpCodeGenerator.Core.UI
{
    internal class MetadataSettingViewModel : ViewModel<MetadataSettingViewModel>
    {
        //public MetadataSettingViewModel(ModelMetadata efMetadata)
        //{
        //    MetaTableInfo dataModel = new MetaTableInfo();
        //    foreach (Microsoft.AspNet.Scaffolding.Core.Metadata.PropertyMetadata p1 in efMetadata.Properties)
        //    {
        //        dataModel.Columns.Add(new MetaColumnInfo(p1));
        //    }
        //    Init(dataModel);
        //}

        //public MetadataSettingViewModel(CodeFunction codefunction)
        //{
        //    MetaTableInfo dataModel = new MetaTableInfo();
        //    foreach (CodeElement ce in codefunction.Parameters)
        //    {
        //        CodeParameter p1 = (CodeParameter)ce;
        //        dataModel.Columns.Add(new MetaColumnInfo(p1));
        //    }
        //    Init(dataModel);
        //}

        public MetadataSettingViewModel(CodeType codeType, bool IsHiddenFields = true)
        {
            MetaTableInfo dataModel = new MetaTableInfo();
            fillCodeTypeMembers(dataModel, codeType, IsHiddenFields);

            Init(codeType, dataModel);
        }

        private void fillCodeTypeMembers(MetaTableInfo dataModel, CodeType codeType, bool IsHiddenFields = true)
        {
            foreach (CodeElement ce in codeType.Members)
            {
                var prop = ce as CodeProperty;
                if (prop == null) continue;
                if (IsHiddenFields && VmUtils.DefaultHiddenFields.Contains(prop.Name)) continue; //默认不显示的字段
                if (dataModel.Columns.Any(d=>d.Name == prop.Name)) continue; //已存在
                dataModel.Columns.Add(new MetaColumnInfo(prop));
            }

            foreach (CodeElement c in codeType.Bases)
            {
                if (c.IsCodeType)
                {
                    fillCodeTypeMembers(dataModel, (CodeType)c);
                }
            }

        }

        //public MetadataSettingViewModel(MetaTableInfo dataModel)
        //{
        //    Init(dataModel);
        //}

        private void Init(CodeType codeType, MetaTableInfo dataModel)
        {
            this.CodeType = codeType;
            this.DataModel = dataModel;
            this.Columns = new ObservableCollection<MetadataFieldViewModel>();
            foreach (MetaColumnInfo f1 in dataModel.Columns)
            {
                this.Columns.Add(new MetadataFieldViewModel(f1));
            }
        }

        public MetadataFieldViewModel this[string name]
        {
            get { return this.Columns.FirstOrDefault(x => x.Name == name); }
        }

        public MetadataFieldViewModel this[int index]
        {
            get { return this.Columns[index]; }
        }

        public MetaTableInfo DataModel { get; private set; }
        public CodeType CodeType { get; private set; }

        private ObservableCollection<MetadataFieldViewModel> m_Columns = null;

        public ObservableCollection<MetadataFieldViewModel> Columns
        {
            get { return m_Columns; }
            private set { this.m_Columns = value; }
        }

    }

    //internal class ModelMetadataViewModel : MetadataSettingViewModel
    //{
    //    //public ModelMetadataViewModel(ModelMetadata efMetadata):base(efMetadata){}
    //    //public ModelMetadataViewModel(CodeFunction codeElements) : base(codeElements) { }
    //    public ModelMetadataViewModel(CodeType codeType) : base(codeType) { }
    //    public ModelMetadataViewModel(MetaTableInfo dataModel) : base(dataModel) { }


    //    private bool m_IsConfirm = false;

    //    public bool IsConfirm
    //    {
    //        get { return m_IsConfirm; }
    //        set
    //        {
    //            if (value == m_IsConfirm)
    //            {
    //                return;
    //            }
    //            m_IsConfirm = value;
    //            OnPropertyChanged();
    //        }
    //    }

    //    private DelegateCommand _okCommand;

    //    public ICommand OkCommand
    //    {
    //        get
    //        {
    //            if (_okCommand == null)
    //            {
    //                _okCommand = new DelegateCommand(_ =>
    //                {
    //                    if (IsConfirm)
    //                        OnClose(result: true);
    //                });
    //            }

    //            return _okCommand;
    //        }
    //    }

    //    public event Action<bool> Close;

    //    private void OnClose(bool result)
    //    {
    //        if (Close != null)
    //        {
    //            Close(result);
    //        }
    //    }
    //}

    internal class MetadataFieldViewModel : ViewModel<MetadataFieldViewModel>
    {
        public MetadataFieldViewModel(MetaColumnInfo data)
        {
            this.DataModel = data;
        }

        public MetaColumnInfo DataModel { get; private set; }

        public string Name
        {
            get { return DataModel.Name; }
            private set
            {
                DataModel.Name = value;
            }
        }

        public string DisplayName
        {
            get { return DataModel.DisplayName; }
            set
            {
                if (value == DataModel.DisplayName)
                {
                    return;
                }
                DataModel.DisplayName = value;
                OnPropertyChanged();
            }
        }

        //public bool Nullable
        //{
        //    get { return DataModel.Nullable; }
        //    set
        //    {
        //        if (value == DataModel.Nullable)
        //        {
        //            return;
        //        }
        //        DataModel.Nullable = value;
        //        OnPropertyChanged();
        //    }
        //}

        public bool Required
        {
            get { return DataModel.Required; }
            set
            {
                if (value == DataModel.Required)
                {
                    return;
                }
                DataModel.Required = value;
                OnPropertyChanged();
            }
        }

        public bool IsDtoVisible
        {
            get { return DataModel.IsDtoVisible; }
            set
            {
                if (value == DataModel.IsDtoVisible)
                {
                    return;
                }
                DataModel.IsDtoVisible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否生成ItemClass的此属性
        /// </summary>
        public bool IsItemVisible
        {
            get { return DataModel.IsItemVisible; }
            set
            {
                if (value == DataModel.IsItemVisible)
                {
                    return;
                }
                DataModel.IsItemVisible = value;
                OnPropertyChanged();
            }
        }

        public string strDataType
        {
            get { return DataModel.strDataType; }
        }

        public int? MaxLength
        {
            get { return DataModel.MaxLength; }
            set
            {
                if (value == DataModel.MaxLength)
                {
                    return;
                }
                DataModel.MaxLength = value;
                OnPropertyChanged();
            }
        }

        public int? RangeMin
        {
            get { return DataModel.RangeMin; }
            set
            {
                if (value == DataModel.RangeMin)
                {
                    return;
                }
                DataModel.RangeMin = value;
                OnPropertyChanged();
            }
        }

        public int? RangeMax
        {
            get { return DataModel.RangeMax; }
            set
            {
                if (value == DataModel.RangeMax)
                {
                    return;
                }
                DataModel.RangeMax = value;
                OnPropertyChanged();
            }
        }

        public euControlType ControlType
        {
            get { return DataModel.ControlType; }
            set
            {
                if (value == DataModel.ControlType)
                {
                    return;
                }
                DataModel.ControlType = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<euControlType> _controlTypeCollection;
        public ObservableCollection<euControlType> ControlTypeCollection
        {
            get
            {
                if (_controlTypeCollection == null)
                {
                    _controlTypeCollection = new ObservableCollection<euControlType>();
                    _controlTypeCollection.Add(euControlType.Text);
                    _controlTypeCollection.Add(euControlType.Textarea);
                    _controlTypeCollection.Add(euControlType.TextEditor);
                    _controlTypeCollection.Add(euControlType.Hidden);
                    _controlTypeCollection.Add(euControlType.DatePicker);
                    _controlTypeCollection.Add(euControlType.DateTimePicker);
                    _controlTypeCollection.Add(euControlType.DropdownList);
                    _controlTypeCollection.Add(euControlType.Checkbox);
                }
                return _controlTypeCollection;
            }
        }

        public Visibility ShowEditorMaxLength
        {
            get
            {
                if (DataModel.DataType == euColumnType.stringCT)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public Visibility ShowEditorRange
        {
            get
            {
                if (DataModel.DataType == euColumnType.intCT 
                    || DataModel.DataType == euColumnType.longCT
                    || DataModel.DataType == euColumnType.floatCT
                    || DataModel.DataType == euColumnType.decimalCT)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

    }
}
