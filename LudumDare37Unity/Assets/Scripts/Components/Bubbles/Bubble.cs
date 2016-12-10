using UnityEngine;

namespace MaxPostnikov.LD37
{
    public abstract class Bubble : MonoBehaviour
    {
        protected const float c_Radius = 0.5f;

        MeshRenderer meshRenderer;
        MaterialPropertyBlock propertyBlock;

        protected float timer;
        protected float shellDelta;

        public virtual void Init()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            propertyBlock = new MaterialPropertyBlock();
        }

        public virtual void UpdateBubble(Shell shell)
        {
            var shellDist = Vector3.Distance(transform.position, shell.transform.position);

            shellDelta = shell.Radius - shellDist;
        }

        protected void SetColor(Color color)
        {
            propertyBlock.SetColor("_Color", color);

            meshRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
