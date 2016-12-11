using System.Collections;
using MaxPostnikov.Utils;
using UnityEngine;

namespace MaxPostnikov.LD37
{
    public class NpcBubble : ColorBubble, IPooled<NpcBubble>
    {
        [Header("NPC Colors")]
        public Color enemyColor = Color.red;
        public Color friendColor = Color.cyan;

        [Header("Player impact")]
        public float friendImpact = 0.1f;
        public float enemyImpact = -0.3f;

        [Header("Settings")]
        public float minPlayerDist = 0.8f;
        public float explosionDelay = 3f;
        
        [Header("Pulse Anim Settings")]
        public float pulseTime = 0.5f;
        public float minPulseScale = 0.9f;
        public float maxPulseScale = 1.1f;

        public bool IsEnemy { get; set; }

        float timer;
        bool isActivated;
        WaitForEndOfFrame waitEndFrame;
        
        #region IPooled

        public int PrefabIndex { get; private set; }

        PrefabsPool<NpcBubble> pool;

        public void OnInstantiate(int prefabIndex, PrefabsPool<NpcBubble> pool)
        {
            PrefabIndex = prefabIndex;

            this.pool = pool;

            base.Init();
        }

        public virtual void OnSpawn() { }

        public virtual void OnRecycle()
        {
            Deactivate();
        }

        public void Recycle()
        {
            if (pool != null)
                pool.Recycle(this);
        }

        #endregion

        public override void Init()
        {
            base.Init();

            waitEndFrame = new WaitForEndOfFrame();
        }

        public void UpdateBubble(Shell shell)
        {
            var dist = Vector3.Distance(transform.position, shell.transform.position);
            var delta = shell.Radius - dist;
            
            if (delta >= Radius)
                Activate();
            else if (delta <= -Radius)
                Deactivate();

            if (!isActivated) return;

            if (IsEnemy) {
                timer += Time.deltaTime;

                if (timer >= explosionDelay) {
                    shell.NpcImpact(enemyImpact, true);

                    Recycle();
                }
            } else {
                var playerDist = Vector3.Distance(transform.position, shell.InnerBubble.position);

                if (playerDist < minPlayerDist) {
                    shell.NpcImpact(friendImpact, false);
                    
                    Recycle();
                }
            }
        }

        void Activate()
        {
            if (isActivated) return;

            timer = 0f;
            isActivated = true;

            if (IsEnemy)
                StartCoroutine(AnimatePulse());

            SetColor(IsEnemy ? enemyColor : friendColor);
        }

        void Deactivate()
        {
            if (!isActivated) return;

            isActivated = false;

            StopAllCoroutines();
            SetColor(mainColor);
        }

        IEnumerator AnimatePulse()
        {
            var dir = -1;
            var elapsed = 0f;
            var scale = transform.localScale;
            var targetScale = GetTargetPulseScale(dir);

            while (isActivated) {
                if (elapsed <= pulseTime)
                    transform.localScale = Vector3.Lerp(scale, targetScale, elapsed / pulseTime);
                else {
                    dir *= -1;
                    elapsed = 0f;
                    scale = transform.localScale;
                    targetScale = GetTargetPulseScale(dir);
                }

                elapsed += Time.deltaTime;

                yield return waitEndFrame;
            }
        }

        Vector3 GetTargetPulseScale(int dir)
        {
            return Vector3.one * (dir < 0 ? minPulseScale : maxPulseScale);
        }
    }
}
