namespace P3k.CameraReticleController.Components
{
   using P3k.CameraReticleController.Interfaces;
   using P3k.CameraReticleController.Services;

   using System;
   using System.Collections.Generic;
   using System.Linq;

   using UnityEngine;
   using UnityEngine.UI;

   /// <summary>
   ///    Owns its own raycast via <see cref="IReticleRaycastProvider" /> and tracks
   ///    the current <see cref="IReticleTarget" /> as state.
   ///    No events, no coupling to the interaction system.
   ///    Public methods <see cref="ShowReticle" />, <see cref="HideAllReticles" />,
   ///    and <see cref="AnimateReticle" /> remain available for external callers.
   /// </summary>
   public class ReticleController : MonoBehaviour
   {
      private static int _animate;

      [SerializeField]
      [Tooltip("Whether reticles should be enabled on start.")]
      private bool _allowReticles = true;

      [SerializeField]
      [Tooltip("Camera used as the reticle ray origin and direction.")]
      private Camera _cameraSource;

      [SerializeField]
      [Tooltip("Maximum detection distance for reticle targets.")]
      private float _detectionDistance = 10f;

      private IReticleRaycastProvider _raycastProvider;

      private IReticleTarget _previousTarget;

      [SerializeField]
      private List<ReticleReference> _reticleReferences;

      [SerializeField]
      [Tooltip("If the reticle image has an Animator, this trigger will be fired to animate it.")]
      private string _defaultReticleAnimatorTriggerName = "Animate";

      [SerializeField]
      private bool _debugAnimate;

      /// <summary>
      ///    The current reticle target, or null. Read-only state.
      /// </summary>
      public IReticleTarget CurrentTarget { get; private set; }

      private void Awake()
      {
         _animate = Animator.StringToHash(_defaultReticleAnimatorTriggerName);
         _raycastProvider = new ReticleRaycastProvider(_cameraSource, _detectionDistance);
         AllowReticles(_allowReticles);
         HideAllReticles();
      }

      private void Update()
      {
         CurrentTarget = _raycastProvider.GetTarget();

         if (CurrentTarget != _previousTarget)
         {
            if (CurrentTarget != null)
            {
               ShowReticle(CurrentTarget.ReticleTag);
            }
            else
            {
               HideAllReticles();
            }

            _previousTarget = CurrentTarget;
         }

         if (_debugAnimate)
         {
            _debugAnimate = false;
            AnimateReticle();
         }
      }

      public void SetCameraSource(Camera camera)
      {
         _cameraSource = camera;
         _raycastProvider = new ReticleRaycastProvider(_cameraSource, _detectionDistance);
      }

      public void SetDetectionDistance(float distance)
      {
         _detectionDistance = distance;
         _raycastProvider = new ReticleRaycastProvider(_cameraSource, _detectionDistance);
      }

      /// <summary>
      ///    Allows or disallows reticles from being activated.
      /// </summary>
      public void AllowReticles(bool allow = true)
      {
         _allowReticles = allow;

         if (!allow)
         {
            HideAllReticles();
         }
      }

      /// <summary>
      ///    Triggers the animation for the reticle matching the provided tag.
      /// </summary>
      public void AnimateReticle()
      {
         if (!_allowReticles)
         {
            return;
         }

         if (CurrentTarget == null || string.IsNullOrEmpty(CurrentTarget.ReticleTag))
         {
            return;
         }

         var reticleRef = _reticleReferences.FirstOrDefault(r => r.Tag == CurrentTarget.ReticleTag);
         if (reticleRef == null)
         {
            return;
         }

         var animator = reticleRef.Animator;
         if (!animator)
         {
            return;
         }

         animator.SetTrigger(_animate);
      }

      /// <summary>
      ///    Deactivates all reticles.
      /// </summary>
      public void HideAllReticles()
      {
         foreach (var reference in _reticleReferences)
         {
            reference.Reticle.enabled = false;
         }
      }

      /// <summary>
      ///    Activates the reticle matching the provided tag and deactivates the rest.
      /// </summary>
      public void ShowReticle(string tag)
      {
         if (!_allowReticles)
         {
            return;
         }

         foreach (var reference in _reticleReferences)
         {
            reference.Reticle.enabled = reference.Tag == tag;
         }
      }
   }

   /// <summary>
   ///    Stores references to a reticle image and its identifying name.
   /// </summary>
   [Serializable]
   internal sealed class ReticleReference
   {
      [SerializeField]
      internal Image Reticle;

      [SerializeField]
      internal string Tag;

      private Animator _animator;

      internal Animator Animator => _animator == null ? _animator = Reticle.GetComponent<Animator>() : _animator;
   }
}
