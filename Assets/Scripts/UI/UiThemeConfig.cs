using UnityEngine;
using UnityEngine.UI;

namespace MmorpgPrototype
{
    [CreateAssetMenu(menuName = "MMORPG/UI Theme", fileName = "UiThemeConfig")]
    public sealed class UiThemeConfig : ScriptableObject
    {
        public Color Panel = new Color(0.018f, 0.025f, 0.035f, 0.94f);
        public Color PanelSoft = new Color(0.035f, 0.052f, 0.068f, 0.78f);
        public Color PanelLine = new Color(0.46f, 0.58f, 0.65f, 0.72f);
        public Color TextPrimary = new Color(0.93f, 0.96f, 1f);
        public Color TextMuted = new Color(0.61f, 0.69f, 0.77f);
        public Color Accent = new Color(0.16f, 0.74f, 0.78f);
        public Color AccentGold = new Color(0.88f, 0.61f, 0.2f);
        public Color Health = new Color(0.18f, 0.82f, 0.4f);
        public Color Energy = new Color(0.18f, 0.64f, 0.94f);
        public Color Danger = new Color(0.9f, 0.24f, 0.2f);
        public Color Disabled = new Color(0.18f, 0.22f, 0.26f);
        public Color Card = new Color(0.045f, 0.065f, 0.082f, 0.96f);
        public Color CardHighlight = new Color(0.1f, 0.22f, 0.25f, 0.96f);
        public Font Font;

        private static UiThemeConfig runtime;
        private Sprite frameSprite;
        private Sprite circleSprite;

        public static UiThemeConfig Runtime
        {
            get
            {
                if (runtime == null)
                {
                    runtime = CreateInstance<UiThemeConfig>();
                    runtime.Font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                }

                return runtime;
            }
        }

        public void StylePanel(Image image, bool strong = false)
        {
            if (image == null)
            {
                return;
            }

            image.color = strong ? Panel : PanelSoft;
            image.sprite = FrameSprite;
            image.type = Image.Type.Sliced;
            image.raycastTarget = false;
            var outline = image.GetComponent<Outline>() ?? image.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(PanelLine.r, PanelLine.g, PanelLine.b, strong ? 0.8f : 0.42f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            outline.useGraphicAlpha = true;
            var shadow = image.GetComponent<Shadow>() ?? image.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.48f);
            shadow.effectDistance = new Vector2(0f, -4f);
        }

        public void StyleButton(Button button, Image image, Color baseColor)
        {
            if (button == null || image == null)
            {
                return;
            }

            image.color = baseColor;
            image.raycastTarget = true;
            var colors = button.colors;
            colors.normalColor = baseColor;
            colors.highlightedColor = Color.Lerp(baseColor, TextPrimary, 0.18f);
            colors.pressedColor = Color.Lerp(baseColor, Color.black, 0.22f);
            colors.selectedColor = Color.Lerp(baseColor, Accent, 0.2f);
            colors.disabledColor = Disabled;
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            var outline = image.GetComponent<Outline>() ?? image.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(PanelLine.r, PanelLine.g, PanelLine.b, 0.58f);
            outline.effectDistance = new Vector2(1f, -1f);
        }

        public void StyleCard(Image image, Color accent)
        {
            if (image == null)
            {
                return;
            }

            image.sprite = FrameSprite;
            image.type = Image.Type.Sliced;
            image.color = Card;
            image.raycastTarget = false;

            var outline = image.GetComponent<Outline>() ?? image.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(accent.r, accent.g, accent.b, 0.82f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;
        }

        public Sprite CircleSprite => circleSprite ?? (circleSprite = CreateCircleSprite());
        public Sprite FrameSprite => frameSprite ?? (frameSprite = CreateFrameSprite());

        private static Sprite CreateCircleSprite()
        {
            const int size = 96;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            var radius = size * 0.48f;
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var distance = Vector2.Distance(new Vector2(x, y), center);
                    var alpha = distance <= radius ? 1f : distance < radius + 2f ? Mathf.Clamp01(radius + 2f - distance) : 0f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }

        private static Sprite CreateFrameSprite()
        {
            const int size = 32;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    var edge = x < 4 || y < 4 || x >= size - 4 || y >= size - 4;
                    var alpha = edge ? 1f : 0.92f;
                    texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }

            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(8f, 8f, 8f, 8f));
        }
    }
}
