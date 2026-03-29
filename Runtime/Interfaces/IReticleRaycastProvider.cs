namespace P3k.CameraReticleController.Interfaces
{
    /// <summary>
    ///    Provides raycast results for reticle targeting.
    ///    Separate from <see cref="IRaycastProvider"/> which serves the interaction system.
    /// </summary>
    public interface IReticleRaycastProvider
    {
        /// <summary>
        ///    Returns the closest hit with an <see cref="IReticleTarget"/> component, or null.
        /// </summary>
        IReticleTarget GetTarget();
    }
}
