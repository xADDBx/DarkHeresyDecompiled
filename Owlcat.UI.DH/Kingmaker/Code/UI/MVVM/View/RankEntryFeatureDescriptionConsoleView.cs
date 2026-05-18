using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.UI.Sound;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class RankEntryFeatureDescriptionConsoleView : BaseCareerPathSelectionTabConsoleView<RankEntryFeatureItemVM>
{
	[Header("Info View")]
	[SerializeField]
	private InfoSectionView m_InfoView;

	[SerializeField]
	private HintView m_ScrollHint;

	[SerializeField]
	private HintView m_ConfirmHint;

	private readonly ReactiveProperty<bool> m_ScrollActive = new ReactiveProperty<bool>();

	protected override void OnBind()
	{
		base.OnBind();
		SetHeader(UIStrings.Instance.CharacterSheet.HeaderFeatureDescriptionTab);
		m_InfoView.Bind(base.ViewModel.InfoVM);
		base.ViewModel.CareerPathVM.CanCommit.Subscribe(delegate(bool canCommit)
		{
			bool flag = canCommit && base.ViewModel.CareerPathVM.LastEntryToUpgrade == base.ViewModel;
			SetNextButtonLabel(flag ? UIStrings.Instance.CharacterSheet.ToSummaryTab : UIStrings.Instance.CharGen.Next);
		}).AddTo(this);
		base.ViewModel.CareerPathVM.ReadOnly.Subscribe(delegate(bool ro)
		{
			ButtonActive.Value = !ro;
		}).AddTo(this);
		IsTabActiveProp.Subscribe(delegate(bool value)
		{
			m_ScrollActive.Value = value && m_InfoView.IsScrollActive;
		}).AddTo(this);
	}

	public override void UpdateState()
	{
	}

	protected override void HandleClickNext()
	{
		if (base.ViewModel != null)
		{
			if (base.ViewModel.CareerPathVM.CanCommit.CurrentValue && base.ViewModel.CareerPathVM.LastEntryToUpgrade == base.ViewModel)
			{
				base.ViewModel.CareerPathVM.SetRankEntry(null);
				return;
			}
			base.ViewModel.CareerPathVM.SelectNextItem();
			ButtonsSounds.Instance.DoctrineNextButton.Click.Play();
		}
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.CareerPathVM.SelectPreviousItem();
	}

	public void AddInput()
	{
	}
}
