namespace KianCommons.UI {
    using ColossalFramework.UI;
    using System;
    using System.IO;
    using System.Reflection;
    using UnityEngine;
    using static KianCommons.Assertion;
    using Object = UnityEngine.Object;

    public static class TextureUtil {

        static UITextureAtlas inGame_;
        static UITextureAtlas inMapEditor_;
        public static UITextureAtlas Ingame {
            get {
                if(!inGame_)
                    inGame_ = GetAtlas("Ingame");
                return inGame_;
            }
        }
        public static UITextureAtlas InMapEditor {
            get {
                if (!inMapEditor_)
                    inMapEditor_ = GetAtlas("InMapEditor");
                return inMapEditor_;
            }
        }

        static string PATH => typeof(TextureUtil).Assembly.GetName().Name + ".Resources.";
        static string ModPath => PluginUtil.GetPlugin().modPath;
        public static string FILE_PATH = ModPath;
        public static bool EmbededResources = true;

        public static UITextureAtlas CreateTextureAtlas(string textureFile, string atlasName, int spriteWidth, int spriteHeight, string[] spriteNames) {
            return CreateTextureAtlas(textureFile, atlasName, spriteNames);
        }

        public static UITextureAtlas CreateTextureAtlas(string textureFile, string atlasName, string[] spriteNames) {
            Texture2D texture2D;
#pragma warning disable CS0618 // Type or member is obsolete
            if (!EmbededResources)
                texture2D = GetTextureFromFile(textureFile);
            else
                texture2D = GetTextureFromAssemblyManifest(textureFile);
#pragma warning restore CS0618 // Type or member is obsolete

            UITextureAtlas uitextureAtlas = ScriptableObject.CreateInstance<UITextureAtlas>();
            Assert(uitextureAtlas != null, "uitextureAtlas");
            Material material = Object.Instantiate<Material>(UIView.GetAView().defaultAtlas.material);
            Assert(material != null, "material");
            material.mainTexture = texture2D;
            uitextureAtlas.material = material;
            uitextureAtlas.name = atlasName;

            int n = spriteNames.Length;
            for (int i = 0; i < n; i++) {
                float num = 1f / (float)spriteNames.Length;
                UITextureAtlas.SpriteInfo spriteInfo = new UITextureAtlas.SpriteInfo {
                    name = spriteNames[i],
                    texture = texture2D,
                    region = new Rect(i * num, 0f, num, 1f)
                };
                uitextureAtlas.AddSprite(spriteInfo);
            }
            return uitextureAtlas;
        }

        public static void AddTexturesInAtlas(UITextureAtlas atlas, Texture2D[] newTextures, bool locked = false) {
            Texture2D[] textures = new Texture2D[atlas.count + newTextures.Length];

            for (int i = 0; i < atlas.count; i++) {
                Texture2D texture2D = atlas.sprites[i].texture;

                if (locked) {
                    // Locked textures workaround
                    RenderTexture renderTexture = RenderTexture.GetTemporary(texture2D.width, texture2D.height, 0);
                    Graphics.Blit(texture2D, renderTexture);

                    RenderTexture active = RenderTexture.active;
                    texture2D = new Texture2D(renderTexture.width, renderTexture.height);
                    RenderTexture.active = renderTexture;
                    texture2D.ReadPixels(new Rect(0f, 0f, (float)renderTexture.width, (float)renderTexture.height), 0, 0);
                    texture2D.Apply();
                    RenderTexture.active = active;

                    RenderTexture.ReleaseTemporary(renderTexture);
                }

                textures[i] = texture2D;
                textures[i].name = atlas.sprites[i].name;
            }

            for (int i = 0; i < newTextures.Length; i++)
                textures[atlas.count + i] = newTextures[i];

            Rect[] regions = atlas.texture.PackTextures(textures, atlas.padding, 4096, false);

            atlas.sprites.Clear();

            for (int i = 0; i < textures.Length; i++) {
                UITextureAtlas.SpriteInfo spriteInfo = atlas[textures[i].name];
                atlas.sprites.Add(new UITextureAtlas.SpriteInfo {
                    texture = textures[i],
                    name = textures[i].name,
                    border = (spriteInfo != null) ? spriteInfo.border : new RectOffset(),
                    region = regions[i]
                });
            }

            atlas.RebuildIndexes();
        }

        public static UITextureAtlas GetAtlas(string name) {
            UITextureAtlas[] atlases = Resources.FindObjectsOfTypeAll(typeof(UITextureAtlas)) as UITextureAtlas[];
            for (int i = 0; i < atlases.Length; i++) {
                if (atlases[i].name == name)
                    return atlases[i];
            }
            return UIView.GetAView().defaultAtlas;
        }

        #region loading textures


        public static Stream GetFileStream(string file) {
            string path = Path.Combine(FILE_PATH, file);
            return File.OpenRead(path) ?? throw new Exception(path + "not find");
        }

        public static Texture2D GetTextureFromFile(string file) {
            using (Stream stream = GetFileStream(file))
                return GetTextureFromStream(stream);
        }

        public static Stream GetManifestResourceStream(string file) {
            string path = string.Concat(PATH, file);
            return Assembly.GetExecutingAssembly().GetManifestResourceStream(path)
                ?? throw new Exception(path + "not find");
        }

        // useful to load cursor textures.
        [Obsolete("load textures from files")]
        public static Texture2D GetTextureFromAssemblyManifest(string file) {
            using (Stream stream = GetManifestResourceStream(file))
                return GetTextureFromStream(stream);
        }

        public static Texture2D GetTextureFromStream(Stream stream) {
            Texture2D texture2D = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            byte[] array = new byte[stream.Length];
            stream.Read(array, 0, array.Length);
            texture2D.filterMode = FilterMode.Bilinear;
            texture2D.LoadImage(array);
            texture2D.wrapMode = TextureWrapMode.Clamp; // for cursor.
            texture2D.Apply();
            return texture2D;
        }

        #endregion
    }
}
