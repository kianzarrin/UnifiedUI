namespace UnifiedUI.API
{
    using ColossalFramework.UI;
    using UnityEngine;

    public interface IUUIButton
    {
        string Name { get; }
        string Group { get; } // future feature.
        string Tooltip { get; } // optional
        string SpritesFile { get; }

        void OnToggle();

        // return false to hide button (for example when action could not be activated in the given context)
        bool ShouldShow(); 
    }

    public interface IUUIMod {
        MonoBehaviour Register(IUUIButton button);
    }
}
