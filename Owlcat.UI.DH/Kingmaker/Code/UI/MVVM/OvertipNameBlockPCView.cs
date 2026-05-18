using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class OvertipNameBlockPCView : View<LightweightOvertipNameBlockVM>
{
	[SerializeField]
	private TextMeshProUGUI m_CharacterName;

	[SerializeField]
	private OwlcatMultiSelectable m_MultiSelectable;

	[SerializeField]
	private FadeAnimator m_Animator;

	private bool CheckVisibleTrigger
	{
		get
		{
			if ((!base.ViewModel.MechanicEntityUIState.IsTBM.CurrentValue || !base.ViewModel.MechanicEntityUIState.IsCurrentUnitTurn.CurrentValue || Game.Instance.Player.IsInCombat) && !base.ViewModel.MechanicEntityUIState.ForceHotKeyPressed.CurrentValue)
			{
				return base.ViewModel.MechanicEntityUIState.IsMouseOverUnit.CurrentValue;
			}
			return true;
		}
	}

	private bool CheckCanBeVisible
	{
		get
		{
			if (base.ViewModel.MechanicEntityUIState.IsVisibleForPlayer.CurrentValue && !base.ViewModel.MechanicEntityUIState.HideOvertip.CurrentValue)
			{
				return !base.ViewModel.MechanicEntityUIState.IsDeadOrUnconsciousIsDead.CurrentValue;
			}
			return false;
		}
	}

	private bool CheckVisibility
	{
		get
		{
			if (CheckCanBeVisible)
			{
				return CheckVisibleTrigger;
			}
			return false;
		}
	}

	protected override void OnBind()
	{
		base.ViewModel.Name.Subscribe(delegate(string value)
		{
			m_CharacterName.text = value;
		}).AddTo(this);
		base.ViewModel.MechanicEntityUIState.IsEnemy.Subscribe(delegate(bool value)
		{
			m_MultiSelectable.SetActiveLayer(value ? "Enemy" : (base.ViewModel.MechanicEntityUIState.IsPlayer.CurrentValue ? "Party" : "Ally"));
		}).AddTo(this);
		base.ViewModel.MechanicEntityUIState.IsCurrentUnitTurn.CombineLatest(base.ViewModel.MechanicEntityUIState.ForceHotKeyPressed, base.ViewModel.MechanicEntityUIState.IsMouseOverUnit, base.ViewModel.MechanicEntityUIState.IsVisibleForPlayer, base.ViewModel.MechanicEntityUIState.HideOvertip, base.ViewModel.MechanicEntityUIState.IsDeadOrUnconsciousIsDead, (bool isCurrentUnitTurn, bool forceHotKeyPressed, bool isMouseOverUnit, bool isVisibleForPlayer, bool hasHiddenCondition, bool isDead) => new { isCurrentUnitTurn, forceHotKeyPressed, isMouseOverUnit, isVisibleForPlayer, hasHiddenCondition, isDead }).DebounceFrame(1, UnityFrameProvider.PreLateUpdate).Subscribe(_ =>
		{
			m_Animator.PlayAnimation(CheckVisibility);
		})
			.AddTo(this);
	}
}
