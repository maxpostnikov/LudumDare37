using System.Collections;
using UnityEngine;

namespace MaxPostnikov.LD37
{
    public class Shell : MonoBehaviour
    {
        [Header("Refs")]
        public MeshRenderer backRenderer;
        public LineRenderer borderRenderer;

        [Header("Settings")]
        public float startRadius = 1f;
        public int segmentCount = 50;
        public float scaleTime = 0.5f;

        public float Radius { get { return currentRadius; } }

        bool isAnimating;
        float targetRadius;
        float currentRadius;
        WaitForEndOfFrame waitEndFrame;

        public void Init()
        {
            currentRadius = startRadius;
            waitEndFrame = new WaitForEndOfFrame();

            borderRenderer.useWorldSpace = false;
            borderRenderer.SetVertexCount(segmentCount + 1);
            
            Generate();
        }

        void Generate()
        {
            var angle = 0f;
            var position = Vector3.zero;

            for (var i = 0; i <= segmentCount; i++) {
                position.x = Mathf.Sin(Mathf.Deg2Rad * angle);
                position.y = Mathf.Cos(Mathf.Deg2Rad * angle);
                position.z = 0f;

                angle += 360f / segmentCount;

                borderRenderer.SetPosition(i, position * currentRadius);
            }

            backRenderer.transform.localScale = Vector3.one * currentRadius * 2f;
        }
        
        public void ChangeRadius(float value)
        {
            if (isAnimating) {
                StopAllCoroutines();

                currentRadius = targetRadius;
            }

            targetRadius = currentRadius + value;

            StartCoroutine(AnimateScale());
        }

        IEnumerator AnimateScale()
        {
            isAnimating = true;

            var elapsed = 0f;
            var radius = currentRadius;

            while (elapsed <= scaleTime) {
                currentRadius = Mathf.Lerp(radius, targetRadius, elapsed / scaleTime);

                Generate();

                elapsed += Time.deltaTime;

                yield return waitEndFrame;
            }

            isAnimating = false;
        }
    }
}
