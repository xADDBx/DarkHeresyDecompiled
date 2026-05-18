using Kingmaker.EntitySystem.Entities;
using Kingmaker.Predictions;
using Kingmaker.UnitLogic.Abilities;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipDestructibleObjectVM : BaseOvertipMapObjectVM
{
	private readonly ReactiveProperty<Vector3> m_CameraDistance = new ReactiveProperty<Vector3>();

	private readonly Transform m_Bone;

	private bool m_IsVisibleForPlayer;

	public readonly MechanicEntityUIState MechanicEntityUIState;

	public readonly OvertipNameBlockVM NameBlockVM;

	public readonly OvertipHealthBlockVM HealthBlockVM;

	public readonly OvertipDamageBlockVM DamageBlockVM;

	public readonly OvertipHitChanceBlockVM HitChanceBlockVM;

	public readonly OvertipCombatTextBlockVM CombatTextBlockVM;

	public readonly OvertipBarkBlockVM BarkBlockVM;

	public float? DeathDelay;

	private DestructibleEntity DestructibleEntity => MapObjectEntity as DestructibleEntity;

	protected override bool UpdateEnabled => MapObjectEntity.IsVisibleForPlayer;

	public ReadOnlyReactiveProperty<Vector3> CameraDistance => m_CameraDistance;

	public ReadOnlyReactiveProperty<bool> HasActiveCombatMessage => CombatTextBlockVM.HasActiveCombatMessage;

	public OvertipDestructibleObjectVM(DestructibleEntity destructibleEntity)
		: base(destructibleEntity)
	{
		m_Bone = destructibleEntity.View.ViewTransform.FindChildRecursive("UI_Overtip_Bone");
		MechanicEntityUIState = UnitUIStateHolder.Instance.GetOrCreateUnitState(destructibleEntity);
		NameBlockVM = new OvertipNameBlockVM(MechanicEntityUIState).AddTo(this);
		HitChanceBlockVM = new OvertipHitChanceBlockVM(MechanicEntityUIState).AddTo(this);
		HealthBlockVM = new OvertipHealthBlockVM(MechanicEntityUIState, HitChanceBlockVM.IsVisible).AddTo(this);
		DamageBlockVM = new OvertipDamageBlockVM(MechanicEntityUIState).AddTo(this);
		CombatTextBlockVM = new OvertipCombatTextBlockVM(MechanicEntityUIState).AddTo(this);
		BarkBlockVM = new OvertipBarkBlockVM().AddTo(this);
	}

	public bool IsVisible()
	{
		if (DestructibleEntity == null || DestructibleEntity.IsDisposed || DestructibleEntity.View == null)
		{
			return false;
		}
		if ((MechanicEntityUIState.IsTBM.CurrentValue || DestructibleEntity.View.VisibleInExploration) && m_IsVisibleForPlayer && !MapObjectEntity.Suppressed)
		{
			return !base.IsCutscene;
		}
		return false;
	}

	public void HighlightChanged()
	{
		m_MapObjectIsHighlighted.Value = MapObjectEntity != null && MapObjectEntity.View != null && MapObjectEntity.View.Highlighted;
	}

	public void ShowBark(string text)
	{
		BarkBlockVM.ShowBark(text);
	}

	public void HideBark()
	{
		BarkBlockVM.HideBark();
	}

	public void SetDeathDelay(float val)
	{
		DeathDelay = val;
	}

	public bool IsPrimaryTarget()
	{
		AbilityTargetUIData currentValue = MechanicEntityUIState.AbilityTargetUIData.CurrentValue;
		AbilityData ability = currentValue.Ability;
		if (ability == null)
		{
			return false;
		}
		if (!currentValue.HitChance.IsAdditionalTarget)
		{
			return !ability.IsAoe;
		}
		return false;
	}

	protected override void OnDispose()
	{
		MarkForRemoval();
	}

	protected override Vector3 GetEntityPosition()
	{
		if ((bool)m_Bone)
		{
			return m_Bone.position;
		}
		if (DestructibleEntity != null && DestructibleEntity.View != null)
		{
			return DestructibleEntity.View.OvertipPosition;
		}
		return Vector3.zero;
	}

	protected override void OnUpdateHandler()
	{
		m_IsVisibleForPlayer = MapObjectEntity?.IsVisibleForPlayer ?? false;
		m_CameraDistance.Value = base.Position - CameraRig.Instance.GetTargetPointPosition();
		base.OnUpdateHandler();
	}
}
