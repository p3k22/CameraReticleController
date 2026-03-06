namespace P3k.CameraReticleController.Interfaces
{
    /// <summary>
    ///    Marks a component as a valid reticle target.
    ///    Fully separate from <see cref="IInteractable"/> — no inheritance.
    /// </summary>
    public interface IReticleTarget
    {
        /// <summary>
        ///    Tag used to select which reticle image to display.
        /// </summary>
        string ReticleTag { get; }
    }
}
