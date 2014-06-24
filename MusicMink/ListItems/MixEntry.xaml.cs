using MusicMink.Collections;
using MusicMink.ViewModels;
using MusicMink.ViewModels.MixEvaluators;
using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Enums;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System;
using System.Linq;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace MusicMink.ListItems
{
    public sealed partial class MixEntry : UserControl
    {
        ObservableCollection<SelectableOption<MixType>> AllMixTypes = new ObservableCollection<SelectableOption<MixType>>()
        {
            new SelectableOption<MixType>(Strings.GetResource("MixEntryMixTypeNone"), MixType.None),

            new SelectableOption<MixType>(Strings.GetResource("MixEntryMixTypeAlbum"), MixType.ALBUM_SUBTYPE),
            new SelectableOption<MixType>(Strings.GetResource("MixEntryMixTypeArtist"), MixType.ARTIST_SUBTYPE),
            new SelectableOption<MixType>(Strings.GetResource("MixEntryMixTypeAlbumArtist"), MixType.ALBUMARTIST_SUBTYPE),
            new SelectableOption<MixType>(Strings.GetResource("MixEntryMixTypeTrack"), MixType.TRACK_SUBTYPE),

            new SelectableOption<MixType>(Strings.GetResource("MixEntryMixTypeRating"), MixType.RATING_SUBTYPE),
            new SelectableOption<MixType>(Strings.GetResource("MixEntryMixTypeLength"), MixType.LENGTH_SUBTYPE),
            new SelectableOption<MixType>(Strings.GetResource("MixEntryMixTypePlayCount"), MixType.PLAYCOUNT_SUBTYPE),

            new SelectableOption<MixType>(Strings.GetResource("MixEntryMixTypeLastPlayed"), MixType.LASTPLAYED_SUBTYPE),

            new SelectableOption<MixType>(Strings.GetResource("MixEntryMixTypePlaylistMember"), MixType.PLAYLISTMEMBER_SUBTYPE),
            new SelectableOption<MixType>(Strings.GetResource("MixEntryMixTypeMixMember"), MixType.MIXMEMBER_SUBTYPE),
        };



        ObservableCollection<SelectableOption<StringEvalType>> StringInfo = new ObservableCollection<SelectableOption<StringEvalType>>()
        {
            new SelectableOption<StringEvalType>(Strings.GetResource("MixEntryStringTypeEquals"), StringEvalType.Equal),
            new SelectableOption<StringEvalType>(Strings.GetResource("MixEntryStringTypeContains"), StringEvalType.SubString),
            new SelectableOption<StringEvalType>(Strings.GetResource("MixEntryStringTypeEndsWith"), StringEvalType.EndsWith),
            new SelectableOption<StringEvalType>(Strings.GetResource("MixEntryStringTypeStartsWith"), StringEvalType.StartsWith),
        };

        ObservableCollection<SelectableOption<NumericEvalType>> NumericInfo = new ObservableCollection<SelectableOption<NumericEvalType>>()
        {
            new SelectableOption<NumericEvalType>(Strings.GetResource("MixEntryNumericTypeStrictlyLessThan"), NumericEvalType.StrictLess),
            new SelectableOption<NumericEvalType>(Strings.GetResource("MixEntryNumericTypeLessThan"), NumericEvalType.Less),
            new SelectableOption<NumericEvalType>(Strings.GetResource("MixEntryNumericTypeEqual"), NumericEvalType.Equal),
            new SelectableOption<NumericEvalType>(Strings.GetResource("MixEntryNumericTypeMoreThan"), NumericEvalType.More),
            new SelectableOption<NumericEvalType>(Strings.GetResource("MixEntryNumericTypeStrictlyMoreThan"), NumericEvalType.StrictMore),
        };

        private int Depth;
        private MixEntry ParentMixEntry;

        const int MAX_DEPTH = 3;

        public MixEntry() : this(null, 0)
        {

        }

        public MixEntry(MixEntry parent, int depth)
        {
            this.InitializeComponent();

            ParentMixEntry = parent;

            if (depth < MAX_DEPTH)
            {
                AllMixTypes.Add(new SelectableOption<MixType>(Strings.GetResource("MixEntryMixTypeAll"), MixType.And));
                AllMixTypes.Add(new SelectableOption<MixType>(Strings.GetResource("MixEntryMixTypeAny"), MixType.Or));
                AllMixTypes.Add(new SelectableOption<MixType>(Strings.GetResource("MixEntryMixTypeNot"), MixType.Not));
            }

            MixEntryType.ItemsSource = AllMixTypes;
            MixEntryType.SelectedIndex = 0;

            StringPicker.ItemsSource = StringInfo;
            StringPicker.SelectedIndex = 0;

            NumericPicker.ItemsSource = NumericInfo;
            NumericPicker.SelectedIndex = 0;

            PlaylistMemberPicker.ItemsSource = LibraryViewModel.Current.PlaylistCollection;
            PlaylistMemberPicker.SelectedIndex = 0;

            MixMemberPicker.ItemsSource = LibraryViewModel.Current.MixCollection;
            MixMemberPicker.SelectedIndex = 0;

            NumericValue.Text = "0";

            Depth = depth;

            if (Depth > 0)
            {
                VisualStateManager.GoToState(this, "MixIsChild", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "MixIsRoot", false);
            }
        }

        public void LoadEvaluator(IMixEvaluator evaluator)
        {
            MixType mixType = evaluator.MixType;

            SelectableOption<MixType> selectedMixTypeEntry = AllMixTypes.Where((m) => { return (m.Type == (mixType & MixType.SUBTYPE_MASK) || m.Type == mixType); }).FirstOrDefault();
            MixEntryType.SelectedItem = selectedMixTypeEntry;

            switch (mixType & MixType.TYPE_MASK)
            {
                case MixType.NUMBER_TYPE:
                    NumericMixEvaluator<IComparable> numericEvaluator = DebugHelper.CastAndAssert<NumericMixEvaluator<IComparable>>(evaluator);

                    SelectableOption<NumericEvalType> selectedNumericTypeEntry = NumericInfo.Where((m) => { return (m.Type == numericEvaluator.EvalType); }).FirstOrDefault();
                    NumericPicker.SelectedItem = selectedNumericTypeEntry;

                    UpdateNumericStartingValue(mixType, numericEvaluator.Target);
                    return;
                case MixType.STRING_TYPE:
                    StringMixEvaluator stringEvaluator = DebugHelper.CastAndAssert<StringMixEvaluator>(evaluator);

                    SelectableOption<StringEvalType> selectedStringTypeEntry = StringInfo.Where((m) => { return (m.Type == stringEvaluator.EvalType); }).FirstOrDefault();
                    StringPicker.SelectedItem = selectedStringTypeEntry;

                    StringValue.Text = stringEvaluator.Target;
                    return;
                case MixType.NESTED_TYPE:
                    NestedMixEvaluator nestedEvaluator = DebugHelper.CastAndAssert<NestedMixEvaluator>(evaluator);

                    foreach (IMixEvaluator mixEvaluator in nestedEvaluator.Mixes)
                    {
                        MixEntry nestedEntry = new MixEntry(this, Depth + 1);
                        nestedEntry.LoadEvaluator(mixEvaluator);
                        NestedList.Children.Add(nestedEntry);
                    }

                    return;
                case MixType.MEMBER_TYPE:
                    return;
                case MixType.RANGE_TYPE:
                    RangeMixEvaluator rangeEvaluator = DebugHelper.CastAndAssert<RangeMixEvaluator>(evaluator);

                    RangeValue.Text = rangeEvaluator.Target.ToString();

                    return;
                default:
                    DebugHelper.Assert(new CallerInfo(), mixType == MixType.None, "Unexpected mix type: {0}", mixType);
                    VisualStateManager.GoToState(this, "UnknownSelected", false);
                    return;
            }
        }

        private void MixEntryType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MixEntryType.SelectedItem == null) return;

            SelectableOption<MixType> selectedType = DebugHelper.CastAndAssert<SelectableOption<MixType>>(MixEntryType.SelectedItem);

            switch (selectedType.Type & MixType.TYPE_MASK)
            {
                case MixType.NUMBER_TYPE:
                    VisualStateManager.GoToState(this, "NumberSelected", false);

                    UpdateNumericState(selectedType.Type);
                    return;
                case MixType.STRING_TYPE:
                    VisualStateManager.GoToState(this, "StringSelected", false);
                    return;
                case MixType.NESTED_TYPE:
                    VisualStateManager.GoToState(this, "NestedSelected", false);
                    return;
                case MixType.RANGE_TYPE:
                    VisualStateManager.GoToState(this, "RangeSelected", false);
                    return;
                case MixType.MEMBER_TYPE:
                    VisualStateManager.GoToState(this, "MemberSelected", false);

                    UpdateMemberState(selectedType.Type);
                    return;
                default:
                    DebugHelper.Assert(new CallerInfo(), selectedType.Type == MixType.None, "Unexpected mix type: {0}", selectedType.Type);
                    VisualStateManager.GoToState(this, "UnknownSelected", false);
                    return;
            }
        }
    
        private void UpdateNumericState(MixType type)
        {
            switch (type & MixType.SUBTYPE_MASK)
            {
                case MixType.LENGTH_SUBTYPE:
                    VisualStateManager.GoToState(this, "NumericDurationSelected", false);
                    break;
                case MixType.RATING_SUBTYPE:
                    VisualStateManager.GoToState(this, "NumericRatingSelected", false);
                    break;
                case MixType.PLAYCOUNT_SUBTYPE:
                    VisualStateManager.GoToState(this, "NumericPlayCountSelected", false);
                    break;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected numeric mix type: {0}", type);
                    VisualStateManager.GoToState(this, "NumericUnknownSelected", false);
                    return;
            }
        }

        private void UpdateMemberState(MixType type)
        {
            switch (type & MixType.SUBTYPE_MASK)
            {
                case MixType.PLAYLISTMEMBER_SUBTYPE:
                    VisualStateManager.GoToState(this, "MemberPlaylistSelected", false);
                    break;
                case MixType.MIXMEMBER_SUBTYPE:
                    VisualStateManager.GoToState(this, "MemberMixSelected", false);
                    break;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected numeric mix type: {0}", type);
                    VisualStateManager.GoToState(this, "MemberUnknownSelected", false);
                    return;
            }
        }

        private void UpdateNumericStartingValue(MixType type, IComparable target)
        {
            switch (type & MixType.SUBTYPE_MASK)
            {
                case MixType.LENGTH_SUBTYPE:
                    NumericValue.Text = target.ToString();
                    break;
                case MixType.RATING_SUBTYPE:
                    NumericStarRater.Rating = DebugHelper.CastAndAssert<uint>(target);
                    break;
                case MixType.PLAYCOUNT_SUBTYPE:
                    NumericValue.Text = target.ToString();
                    break;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected numeric mix type: {0}", type);
                    NumericValue.Text = "0";
                    return;
            }
        }
    
        public IMixEvaluator ConvertToEvaluator()
        {
            if (MixEntryType.SelectedItem == null) return new NoneMixEvaluator();

            SelectableOption<MixType> selectedType = DebugHelper.CastAndAssert<SelectableOption<MixType>>(MixEntryType.SelectedItem);

            switch (selectedType.Type & MixType.TYPE_MASK)
            {
                case MixType.NUMBER_TYPE:
                    if (NumericPicker.SelectedItem == null) return new NoneMixEvaluator();

                    SelectableOption<NumericEvalType> selectedNumericType = DebugHelper.CastAndAssert<SelectableOption<NumericEvalType>>(NumericPicker.SelectedItem);

                    string newTarget = ConvertNumericValueToString(selectedType.Type);

                    return MixViewModel.NumericMixEntryModelToMixEvaluator(selectedType.Type | NumericMixEvaluator<int>.NumericEvalTypeToMixTypeVariant(selectedNumericType.Type), newTarget);
                case MixType.STRING_TYPE:
                    if (StringPicker.SelectedItem == null) return new NoneMixEvaluator();

                    SelectableOption<StringEvalType> selectedStringType = DebugHelper.CastAndAssert<SelectableOption<StringEvalType>>(StringPicker.SelectedItem);

                    return MixViewModel.StringMixEntryModelToMixEvaluator(selectedType.Type | StringMixEvaluator.StringEvalTypeToMixTypeVariant(selectedStringType.Type), StringValue.Text);
                case MixType.NESTED_TYPE:
                    List<IMixEvaluator> mixes = new List<IMixEvaluator>();

                    foreach (object listItem in NestedList.Children)
                    {
                        MixEntry listItemAsMixEntry = DebugHelper.CastAndAssert<MixEntry>(listItem);

                        mixes.Add(listItemAsMixEntry.ConvertToEvaluator());
                    }

                    return MixViewModel.NestedMixEntryModelToMixEvaluator(selectedType.Type, mixes);
                case MixType.RANGE_TYPE:
                    return MixViewModel.RangeMixEntryModelToMixEvaluator(selectedType.Type | RangeMixEvaluator.RangeEvalTypeToMixTypeVariant(RangeEvalType.Days), RangeValue.Text);
                case MixType.MEMBER_TYPE:
                    if (selectedType.Type == MixType.PLAYLISTMEMBER_SUBTYPE)
                    {
                        if (PlaylistMemberPicker.SelectedItem == null) return new NoneMixEvaluator();

                        PlaylistViewModel selectedPlaylist = DebugHelper.CastAndAssert<PlaylistViewModel>(PlaylistMemberPicker.SelectedItem);

                        return MixViewModel.MemberMixEntryModelToMixEvaluator(MixType.PlaylistMemberContains, selectedPlaylist.PlaylistId.ToString());
                    }
                    else if (selectedType.Type == MixType.MIXMEMBER_SUBTYPE)
                    {
                        if (MixMemberPicker.SelectedItem == null) return new NoneMixEvaluator();

                        MixViewModel selectedMix = DebugHelper.CastAndAssert<MixViewModel>(MixMemberPicker.SelectedItem);

                        return MixViewModel.MemberMixEntryModelToMixEvaluator(MixType.MixMemberContains, selectedMix.MixId.ToString());
                    }
                    else
                    {
                        DebugHelper.Alert(new CallerInfo(), "Unexpected member type: {0}", selectedType);
                        return new NoneMixEvaluator();
                    }
                default:
                    DebugHelper.Assert(new CallerInfo(), selectedType.Type == MixType.None, "Unexpected mix type: {0}", selectedType.Type);
                    return new NoneMixEvaluator();
            }
        }


        private string ConvertNumericValueToString(MixType type)
        {
            switch (type & MixType.SUBTYPE_MASK)
            {
                case MixType.LENGTH_SUBTYPE:
                    return NumericValue.Text;
                case MixType.RATING_SUBTYPE:
                    return NumericStarRater.Rating.ToString();
                case MixType.PLAYCOUNT_SUBTYPE:
                    return NumericValue.Text;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected numeric mix type: {0}", type);
                    return "0";
            }
        }


        private IMixEvaluator ConvertToStringEvaluator(MixType selectedType)
        {
            throw new System.NotImplementedException();
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            NestedList.Children.Add(new MixEntry(this, Depth + 1));
        }

        private void RemoveMixButton_Click(object sender, RoutedEventArgs e)
        {
            if (ParentMixEntry != null)
            {
                ParentMixEntry.RemoveEntry(this);
            }
        }

        private void RemoveEntry(MixEntry mixEntry)
        {
            NestedList.Children.Remove(mixEntry);
        }   
    }
}
