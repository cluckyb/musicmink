using MusicMink.ViewModels;
using MusicMinkAppLayer.Diagnostics;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MusicMink.Converters
{
    public class MixItemStyleSelector : StyleSelector
    {
        public Style EmptyStyle { get; set; }

        protected override Style SelectStyleCore(object item, DependencyObject container)
        {
            MixViewModel itemAsMixViewModel = DebugHelper.CastAndAssert<MixViewModel>(item);

            if (itemAsMixViewModel.IsHidden)
            {
                return EmptyStyle;
            }

            return null;
        }
    }
}
