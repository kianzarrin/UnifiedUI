namespace UnifiedUI.Util {
    using ColossalFramework.Plugins;
    using ICities;
    using KianCommons;
    using KianCommons.Plugins;
    using System;
    using static KianCommons.Plugins.PluginUtil;

    public class PluginUtil {
        public static PluginUtil Instance;
        public PluginMod NodeController = new PluginMod("Node Controller");
        public PluginMod NetworkDetective = new PluginMod("Network detective");
        public PluginMod PedestrianBridge = new PluginMod("Pedestrian Bridge");
        public PluginMod IntersectionMarking = new PluginMod("Intersection Marking", 2140418403ul);
        public PluginMod RoundaboutBuilder = new PluginMod("Roundabout Builder");

        public static void Init() {
            Instance = new PluginUtil();
            //Log.Info("Pluggins:");
            //foreach (PluginManager.PluginInfo current in PluginManager.instance.GetPluginsInfo()) {
            //    Log.Info($"name:{current.GetModName()} id:{current.publishedFileID.AsUInt64}");
            //}
        }
    }

    [Obsolete]
    public class PluginMod {
        public PluginManager.PluginInfo PluginInfo { get; set; }
        public bool IsActive => PluginInfo?.isEnabled ?? false;
        public bool Exists => PluginInfo != null;

        public ulong[] SearchIDs { get; private set; }

        public string SearchName { get; private set; }

        public SearchOptionT SearchOptios { get; private set; }

        public PluginMod(string searchName, ulong searchId, SearchOptionT searchOptions = DefaultsearchOptions) :
            this(searchName, new[] { searchId }, searchOptions) { }

        public PluginMod(string searchName, ulong[] searchIds = null, SearchOptionT searchOptions = DefaultsearchOptions) {
            SearchName = searchName;
            SearchIDs = searchIds;
            SearchOptios = searchOptions;
            PluginInfo = GetPlugin(searchName, searchIds, searchOptions);
        }
    }
}
