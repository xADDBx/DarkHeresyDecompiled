using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.Gameplay.Features.AreaEffects;
using Kingmaker.UnitLogic.Abilities.Blueprints;
using Kingmaker.UnitLogic.Mechanics;
using Kingmaker.Utility;
using Kingmaker.View.MapObjects.SriptZones;
using Kingmaker.View.Mechanics;
using Kingmaker.Visual.Particles;
using Owlcat.Runtime.Core.Logging;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.View.MapObjects;

[KnowledgeDatabaseID("6c9e918a566788343b39a46a9e3f3a1c")]
public class AreaEffectView : MechanicEntityView
{
	[SerializeField]
	private BlueprintAreaEffectReference m_Blueprint;

	[CanBeNull]
	private MechanicsContext m_Context;

	private TargetWrapper m_Target;

	private TimeSpan m_CreationTime;

	private TimeSpan? m_Duration;

	private bool m_UsePatternFromAbility;

	private GameObject m_SpawnedFx;

	private NavmeshCut m_NavmeshCut;

	public bool HasSpawnedFx => m_SpawnedFx != null;

	public IScriptZoneShape Shape { get; private set; }

	public bool OnUnit { get; set; }

	public MechanicsContext Context => m_Context;

	public new AreaEffectEntity Data => (AreaEffectEntity)base.Data;

	protected override void Awake()
	{
		base.Awake();
		Shape = GetComponent<IScriptZoneShape>();
	}

	public void InitAtRuntime([NotNull] MechanicsContext context, [NotNull] BlueprintAreaEffect blueprint, [NotNull] TargetWrapper target, TimeSpan creationTime, TimeSpan? duration, bool usePatternFromAbility = false, bool getOrientationFromCaster = false)
	{
		m_Blueprint = blueprint.ToReference<BlueprintAreaEffectReference>();
		m_Context = context;
		m_Target = target;
		m_CreationTime = creationTime;
		m_Duration = duration;
		m_UsePatternFromAbility = usePatternFromAbility;
		base.name = $"Area effect ({blueprint})";
		base.ViewTransform.position = (OnUnit ? target.Point : target.NearestNode.Vector3Position());
		if (context.MaybeCaster != null && getOrientationFromCaster)
		{
			base.ViewTransform.rotation = Quaternion.Euler(0f, context.MaybeCaster.Orientation, 0f);
		}
		new GameObject("Locator_GroundFX").transform.SetParent(base.ViewTransform, worldPositionStays: false);
	}

	protected override void OnDidAttachToData()
	{
		base.OnDidAttachToData();
		SyncTransform();
		if (Data.IsInGame)
		{
			SpawnFxs();
		}
	}

	protected override void OnWillDetachFromData()
	{
		base.OnWillDetachFromData();
		RemoveFxs();
	}

	public override void UpdateViewActive()
	{
		base.UpdateViewActive();
		if (Data.IsInGame)
		{
			SpawnFxs();
		}
		else
		{
			RemoveFxs();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (m_NavmeshCut != null)
		{
			UnityEngine.Object.Destroy(m_NavmeshCut);
		}
	}

	public override Entity CreateEntityData(bool load)
	{
		if ((object)m_Target == null)
		{
			m_Target = new TargetWrapper(base.ViewTransform.position);
		}
		return Entity.Initialize(new AreaEffectEntity(this, m_Context, m_Blueprint.Get(), m_Target, m_CreationTime, m_Duration, OnUnit));
	}

	public void SpawnFxs()
	{
		GameObject gameObject = m_Blueprint.Get().FXSettings?.VisualFXSettings?.AreaEffectFx?.Load();
		if (!(gameObject != null) || (bool)m_SpawnedFx)
		{
			return;
		}
		if (OnUnit)
		{
			MechanicEntityView mechanicEntityView = m_Target.Entity?.View;
			if (mechanicEntityView == null)
			{
				LogChannel.Default.Error("Missing target unit view reference during FX spawn. m_Target " + m_Target.EntityRef.Id + ". AreaEffectView " + UniqueId);
			}
			else
			{
				m_SpawnedFx = FxHelper.SpawnFxOnEntity(gameObject, mechanicEntityView);
			}
		}
		else
		{
			m_SpawnedFx = FxHelper.SpawnFxOnGameObject(gameObject, base.gameObject);
		}
	}

	public void RemoveFxs()
	{
		if (m_SpawnedFx != null)
		{
			FxHelper.Destroy(m_SpawnedFx);
			m_SpawnedFx = null;
		}
	}

	private void LateUpdate()
	{
		if (OnUnit)
		{
			base.ViewTransform.position = m_Target.Point;
		}
		else if (m_SpawnedFx != null)
		{
			m_SpawnedFx.transform.position = base.ViewTransform.position;
		}
	}

	protected override void OnDestroy()
	{
		RemoveFxs();
		base.OnDestroy();
	}

	public void SyncTransform()
	{
		base.transform.position = Data.Position;
		base.transform.rotation = Quaternion.Euler(0f, Data.Orientation, 0f);
	}
}
