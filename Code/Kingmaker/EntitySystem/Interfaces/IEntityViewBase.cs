using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Utility.DotNetExtensions;
using UnityEngine;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IEntityViewBase
{
	Transform ViewTransform { get; }

	string GameObjectName { get; set; }

	GameObject GO { get; }

	bool IsVisible { get; }

	bool IsInGame { get; }

	string UniqueViewId { get; set; }

	bool IsInGameBySettings { get; }

	IEntity Data { get; }

	bool CreatedAtRuntime { get; }

	ReadonlyList<EntityRef> ChildrenEntityRefs { get; }

	void UpdateViewActive();

	void OnInFogOfWarChanged();

	void AttachToData(IEntity data);

	void DetachFromData();

	void DestroyViewObject();

	void MarkCreatedAtRuntime();
}
