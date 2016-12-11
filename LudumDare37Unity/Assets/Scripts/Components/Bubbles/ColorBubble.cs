using UnityEngine;

namespace MaxPostnikov.LD37
{
    public class ColorBubble : MonoBehaviour
    {
        [Header("Color Bubble")]
        public Color mainColor = Color.white;

        public float Radius { get; private set; }
        
        MeshRenderer meshRenderer;
        MaterialPropertyBlock propertyBlock;

        public virtual void Init()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            propertyBlock = new MaterialPropertyBlock();

            Radius = transform.localScale.x / 2f;

            SetColor(mainColor);
        }
        
        protected void SetColor(Color color)
        {
            propertyBlock.SetColor("_Color", color);

            meshRenderer.SetPropertyBlock(propertyBlock);
        }
    }
}
