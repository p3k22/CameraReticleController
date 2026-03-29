namespace P3k.CameraReticleController.Components
{
   using P3k.CameraReticleController.Interfaces;
   using P3k.CameraReticleController.Services;

   using System;
   using System.Collections;
   using System.Collections.Generic;
   using System.Linq;

   using UnityEngine;
   using UnityEngine.UI;

   /// <summary>
   ///    Owns its own raycast via <see cref="IReticleRaycastProvider" /> and tracks
   ///    the current <see cref="IReticleTarget" /> as state.
   ///    Public methods <see cref="ShowReticle" />, <see cref="HideAllReticles" />,
   ///    and <see cref="AnimateReticle" /> remain available for external callers.
   /// </summary>
   public class ReticleController : MonoBehaviour, IReticleController
   {
      private static int _animateHash;

      private readonly HashSet<IReticleTarget> _suppressed = new();

      [SerializeField]
      [Tooltip("Whether reticles should be enabled on start.")]
      private bool _allowReticles = true;

      [SerializeField]
      [Tooltip("Camera used as the reticle ray origin and direction.")]
      private Camera _cameraSource;

      private Dictionary<string, IReticleReference> _reticleLookup;

      [SerializeField]
      [Tooltip("Maximum detection distance for reticle targets.")]
      private float _detectionDistance = 10f;

      private Coroutine _hideCoroutine;

      private IReticleRaycastProvider _raycastProvider;

      private IReticleTarget _previousTarget;

      [SerializeField]
      private List<ReticleReference> _reticleReferences;

      [SerializeField]
      [Tooltip("If the reticle image has an Animator, this trigger will be fired to animate it.")]
      private string _defaultReticleAnimatorTriggerName = "Animate";

      /// <summary>
      ///    The current reticle target, or null. Read-only state.
      /// </summary>
      public IReticleTarget CurrentTarget { get; private set; }

      private void Awake()
      {
         _animateHash = Animator.StringToHash(_defaultReticleAnimatorTriggerName);
         _raycastProvider = new ReticleRaycastProvider(_cameraSource, _detectionDistance);

         _reticleLookup = new Dictionary<string, IReticleReference>();

         foreach (var reference in _reticleReferences)
         {
            if (!string.IsNullOrEmpty(reference.Tag))
            {
               _reticleLookup[reference.Tag] = reference;
            }
         }

         AllowReticles(_allowReticles);
         HideAllReticles();
      }

      private void Update()
      {
         var target = _raycastProvider.GetTarget();
         CurrentTarget = target != null && _suppressed.Contains(target) ? null : target;

         if (CurrentTarget == _previousTarget)
         {
            return;
         }

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
      ///    Triggers the animation for the currently enabled reticle.
      /// </summary>
      public void AnimateReticle()
      {
         if (!_allowReticles)
         {
            return;
         }

         var active = _reticleReferences.FirstOrDefault(r => r.Image.enabled);
         if (active?.Animator != null)
         {
            active.Animator.SetTrigger(_animateHash);
         }
      }

      /// <summary>
      ///    Triggers the animation for the reticle identified by <paramref name="tag" />.
      /// </summary>
      public void AnimateReticle(string tag)
      {
         if (!_allowReticles)
         {
            return;
         }

         if (_reticleLookup.TryGetValue(tag, out var reticleRef))
         {
            if (reticleRef?.Animator == null)
            {
               return;
            }

            reticleRef.Animator.SetTrigger(_animateHash);
         }
      }

      /// <summary>
      ///    Deactivates all reticles after any playing animations have finished.
      /// </summary>
      public void HideAllReticles()
      {
         if (_hideCoroutine != null)
         {
            StopCoroutine(_hideCoroutine);
         }

         _hideCoroutine = StartCoroutine(HideAllReticlesAfterAnimations());
      }

      private IEnumerator HideAllReticlesAfterAnimations()
      {
         foreach (var reference in _reticleReferences)
         {
            if (!reference.Image.enabled)
            {
               continue;
            }

            var animator = reference.Animator;
            if (animator && animator.runtimeAnimatorController)
            {
               var state = animator.GetCurrentAnimatorStateInfo(0);
               if (!state.IsName("Empty"))
                  yield return new WaitUntil(() => animator.GetCurrentAnimatorStateInfo(0).IsName("Empty"));
            }
         }

         foreach (var reference in _reticleReferences)
         {
            reference.Image.enabled = false;
         }

         _hideCoroutine = null;
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
      ///    Activates the reticle matching the provided tag and deactivates the rest.
      /// </summary>
      public void ShowReticle(string tag)
      {
         if (!_allowReticles)
         {
            return;
         }

         if (_hideCoroutine != null)
         {
            StopCoroutine(_hideCoroutine);
            _hideCoroutine = null;
         }

         foreach (var reference in _reticleReferences)
         {
            reference.Image.enabled = reference.Tag == tag;
         }
      }

      /// <summary>
      ///    Suppresses the reticle for a specific target. The reticle will not display
      ///    while the target remains in the suppression list.
      /// </summary>
      public void Suppress(IReticleTarget target)
      {
         _suppressed.Add(target);
      }

      /// <summary>
      ///    Removes a target from the suppression list, allowing its reticle to display again.
      /// </summary>
      public void Unsuppress(IReticleTarget target)
      {
         _suppressed.Remove(target);
      }

      public bool TryGetReticle(string tag, out IReticleReference reticle)
      {
         return _reticleLookup.TryGetValue(tag, out reticle);
      }
   }

   /// <summary>
   ///    Stores references to a reticle image and its identifying name.
   /// </summary>
   [Serializable]
   internal sealed class ReticleReference : IReticleReference
   {
      [SerializeField]
      private Image _reticle;

      [SerializeField]
      private string _tag;

      private Animator _animator;

      public Image Image => _reticle;

      public string Tag => _tag;

      public Animator Animator => _animator == null ? _animator = Image.GetComponent<Animator>() : _animator;
   }
}