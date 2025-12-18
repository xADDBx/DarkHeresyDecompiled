using Kingmaker.Blueprints.Root.Strings;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CareerPathSelectionsSummaryPCView : BaseCareerPathSelectionTabPCView<CareerPathVM>
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
		SetNextButtonInteractable(value: true);
		SetBackButtonInteractable(value: false);
		base.ViewModel.CanCommit.CombineLatest(base.ViewModel.PointerItem, (bool canCommit, IRankEntrySelectItem pointerItem) => canCommit && pointerItem == null).Subscribe(delegate(bool value)
		{
			base.CanCommit = value;
			SetFinishInteractable(value);
		}).AddTo(this);
		m_InfoView.Bind(base.ViewModel.TabInfoSectionVM);
		SetButtonVisibility(base.ViewModel.IsInLevelupProcess);
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		SetButtonVisibility(value: false);
	}

	public override void UpdateState()
	{
		SetButtonVisibility(base.ViewModel.IsInLevelupProcess && (!base.ViewModel.IsSelected.Value || base.ViewModel.CurrentRank.CurrentValue != 0));
	}

	protected override void HandleClickNext()
	{
		base.ViewModel.SelectNextItem();
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.SelectPreviousItem();
	}

	protected override void HandleClickFinish()
	{
		base.ViewModel.Commit();
	}
}
