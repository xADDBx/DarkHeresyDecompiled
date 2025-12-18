using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UnitLogic.Levelup;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CareerPathDescriptionPCView : BaseCareerPathSelectionTabPCView<CareerPathVM>
{
	[Header("Info View")]
	[SerializeField]
	private InfoSectionView m_InfoView;

	public override void Initialize()
	{
		base.Initialize();
		m_InfoView.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		SetHeader(null);
		SetNextButtonLabel(UIStrings.Instance.CharGen.Next);
		SetBackButtonLabel(UIStrings.Instance.CharGen.Back);
		SetFinishButtonLabel(UIStrings.Instance.Tutorial.Complete);
		SetButtonSound(ButtonSoundsEnum.DoctrineNextSound);
		ObservableSubscribeExtensions.Subscribe(base.ViewModel.OnUpdateData, delegate
		{
			if (base.ViewModel != null)
			{
				SetButtonVisibility(base.ViewModel.IsInLevelupProcess && (!base.ViewModel.IsSelected.Value || base.ViewModel.CurrentRank.CurrentValue != 0));
			}
		});
		base.ViewModel.IsSelected.CombineLatest(base.ViewModel.CurrentRank, (bool selected, int rank) => selected && rank == 0).Subscribe(delegate(bool value)
		{
			SetButtonVisibility(!value && base.ViewModel.IsInLevelupProcess);
		}).AddTo(this);
		m_InfoView.Bind(base.ViewModel.TabInfoSectionVM);
	}

	public override void UpdateState()
	{
		SetBackButtonInteractable(value: false);
		SetNextButtonInteractable(value: true);
		HintText.Value = GetHintText();
	}

	protected override void HandleClickNext()
	{
		base.ViewModel?.SelectNextItem();
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.SelectPreviousItem();
	}

	protected override void HandleClickFinish()
	{
		base.ViewModel.Commit();
	}

	private string GetHintText()
	{
		if (base.ViewModel.IsAvailableToUpgrade)
		{
			return string.Empty;
		}
		if (base.ViewModel.Unit.IsInCombat)
		{
			return UIStrings.Instance.CharacterSheet.UnitIsInCombatButtonHint.Text;
		}
		LevelUpManager levelUpManager = base.ViewModel.UnitProgressionVM.LevelUpManager;
		if (levelUpManager != null)
		{
			if (levelUpManager.TargetUnit != base.ViewModel.Unit)
			{
				return UIStrings.Instance.CharacterSheet.LevelUpOnOtherUnitButtonHint.Text;
			}
			if (levelUpManager.Path == base.ViewModel.CareerPath)
			{
				return string.Empty;
			}
		}
		return UIStrings.Instance.CharacterSheet.NoRanksForUpgradeButtonHint.Text;
	}
}
