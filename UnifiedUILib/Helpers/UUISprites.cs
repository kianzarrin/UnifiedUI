namespace UnifiedUI.Helpers {
    using System.IO;
    using ColossalFramework.UI;
    using UnifiedUI.GUI;
    public struct UUISprites {
        public UITextureAtlas Atlas;
        public string NormalSprite, HoveredSprite, PressedSprite, DisabledSprite;
        internal string[] SpriteNames => new string[] {
                NormalSprite ?? "",
                HoveredSprite ?? "",
                PressedSprite ?? "",
                DisabledSprite ?? "",
        };

        public static UUISprites CreateFromFile(string filePath) {
            var atlas = ButtonBase.GetOrCreateAtlas(
                atlasName:Path.GetFileNameWithoutExtension(filePath),
                spriteFile: filePath);
            return new UUISprites {
                Atlas = atlas,
                NormalSprite = ButtonBase.ICON_NORMAL,
                HoveredSprite = ButtonBase.ICON_HOVERED,
                PressedSprite = ButtonBase.ICON_PRESSED,
                DisabledSprite = ButtonBase.ICON_DISABLED,
            };
        }
    }
}
