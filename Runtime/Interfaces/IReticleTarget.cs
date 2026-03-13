namespace P3k.CameraReticleController.Interfaces
{
   using P3k.CameraReticleController.Enums;

   public interface IReticleTarget
   {
      string ReticleTag { get; }

      ReticleType ReticleType { get; }
   }
}
