namespace P3k.CameraReticleController.Services
{
   using P3k.CameraReticleController.Interfaces;

   using System.Linq;

   using UnityEngine;

   /// <summary>
    ///    Performs local physics raycasts from a camera to find the closest
    ///    <see cref="IReticleTarget"/>. All layers, pre-allocated buffer.
    /// </summary>
    public class ReticleRaycastProvider : IReticleRaycastProvider
    {
        private readonly Camera _camera;

        private readonly float _maxDistance;

        private readonly RaycastHit[] _hits = new RaycastHit[16];

        public ReticleRaycastProvider(Camera camera, float maxDistance)
        {
            _camera = camera;
            _maxDistance = maxDistance;
        }

        public IReticleTarget GetTarget()
        {
            if (!_camera)
            {
                return null;
            }

            var cameraTransform = _camera.transform;
            var ray = new Ray(cameraTransform.position, cameraTransform.forward);
            var hitCount = Physics.RaycastNonAlloc(ray, _hits, _maxDistance);

            IReticleTarget closest = null;
            var closestDistance = float.MaxValue;

            for (var i = 0; i < hitCount; i++)
            {
                var hitCollider = _hits[i].collider;

                if (!hitCollider)
                {
                    continue;
                }

                var target = hitCollider.GetComponent<IReticleTarget>();

                if (target == null)
                {
                    continue;
                }

                if (_hits[i].distance < closestDistance)
                {
                    closestDistance = _hits[i].distance;
                    closest = target;
                }
            }

            return closest;
        }
    }
}
