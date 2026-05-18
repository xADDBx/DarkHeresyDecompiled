using System.Collections;
using Kingmaker.View;
using Kingmaker.Visual.Animation;
using Kingmaker.Visual.Animation.Kingmaker;
using Kingmaker.Visual.CharactersRigidbody;
using Kingmaker.Visual.CharacterSystem.Dismemberment;
using UnityEngine;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IAbstractUnitEntityView : IMechanicEntityView, IEntityView
{
	UnitAnimationManager AnimationManager { get; }

	UnitMovementAgent MovementAgent { get; }

	RigidbodyCreatureController RigidbodyController { get; }

	UnitDismembermentManager DismembermentManager { get; }

	IKController IkController { get; }

	Animator Animator { get; }

	ViewInterpolationHelper InterpolationHelper { get; }

	Vector2 CameraOrientedBoundsSize { get; }

	Bounds RenderersBounds { get; }

	bool IsGetUp { get; }

	bool LimbsApartDismembermentRestricted { get; set; }

	Coroutine StartCoroutine(IEnumerator routine);

	void StopCoroutine(Coroutine routine);

	void ForcePlaceAboveGround();

	void UnFade();

	void FadeHide();

	void ForcePeacefulLook(bool peaceful);

	void EnterProneState();

	void LeaveProneState();

	void ResetHoverHighlighted();

	void UpdateHighlight(bool raiseEvent = true);

	void HandleDeath();
}
