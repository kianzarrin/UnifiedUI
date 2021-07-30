namespace UnifiedUILib.GUI.Button3 {
    using ColossalFramework.UI;
    using System;
    using System.Collections.Generic;
    using System.Text;
    using UnityEngine;

    public class MultiAtlasUIButton : CustomUIButton {
        UITextureAtlas _fgAtlas;
        UITextureAtlas _bgAtlas;

        public UITextureAtlas FgAtlas {
            get => _fgAtlas ?? atlas;
            set {
                if (!Equals(value, _fgAtlas)) {
                    _fgAtlas = value;
                    Invalidate();
                }
            }
        }
        public UITextureAtlas BgAtlas {
            get => _bgAtlas ?? atlas;
            set {
                if (!Equals(value, _bgAtlas)) {
                    _bgAtlas = value;
                    Invalidate();
                }
            }
        }
        public UITextureAtlas Atlas {
            set {
                _fgAtlas = value;
                _bgAtlas = value;
                atlas = value;
            }
        }
        protected UIRenderData BgRenderData { get; set; }
        protected UIRenderData FgRenderData { get; set; }


        protected override void OnRebuildRenderData() {
            if (textRenderData == null)
                m_RenderData.Add(UIRenderData.Obtain());
            else
                textRenderData.Clear();

            if (BgRenderData == null) {
                BgRenderData = UIRenderData.Obtain();
                m_RenderData.Add(BgRenderData);
            } else
                BgRenderData.Clear();

            if (FgRenderData == null) {
                FgRenderData = UIRenderData.Obtain();
                m_RenderData.Add(FgRenderData);
            } else
                FgRenderData.Clear();

            if (BgAtlas is UITextureAtlas bgAtlas && FgAtlas is UITextureAtlas fgAtlas) {
                BgRenderData.material = bgAtlas.material;
                FgRenderData.material = fgAtlas.material;

                RenderBackground();
                RenderForeground();
                RenderText();
            }
        }
        protected override UITextureAtlas.SpriteInfo GetBackgroundSprite() {
            if (BgAtlas is not UITextureAtlas atlas)
                return null;

            var spriteInfo = state switch {
                ButtonState.Normal => atlas[normalBgSprite],
                ButtonState.Focused => atlas[focusedBgSprite],
                ButtonState.Hovered => atlas[hoveredBgSprite],
                ButtonState.Pressed => atlas[pressedBgSprite],
                ButtonState.Disabled => atlas[disabledBgSprite],
            };

            return spriteInfo ?? atlas[normalBgSprite];
        }
        protected override UITextureAtlas.SpriteInfo GetForegroundSprite() {
            if (FgAtlas is not UITextureAtlas atlas)
                return null;

            var spriteInfo = state switch {
                ButtonState.Normal => atlas[normalFgSprite],
                ButtonState.Focused => atlas[focusedFgSprite],
                ButtonState.Hovered => atlas[hoveredFgSprite],
                ButtonState.Pressed => atlas[pressedFgSprite],
                ButtonState.Disabled => atlas[disabledFgSprite],
            };

            return spriteInfo ?? atlas[normalFgSprite];
        }

        protected override void RenderBackground() {
            if (GetBackgroundSprite() is UITextureAtlas.SpriteInfo backgroundSprite) {
                var color = ApplyOpacity(GetActiveColor());
                var renderOptions = default(UISprite.RenderOptions);

                renderOptions.atlas = BgAtlas;
                renderOptions.color = color;
                renderOptions.fillAmount = 1f;
                renderOptions.flip = UISpriteFlip.None;
                renderOptions.offset = pivot.TransformToUpperLeft(size, arbitraryPivotOffset);
                renderOptions.pixelsToUnits = PixelsToUnits();
                renderOptions.size = size;
                renderOptions.spriteInfo = backgroundSprite;

                if (backgroundSprite.isSliced)
                    UISlicedSprite.RenderSprite(BgRenderData, renderOptions);
                else
                    UISprite.RenderSprite(BgRenderData, renderOptions);
            }
        }
        protected override void RenderForeground() {
            if (GetForegroundSprite() is UITextureAtlas.SpriteInfo foregroundSprite) {
                var foregroundRenderSize = GetForegroundRenderSize(foregroundSprite);
                var foregroundRenderOffset = GetForegroundRenderOffset(foregroundRenderSize);
                var color = ApplyOpacity(GetActiveColor());

                UISprite.RenderOptions renderOptions = default(UISprite.RenderOptions);
                renderOptions.atlas = FgAtlas;
                renderOptions.color = color;
                renderOptions.fillAmount = 1f;
                renderOptions.flip = UISpriteFlip.None;
                renderOptions.offset = foregroundRenderOffset;
                renderOptions.pixelsToUnits = PixelsToUnits();
                renderOptions.size = foregroundRenderSize;
                renderOptions.spriteInfo = foregroundSprite;

                if (foregroundSprite.isSliced)
                    UISlicedSprite.RenderSprite(FgRenderData, renderOptions);
                else
                    UISprite.RenderSprite(FgRenderData, renderOptions);
            }
        }
    }
}
