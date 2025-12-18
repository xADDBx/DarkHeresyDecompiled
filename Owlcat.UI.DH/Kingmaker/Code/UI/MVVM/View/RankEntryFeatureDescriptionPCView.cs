using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Owlcat.Runtime.Core.Utility;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class RankEntryFeatureDescriptionPCView : BaseCareerPathSelectionTabPCView<RankEntryFeatureItemVM>
{
	[Header("Info View")]
	[SerializeField]
	private InfoSectionView m_InfoView;

	protected override void OnBind()
	{
		base.OnBind();
		m_InfoView.Bind(base.ViewModel.InfoVM);
		SetNextButtonLabel(UIStrings.Instance.CharGen.Next);
		SetBackButtonLabel(UIStrings.Instance.CharGen.Back);
		SetFinishButtonLabel(UIStrings.Instance.Tutorial.Complete);
		SetButtonSound(ButtonSoundsEnum.DoctrineNextSound);
		base.ViewModel.CareerPathVM.CanCommit.CombineLatest(base.ViewModel.CareerPathVM.PointerItem, (bool canCommit, IRankEntrySelectItem pointerItem) => canCommit && pointerItem == null).Subscribe(delegate(bool value)
		{
			base.CanCommit = value;
			SetFinishInteractable(value);
		}).AddTo(this);
		base.ViewModel.CareerPathVM.CurrentRank.Subscribe(delegate(int value)
		{
			string header = ((base.ViewModel.Rank.HasValue && base.ViewModel.Rank.Value <= value) ? UIStrings.Instance.CharacterSheet.HeaderFeatureDescriptionTab : UIStrings.Instance.CharacterSheet.HeaderImprovement);
			SetHeader(header);
		}).AddTo(this);
		base.ViewModel.CareerPathVM.ReadOnly.Subscribe(delegate(bool ro)
		{
			SetButtonVisibility(!ro);
		}).AddTo(this);
	}

	public override void UpdateState()
	{
		bool flag = base.ViewModel.CanSelect() && base.ViewModel.CareerPathVM.LastEntryToUpgrade != base.ViewModel;
		SetNextButtonInteractable(flag);
		m_HighlightButton.Or(null)?.gameObject.SetActive(!flag);
		bool backButtonInteractable = base.ViewModel.CareerPathVM.FirstEntryToUpgrade != base.ViewModel;
		SetBackButtonInteractable(backButtonInteractable);
	}

	protected override void HandleClickNext()
	{
		if (base.ViewModel.CareerPathVM.CanCommit.CurrentValue && base.ViewModel.CareerPathVM.LastEntryToUpgrade == base.ViewModel)
		{
			base.ViewModel.CareerPathVM.SetRankEntry(null);
		}
		else
		{
			base.ViewModel.CareerPathVM.SelectNextItem();
		}
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.CareerPathVM.SelectPreviousItem();
	}

	protected override void HandleClickFinish()
	{
		base.ViewModel.CareerPathVM.Commit();
	}
}
