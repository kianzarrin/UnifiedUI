namespace UnifiedUI.Helpers {
    using System;
    using System.IO;
    using ColossalFramework.UI;
    using UnifiedUI.GUI;

    [Obsolete]
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
                NormalSprite = ButtonBase.BG_NORMAL,
                HoveredSprite = ButtonBase.BG_HOVERED,
                PressedSprite = ButtonBase.BG_PRESSED,
                DisabledSprite = ButtonBase.BG_DISABLED,
            };
        }
    }
}
