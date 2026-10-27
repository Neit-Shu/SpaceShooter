using UnityEngine;

namespace SpaceShooter
{
    // Фон: медленно падающие звёзды разной «глубины» (чем меньше звезда, тем она медленнее).
    public class Starfield : MonoBehaviour
    {
        const int Count = 45;

        Transform[] stars;
        float[] speeds;

        void Start()
        {
            var gm = GameManager.Instance;
            stars = new Transform[Count];
            speeds = new float[Count];

            for (int i = 0; i < Count; i++)
            {
                float depth = Random.value;
                float size = Mathf.Lerp(0.03f, 0.09f, depth);

                var go = new GameObject("Star");
                go.transform.SetParent(transform, false);
                go.transform.localScale = new Vector3(size, size, 1f);
                go.transform.position = new Vector3(
                    Random.Range(-gm.HalfWidth, gm.HalfWidth),
                    Random.Range(-gm.HalfHeight, gm.HalfHeight),
                    0f);

                var sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = SpriteFactory.Square;
                sr.color = new Color(1f, 1f, 1f, Mathf.Lerp(0.25f, 0.8f, depth));
                sr.sortingOrder = -10;

                stars[i] = go.transform;
                speeds[i] = Mathf.Lerp(0.4f, 2.5f, depth);
            }
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || stars == null) return;

            for (int i = 0; i < stars.Length; i++)
            {
                Vector3 pos = stars[i].position;
                pos.y -= speeds[i] * Time.deltaTime;

                if (pos.y < -gm.HalfHeight - 0.2f)
                {
                    pos.y = gm.HalfHeight + 0.2f;
                    pos.x = Random.Range(-gm.HalfWidth, gm.HalfWidth);
                }

                stars[i].position = pos;
            }
        }
    }
}
