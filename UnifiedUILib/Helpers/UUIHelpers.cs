using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("NetworkMultitool")]
[assembly: InternalsVisibleTo("NodeController")]
[assembly: InternalsVisibleTo("NodeMarkup")]
[assembly: InternalsVisibleTo("BuildingSpawnPoints")]
[assembly: InternalsVisibleTo("NoBigTruck")] // is it needed?

namespace UnifiedUI.Helpers {
    using ColossalFramework;
    using ColossalFramework.Plugins;
    using ColossalFramework.UI;
    using ICities;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using UnityEngine;
    using KianCommons.UI;


    public static class UUIHelpers {
        #region macsurgey compatibility

        [Obsolete("use UnifiedUI.Helpers.UUISprites instead", error:true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal struct UUISprites {
            public UITextureAtlas Atlas;
            public string NormalSprite, HoveredSprite, PressedSprite, DisabledSprite;
            public Helpers.UUISprites Convert() {
                return new Helpers.UUISprites {
                    Atlas = Atlas,
                    NormalSprite = NormalSprite, HoveredSprite = HoveredSprite, PressedSprite = PressedSprite, DisabledSprite = DisabledSprite
                };
            }
        }

        [Obsolete("use UnifiedUI.Helpers.UUISprites instead", error: true)]
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        internal static UIComponent RegisterToolButton(
            string name, string groupName, string tooltip, UUISprites sprites, ToolBase tool,
            SavedInputKey activationKey, IEnumerable<SavedInputKey> activeKeys) {
            var hotkeys = new UUIHotKeys { ActivationKey = activationKey };
            foreach (var item in activeKeys)
                hotkeys.AddInToolKey(item);

            return RegisterToolButton(
                name: name,
                groupName: groupName,
                tooltip: tooltip,
                tool: tool,
                sprites: sprites.Convert(),
                hotkeys: hotkeys);
        }
        #endregion

        const string UUI_NAME = "UnifiedUI.API.UUIAPI";

        const string ASSEMLY_NAME = "UnifiedUILib";

        /// <typeparam name="TDelegate">delegate type</typeparam>
        /// <returns>Type[] representing arguments of the delegate.</returns>
        internal static Type[] GetParameterTypes<TDelegate>()
            where TDelegate : Delegate =>
            typeof(TDelegate)
            .GetMethod("Invoke")
            .GetParameters()
            .Select(p => p.ParameterType)
            .ToArray();

        /// <summary>
        /// Gets directly declared method based on a delegate that has
        /// the same name as the target method
        /// </summary>
        /// <param name="type">the class/type where the method is declared</param>
        /// <param name="name">the name of the method</param>
        internal static MethodInfo GetMethod<TDelegate>(this Type type, string name) where TDelegate : Delegate {
            return type.GetMethod(
                name,
                types: GetParameterTypes<TDelegate>())
                ?? throw new Exception("could not find method " + name);
        }

        internal static TDelegate CreateDelegate<TDelegate>(Type type, string name) where TDelegate : Delegate {
            var method = type.GetMethod<TDelegate>(name);
            return (TDelegate)Delegate.CreateDelegate(typeof(TDelegate), method);
        }

        internal static IEnumerable<PluginManager.PluginInfo> Plugins => PluginManager.instance.GetPluginsInfo();

        internal static PluginManager.PluginInfo GetUUIPlugin() =>
            Plugins.FirstOrDefault(p => p.IsUUIMod());


        static bool IsUUIMod(this PluginManager.PluginInfo p) =>
            p?.userModInstance?.GetType()?.Assembly?.GetType(UUI_NAME) != null;

        static bool IsUUILib(this Assembly assembly) => assembly.GetName().Name == ASSEMLY_NAME;

        internal static Assembly GetUUILib() {
            Assembly ret = null;
            if (IsUUIEnabled()) {
                ret  = GetUUIPlugin().GetAssemblies().First(IsUUILib);
                Debug.Log($"using {ret} from UnifiedUI Mod");
                return ret;
            }


            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var assembly in assemblies.Where(IsUUILib)) {
                if (ret == null || ret.GetName().Version < assembly.GetName().Version)
                    ret = assembly;
            }
            if (ret == null ) {
                string sAssemblies = string.Join("\n", assemblies.Select(asm => asm.ToString()).ToArray());
                throw new Exception($"failed to get latest {ASSEMLY_NAME}. assemblies are:\n" + sAssemblies);
            }
            Debug.Log($"using latest {ASSEMLY_NAME} version: {ret}");
            return ret;
        }

        private static Type uui_ = null;
        internal static Type GetUUI() => uui_ ??= GetUUILib().GetType(UUI_NAME, throwOnError: true);

        #region register with FileName
        internal delegate UIComponent RegisterCustomHandler
            (string name, string groupName, string tooltip, string spritefile,
            Action<bool> onToggle, Action<ToolBase> onToolChanged,
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys);
        internal delegate UIComponent RegisterToolHandler
            (string name, string groupName, string tooltip, string spritefile, ToolBase tool,
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys);



        /// <summary>
        /// register a button to tie to the given tool.
        /// </summary>
        /// <param name="name">game object name for button</param>
        /// <param name="groupName">the group under which button will be added. use null to addd to the default gorup.</param>
        /// <param name="spritefile">full path to the file that contains 4 40x40x button sprites(see example)</param>
        /// <param name="tool">the tool to tie the buttont to.</param>
        /// <param name="activationKey">hot key to trigger the button</param>
        /// <param name="activeKeys">turn off these hotkeys in other mods</param>
        /// <returns>component containing the button. you can hide this component if necessary.</returns>
        [Obsolete]
        public static UIComponent RegisterToolButton(
            string name, string groupName, string tooltip, string spritefile, ToolBase tool,
            SavedInputKey activationKey = null, Dictionary<SavedInputKey, Func<bool>> activeKeys = null) {
            var Register = CreateDelegate<RegisterToolHandler>(GetUUI(), "Register");
            return Register(
                name: name,
                groupName: groupName,
                tooltip: tooltip,
                spritefile: spritefile,
                tool: tool,
                activationKey: activationKey,
                activeKeys: activeKeys);
        }

        /// <summary>
        /// register a button to tie to the given tool.
        /// </summary>
        /// <param name="name">game object name for button</param>
        /// <param name="groupName">the group under which button will be added. use null to addd to the default gorup.</param>
        /// <param name="spritefile">full path to the file that contains 4 40x40x button sprites(see example)</param>
        /// <param name="tool">the tool to tie the buttont to.</param>
        /// <param name="activationKey">hot key to trigger the button</param>
        /// <param name="activeKeys">turn off these hotkeys in other mods</param>
        /// <returns>component containing the button. you can hide this component if necessary.</returns>
        [Obsolete]
        public static UIComponent RegisterToolButton(
            string name, string groupName, string tooltip, string spritefile, ToolBase tool,
            SavedInputKey activationKey, IEnumerable<SavedInputKey> activeKeys) {
            var activeKeys2 = new Dictionary<SavedInputKey, Func<bool>>();
            foreach (var key in activeKeys)
                activeKeys2[key] = null;

            return RegisterToolButton(
                name: name,
                groupName: groupName,
                tooltip: tooltip,
                spritefile: spritefile,
                tool: tool,
                activationKey: activationKey,
                activeKeys: activeKeys2);
        }


        /// <summary>
        /// register a custom button .
        /// </summary>
        /// <param name="name">game object name for button</param>
        /// <param name="groupName">the group under which button will be added. use null to addd to the default gorup.</param>
        /// <param name="spritefile">full path to the file that contains 4 40x40x button sprites(see example)</param>
        /// <param name="onToggle">call-back for when the button is activated/deactivated</param>
        /// <param name="onToolChanged">call-back for when any active tool changes.</param>
        /// <param name="activationKey">hot key to trigger the button</param>
        /// <param name="activeKeys">hotkey->active dictionary. turns off these hotkeys in other mods while active</param>
        /// <returns>wrapper for the button which you can use to change the its state.</returns>
        [Obsolete]
        public static UUICustomButton RegisterCustomButton(
            string name, string groupName, string tooltip, string spritefile,
            Action<bool> onToggle, Action<ToolBase> onToolChanged = null,
            SavedInputKey activationKey = null, Dictionary<SavedInputKey, Func<bool>> activeKeys = null) {
            var Register = CreateDelegate<RegisterCustomHandler>(GetUUI(), "Register");
            UIComponent component = Register(
                name: name,
                groupName: groupName,
                tooltip: tooltip,
                spritefile: spritefile,
                onToggle: onToggle,
                onToolChanged: onToolChanged,
                activationKey: activationKey,
                activeKeys: activeKeys);
            return new UUICustomButton(component);
        }

        /// <summary>
        /// register a custom button .
        /// </summary>
        /// <param name="name">game object name for button</param>
        /// <param name="groupName">the group under which button will be added. use null to addd to the default gorup.</param>
        /// <param name="spritefile">full path to the file that contains 4 40x40x button sprites(see example)</param>
        /// <param name="onToggle">call-back for when the button is activated/deactivated</param>
        /// <param name="onToolChanged">call-back for when any active tool changes.</param>
        /// <param name="activationKey">hot key to trigger the button</param>
        /// <param name="activeKeys">hotkey->active dictionary. turns off these hotkeys in other mods while active</param>
        /// <returns>wrapper for the button which you can use to change the its state.</returns>
        [Obsolete]
        public static UUICustomButton RegisterCustomButton(
            string name, string groupName, string tooltip, string spritefile,
            Action<bool> onToggle, Action<ToolBase> onToolChanged,
            SavedInputKey activationKey, IEnumerable<SavedInputKey> activeKeys) {
            var activeKeys2 = new Dictionary<SavedInputKey, Func<bool>>();
            foreach (var key in activeKeys)
                activeKeys2[key] = null;

            return RegisterCustomButton(
                name: name,
                groupName: groupName,
                tooltip: tooltip,
                spritefile: spritefile,
                onToggle: onToggle,
                onToolChanged: onToolChanged,
                activationKey: activationKey,
                activeKeys: activeKeys2);
        }
        #endregion

        #region Register with atlas
        internal delegate UIComponent RegisterCustomHandler2
            (string name, string groupName, string tooltip,
            UITextureAtlas atlas, string[] spriteNames,
            Action<bool> onToggle, Action<ToolBase> onToolChanged,
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys);
        internal delegate UIComponent RegisterToolHandler2
            (string name, string groupName, string tooltip, ToolBase tool,
            UITextureAtlas atlas, string[] spriteNames, 
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys);

        /// <summary>
        /// register a button to tie to the given tool.
        /// </summary>
        /// <param name="name">game object name for button</param>
        /// <param name="groupName">the group under which button will be added. use null to addd to the default gorup.</param>
        /// <param name="tool">the tool to tie the buttont to.</param>
        /// <returns>component containing the button. you can hide this component if necessary.</returns>
        [Obsolete]
        public static UIComponent RegisterToolButton(
            string name, string groupName, string tooltip, ToolBase tool, Helpers.UUISprites sprites, UUIHotKeys hotkeys = null) {
            var Register = CreateDelegate<RegisterToolHandler2>(GetUUI(), "Register");
            return Register(
                name: name,
                groupName: groupName,
                tooltip: tooltip,
                tool: tool,
                atlas: sprites.Atlas,
                spriteNames: sprites.SpriteNames,
                activationKey: hotkeys?.ActivationKey,
                activeKeys: hotkeys?.InToolKeys);
        }

        /// <summary>
        /// register a custom button .
        /// </summary>
        /// <param name="name">game object name for button</param>
        /// <param name="groupName">the group under which button will be added. use null to addd to the default gorup.</param>
        /// <param name="onToggle">call-back for when the button is activated/deactivated</param>
        /// <param name="onToolChanged">call-back for when any active tool changes.</param>
        /// <returns>wrapper for the button which you can use to change the its state.</returns>
        [Obsolete]
        public static UUICustomButton RegisterCustomButton(
            string name, string groupName, string tooltip, Helpers.UUISprites sprites,
            Action<bool> onToggle, Action<ToolBase> onToolChanged = null,
            UUIHotKeys hotkeys = null) {
            var Register = CreateDelegate<RegisterCustomHandler2>(GetUUI(), "Register");
            UIComponent component = Register(
                name: name,
                groupName: groupName,
                tooltip: tooltip,
                atlas: sprites.Atlas,
                spriteNames: sprites.SpriteNames,
                onToggle: onToggle,
                onToolChanged: onToolChanged,
                activationKey: hotkeys?.ActivationKey,
                activeKeys: hotkeys?.InToolKeys);
            return new UUICustomButton(component);
        }

        #endregion

        #region Register with Icon
        internal delegate UIComponent RegisterToolHandler3
            (string name, string groupName, string tooltip, ToolBase tool,
             Texture2D texture,
             SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys);
        internal delegate UIComponent RegisterCustomHandler3
            (string name, string groupName, string tooltip, Texture2D texture,
            Action<bool> onToggle, Action<ToolBase> onToolChanged,
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys);

        /// <summary>
        /// register a button to tie to the given tool.
        /// </summary>
        /// <param name="name">game object name for button. must be unique.</param>
        /// <param name="groupName">the group under which button will be added. use null to addd to the default gorup.</param>
        /// <param name="tool">the tool to tie the buttont to.</param>
        /// <returns>component containing the button. you can hide this component if necessary.</returns>
        public static UIComponent RegisterToolButton(
            string name, string groupName, string tooltip, ToolBase tool, Texture2D icon, UUIHotKeys hotkeys = null) {
            var Register = CreateDelegate<RegisterToolHandler3>(GetUUI(), "Register");
            return Register(
                name: name,
                groupName: groupName,
                tooltip: tooltip,
                tool: tool,
                texture: icon,
                activationKey: hotkeys?.ActivationKey,
                activeKeys: hotkeys?.InToolKeys);
        }

        /// <summary>
        /// register a custom button .
        /// </summary>
        /// <param name="name">game object name for button</param>
        /// <param name="groupName">the group under which button will be added. use null to addd to the default gorup.</param>
        /// <param name="onToggle">call-back for when the button is activated/deactivated</param>
        /// <param name="onToolChanged">call-back for when any active tool changes.</param>
        /// <returns>wrapper for the button which you can use to change the its state.</returns>
        public static UUICustomButton RegisterCustomButton(
            string name, string groupName, string tooltip, Texture2D icon,
            Action<bool> onToggle, Action<ToolBase> onToolChanged = null,
            UUIHotKeys hotkeys = null) {
            var Register = CreateDelegate<RegisterCustomHandler3>(GetUUI(), "Register");
            UIComponent component = Register(
                name: name,
                groupName: groupName,
                tooltip: tooltip,
                texture: icon,
                onToggle: onToggle,
                onToolChanged: onToolChanged,
                activationKey: hotkeys?.ActivationKey,
                activeKeys: hotkeys?.InToolKeys);
            return new UUICustomButton(component);
        }

        #endregion


        internal delegate void AttachAlienHandler(UIComponent alien, string groupName);

        public static void AttachAlien(UIComponent alien, string groupName) {
            var attachAlien = CreateDelegate<AttachAlienHandler>(GetUUI(), "AttachAlien");
            attachAlien(alien: alien, groupName: groupName);
        }

        internal delegate void RegisterHotkeysHandler(Action onToggle,
            SavedInputKey activationKey, Dictionary<SavedInputKey, Func<bool>> activeKeys);

        /// <summary>
        /// register hotkeys.
        /// </summary>
        /// <param name="onToggle">call back for when activationKey is pressed.</param>
        /// <param name="onToolChanged">call-back for when any active tool changes.</param>
        /// <param name="activationKey">hot key to toggle</param>
        /// <param name="activeKeys">hotkey->active dictionary. turns off these hotkeys in other mods while active</param>
        public static void RegisterHotkeys(
            Action onToggle, SavedInputKey activationKey = null, Dictionary<SavedInputKey, Func<bool>> activeKeys = null) {
            var Register = CreateDelegate<RegisterHotkeysHandler>(GetUUI(), "Register");
            Register(
                onToggle: onToggle,
                activationKey: activationKey,
                activeKeys: activeKeys);
        }

        /// <summary>
        /// Destroy all gameObjects, components, and children
        /// </summary>
        public static void Destroy(this Component button) => GameObject.Destroy(button?.gameObject);

        /// <summary>
        /// Gets the path to the mod that has the user mod instance.
        /// </summary>
        /// <param name="userModInstance">instance of IUserMod</param>
        /// <returns>path to mod</returns>
        public static string GetModPath(this IUserMod userModInstance) =>
            Plugins.FirstOrDefault(p => p?.userModInstance == userModInstance)?.modPath;

        public static string GetModPath<UserModT>() where UserModT : IUserMod =>
            Plugins.FirstOrDefault(p => p?.userModInstance is UserModT)?.modPath;

        /// <summary>
        /// Gets the full path to a file from the input mod
        /// </summary>
        /// <param name="userModInstance">instance of IUserMod</param>
        /// <param name="paths">diretory,file names to combine</param>
        /// <returns>full path to file.</returns>
        public static string GetFullPath(this IUserMod userModInstance, params string[] paths) {
            string ret = userModInstance.GetModPath();
            foreach (string path in paths)
                ret = Path.Combine(ret, path);
            return ret;
        }

        /// <summary>
        /// Gets the full path to a file from the input mod
        /// </summary>
        /// <typeparam name="UserModT">user mod type</typeparam>
        /// <param name="paths">diretories/files to combine</param>
        /// <returns>full path to file.</returns>
        public static string GetFullPath<UserModT>(params string[] paths) where UserModT : IUserMod {
            string ret = GetModPath<UserModT>();
            foreach(string path in paths)
                ret = Path.Combine(ret, path);
            return ret;
        }

        public static Texture2D LoadTexture(string filePath) {
            var t = TextureUtil.GetTextureFromFile(filePath);
            t.name = Path.GetFileNameWithoutExtension(filePath);
            return t;
        }

        /// <summary>
        /// test if UnifiedUI is present and enable.
        /// </summary>
        public static bool IsUUIEnabled() {
            var uui = GetUUIPlugin();
            return uui != null && uui.isEnabled;
        }

        internal delegate bool KeyActivatedDelegate(SavedInputKey key);

        /// <summary>
        /// checks if hotkey is activated on key Down/Up depending on UUI's user settings.
        /// </summary>
        public static bool KeyActivated(this SavedInputKey key) {
            if (GetUUI() != null) {
                var KeyActivated = CreateDelegate<KeyActivatedDelegate>(GetUUI(), "KeyActivated");
                return KeyActivated(key);
            } else {
                return key.IsKeyUp();
            }
        }
    }
}
