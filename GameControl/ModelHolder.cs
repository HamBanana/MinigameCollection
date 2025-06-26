using Unity.VisualScripting;
using UnityEngine;

namespace Ham.GameControl
{
    public class ModelHolder : MonoBehaviour
    {
        protected bool IsAnimating = false;
        protected int AnimatedMaterialIndex = -1;
        protected Color AnimatedOriginalBaseColor;
        protected float AnimatedDuration = 1f;
        protected float AnimatedTimer = 0f;

        private MeshRenderer meshRenderer;

        void Start()
        {
            this.meshRenderer = this.GetComponentInChildren<MeshRenderer>();
        }
        void Update()
        {
            if (this.IsAnimating || this.AnimatedMaterialIndex > -1){
                Debug.Log("Animating...");
                this.UpdateAnimation();
            }  
        }

        private void UpdateAnimation(){
            Material[] materials = this.meshRenderer.materials;
            float t = Time.deltaTime / 0.5f;
            float greenValue = Mathf.PingPong(t, 0.5f) * this.AnimatedDuration;
            Color c = materials[this.AnimatedMaterialIndex].GetColor("_BaseColor");
            c.g = greenValue;
            materials[this.AnimatedMaterialIndex].SetColor("_BaseColor", c);
            this.meshRenderer.materials = materials;
            if (this.AnimatedTimer > this.AnimatedDuration){
                this.IsAnimating = false;
                this.AnimatedTimer = 0f;
                this.AnimatedDuration = 0f;
                this.AnimatedOriginalBaseColor = Color.white;

                Debug.Log("Animation done....");
            }
        }

        protected void ChangeMaterial(Material mat, int index = 1){
            MeshRenderer renderer = this.meshRenderer;
            var mats = renderer.materials;
            if (index > 0 && index < mats.Length){
                mats[index] = mat;
                renderer.materials = mats;
            }
        }

        protected void FlashMaterialColor(string materialname, Color color, float duration = 0.5f){
            if (this.IsAnimating){return;}
            Debug.Log("Flashing material color");
            MeshRenderer rend = this.GetComponentInChildren<MeshRenderer>();

            Material[] mats = this.meshRenderer.materials;
            Debug.Log("Looking for material: " + materialname);
            
            this.AnimatedTimer = 0.5f;
            this.AnimatedDuration = duration;


            for (int i = 1; i < mats.Length; i++){
                if (mats[i].name.StartsWith(materialname)){
                    Debug.Log("Material found");
                    this.AnimatedOriginalBaseColor = mats[i].GetColor("_BaseColor");
                    Debug.Log("BaseColor: " + this.AnimatedOriginalBaseColor.ToString());
                    this.AnimatedMaterialIndex = i;
                }
            }
            this.IsAnimating = true;
            Debug.Log("Animation started...");
            
            //rend.materials[(int)matindex].SetColor("_BaseColor", Color.Lerp((Color)originalbasecolor, color, t));
            //rend.materials = materials;
            
            //rend.materials[(int)matindex].SetColor("_BaseColor", Color.Lerp(color, (Color)originalbasecolor, t));
            //rend.materials = materials;
        }
    }
}
