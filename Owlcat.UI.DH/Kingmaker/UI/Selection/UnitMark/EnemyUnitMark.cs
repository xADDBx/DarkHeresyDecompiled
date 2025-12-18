using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using Kingmaker.Blueprints.Root;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Mechanics.Entities;
using Kingmaker.Networking;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Commands;
using Kingmaker.UnitLogic.Commands.Base;
using Pathfinding;
using UnityEngine;

namespace Kingmaker.UI.Selection.UnitMark;

public class EnemyUnitMark : BaseSurfaceUnitMark, IUnitCommandStartHandler<EntitySubscriber>, IUnitCommandStartHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitCommandStartHandler, EntitySubscriber>, IUnitCommandEndHandler<EntitySubscriber>, IUnitCommandEndHandler, IEventTag<IUnitCommandEndHandler, EntitySubscriber>, ICellAbilityHandler<EntitySubscriber>, ICellAbilityHandler, IEntitySubscriber, IEventTag<ICellAbilityHandler, EntitySubscriber>, INetPingEntity, IUnitPathManagerHandler, IAbilityTargetHoverUIHandler
{
	[Header("Exploration")]
	[SerializeField]
	private UnitMarkDecal m_ExplorationSelectedDecal;

	[Header("Combat")]
	[SerializeField]
	private UnitMarkDecal m_CombatDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatSelectedDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatHeroicDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatSelectedHeroicDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatBrokenDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatSelectedBrokenDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatIsInAoeDecal;

	[SerializeField]
	private UnitMarkDecal m_CombatAbilityTargetDecal;

	[Header("Coop")]
	[SerializeField]
	private UnitMarkDecal m_PingTarget;

	[Header("Attack Of Opportunity")]
	[SerializeField]
	private UnitMarkDecal m_CombatAttackOfOpportunityDecal;

	[SerializeField]
	private float m_AttackOfOpportunityDecalRadius = 1.2f;

	[SerializeField]
	private float m_AttackOfOpportunityAnimationScale = 1.2f;

	private Tween m_PingTween;

	private Tweener m_AttackOfOpportunityAnimator;

	public override void Initialize(AbstractUnitEntity unit)
	{
		base.Initialize(unit);
		SetUnitSize(unit.SizeRect.Width > 1);
		m_PingTarget?.SetActive(state: false);
	}

	protected override List<UnitMarkDecal> GetAllDecals()
	{
		return new List<UnitMarkDecal> { m_ExplorationSelectedDecal, m_CombatDecal, m_CombatIsInAoeDecal, m_CombatSelectedDecal, m_CombatHeroicDecal, m_CombatSelectedHeroicDecal, m_CombatBrokenDecal, m_CombatSelectedBrokenDecal };
	}

	protected override UnitMarkDecal GetAbilityTargetDecal()
	{
		return m_CombatAbilityTargetDecal;
	}

	public override void HandleStateChanged()
	{
		if (base.Unit != null)
		{
			bool flag = base.State.HasFlag(UnitMarkState.IsInCombat);
			bool flag2 = base.State.HasFlag(UnitMarkState.HasAttackOfOpportunity);
			bool flag3 = base.State.HasFlag(UnitMarkState.CurrentTurn);
			bool flag4 = base.State.HasFlag(UnitMarkState.Heroic);
			bool flag5 = base.State.HasFlag(UnitMarkState.Broken);
			bool flag6 = flag4 || flag5;
			m_ExplorationSelectedDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && base.State.HasFlag(UnitMarkState.CastingSpell));
			m_CombatDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag && !flag3 && !flag6 && !flag2);
			m_CombatSelectedDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag3 && !flag6 && !flag2);
			m_CombatIsInAoeDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && base.State.HasFlag(UnitMarkState.IsInAoEPattern));
			m_CombatAttackOfOpportunityDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag2);
			m_CombatHeroicDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag && !flag3 && flag4 && !flag2);
			m_CombatSelectedHeroicDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag3 && flag4 && !flag2);
			m_CombatBrokenDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag && !flag3 && flag5 && !flag2);
			m_CombatSelectedBrokenDecal.SetActive(!BaseUnitMark.IsHideAllUI && !BaseUnitMark.IsCutscene && flag3 && flag5 && !flag2);
		}
	}

	public void HandleUnitCommandDidStart(AbstractUnitCommand command)
	{
		SetUnitCastingSpellState(command, active: true);
	}

	public void HandleUnitCommandDidEnd(AbstractUnitCommand command)
	{
		SetUnitCastingSpellState(command, active: false);
	}

	private void SetUnitCastingSpellState(AbstractUnitCommand command, bool active)
	{
		if (command is UnitUseAbility unitUseAbility && unitUseAbility.TargetUnit == base.Unit)
		{
			SetState(UnitMarkState.CastingSpell, active);
		}
	}

	public void HandleCellAbility(AbilityTargetUIData abilityTarget)
	{
		SetState(UnitMarkState.IsInAoEPattern, active: true);
	}

	public void HandleCellAbilityClear()
	{
		SetState(UnitMarkState.IsInAoEPattern, active: false);
	}

	protected override void HandleAbilityTargetSelectionStartImpl(AbilityData ability)
	{
	}

	protected override void HandleAbilityTargetSelectionEndImpl(AbilityData ability)
	{
		SetState(UnitMarkState.IsInAoEPattern, active: false);
	}

	public void HandlePingEntity(NetPlayer player, Entity entity)
	{
		if (entity != base.Unit)
		{
			return;
		}
		m_PingTween?.Kill();
		int index = player.Index - 1;
		m_PingTarget.SetMaterial(ConfigRoot.Instance.UIConfig.CoopPlayersPingsMaterials[index]);
		m_PingTarget?.SetActive(state: true);
		EventBus.RaiseEvent(delegate(INetAddPingMarker h)
		{
			h.HandleAddPingEntityMarker(entity);
		});
		m_PingTween = DOTween.To(() => 1f, delegate
		{
		}, 0f, 7.5f).SetUpdate(isIndependentUpdate: true).OnComplete(delegate
		{
			m_PingTarget?.SetActive(state: false);
			EventBus.RaiseEvent(delegate(INetAddPingMarker h)
			{
				h.HandleRemovePingEntityMarker(entity);
			});
			m_PingTween = null;
		})
			.OnKill(delegate
			{
				m_PingTarget?.SetActive(state: false);
				EventBus.RaiseEvent(delegate(INetAddPingMarker h)
				{
					h.HandleRemovePingEntityMarker(entity);
				});
				m_PingTween = null;
			});
	}

	public void HandlePathAdded(Path path, float cost, List<BaseUnitEntity> enemiesAoO)
	{
		bool flag = enemiesAoO.Contains(base.Unit);
		SetState(UnitMarkState.HasAttackOfOpportunity, flag);
		SetAttackOfOpportunityAnimation(flag);
		BaseUnitEntity baseUnitEntity = EventInvokerExtensions.BaseUnitEntity;
		if (flag && baseUnitEntity != null)
		{
			GameObject gameObject = m_CombatAttackOfOpportunityDecal.GameObject;
			SetAttackOfOpportunityPosition(gameObject.transform, gameObject.transform.parent, m_AttackOfOpportunityDecalRadius, baseUnitEntity.CurrentNode.position);
		}
	}

	public void HandlePathRemoved()
	{
		SetState(UnitMarkState.HasAttackOfOpportunity, active: false);
		SetAttackOfOpportunityAnimation(hasAttackOfOpportunity: false);
	}

	public void HandleCurrentNodeChanged(float cost)
	{
	}

	private static void SetAttackOfOpportunityPosition(Transform arrow, Transform target, float radius, Vector3 point)
	{
		Vector3 vector = point - target.position;
		if (!(vector.sqrMagnitude < 1E-06f))
		{
			vector.y = 0f;
			Vector3 normalized = vector.normalized;
			arrow.position = target.position + normalized * radius;
			arrow.forward = normalized;
		}
	}

	private void SetAttackOfOpportunityAnimation(bool hasAttackOfOpportunity)
	{
		if (hasAttackOfOpportunity)
		{
			m_AttackOfOpportunityAnimator?.Kill();
			m_AttackOfOpportunityAnimator = m_CombatAttackOfOpportunityDecal.GameObject.transform.DOScale(m_AttackOfOpportunityAnimationScale, 0.4f).SetLoops(-1, LoopType.Yoyo).SetUpdate(isIndependentUpdate: true);
		}
		else
		{
			m_AttackOfOpportunityAnimator.Rewind();
			m_AttackOfOpportunityAnimator?.Kill();
		}
	}

	public void HandleAbilityTargetHover(AbilityData ability, bool hover)
	{
		if (!hover)
		{
			SetState(UnitMarkState.IsInAoEPattern, active: false);
		}
	}
}
