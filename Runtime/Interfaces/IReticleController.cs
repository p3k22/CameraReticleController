namespace P3k.CameraReticleController.Interfaces
{
   using System.Linq;

   using UnityEngine;

   public interface IReticleController
   {
      IReticleTarget CurrentTarget { get; }

      void AllowReticles(bool allow = true);

      void AnimateReticle();

      void AnimateReticle(string tag);

      void HideAllReticles();

      void SetCameraSource(Camera camera);

      void SetDetectionDistance(float distance);

      void ShowReticle(string tag);

      void Suppress(IReticleTarget target);

      void Unsuppress(IReticleTarget target);

      bool TryGetReticle(string tag, out IReticleReference reticle);
   }
}
