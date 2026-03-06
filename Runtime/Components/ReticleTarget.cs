namespace P3k.CameraReticleController.Components
{
   using P3k.CameraReticleController.Interfaces;

   using System.Linq;

   using UnityEngine;

   public class ReticleTarget : MonoBehaviour, IReticleTarget
   {
      [field: SerializeField]
      public string ReticleTag { get; private set; } = "default";
   }
}
