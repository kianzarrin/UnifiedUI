namespace UnitedUI.Util {
    using ColossalFramework.Plugins;
    using KianCommons;

    public class PluginUtil {
        public static PluginUtil Instance;
        public PluginMod NodeController = new PluginMod("Node Controller");
        public PluginMod NetworkDetective = new PluginMod("Network detective");
        public PluginMod PedestrianBridge = new PluginMod("Pedestrian Bridge");
        public PluginMod IntersectionMarkup = new PluginMod("Intersection Marking", 2140418403ul);
        public PluginMod RoundaboutBuilder = new PluginMod("Roundabout Builder");

        public static void Init() => Instance = new PluginUtil();
    }

    public enum SearchModeT {
        Contains,
        StartsWidth,
    }

    public class PluginMod {
        public PluginManager.PluginInfo PluginInfo { get; set; }
        public bool IsActive => PluginInfo?.isEnabled ?? false;
        public bool Exists => PluginInfo != null;

        public ulong[] SearchIDs { get; private set; }

        public string SearchName { get; private set; }

        public SearchModeT SearchMode { get; private set; }

        public PluginMod(string searchName, ulong searchId, SearchModeT searchMode = SearchModeT.Contains) :
            this(searchName, new[] { searchId }, searchMode) { }

        public PluginMod(string searchName, ulong[] searchIds = null, SearchModeT searchMode = SearchModeT.Contains) {
            SearchName = searchName;
            SearchIDs = searchIds;
            SearchMode = searchMode;
            PluginInfo = null;
            foreach (PluginManager.PluginInfo current in PluginManager.instance.GetPluginsInfo()) {
                if (Match(current)) {
                    PluginInfo = current;
                    Log.Info("Found pluggin:" + current.name);
                    return;
                }
            }
        }
        public bool Match(PluginManager.PluginInfo plugin) {
            string name1 = plugin.name.ToLower();
            string name2 = SearchName.ToLower();
            if (SearchMode == SearchModeT.Contains) {
                if (name1.Contains(name2))
                    return true;
            } else if (SearchMode == SearchModeT.StartsWidth) {
                if (name1.StartsWith(name2))
                    return true;
            }
            if (SearchIDs != null) {
                foreach (var id in SearchIDs) {
                    if (id == plugin.publishedFileID.AsUInt64)
                        return true;
                }
            }
            return false;
        }
    }
}
