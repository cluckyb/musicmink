using MusicMink.Collections;
using MusicMink.ViewModels;
using MusicMink.ViewModels.MixEvaluators;
using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Enums;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Linq;

namespace MusicMink.Dialogs
{
    public sealed partial class EditMix : ContentDialog
    {
        private MixViewModel Mix
        {
            get
            {
                return DebugHelper.CastAndAssert<MixViewModel>(this.DataContext);
            }
        }

        ObservableCollection<SelectableOption<MixSortOrder>> AllSortTypes = new ObservableCollection<SelectableOption<MixSortOrder>>()
        {
            new SelectableOption<MixSortOrder>(Strings.GetResource("EditMixEditOrderPropertyDefault"), MixSortOrder.None),
            new SelectableOption<MixSortOrder>(Strings.GetResource("EditMixEditOrderPropertyAlbumArtist"), MixSortOrder.ALBUMARTISTNAMESORT),
            new SelectableOption<MixSortOrder>(Strings.GetResource("EditMixEditOrderPropertyAlbum"), MixSortOrder.ALBUMNAMESORT),
            new SelectableOption<MixSortOrder>(Strings.GetResource("EditMixEditOrderPropertyArtist"), MixSortOrder.ARTISTNAMESORT),
            new SelectableOption<MixSortOrder>(Strings.GetResource("EditMixEditOrderPropertyDuration"), MixSortOrder.DURATIONSORT),
            new SelectableOption<MixSortOrder>(Strings.GetResource("EditMixEditOrderPropertyLastPlayed"), MixSortOrder.LASTPLAYEDSORT),
            new SelectableOption<MixSortOrder>(Strings.GetResource("EditMixEditOrderPropertyPlayCount"), MixSortOrder.PLAYCOUNTSORT),
            new SelectableOption<MixSortOrder>(Strings.GetResource("EditMixEditOrderPropertyRating"), MixSortOrder.RATINGSORT),
            new SelectableOption<MixSortOrder>(Strings.GetResource("EditMixEditOrderPropertyTrackName"), MixSortOrder.TRACKNAMESORT),
        };

        ObservableCollection<SelectableOption<MixSortOrder>> AllSortOrders = new ObservableCollection<SelectableOption<MixSortOrder>>()
        {
            new SelectableOption<MixSortOrder>(Strings.GetResource("EditMixEditOrderPropertyDirectionAscending"), MixSortOrder.SORTORDER_ASC),
            new SelectableOption<MixSortOrder>(Strings.GetResource("EditMixEditOrderPropertyDirectionDescending"), MixSortOrder.SORTORDER_DESC),
        };

        public EditMix(MixViewModel mixToEdit)
        {
            this.InitializeComponent();

            this.DataContext = mixToEdit;

            SortTypeComboBox.ItemsSource = AllSortTypes;
            SelectableOption<MixSortOrder> selectedTypeEntry = AllSortTypes.Where((m) => { return (m.Type == (Mix.SortType & MixSortOrder.PROPERTY_MASK) || m.Type == Mix.SortType); }).FirstOrDefault();
            SortTypeComboBox.SelectedItem = selectedTypeEntry;

            SortOrderComboBox.ItemsSource = AllSortOrders;
            SelectableOption<MixSortOrder> selectedOrderEntry = AllSortOrders.Where((m) => { return (m.Type == (Mix.SortType & MixSortOrder.ORDER_MASK) || m.Type == Mix.SortType); }).FirstOrDefault();
            SortOrderComboBox.SelectedItem = selectedOrderEntry;

            RootMixEntry.LoadEvaluator(mixToEdit.RootEvaluator);

            RootMixEntry.TextBotGotFocus += HandleMixLimitTextBoxGotFocus;
            RootMixEntry.TextBotLostFocus += HandleMixLimitTextBoxLostFocus;
        }

        private void HandleContentDialogPrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if ((deleteMix.IsChecked.HasValue && deleteMix.IsChecked.Value) &&
                (deleteMixConfirm.IsChecked.HasValue && deleteMixConfirm.IsChecked.Value))
            {
                LibraryViewModel.Current.DeleteMix(Mix);
            }
            else
            {
                Mix.Name = editMixName.Text;

                if (mixLimitCheckBox.IsChecked.HasValue)
                {
                    Mix.HasLimit = mixLimitCheckBox.IsChecked.Value;
                }

                uint newMixLimit;

                if (uint.TryParse(mixLimitTextBox.Text, out newMixLimit))
                {
                    Mix.Limit = newMixLimit;
                }

                if (mixHiddenCheckBox.IsChecked.HasValue)
                {
                    Mix.IsHidden = mixHiddenCheckBox.IsChecked.Value;
                }

                SelectableOption<MixSortOrder> selectedType = DebugHelper.CastAndAssert<SelectableOption<MixSortOrder>>(SortTypeComboBox.SelectedItem);
                SelectableOption<MixSortOrder> selectedOrder = DebugHelper.CastAndAssert<SelectableOption<MixSortOrder>>(SortOrderComboBox.SelectedItem);

                Mix.SortType = selectedType.Type | selectedOrder.Type;

                IMixEvaluator mixEval = RootMixEntry.ConvertToEvaluator();
                Mix.SetEvaluator(mixEval);

                Mix.Reset();
            }
        }

        private void HandleContentDialogSecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }        

        private void HandleMixLimitTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            Transform stackPanelRestore = DebugHelper.CastAndAssert<Transform>(((TextBox)sender).TransformToVisual(rootGrid));

            double shift = 0;
            if (stackPanelRestore is MatrixTransform)
            {
                shift = (stackPanelRestore as MatrixTransform).Matrix.OffsetY;
            }
            else if (stackPanelRestore is TranslateTransform)
            {
                shift = (stackPanelRestore as TranslateTransform).Y;
            }
            else
            {
                DebugHelper.Alert(new CallerInfo(), "Unexpected transform type {0}", stackPanelRestore.GetType());
            }

            TranslateTransform shiftDown = new TranslateTransform();
            shiftDown.Y = 90 - shift;

            TransformGroup group = new TransformGroup();

            group.Children.Add(shiftDown);

            rootGrid.RenderTransform = group;

            RootScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
        }

        private void HandleMixLimitTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            rootGrid.RenderTransform = null;

            RootScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
        }

        private void HandleEditMixNameGotFocus(object sender, RoutedEventArgs e)
        {
            RootScrollViewer.VerticalScrollMode = ScrollMode.Disabled;
        }

        private void HandleEditMixNameLostFocus(object sender, RoutedEventArgs e)
        {
            RootScrollViewer.VerticalScrollMode = ScrollMode.Enabled;
        }

        private void HandleDeleteMixChecked(object sender, RoutedEventArgs e)
        {
            deleteMixConfirm.Visibility = Visibility.Visible;
        }

        private void HandleDeleteMixUnchecked(object sender, RoutedEventArgs e)
        {
            deleteMixConfirm.IsChecked = false;
            deleteMixConfirm.Visibility = Visibility.Collapsed;
        }
    }
}
