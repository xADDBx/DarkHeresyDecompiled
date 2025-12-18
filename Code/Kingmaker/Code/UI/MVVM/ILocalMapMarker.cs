using Kingmaker.EntitySystem.Entities.Base;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public interface ILocalMapMarker
{
	bool IsDisposed { get; }

	LocalMapMarkType GetMarkerType();

	string GetDescription();

	Vector3 GetPosition();

	bool IsVisible();

	bool IsMapObject();

	Entity GetEntity();
}
