using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Framework.Pathfinding;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.EntitySystem.Interfaces;

public interface IEntityView
{
	string name { get; }

	IEntity Data { get; }

	Transform ViewTransform { get; }

	GameObject GO { get; }

	EntityFader Fader { get; }

	Vector3 Position { get; }

	Quaternion Rotation { get; }

	float Orientation => Rotation.eulerAngles.y;

	string GameObjectName { get; set; }

	bool IsVisible { get; }

	bool IsInGame { get; }

	string EntityId { get; }

	bool IsInGameBySettings { get; }

	bool CreatedAtRuntime { get; }

	ReadonlyList<EntityRef> ChildrenEntityRefs { get; }

	ReadonlyList<GridObstacle> GridObstacles { get; }

	List<NavmeshCut> NavmeshCuts { get; }

	void UpdateViewActive();

	void OnInFogOfWarChanged();

	void AttachToData(IEntity data);

	void DetachFromData();

	void DestroyViewObject();

	void MarkCreatedAtRuntime();
}
