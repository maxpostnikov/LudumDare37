using System.Collections;
using MaxPostnikov.Utils;
using UnityEngine;

namespace MaxPostnikov.LD37
{
    public class NpcBubble : Bubble, IPooled<NpcBubble>
    {
        [Header("Colors")]
        public Color defaultColor = Color.white;
        public Color enemyColor = Color.red;
        public Color friendColor = Color.cyan;

        [Header("Settings")]
        public float minPlayerDist = 0.8f;
        public float explosionDelay = 3f;

        [Header("Shell damage")]
        public float changeRadiusFriend = 0.1f;
        public float changeRadiusEnemy = -0.3f;

        [Header("Pulse Anim Settings")]
        public float pulseTime = 0.5f;
        public float minPulseScale = 0.9f;
        public float maxPulseScale = 1.1f;

        public bool IsEnemy { get; set; }

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

        public void UpdateBubble(Shell shell, PlayerBubble player)
        {
            base.UpdateBubble(shell);

            if (shellDelta >= c_Radius)
                Activate();
            else if (shellDelta <= -c_Radius)
                Deactivate();

            if (!isActivated) return;

            if (IsEnemy) {
                timer += Time.deltaTime;

                if (timer >= explosionDelay) {
                    shell.ChangeRadius(changeRadiusEnemy);

                    Recycle();
                }
            } else {
                var playerDist = Vector3.Distance(transform.position, player.transform.position);

                if (playerDist < minPlayerDist) {
                    shell.ChangeRadius(changeRadiusFriend);
                    
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
            SetColor(defaultColor);
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
