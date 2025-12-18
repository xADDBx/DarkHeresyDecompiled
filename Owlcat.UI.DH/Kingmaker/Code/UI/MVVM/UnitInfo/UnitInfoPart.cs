using Kingmaker.Utility.Attributes;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.UnitInfo;

public abstract class UnitInfoPart : View<UnitInfoVM>
{
	[SerializeField]
	protected bool m_HasCompareContent;

	[ShowIf("m_HasCompareContent")]
	[SerializeField]
	protected CanvasGroup m_CompareContent;

	protected override void OnBind()
	{
		base.ViewModel.IsHover.CombineLatest(base.ViewModel.Data.HasLineOfSight, base.ViewModel.Data.HasHit, base.ViewModel.HasAbility, base.ViewModel.Data.Damage, base.ViewModel.Data.MaxHeal, base.ViewModel.IsPreciseAttack, base.ViewModel.Data.PreciseAttackHasNoTarget, base.ViewModel.Data.IsDeadOrUnconsciousIsDead, base.ViewModel.Data.HasConcentration, delegate(bool isHover, bool hasLineOfSight, bool hasHit, bool hasAbility, UnitInfoReactiveData.PredictedDamage damage, int maxHeal, bool isPreciseAttack, bool preciseAttackHasNoTarget, bool isDeadOrUnconsciousIsDead, bool hasConcentration)
		{
			UnitInfoPartState result = default(UnitInfoPartState);
			result.HasLineOfSight = hasLineOfSight;
			result.HasHit = hasHit;
			result.HasAbility = hasAbility;
			result.HasDamage = damage.OverallMaxDamage > 0;
			result.HasHeal = maxHeal > 0;
			result.IsPreciseAttack = isPreciseAttack;
			result.PreciseAttackHasNoTarget = preciseAttackHasNoTarget;
			result.IsDeadOrUnconscious = isDeadOrUnconsciousIsDead;
			result.HasConcentration = hasConcentration;
			return result;
		}).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(Show)
			.AddTo(this);
		base.ViewModel.HasCompareData.Subscribe(ShowCompare).AddTo(this);
	}

	private void Show(UnitInfoPartState state)
	{
		ShowImpl(state);
		base.ViewModel.SetDirtyContent(isDirty: true);
	}

	protected virtual void ShowImpl(UnitInfoPartState state)
	{
	}

	private void ShowCompare(bool show)
	{
		if (m_HasCompareContent)
		{
			m_CompareContent.alpha = (show ? 1f : 0f);
			ShowCompareImpl(show);
		}
	}

	protected virtual void ShowCompareImpl(bool show)
	{
	}
}
