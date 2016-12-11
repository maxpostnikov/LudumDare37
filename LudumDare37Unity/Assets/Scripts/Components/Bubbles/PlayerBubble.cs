using UnityEngine;

namespace MaxPostnikov.LD37
{
    public class PlayerBubble : Bubble
    {
        enum State { Idle, Move, Decelerate }

        [Header("Colors")]
        public Color color = Color.blue;

        [Header("Settings")]
        public float maxSpeed = 3f;
        public float minTargetDist = 0.1f;
        public float decelerateTime = 1f;
        public AnimationCurve decelerateCurve;

        new Camera camera;
        float currentSpeed;
        float lastMoveSpeed;
        State currentState;
        Vector3 targetDir;
        Vector3 targetPosition;
        Vector3 mousePosition;

        public override void Init()
        {
            base.Init();

            camera = Camera.main;

            SetColor(color);
        }

        public override void UpdateBubble(Shell shell)
        {
            base.UpdateBubble(shell);

            if (Input.GetMouseButton(0) && mousePosition != Input.mousePosition) {
                currentSpeed = maxSpeed;
                currentState = State.Move;
                mousePosition = Input.mousePosition;

                targetPosition = camera.ScreenToWorldPoint(Input.mousePosition);
                targetPosition.z = 0f;

                targetDir = targetPosition - transform.position;
            }

            if (Input.GetMouseButtonUp(0)) {
                timer = 0f;
                lastMoveSpeed = currentSpeed;
                currentState = State.Decelerate;
            }

            if (currentState == State.Idle)
                return;

            if (currentState == State.Decelerate) {
                timer += Time.deltaTime;

                if (timer > decelerateTime)
                    StopMoving();
                else
                    currentSpeed = Mathf.Lerp(lastMoveSpeed, 0, decelerateCurve.Evaluate(timer / decelerateTime));
            }
            
            var dist = Vector3.Distance(transform.position, targetPosition);

            if (dist <= minTargetDist)
                StopMoving();
            else {
                if (TryMoveShell(shell.transform, shell.Radius))
                    shell.CameraFollow();

                transform.Translate(GetTranslation(targetDir));
            }
        }

        void StopMoving()
        {
            currentState = State.Idle;
            mousePosition = Vector3.zero;
        }

        bool TryMoveShell(Transform shellTransform, float shellRadius)
        {
            if (shellDelta > c_Radius)
                return false;

            if (currentState != State.Decelerate)
                currentSpeed = Mathf.Clamp(maxSpeed / shellRadius, 0, maxSpeed);

            var shellDir = targetPosition - shellTransform.position;

            shellTransform.Translate(GetTranslation(shellDir));

            return true;
        }

        Vector3 GetTranslation(Vector3 dir)
        {
            dir.z = 0f;

            return dir.normalized * currentSpeed * Time.deltaTime;
        }
    }
}
