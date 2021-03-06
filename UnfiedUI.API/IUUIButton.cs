using ColossalFramework.UI;
namespace UnfiedUI.API
{
    public interface IUUIButton
    {
        string Name { get; }
        string Group { get; } // future feature.
        string Tooltip { get; } // optional
        string SpritesFile { get; }
        ToolBase Tool { get; } // optional
        UIComponent Window { get; } // optional
        void OnToggle(); // optional
    }

    public interface UUIMod {
        void Register(IUUIButton button);
    }
}
