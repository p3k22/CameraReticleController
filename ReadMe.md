# CameraReticleController

Tag-driven camera reticle system for Unity. Raycasts from a camera each frame, finds the closest `IReticleTarget`, and shows the matching reticle UI image. `AnimateReticle(ReticleType)` is input-filtered, so only matching target types animate.

---

## How It Works

```
Every frame:
  ReticleController → raycasts from camera → finds closest IReticleTarget
    → target changed?
       ├─ Target found  → ShowReticle(tag) — activates the matching reticle image
       └─ Target lost   → HideAllReticles() — deactivates all reticle images
```

The reticle system is visual-only. It exposes `CurrentTarget` as read-only state for external systems to query.

---

## Requirements

| Dependency | Source |
|---|---|
| **Unity 2020.1+** | — |

No additional packages required.

---

## Setup

### 1. Add `ReticleTarget` to objects the camera can look at

Put the `ReticleTarget` component on any GameObject with a **Collider**.

Set:
- **Reticle Tag**: which reticle image should appear (e.g. `"default"`, `"Click"`, `"Push"`)
- **Reticle Type**: input/interaction category used by `AnimateReticle(ReticleType)`

### 2. Add `ReticleController` to the player or camera rig

Drag it onto the player object or camera rig. Configure in the Inspector:

| Field | What it does |
|---|---|
| **Allow Reticles** | Whether reticles are enabled on start. |
| **Camera Source** | The camera to raycast from (origin + forward direction). |
| **Detection Distance** | Max raycast range (default `10`). |
| **Reticle References** | List of tag → UI Image mappings (see below). |
| **Default Reticle Animator Trigger Name** | Animator trigger name for reticle animations (default `"Animate"`). |

### 3. Set up Reticle References

Each entry in the **Reticle References** list maps a tag to a UI `Image`:

| Field | What it does |
|---|---|
| **Tag** | Matches `ReticleTarget.ReticleTag` on your objects (e.g. `"Click"`). |
| **Reticle** | The UI `Image` component to show when this tag is active. |

If the `Image` has an `Animator` component, calling `AnimateReticle(type)` fires the configured trigger only when `CurrentTarget.ReticleType == type`.

### 4. (Optional) Use the included prefab

The package includes a **UI Camera** prefab under `PackageResources/Prefabs/` and cursor images under `PackageResources/Images/` to get started quickly.

---

## Implementing `IReticleTarget` Manually

`ReticleTarget` is the included implementation, but you can implement the interface on any component:

```csharp
using P3k.CameraReticleController.Enums;
using P3k.CameraReticleController.Interfaces;
using UnityEngine;

public class CustomTarget : MonoBehaviour, IReticleTarget
{
    public string ReticleTag => "Click";
    public ReticleType ReticleType => ReticleType.Type01;
}
```

Put this on a GameObject with a **Collider**. `ReticleTag` selects the displayed reticle; `ReticleType` controls animation filtering.

---

## Controlling Reticles from Code

```csharp
var reticle = GetComponent<ReticleController>();

// Read the current target (null if nothing is being looked at)
var target = reticle.CurrentTarget;

// Enable or disable reticles (e.g. during a cutscene)
reticle.AllowReticles(false);
reticle.AllowReticles(true);

// Manually show a specific reticle by tag
reticle.ShowReticle("Click");

// Hide all reticles
reticle.HideAllReticles();

// Trigger reticle animation for a specific input/interaction type
reticle.AnimateReticle(ReticleType.Type01);
```

---

## API Reference

### `ReticleController`

| Member | Description |
|---|---|
| `CurrentTarget` | The current `IReticleTarget` under the camera, or `null`. Read-only. |
| `AllowReticles(bool)` | Enable or disable reticles. Disabling hides all reticles immediately. |
| `ShowReticle(string tag)` | Activate the reticle matching the tag, deactivate all others. |
| `HideAllReticles()` | Deactivate all reticle images. |
| `AnimateReticle(ReticleType type)` | Trigger animation only if `CurrentTarget` exists and its `ReticleType` matches `type`. |
| `SetCameraSource(Camera camera)` | Reassign the camera used for raycast origin/direction. |
| `SetDetectionDistance(float distance)` | Update raycast max distance. |

### `ReticleTarget`

| Member | Description |
|---|---|
| `ReticleTag` | The tag string used to match this target to a reticle image. Configurable in the Inspector. Default: `"default"`. |
| `ReticleType` | The interaction/input type for animation filtering. Configurable in the Inspector. Default: `ReticleType.Type01`. |

### `IReticleTarget`

| Member | Description |
|---|---|
| `ReticleTag` | Tag used to select which reticle image to display. |
| `ReticleType` | Type used by `AnimateReticle(ReticleType)` to filter animation requests. |

### `IReticleRaycastProvider`

| Member | Description |
|---|---|
| `GetTarget()` | Returns the closest `IReticleTarget` hit by the raycast, or `null`. |

### `ReticleRaycastProvider`

| Member | Description |
|---|---|
| `ReticleRaycastProvider(Camera, float)` | Constructor. Takes the camera and max raycast distance. |
| `GetTarget()` | Performs a `Physics.RaycastNonAlloc` from the camera forward and returns the closest `IReticleTarget`. Pre-allocated buffer of 16 hits. |

---

## Package Resources

| Resource | Path |
|---|---|
| Cursor images | `PackageResources/Images/` |
| Click animation + controller | `PackageResources/Animations/` |
| UI Camera prefab | `PackageResources/Prefabs/` |



---

## License

See repository for license details.
