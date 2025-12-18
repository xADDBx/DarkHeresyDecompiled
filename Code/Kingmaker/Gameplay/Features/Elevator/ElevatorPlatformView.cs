using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Pathfinding;
using Kingmaker.View;
using Kingmaker.View.MapObjects;
using Owlcat.Fmw.Blueprints;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.Elevator;

[RequireComponent(typeof(ValidNavmeshArea))]
[KnowledgeDatabaseID("c825ab9c8b0946c8809edfc94a5a33f5")]
public sealed class ElevatorPlatformView : MapObjectView, IElevatorPlatformConfig
{
	public BpRef<BlueprintCutscene> Cutscene = new BpRef<BlueprintCutscene>();

	[ValidateNotEmpty]
	public LocatorView[] PartyPlaces = new LocatorView[0];

	[ValidateNotEmpty]
	public ElevatorPlatformRouteSettings[] Routes = new ElevatorPlatformRouteSettings[0];

	[NonSerialized]
	private Vector2 _size;

	string IElevatorPlatformConfig.EntityId => UniqueId;

	bool IElevatorPlatformConfig.IsInGame => base.IsInGameBySettings;

	Vector2 IElevatorPlatformConfig.Size => _size;

	BlueprintCutscene? IElevatorPlatformConfig.Cutscene => Cutscene;

	IEnumerable<EntityRef<LocatorEntity>> IElevatorPlatformConfig.PartyPlaces => PartyPlaces.Select((LocatorView i) => new EntityRef<LocatorEntity>(i.UniqueId));

	IEnumerable<EntityRef<ElevatorPlatformStopEntity>> IElevatorPlatformConfig.Stops => Routes.SelectMany((ElevatorPlatformRouteSettings i) => Enumerable.Empty<EntityRef<ElevatorPlatformStopEntity>>().Append(new EntityRef<ElevatorPlatformStopEntity>(i.From.UniqueId)).Append(new EntityRef<ElevatorPlatformStopEntity>(i.To.UniqueId))).Distinct();

	IEnumerable<IElevatorPlatformRouteSettings> IElevatorPlatformConfig.Routes => Routes;

	public override bool CreatesDataOnLoad => true;

	public override bool AllowChildrenEntityViews => true;

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new ElevatorPlatformEntity(this));
	}

	protected override void Awake()
	{
		base.Awake();
		Bounds? bounds = null;
		foreach (Collider collider in m_Colliders)
		{
			if (collider.enabled && collider.gameObject.activeInHierarchy)
			{
				if (!bounds.HasValue)
				{
					bounds = collider.bounds;
				}
				else
				{
					bounds.Value.Encapsulate(collider.bounds);
				}
			}
		}
		_size = (bounds?.size ?? Vector3.zero).To2D();
	}
}
