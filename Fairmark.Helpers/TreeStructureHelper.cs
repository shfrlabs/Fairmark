using Fairmark.Models;
using System;
using System.Collections.ObjectModel;
using Windows.Storage;

namespace Fairmark.Helpers
{
    public static class TreeStructureHelper
    {
        public static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        public static ObservableCollection<TreeNode> nodes = new ObservableCollection<TreeNode>();

        public static void InitializeTreeStructure()
        {
            if (localSettings.Values.ContainsKey("treeStructure"))
            {
                var savedNodes = localSettings.Values["treeStructure"] as ObservableCollection<TreeNode>;
                if (savedNodes != null)
                {
                    nodes = savedNodes;
                }
            }
            nodes.CollectionChanged += (s, e) =>
            {
                SaveTreeStructure();
            };
        }

        private static void SaveTreeStructure()
        {
            localSettings.Values["treeStructure"] = nodes;
        }
    }
}