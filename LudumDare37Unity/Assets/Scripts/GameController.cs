using UnityEngine;
using MaxPostnikov.Utils;

namespace MaxPostnikov.LD37
{
    public class GameController : MonoBehaviour
    {
        [Header("Refs")]
        public Shell shell;
        public PlayerBubble playerBubble;
        public NpcBubble[] npcBubblePrefabs;

        [Header("Settings")]
        public int npcOnStart = 10;
        public int npcOnRecycle = 2;

        PrefabsPool<NpcBubble> npcBubblePool;

        void Start()
        {
            shell.Init();
            playerBubble.Init();

            npcBubblePool = new PrefabsPool<NpcBubble>(npcBubblePrefabs, transform, 3);
            npcBubblePool.Recycled += OnBubbleRecycled;

            SpawnNpc(npcOnStart);
        }

        void Update()
        {
            playerBubble.UpdateBubble(shell);

            for (var i = 0; i < npcBubblePool.SpawnedCount; i++)
                npcBubblePool.Spawned[i].UpdateBubble(shell, playerBubble);
        }

        void OnBubbleRecycled(NpcBubble obj)
        {
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
    }
}
