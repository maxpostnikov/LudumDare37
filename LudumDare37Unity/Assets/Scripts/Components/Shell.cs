using System.Collections;
using UnityEngine;

namespace MaxPostnikov.LD37
{
    public class Shell : MonoBehaviour
    {
        [Header("Refs")]
        public MeshRenderer backRenderer;
        public LineRenderer borderRenderer;
        public ColorBubble innerBubble;

        [Header("Border settings")]
        public float startRadius = 1f;
        public int segmentCount = 50;

        [Header("Movement settings")]
        public float speed = 5f;
        public float smoothTime = 0.5f;
        public float minTargetDist = 0.1f;

        [Header("Anim settings")]
        public float scaleTime = 0.5f;
        
        public float Radius { get { return currentRadius; } }

        public Transform InnerBubble { get { return innerBubble.transform; } }
        
        bool isAnimating;
        float targetRadius;
        float currentRadius;
        Transform cameraTransform;
        WaitForEndOfFrame waitEndFrame;

        new Camera camera;
        Vector3 delta, offset;
        Vector3 mousePos, targetPos, targetBubblePos;
        float xVelocity, yVelocity, xCamVelocity, yCamVelocity;

        float InnerRadius { get { return currentRadius - innerBubble.Radius; } }

        public void Init()
        {
            innerBubble.Init();

            currentRadius = startRadius;
            waitEndFrame = new WaitForEndOfFrame();

            camera = Camera.main;
            cameraTransform = Camera.main.transform;

            targetPos = transform.position;
            targetBubblePos = innerBubble.transform.localPosition;

            borderRenderer.useWorldSpace = false;
            borderRenderer.SetVertexCount(segmentCount + 1);
            
            Generate();
        }

        public void UpdateShell()
        {
            transform.position = SmoothTranslate(transform.position, targetPos, ref xVelocity, ref yVelocity);
            cameraTransform.position = SmoothTranslate(cameraTransform.position, transform.position, ref xCamVelocity, ref yCamVelocity);

            if (!Input.GetMouseButton(0))
                return;

            mousePos = camera.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            delta = mousePos - innerBubble.transform.position;
            if (delta.magnitude <= minTargetDist)
                return;

            offset = GetOffset();

            targetBubblePos.x += offset.x;
            targetBubblePos.y += offset.y;
            targetBubblePos.z = 0f;

            var dist = Vector2.Distance(Vector2.zero, targetBubblePos);
            if (dist > InnerRadius) {
                targetBubblePos = targetBubblePos.normalized * InnerRadius;

                delta = mousePos - transform.position;
                offset = GetOffset(1f / currentRadius);

                targetPos.x += offset.x;
                targetPos.y += offset.y;
                targetPos.z = 0f;
            }

            innerBubble.transform.localPosition = targetBubblePos;
        }

        Vector3 SmoothTranslate(Vector3 current, Vector3 target, ref float xVelocity, ref float yVelocity)
        {
            current.x = Mathf.SmoothDamp(current.x, target.x, ref xVelocity, smoothTime);
            current.y = Mathf.SmoothDamp(current.y, target.y, ref yVelocity, smoothTime);

            return current;
        }

        Vector3 GetOffset(float speedMult = 1f)
        {
            delta.z = 0f;

            return delta.normalized * speed * speedMult * Time.deltaTime;
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
