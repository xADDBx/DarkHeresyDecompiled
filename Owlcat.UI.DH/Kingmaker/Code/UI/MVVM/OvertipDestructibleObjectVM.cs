using Kingmaker.EntitySystem.Entities;
using Kingmaker.View;
using Owlcat.Runtime.Core.Utility;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipDestructibleObjectVM : BaseOvertipMapObjectVM
{
	private readonly ReactiveProperty<Vector3> m_CameraDistance = new ReactiveProperty<Vector3>();

	private readonly ReactiveProperty<bool> m_IsVisibleForPlayer = new ReactiveProperty<bool>(value: false);

	public float? DeathDelay;

	public readonly MechanicEntityUIState MechanicEntityUIState;

	public readonly OvertipNameBlockVM NameBlockVM;

	public readonly OvertipHealthBlockVM HealthBlockVM;

	public readonly OvertipDamageBlockVM DamageBlockVM;

	public readonly OvertipHitChanceBlockVM HitChanceBlockVM;

	public readonly OvertipCombatTextBlockVM CombatTextBlockVM;

	public readonly OvertipBarkBlockVM BarkBlockVM;

	private readonly Transform m_Bone;

	public ReadOnlyReactiveProperty<bool> IsVisibleForPlayer => m_IsVisibleForPlayer;

	public ReadOnlyReactiveProperty<Vector3> CameraDistance => m_CameraDistance;

	public ReadOnlyReactiveProperty<bool> HasActiveCombatMessage => CombatTextBlockVM.HasActiveCombatMessage;

	public bool VisibleInExploration => DestructibleEntity?.View.VisibleInExploration ?? false;

	protected override bool UpdateEnabled => MapObjectEntity.IsVisibleForPlayer;

	private DestructibleEntity DestructibleEntity => MapObjectEntity as DestructibleEntity;

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

	protected override Vector3 GetEntityPosition()
	{
		return m_Bone?.position ?? DestructibleEntity.View.OvertipPosition;
	}

	protected override void OnUpdateHandler()
	{
		m_IsVisibleForPlayer.Value = MapObjectEntity?.IsVisibleForPlayer ?? false;
		m_CameraDistance.Value = base.Position - CameraRig.Instance.GetTargetPointPosition();
		base.OnUpdateHandler();
	}

	public void HighlightChanged()
	{
		m_MapObjectIsHighlighted.Value = MapObjectEntity?.View.Highlighted ?? false;
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
}
