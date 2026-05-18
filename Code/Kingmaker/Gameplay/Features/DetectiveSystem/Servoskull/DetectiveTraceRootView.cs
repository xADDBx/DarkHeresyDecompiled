using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Framework.EntitySystem.Interfaces.Config;
using Kingmaker.View.MapObjects;
using R3;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[Serializable]
[KDB("Интеракт, который переведет указанные следы в состояние Found и позволит взаимодействовать с ними.")]
[RequireComponent(typeof(InteractionDetectiveTrace))]
[KnowledgeDatabaseID("d84242375ea04faf831b7df6b3fb50cb")]
public class DetectiveTraceRootView : MapObjectView, IDetectiveTraceRootConfig, IMapObjectEntityConfig, IMechanicEntityConfig, IEntityConfig
{
	public List<DetectiveTraceView> RootTraces = new List<DetectiveTraceView>();

	private bool m_CanShowMarker;

	private CompositeDisposable m_TracesDisposableBag = new CompositeDisposable();

	public IEnumerable<EntityRef<DetectiveTraceEntity>> Traces => RootTraces.Select((DetectiveTraceView i) => new EntityRef<DetectiveTraceEntity>(i.UniqueId));

	public override Entity CreateEntityData(bool load)
	{
		InteractionDetectiveTrace component = GetComponent<InteractionDetectiveTrace>();
		m_CanShowMarker = component.ShowNotFollowedOnMap && !component.IsVariative;
		return Entity.Initialize(new DetectiveTraceRootEntity(this));
	}

	protected override void OnEnable()
	{
		RootTraces.ForEach(delegate(DetectiveTraceView t)
		{
			t.CurrentStatus.Subscribe(delegate
			{
				UpdateMarkers();
			}).AddTo(m_TracesDisposableBag);
		});
		base.OnEnable();
	}

	protected override void OnDisable()
	{
		m_TracesDisposableBag.Clear();
		base.OnDisable();
	}

	protected override void OnVisibilityChanged()
	{
		base.OnVisibilityChanged();
		UpdateMarkers();
	}

	public override void OnInFogOfWarChanged()
	{
		base.OnInFogOfWarChanged();
		UpdateMarkers();
	}

	public void UpdateMarkers()
	{
		if (m_CanShowMarker)
		{
			if (!base.Data.IsInGame)
			{
				RemoveMarkers();
			}
			else if (RootTraces.Any(delegate(DetectiveTraceView trace)
			{
				DetectiveTraceStatus? detectiveTraceStatus = trace.Data?.Status;
				return !detectiveTraceStatus.HasValue || detectiveTraceStatus.GetValueOrDefault() != DetectiveTraceStatus.Followed;
			}))
			{
				CreateMarkers();
			}
			else
			{
				RemoveMarkers();
			}
		}
	}

	private void CreateMarkers()
	{
		LocalMapMarkerPart orCreate = base.Data.GetOrCreate<LocalMapMarkerPart>();
		orCreate.IsRuntimeCreated = true;
		orCreate.Settings.Type = LocalMapMarkType.DetectiveTrace;
	}

	private void RemoveMarkers()
	{
		base.Data.Remove<LocalMapMarkerPart>();
	}
}
