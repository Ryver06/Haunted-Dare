using System;
using UnityEngine;
using UnityEngine.UI;

public class NoiseMeter : MonoBehaviour
{
   public static NoiseMeter Instance;
   
   [Header("Noise")]
   [SerializeField] private float maxNoise = 100f;
   [SerializeField] private float noise;
   [SerializeField] private float noiseDrain;
   [SerializeField] private float tooLoud;
   
   [Header("Ui")] 
   [SerializeField] private Image noiseMeter; 
   [SerializeField] private GameObject noiseUI;


 private bool active = false; //is this mode active or not
   private void Awake()
   {
      Instance = this;
   }

   private void Update()
   {
       UpdateNoiseMeter();
       TooLoud();
   }
   
   
   private void UpdateNoiseMeter()
   {
       if (!active) return; //if this mode isn't active, don't update the noise meter
       
       
       
           float targetFillAmount = (float)noise / maxNoise;
           noiseMeter.fillAmount = targetFillAmount;
       

       
       if (noise >= maxNoise)
       {
           noise = maxNoise;
       }
       
       noise -= noiseDrain + Time.deltaTime;

       if (noise <= 0)
       {
           noise = 0;
       }
   }

   public void MakeNoise(float sound)
   {
       if (!active) return;
       
       noise += sound;
   }

   public void ActivateNoiseMeter()
   {
       active = true;
       noiseUI.SetActive(true);
   }
   
   public void DeactivateNoiseMeter()
   {
       active = false;
       noiseUI.SetActive(false);
   }

   private void TooLoud()
   {
       if (noise >= tooLoud)
       {
           EdrickBehaviour.Instance.HeardPlayer();
       }
   }
}
