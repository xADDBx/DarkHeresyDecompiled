using System;
using System.Collections.Generic;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Items;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.UI.AR;
using Kingmaker.UnitLogic;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.UnitLogic.Abilities.Components.Patterns;
using Kingmaker.UnitLogic.Enums;
using Kingmaker.UnitLogic.Parts;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.View.Covers;
using Kingmaker.View.Mechanics.Entities;
using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using Pathfinding;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LineOfSightVM : ViewModel, IAbilityTargetSelectionUIHandler, ISubscriber, IVirtualPositionUIHandler, IUnitActiveEquipmentSetHandler, ISubscriber<IBaseUnitEntity>, IUnitDirectHoverUIHandler, ICellAbilityHandler<EntitySubscriber>, ICellAbilityHandler, ISubscriber<IMechanicEntity>, IEntitySubscriber, IEventTag<ICellAbilityHandler, EntitySubscriber>
{
	private readonly ReactiveProperty<bool> m_IsVisible = new ReactiveProperty<bool>();

	private readonly ReactiveProperty<Vector3> m_StartPos = new ReactiveProperty<Vector3>(Vector3.zero);

	private readonly ReactiveProperty<Vector3> m_EndPos = new ReactiveProperty<Vector3>(Vector3.zero);

	private readonly ReactiveProperty<Vector3?> m_BestStartPos = new ReactiveProperty<Vector3?>(null);

	private readonly ReactiveProperty<float> m_HitChance = new ReactiveProperty<float>(0f);

	private readonly MechanicEntityUIState m_MechanicEntityUIState;

	private readonly MechanicEntity m_CurrentUnit;

	private AbilityData m_CurrentAbility;

	public readonly MechanicEntity Owner;

	public Vector3 StartObjectOffset { get; private set; }

	public Vector3 EndObjectOffset { get; private set; }

	public ReadOnlyReactiveProperty<bool> IsVisible => m_IsVisible;

	public ReadOnlyReactiveProperty<Vector3> StartPos => m_StartPos;

	public ReadOnlyReactiveProperty<Vector3> EndPos => m_EndPos;

	public ReadOnlyReactiveProperty<Vector3?> BestStartPos => m_BestStartPos;

	public ReadOnlyReactiveProperty<float> HitChance => m_HitChance;

	public IEntity GetSubscribingEntity()
	{
		return m_MechanicEntityUIState.MechanicEntity.MechanicEntity;
	}

	private LineOfSightVM(Vector3 start, Vector3 endPosition, MechanicEntity currentUnit, MechanicEntity owner)
	{
		m_StartPos.Value = start;
		StartObjectOffset = UnitPathManager.Instance?.GetCellOffsetForUnit(currentUnit) ?? Vector3.zero;
		m_EndPos.Value = endPosition;
		EndObjectOffset = UnitPathManager.Instance?.GetCellOffsetForUnit(owner) ?? Vector3.zero;
		Owner = owner;
		m_CurrentUnit = currentUnit;
		m_CurrentAbility = Game.Instance.Controllers.SelectedAbilityHandler?.Ability;
		m_MechanicEntityUIState = UnitUIStateHolder.Instance.GetOrCreateUnitState(owner);
		EventBus.Subscribe(this).AddTo(this);
		EventBus.RaiseEvent(delegate(ILineOfSightHandler h)
		{
			h.OnLineOfSightCreated(this);
		});
	}

	public LineOfSightVM(MechanicEntity start, MechanicEntity end)
		: this(Game.Instance.Controllers.VirtualPositionController?.GetDesiredPosition(start) ?? start.Position, end.Position, start, end)
	{
		ObservableSubscribeExtensions.Subscribe(Observable.EveryUpdate(), delegate
		{
			UpdatePosition();
		}).AddTo(this);
		UpdatePosition();
		UpdateHitChance();
	}

	protected override void OnDispose()
	{
		EventBus.RaiseEvent(delegate(ILineOfSightHandler h)
		{
			h.OnLineOfSightDestroyed(this);
		});
	}

	public void HandleAbilityTargetSelectionStart(AbilityData ability)
	{
		m_CurrentAbility = ability;
		UpdateHitChance();
	}

	public void HandleAbilityTargetSelectionEnd(AbilityData ability)
	{
		m_CurrentAbility = null;
		UpdateHitChance();
	}

	public void HandleVirtualPositionChanged(Vector3? position)
	{
		UpdatePosition();
		UpdateHitChance();
	}

	public void HandleUnitChangeActiveEquipmentSet()
	{
		UpdateHitChance();
	}

	public void HandleHoverChange(AbstractUnitEntityView unitEntityView, bool isHover, bool isDirect)
	{
		UpdateVisibility();
	}

	public void HandleCellAbility(AbilityTargetUIData abilityTargets)
	{
		UpdateVisibility();
	}

	public void HandleCellAbilityClear()
	{
		UpdateVisibility();
	}

	private void UpdatePosition()
	{
		if (Owner != null)
		{
			PartLifeState partLifeState = m_MechanicEntityUIState?.MechanicEntity.LifeState;
			if ((partLifeState == null || partLifeState.State != UnitLifeState.Dead) && !Owner.IsDisposed)
			{
				using (ProfileScope.New("LineOfSight UpdatePosition"))
				{
					if (!LoadingProcess.Instance.IsLoadingInProcess && Owner.IsVisibleForPlayer)
					{
						m_StartPos.Value = Game.Instance.Controllers.VirtualPositionController?.GetDesiredPosition(m_CurrentUnit) ?? m_CurrentUnit.Position;
						StartObjectOffset = UnitPathManager.Instance.Or(null)?.GetCellOffsetForUnit(m_CurrentUnit) ?? Vector3.zero;
						m_EndPos.Value = Owner.Position;
						EndObjectOffset = UnitPathManager.Instance.Or(null)?.GetCellOffsetForUnit(Owner) ?? Vector3.zero;
						m_BestStartPos.Value = UnitPredictionManager.RealHologramPosition;
					}
					return;
				}
			}
		}
		Dispose();
	}

	private void UpdateVisibility()
	{
		ItemEntityWeapon itemEntityWeapon = TryGetCurrentWeapon();
		AbilityData obj = m_CurrentAbility ?? itemEntityWeapon?.Abilities.FirstOrDefault()?.Data;
		bool flag = itemEntityWeapon?.Blueprint.IsMelee ?? false;
		bool flag2 = obj?.HasCustomDirectMovement() ?? false;
		bool flag3 = m_CurrentAbility != null || m_MechanicEntityUIState.IsMouseOverUnit.CurrentValue || m_MechanicEntityUIState.IsAoETarget.CurrentValue || (Game.Instance.Controllers.VirtualPositionController?.HasVirtualPosition ?? false);
		m_IsVisible.Value = Owner.IsVisibleForPlayer && !flag && !flag2 && flag3 && HitChance.CurrentValue > 0f;
	}

	private void UpdateHitChance()
	{
		ItemEntityWeapon itemEntityWeapon = TryGetCurrentWeapon();
		AbilityData abilityData = ((!(m_CurrentAbility == null)) ? m_CurrentAbility : itemEntityWeapon?.Abilities.FirstOrDefault()?.Data);
		m_HitChance.Value = GetAbilityHitChance(abilityData);
		UpdateVisibility();
	}

	private float GetAbilityHitChance(AbilityData abilityData)
	{
		if (abilityData == null)
		{
			return LosCalculations.GetWarhammerLos(StartPos.CurrentValue, m_CurrentUnit.SizeRect, Owner).CoverType switch
			{
				LosCalculations.CoverType.Obstacle => 80f, 
				LosCalculations.CoverType.Cover => 50f, 
				LosCalculations.CoverType.LosBlocker => 0f, 
				_ => throw new ArgumentOutOfRangeException(), 
			};
		}
		GridNodeBase gridNode = AoEPatternHelper.GetGridNode(StartPos.CurrentValue);
		if (!abilityData.CanTargetFromNode(gridNode, null, Owner, out var _, out var _))
		{
			return 0f;
		}
		GridNodeBase bestShootingPositionForDesiredPosition = abilityData.GetBestShootingPositionForDesiredPosition(EndPos.CurrentValue);
		AbilityTargetUIData abilityTargetUIData;
		if (abilityData.IsBurst)
		{
			GridNodeBase gridNode2 = AoEPatternHelper.GetGridNode(EndPos.CurrentValue);
			OrientedPatternData orientedPattern = abilityData.GetPatternSettings().GetOrientedPattern(abilityData, bestShootingPositionForDesiredPosition, gridNode2, abilityData.Caster.Size);
			List<AbilityTargetUIData> listToFill = new List<AbilityTargetUIData>();
			abilityData.GatherAffectedTargetsData(orientedPattern, bestShootingPositionForDesiredPosition.Vector3Position(), Owner, in listToFill);
			abilityTargetUIData = listToFill.FirstOrDefault((AbilityTargetUIData t) => t.Target == Owner);
		}
		else
		{
			abilityTargetUIData = AbilityTargetUIDataCache.Instance.GetOrCreate(abilityData, bestShootingPositionForDesiredPosition.Vector3Position(), Owner, null, null, null);
		}
		return abilityTargetUIData.HitChance.HitWithAvoidanceChance;
	}

	private ItemEntityWeapon TryGetCurrentWeapon()
	{
		HandsEquipmentSet obj = (m_CurrentUnit as UnitEntity)?.Body.CurrentHandsEquipmentSet;
		ItemEntityWeapon itemEntityWeapon = obj?.PrimaryHand.MaybeWeapon;
		ItemEntityWeapon itemEntityWeapon2 = obj?.SecondaryHand.MaybeWeapon;
		if (CheckWeapon(itemEntityWeapon))
		{
			return itemEntityWeapon;
		}
		if (CheckWeapon(itemEntityWeapon2))
		{
			return itemEntityWeapon2;
		}
		return itemEntityWeapon;
		static bool CheckWeapon(ItemEntityWeapon weapon)
		{
			return weapon?.Blueprint.IsRanged ?? false;
		}
	}
}
