using System.Collections;
using UnityEngine;

namespace MaxPostnikov.LD37
{
    public class Shell : MonoBehaviour
    {
        [Header("Refs")]
        public MeshRenderer backRenderer;
        public LineRenderer borderRenderer;

        [Header("Border settings")]
        public float startRadius = 1f;
        public int segmentCount = 50;

        [Header("Anim settings")]
        public float scaleTime = 0.5f;
        public float smoothTime = 0.5f;

        public float Radius { get { return currentRadius; } }
        
        bool isAnimating;
        float targetRadius;
        float currentRadius;
        Vector3 cameraPosition;
        Transform cameraTransform;
        WaitForEndOfFrame waitEndFrame;
        float xDampVelocity, yDampVelocity;

        public void Init()
        {
            currentRadius = startRadius;
            waitEndFrame = new WaitForEndOfFrame();
            cameraTransform = Camera.main.transform;

            borderRenderer.useWorldSpace = false;
            borderRenderer.SetVertexCount(segmentCount + 1);
            
            Generate();
        }

        public void CameraFollow()
        {
            cameraPosition = cameraTransform.position;

            cameraPosition.x = Mathf.SmoothDamp(cameraPosition.x, transform.position.x, ref xDampVelocity, smoothTime);
            cameraPosition.y = Mathf.SmoothDamp(cameraPosition.y, transform.position.y, ref yDampVelocity, smoothTime);

            cameraTransform.position = cameraPosition;
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
