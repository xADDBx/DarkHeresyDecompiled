using System.Diagnostics.CodeAnalysis;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UI.Pointer;
using Kingmaker.View;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public ref struct InputContext
{
	[MaybeNull]
	public BaseUnitEntity Unit;

	[MaybeNull]
	public CameraRig CameraRig;

	[MaybeNull]
	public CursorController Cursor;

	public readonly void SetUnitMovement(Vector2 value)
	{
		Game.Instance.Controllers.SynchronizedDataController.PushLeftStickMovement(Unit, value.normalized, value.magnitude);
	}

	public readonly void SetCameraOrbit(Vector2 value)
	{
		CameraRig.Rotate(value.x);
		CameraRig.CameraZoom.GamepadScrollPosition = value.y * 0.05f;
	}

	public readonly void SetCameraMovement(Vector2 value)
	{
		CameraRig.Scroll(value);
	}
}
