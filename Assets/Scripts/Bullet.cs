using UnityEngine;

namespace SpaceShooter
{
    // Пуля: летит вверх, при попадании уничтожает врага и приносит очки.
    public class Bullet : MonoBehaviour
    {
        public float speed = 14f;

        bool used;

        public static Bullet Spawn(Transform parent, Vector3 position)
        {
            var go = new GameObject("Bullet");
            go.transform.SetParent(parent, false);
            go.transform.position = position;
            return go.AddComponent<Bullet>();
        }

        void Awake()
        {
            var sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Square;
            sr.color = new Color(1f, 0.95f, 0.4f);
            sr.sortingOrder = 5;
            transform.localScale = new Vector3(0.12f, 0.4f, 1f);

            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = gameObject.AddComponent<BoxCollider2D>();
            col.isTrigger = true;
            col.size = Vector2.one;   // в локальных координатах; масштаб объекта задаёт реальный размер
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.IsGameOver) return;

            transform.position += Vector3.up * speed * Time.deltaTime;

            if (transform.position.y > gm.HalfHeight + 1f)
            {
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (used) return;

            var enemy = other.GetComponent<Enemy>();
            if (enemy == null) return;

            // Kill() вернёт false, если враг уже убит другой пулей в этот же кадр
            if (enemy.Kill())
            {
                used = true;
                if (GameManager.Instance != null) GameManager.Instance.AddScore(enemy.Points);
                Destroy(gameObject);
            }
        }
    }
}
