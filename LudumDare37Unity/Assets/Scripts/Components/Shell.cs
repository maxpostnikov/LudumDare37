using System.Collections;
using UnityEngine;

namespace MaxPostnikov.LD37
{
    public class Shell : MonoBehaviour
    {
        public event System.Action<float> RadiusChange;

        [Header("Refs")]
        public MeshRenderer backRenderer;
        public LineRenderer borderRenderer;
        public ColorBubble innerBubble;

        [Header("Border settings")]
        public float startRadius = 1f;
        public int segmentCount = 50;
        public float scaleTime = 0.5f;

        [Header("Movement settings")]
        public float speed = 5f;
        public float smoothTime = 0.5f;
        public float minTargetDist = 0.1f;

        [Header("Decrease settings")]
        public float decreaseTime = 3f;
        public float decreaseValue = -0.1f;
        
        public float Radius { get { return currentRadius; } }

        public float DecreaseProgress { get { return 1f - Mathf.Clamp01(decreaseTimer / decreaseTime); } }

        public Transform InnerBubble { get { return innerBubble.transform; } }
        
        float decreaseTimer;
        bool isAnimating;
        float targetRadius;
        float currentRadius;
        Transform cameraTransform;
        WaitForEndOfFrame waitEndFrame;

        new Camera camera;
        Vector3 delta, offset;
        Vector3 touchPosition;
        Vector3 mousePos, targetPos, targetBubblePos;
        float xVelocity, yVelocity, xCamVelocity, yCamVelocity;

        float InnerRadius { get { return currentRadius - innerBubble.Radius; } }

        public void Init()
        {
            innerBubble.Init();
            
            waitEndFrame = new WaitForEndOfFrame();

            camera = Camera.main;
            cameraTransform = Camera.main.transform;
            
            borderRenderer.useWorldSpace = false;
            borderRenderer.SetVertexCount(segmentCount + 1);
        }

        public void Reset()
        {
            isAnimating = false;
            decreaseTimer = 0f;

            targetPos = transform.position = Vector3.zero;
            targetBubblePos = innerBubble.transform.localPosition = Vector3.zero;

            currentRadius = startRadius;

            Generate();
        }

        public void UpdateShell()
        {
            decreaseTimer += Time.deltaTime;
            if (decreaseTimer > decreaseTime) {
                decreaseTimer = 0f;
                ChangeRadius(decreaseValue);
            }

            transform.position = SmoothTranslate(transform.position, targetPos, ref xVelocity, ref yVelocity);
            cameraTransform.position = SmoothTranslate(cameraTransform.position, transform.position, ref xCamVelocity, ref yCamVelocity);
            
            if (IsNeedToMove) {
                mousePos = camera.ScreenToWorldPoint(InputPosition);
                mousePos.z = 0f;

                delta = mousePos - innerBubble.transform.position;
                if (delta.magnitude > minTargetDist) {
                    offset = GetOffset();

                    targetBubblePos.x += offset.x;
                    targetBubblePos.y += offset.y;
                    targetBubblePos.z = 0f;

                    if (!TryKeepInBounds()) {
                        delta = mousePos - transform.position;
                        offset = GetOffset(1f / currentRadius);

                        targetPos.x += offset.x;
                        targetPos.y += offset.y;
                        targetPos.z = 0f;
                    }
                }
            } else {
                TryKeepInBounds();
            }
            
            innerBubble.transform.localPosition = targetBubblePos;
        }

        bool IsNeedToMove {
            get {
                return Input.touchCount > 0 || Input.GetMouseButton(0);
            }
        }

        Vector3 InputPosition {
            get {
                if (Input.touchCount > 0) {
                    var touch = Input.GetTouch(0);

                    touchPosition.x = touch.position.x;
                    touchPosition.y = touch.position.y;

                    return touchPosition;
                }
                
                return Input.mousePosition;
            }
        }

        bool TryKeepInBounds()
        {
            var dist = Vector2.Distance(Vector2.zero, targetBubblePos);
            if (dist > InnerRadius) {
                targetBubblePos = targetBubblePos.normalized * InnerRadius;

                return false;
            }

            return true;
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
        
        public void NpcImpact(float value, bool isEnemy)
        {
            if (!isEnemy) decreaseTimer = 0f;

            ChangeRadius(value);
        }

        void ChangeRadius(float value)
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

            if (RadiusChange != null)
                RadiusChange(currentRadius);

            isAnimating = false;
        }
    }
}
