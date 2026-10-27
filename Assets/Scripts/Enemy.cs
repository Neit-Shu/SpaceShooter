using UnityEngine;

namespace SpaceShooter
{
    // Враг двух видов:
    //  - красный круг: летит прямо вниз, 10 очков;
    //  - оранжевый треугольник: летит «змейкой», 20 очков.
    public class Enemy : MonoBehaviour
    {
        public int Points { get; private set; }

        float speed;
        bool zigzag;
        float baseX;
        float phase;
        bool dead;

        public static Enemy Spawn(Transform parent, Vector3 position, float speed, bool zigzag)
        {
            var go = new GameObject("Enemy");
            go.transform.SetParent(parent, false);
            go.transform.position = position;

            var enemy = go.AddComponent<Enemy>();
            enemy.speed = speed;
            enemy.zigzag = zigzag;
            enemy.baseX = position.x;
            enemy.phase = Random.value * Mathf.PI * 2f;
            enemy.Setup();
            return enemy;
        }

        void Setup()
        {
            var sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sortingOrder = 5;

            if (zigzag)
            {
                sr.sprite = SpriteFactory.Triangle;
                sr.color = new Color(1f, 0.6f, 0.15f);
                transform.rotation = Quaternion.Euler(0f, 0f, 180f);   // вершиной вниз
                transform.localScale = new Vector3(0.8f, 0.8f, 1f);
                Points = 20;
            }
            else
            {
                sr.sprite = SpriteFactory.Circle;
                sr.color = new Color(1f, 0.3f, 0.35f);
                transform.localScale = new Vector3(0.85f, 0.85f, 1f);
                Points = 10;
            }

            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.42f;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.IsGameOver || dead) return;

            Vector3 pos = transform.position;
            pos.y -= speed * Time.deltaTime;

            if (zigzag)
            {
                float limit = gm.HalfWidth - 0.5f;
                pos.x = Mathf.Clamp(baseX + Mathf.Sin(Time.time * 2.5f + phase) * 1.6f, -limit, limit);
            }

            transform.position = pos;

            if (pos.y < -gm.HalfHeight - 0.5f)
            {
                dead = true;
                gm.EnemyLeaked();
                Destroy(gameObject);
            }
        }

        // Возвращает true, если враг был убит именно этим вызовом
        public bool Kill()
        {
            if (dead) return false;
            dead = true;
            Destroy(gameObject);
            return true;
        }
    }
}
