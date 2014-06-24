using MusicMinkAppLayer.Enums;
using MusicMinkAppLayer.Models;
using System.ComponentModel;

namespace MusicMink.ViewModels
{
    public class MixEntryViewModel : BaseViewModel<MixEntryModel>
    {
        public static class Properties
        {
            public const string EntryId = "EntryId";

            public const string Input = "Input";
            public const string Type = "Type";
        }

        public MixEntryViewModel(MixEntryModel model)
            : base(model)
        {
            model.PropertyChanged += HandleModelPropertyChanged;
        }

        void HandleModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case MixEntryModel.Properties.Input:
                    NotifyPropertyChanged(Properties.Input);
                    break;
                case MixEntryModel.Properties.Type:
                    NotifyPropertyChanged(Properties.Type);
                    break;
            }
        }

        public MixType Type
        {
            get
            {
                return rootModel.Type;
            }
        }

        public string Input
        {
            get
            {
                return rootModel.Input;
            }
        }
    }
}
