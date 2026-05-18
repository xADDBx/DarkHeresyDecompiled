using Code.View.UI.UIUtils;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.GameModes;
using Kingmaker.View;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class PointMarkerVM : ViewModel
{
	public readonly bool UsedSubtypeIcons;

	private readonly ReactiveProperty<Sprite> m_Portrait = new ReactiveProperty<Sprite>();

	private readonly ReactiveProperty<bool> m_IsVisible = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<UnitRelation> m_Relation = new ReactiveProperty<UnitRelation>();

	private readonly ReactiveProperty<EntityPointMarkObjectType> m_AnotherPointMarkObjectType = new ReactiveProperty<EntityPointMarkObjectType>();

	private Vector3 m_PositionInUI = Vector3.zero;

	public readonly BaseUnitEntity Unit;

	public readonly Entity AnotherEntity;

	public readonly GameObject PingPosition;

	private readonly ReactiveProperty<LineOfSightVM> m_LineOfSight = new ReactiveProperty<LineOfSightVM>();

	public ReadOnlyReactiveProperty<Sprite> Portrait => m_Portrait;

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public ReadOnlyReactiveProperty<UnitRelation> Relation => m_Relation;

	public ReadOnlyReactiveProperty<EntityPointMarkObjectType> AnotherPointMarkObjectType => m_AnotherPointMarkObjectType;

	public Vector3 Position => Unit?.Position ?? AnotherEntity?.Position ?? PingPosition.transform.position;

	public ReadOnlyReactiveProperty<LineOfSightVM> LineOfSight => m_LineOfSight;

	public PointMarkerVM(BaseUnitEntity unitEntity)
	{
		Unit = unitEntity;
		m_Portrait.Value = UIUtilityUnit.GetSurfaceCombatStandardPortrait(unitEntity, PortraitCombatSize.Icon);
		m_Relation.Value = GetUnitRelation(unitEntity);
		UsedSubtypeIcons = UIUtilityUnit.UsedSubtypeIcon(unitEntity);
	}

	public PointMarkerVM(Entity anotherEntity, bool isPing = false)
	{
		AnotherEntity = anotherEntity;
		m_AnotherPointMarkObjectType.Value = GetEntityPointMarkObjectType(anotherEntity, null, isPing);
		UsedSubtypeIcons = isPing;
	}

	public PointMarkerVM(GameObject pingPosition)
	{
		PingPosition = pingPosition;
		m_AnotherPointMarkObjectType.Value = GetEntityPointMarkObjectType(null, pingPosition, isPing: true);
		UsedSubtypeIcons = true;
	}

	public void Update()
	{
		UpdateVisibility();
	}

	public void ScrollToUnit()
	{
		if (AnotherEntity == null && PingPosition == null)
		{
			Game.Instance.Controllers.CameraController?.Follower?.Release();
		}
		CameraRig.Instance.ScrollTo(Position);
	}

	public void SetLineOfSightVM(LineOfSightVM los)
	{
		m_LineOfSight.Value = los;
	}

	private void UpdateVisibility()
	{
		if ((Game.Instance.CurrentModeType != GameModeType.Default && Game.Instance.CurrentModeType != GameModeType.GlobalMap && Game.Instance.CurrentModeType != GameModeType.Pause) || Game.Instance.GetCamera() == null)
		{
			m_IsVisible.Value = false;
			return;
		}
		m_PositionInUI = Game.Instance.GetCamera().WorldToScreenPoint(Position);
		m_IsVisible.Value = m_PositionInUI.x <= 0f || m_PositionInUI.x >= (float)Game.Instance.GetCamera().pixelWidth || m_PositionInUI.y <= 0f || m_PositionInUI.y >= (float)Game.Instance.GetCamera().pixelHeight;
	}

	private UnitRelation GetUnitRelation(BaseUnitEntity unitEntity)
	{
		if (unitEntity.Faction.IsPlayer)
		{
			return UnitRelation.Self;
		}
		if (unitEntity.Faction.Neutral)
		{
			return UnitRelation.Neutral;
		}
		if (unitEntity.Faction.IsPlayerEnemy)
		{
			return UnitRelation.Enemy;
		}
		return UnitRelation.Ally;
	}

	private EntityPointMarkObjectType GetEntityPointMarkObjectType(Entity entity, GameObject pingPosition, bool isPing)
	{
		if (isPing)
		{
			if (!(pingPosition != null))
			{
				return EntityPointMarkObjectType.PingEntity;
			}
			return EntityPointMarkObjectType.PingPosition;
		}
		return EntityPointMarkObjectType.Quest;
	}
}
