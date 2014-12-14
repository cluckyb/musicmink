using MusicMinkAppLayer.Diagnostics;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace MusicMink.Common
{
    /// <summary>
    /// LongListSelector doesn't remember its position across navigation / suspend+resume.
    /// This creates a poor experience for the user, so use a helper to help remember last
    /// visited locations and return to them (or at least as close as possible)
    /// </summary>
    public class ListRestoreHelper
    {
        private const string LAST_INDEX_KEY = "LastVisibleIndex_";
        private int lastFrameInViewIndex = -1;

        private ListViewBase Root;

        private string Key;

        private string FullKey
        {
            get
            {
                return LAST_INDEX_KEY + Key;
            }
        }

        public ListRestoreHelper(ListViewBase root)
        {
            Root = root;
            Key = string.Empty;
        }

        public ListRestoreHelper(ListViewBase root, string key)
        {
            Root = root;
            Key = key;
        }

        public void ScrollSavedItemIntoView()
        {
            if (lastFrameInViewIndex >= 0)
            {
                if (Root.Items.Count > lastFrameInViewIndex)
                {
                    Root.ScrollIntoView(Root.Items[lastFrameInViewIndex]);
                }
            }
        }

        internal void LoadState(Dictionary<string, object> pageState)
        {
            object lastIndex;

            if (pageState != null && pageState.TryGetValue(FullKey, out lastIndex))
            {
                lastFrameInViewIndex = DebugHelper.CastAndAssert<int>(lastIndex);
            }
        }

        internal void SaveState(Dictionary<string, object> pageState)
        {
            if (Root.ItemsPanelRoot == null) return;

            ItemsStackPanel stackPanel = Root.ItemsPanelRoot as ItemsStackPanel;

            if (stackPanel != null)
            {
                pageState.Add(FullKey, stackPanel.LastVisibleIndex);
            }
            else
            {
                ItemsWrapGrid wrapGrid = Root.ItemsPanelRoot as ItemsWrapGrid;

                if (wrapGrid != null)
                {
                    pageState.Add(FullKey, wrapGrid.LastVisibleIndex);
                }
                else
                {
                    DebugHelper.Alert(new CallerInfo(), "Unexpected root type: {0}", Root.ItemsPanelRoot.GetType());
                }
            }

        }
    }
}
