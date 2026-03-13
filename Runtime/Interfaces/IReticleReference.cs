namespace P3k.CameraReticleController.Interfaces
{
   using System.Linq;

   using UnityEngine;
   using UnityEngine.UI;

   public interface IReticleReference
   {
      Image Image { get; }

      string Tag { get; }

      Animator Animator { get; }
   }
}
