using UnityEngine;

namespace SpaceShooter
{
    // Корабль игрока: движение, стрельба, столкновения с врагами.
    public class PlayerController : MonoBehaviour
    {
        [Header("Настройки")]
        public float moveSpeed = 7f;
        public float fireInterval = 0.18f;        // пауза между выстрелами, сек
        public float invulnerableSeconds = 1.5f;  // неуязвимость после попадания

        SpriteRenderer sr;
        float fireTimer;
        float invulnerableTimer;

        public static PlayerController Create()
        {
            var go = new GameObject("Player");
            return go.AddComponent<PlayerController>();
        }

        void Awake()
        {
            sr = gameObject.AddComponent<SpriteRenderer>();
            sr.sprite = SpriteFactory.Triangle;
            sr.color = new Color(0.3f, 0.9f, 1f);
            sr.sortingOrder = 10;
            transform.localScale = new Vector3(0.9f, 0.9f, 1f);

            // Для триггеров нужен хотя бы один Rigidbody2D. Делаем его кинематическим:
            // двигаем корабль сами, физика им не управляет.
            var rb = gameObject.AddComponent<Rigidbody2D>();
            rb.bodyType = RigidbodyType2D.Kinematic;

            var col = gameObject.AddComponent<CircleCollider2D>();
            col.isTrigger = true;
            col.radius = 0.35f;   // чуть меньше спрайта, чтобы попадания были «честными»
        }

        public void ResetPlayer()
        {
            var gm = GameManager.Instance;
            transform.position = new Vector3(0f, -gm.HalfHeight + 1.2f, 0f);
            fireTimer = 0f;
            invulnerableTimer = 1.5f;   // короткая защита в начале игры
            sr.enabled = true;
        }

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            if (gm.IsGameOver)
            {
                sr.enabled = false;
                return;
            }

            // Движение
            var input = new Vector2(GameInput.Horizontal(), GameInput.Vertical());
            if (input.sqrMagnitude > 1f) input.Normalize();   // по диагонали не быстрее

            Vector3 pos = transform.position + (Vector3)(input * moveSpeed * Time.deltaTime);
            float maxX = gm.HalfWidth - 0.5f;
            float maxY = gm.HalfHeight - 0.5f;
            pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
            pos.y = Mathf.Clamp(pos.y, -maxY, maxY);
            transform.position = pos;

            // Стрельба
            fireTimer -= Time.deltaTime;
            if (GameInput.Fire() && fireTimer <= 0f)
            {
                fireTimer = fireInterval;
                Bullet.Spawn(gm.World, transform.position + Vector3.up * 0.55f);
            }

            // Мигание во время неуязвимости
            if (invulnerableTimer > 0f)
            {
                invulnerableTimer -= Time.deltaTime;
                sr.enabled = Mathf.FloorToInt(invulnerableTimer * 12f) % 2 == 0;
            }
            else
            {
                sr.enabled = true;
            }
        }

        // Stay, а не Enter: если неуязвимость закончилась, пока враг ещё касается корабля,
        // попадание всё равно засчитается.
        void OnTriggerStay2D(Collider2D other)
        {
            var gm = GameManager.Instance;
            if (gm == null || gm.IsGameOver || invulnerableTimer > 0f) return;

            var enemy = other.GetComponent<Enemy>();
            if (enemy == null) return;

            if (enemy.Kill())
            {
                gm.LoseLife();
                invulnerableTimer = invulnerableSeconds;
            }
        }
    }
}
