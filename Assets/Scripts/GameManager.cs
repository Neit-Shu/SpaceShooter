using UnityEngine;

namespace SpaceShooter
{
    // Главный скрипт: настраивает камеру, создаёт игрока и фон, спавнит врагов,
    // ведёт счёт и жизни, рисует интерфейс.
    // Создаётся сам при нажатии Play — добавлять в сцену ничего не нужно.
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Правила")]
        public int startLives = 3;
        public bool enemyLeakCostsLife = true;   // пропущенный враг отнимает жизнь

        [Header("Сложность")]
        public float startSpawnInterval = 1.2f;  // пауза между врагами в начале, сек
        public float minSpawnInterval = 0.35f;   // пауза после полного «разгона»
        public float rampSeconds = 90f;          // за сколько секунд сложность доходит до максимума

        public int Score { get; private set; }
        public int Lives { get; private set; }
        public int HighScore { get; private set; }
        public bool IsGameOver { get; private set; }

        // Половина ширины и высоты видимой области в мировых координатах
        public float HalfWidth { get; private set; }
        public float HalfHeight { get; private set; }

        // Родитель для врагов и пуль: при рестарте поле чистится одним циклом
        public Transform World { get; private set; }

        const string HighScoreKey = "SpaceShooter.HighScore";

        Camera cam;
        PlayerController player;
        float elapsed;
        float spawnTimer;
        GUIStyle left, right, center, hint;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoCreate()
        {
            if (Instance != null) return;
            new GameObject("GameManager").AddComponent<GameManager>();
        }

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                enabled = false;
                Destroy(gameObject);
                return;
            }
            Instance = this;

            HighScore = PlayerPrefs.GetInt(HighScoreKey, 0);

            SetupCamera();
            World = new GameObject("World").transform;
            new GameObject("Starfield").AddComponent<Starfield>();
            player = PlayerController.Create();

            StartGame();
        }

        void SetupCamera()
        {
            cam = Camera.main;
            if (cam == null)
            {
                var go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                cam = go.AddComponent<Camera>();
            }

            cam.orthographic = true;
            cam.orthographicSize = 5f;
            cam.transform.position = new Vector3(0f, 0f, -10f);
            cam.transform.rotation = Quaternion.identity;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.04f, 0.05f, 0.10f);

            UpdateBounds();
        }

        void UpdateBounds()
        {
            HalfHeight = cam.orthographicSize;
            HalfWidth = HalfHeight * cam.aspect;
        }

        public void StartGame()
        {
            foreach (Transform child in World)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }

            Score = 0;
            Lives = startLives;
            IsGameOver = false;
            elapsed = 0f;
            spawnTimer = 1f;

            player.ResetPlayer();
        }

        void Update()
        {
            UpdateBounds();

            if (IsGameOver)
            {
                if (GameInput.RestartPressed()) StartGame();
                return;
            }

            elapsed += Time.deltaTime;
            spawnTimer -= Time.deltaTime;
            if (spawnTimer <= 0f)
            {
                SpawnEnemy();
                float t = Mathf.Clamp01(elapsed / rampSeconds);
                spawnTimer = Mathf.Lerp(startSpawnInterval, minSpawnInterval, t);
            }
        }

        void SpawnEnemy()
        {
            float t = Mathf.Clamp01(elapsed / rampSeconds);
            float speed = Mathf.Lerp(2.2f, 4.8f, t) * Random.Range(0.85f, 1.15f);
            bool zigzag = Random.value < Mathf.Lerp(0.2f, 0.5f, t);
            float x = Random.Range(-HalfWidth + 0.8f, HalfWidth - 0.8f);

            Enemy.Spawn(World, new Vector3(x, HalfHeight + 0.8f, 0f), speed, zigzag);
        }

        public void AddScore(int points)
        {
            if (IsGameOver) return;
            Score += points;
        }

        public void LoseLife()
        {
            if (IsGameOver) return;
            Lives--;
            if (Lives <= 0)
            {
                Lives = 0;
                EndGame();
            }
        }

        // Вызывается врагом, который долетел до нижнего края
        public void EnemyLeaked()
        {
            if (enemyLeakCostsLife) LoseLife();
        }

        void EndGame()
        {
            IsGameOver = true;
            if (Score > HighScore) HighScore = Score;
            PlayerPrefs.SetInt(HighScoreKey, HighScore);
            PlayerPrefs.Save();
        }

        // Простой интерфейс через OnGUI — не требует Canvas и дополнительных пакетов.
        // Текст на английском: так он точно отрисуется стандартным шрифтом.
        void OnGUI()
        {
            if (left == null)
            {
                left = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperLeft };
                right = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperRight };
                center = new GUIStyle(GUI.skin.label) { fontStyle = FontStyle.Bold, alignment = TextAnchor.UpperCenter };
                hint = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.UpperCenter };
            }

            int size = Mathf.Max(14, Mathf.RoundToInt(Screen.height * 0.04f));
            float pad = size * 0.6f;

            left.fontSize = size;
            right.fontSize = size;
            center.fontSize = size;
            hint.fontSize = Mathf.RoundToInt(size * 0.7f);
            left.normal.textColor = Color.white;
            right.normal.textColor = Color.white;
            center.normal.textColor = Color.white;
            hint.normal.textColor = new Color(1f, 1f, 1f, 0.7f);

            GUI.Label(new Rect(pad, pad, Screen.width * 0.5f, size * 2f), "SCORE  " + Score, left);
            GUI.Label(new Rect(Screen.width * 0.5f - pad, pad, Screen.width * 0.5f, size * 2f), "LIVES  " + Lives, right);

            if (!IsGameOver && elapsed < 8f)
            {
                GUI.Label(new Rect(0f, Screen.height - size * 2.2f, Screen.width, size * 2f),
                    "WASD / Arrows - move      Space / Mouse - fire", hint);
            }

            if (IsGameOver)
            {
                float top = Screen.height * 0.28f;
                center.fontSize = size * 3;
                GUI.Label(new Rect(0f, top, Screen.width, size * 4f), "GAME OVER", center);
                center.fontSize = size;
                GUI.Label(new Rect(0f, top + size * 4f, Screen.width, size * 2f),
                    "Score " + Score + "      Best " + HighScore, center);
                GUI.Label(new Rect(0f, top + size * 6f, Screen.width, size * 2f), "Press R to restart", center);
            }
        }
    }
}
