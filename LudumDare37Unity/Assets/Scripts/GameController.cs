using UnityEngine;
using MaxPostnikov.Utils;

namespace MaxPostnikov.LD37
{
    public interface IUIController
    {
        void Restart();
    }

    public class GameController : MonoBehaviour, IUIController
    {
        [Header("Refs")]
        public Shell shell;
        public NpcBubble[] npcBubblePrefabs;

        [Header("UI Refs")]
        public ProgressBar progressBar;
        public GameOverPopup gameOverPopup;

        [Header("Settings")]
        public int npcOnStart = 10;
        public int npcOnRecycle = 2;
        public float minShellRadius = 1f;
        public int scorePerFriend = 50;

        int totalScore;
        bool isGameOver;
        PrefabsPool<NpcBubble> npcBubblePool;

        int TotalScore {
            get {
                return totalScore;
            }
            set {
                totalScore = value;

                progressBar.SetScore(value);
            }
        }

        void Start()
        {
            gameOverPopup.Init(this);

            shell.Init();
            shell.RadiusChange += OnShellRadiusChange;
            
            npcBubblePool = new PrefabsPool<NpcBubble>(npcBubblePrefabs, transform, 3);
            npcBubblePool.Recycled += OnNpcRecycled;

            Reset();
        }

        void Reset()
        {
            progressBar.Show();
            
            shell.Reset();

            npcBubblePool.RecycleAll();
            SpawnNpc(npcOnStart);

            TotalScore = 0;
            isGameOver = false;
        }

        void Update()
        {
            if (isGameOver) return;

            shell.UpdateShell();

            for (var i = 0; i < npcBubblePool.SpawnedCount; i++)
                npcBubblePool.Spawned[i].UpdateBubble(shell);

            progressBar.SetProgress(shell.DecreaseProgress);
        }

        void OnShellRadiusChange(float radius)
        {
            if (radius <= minShellRadius)
                GameOver();
        }

        void OnNpcRecycled(NpcBubble npc)
        {
            if (isGameOver) return;

            if (!npc.IsEnemy)
                TotalScore += Mathf.RoundToInt(scorePerFriend * shell.Radius);

            SpawnNpc(npcOnRecycle);
        }

        void SpawnNpc(int count)
        {
            for (var i = 0; i < count; i++) {
                var npc = npcBubblePool.SpawnRandom();
                npc.IsEnemy = Random.value >= 0.5f;

                //TODO: also check other bubbles overlapp; way to define range
                var position = Vector3.zero;
                do {
                    position.x = Random.Range(-10f, 10f);
                    position.y = Random.Range(-10f, 10f);
                } while (Vector3.Distance(position, shell.transform.position) < shell.Radius * 2f);

                npc.transform.position = position;
            }
        }
        
        void GameOver()
        {
            isGameOver = true;

            progressBar.Hide();
            gameOverPopup.Show(TotalScore);
        }

        public void Restart()
        {
            Reset();
        }
    }
}
