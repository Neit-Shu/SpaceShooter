using UnityEngine;

namespace SpaceShooter
{
    // Рисует простые белые спрайты (квадрат, круг, треугольник) прямо из кода,
    // чтобы прототип работал без единого файла картинки.
    // Цвет задаётся через SpriteRenderer.color. Размер каждого спрайта — 1 юнит.
    public static class SpriteFactory
    {
        static Sprite square, circle, triangle;

        public static Sprite Square
        {
            get
            {
                if (square == null) square = Build(4, (x, y) => 1f);
                return square;
            }
        }

        public static Sprite Circle
        {
            get
            {
                if (circle == null)
                {
                    circle = Build(64, (x, y) =>
                    {
                        float r = Mathf.Sqrt(x * x + y * y);
                        return Mathf.Clamp01((1f - r) * 32f);
                    });
                }
                return circle;
            }
        }

        // Вершина вверху, основание внизу
        public static Sprite Triangle
        {
            get
            {
                if (triangle == null)
                {
                    triangle = Build(64, (x, y) =>
                    {
                        float halfWidth = (1f - y) * 0.5f;
                        return Mathf.Clamp01((halfWidth - Mathf.Abs(x)) * 32f);
                    });
                }
                return triangle;
            }
        }

        // coverage получает координаты от -1 до 1 и возвращает непрозрачность пикселя (0..1)
        static Sprite Build(int size, System.Func<float, float, float> coverage)
        {
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;

            for (int py = 0; py < size; py++)
            {
                for (int px = 0; px < size; px++)
                {
                    float x = (px + 0.5f) / size * 2f - 1f;
                    float y = (py + 0.5f) / size * 2f - 1f;
                    tex.SetPixel(px, py, new Color(1f, 1f, 1f, coverage(x, y)));
                }
            }
            tex.Apply();

            return Sprite.Create(tex, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
        }
    }
}
