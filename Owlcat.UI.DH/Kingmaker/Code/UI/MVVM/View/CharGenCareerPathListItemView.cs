using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.UnitLogic.Progression.Paths;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCareerPathListItemView : CareerPathListItemCommonView, IFunc02ClickHandler, IConsoleEntity
{
	[SerializeField]
	private OwlcatButton m_InspectButton;

	public bool CanFunc02Click()
	{
		return true;
	}

	public void OnFunc02Click()
	{
		base.ViewModel.SetCareerPath();
	}

	public string GetFunc02ClickHint()
	{
		return string.Empty;
	}

	protected override void OnBind()
	{
		ShouldShowTooltip = false;
		base.OnBind();
		ObservableSubscribeExtensions.Subscribe(m_InspectButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.SetCareerPath();
			OnHoverEnd();
		}).AddTo(this);
	}

	protected override void UpdateButtonState()
	{
		m_MainButton.SetActiveLayer(ItemState.Value.ToString());
		if ((bool)m_SelectedIcon)
		{
			m_SelectedIcon.gameObject.SetActive(base.ViewModel.IsSelected.Value);
		}
	}

	protected override void HandleClick()
	{
		if (base.ViewModel.Unit.CanEditCareer() && base.ViewModel.CareerPath.Tier == CareerPathTier.One)
		{
			base.ViewModel.SetSelectedFromView(state: true);
		}
	}

	public override bool CanConfirmClick()
	{
		return m_MainButton.CanConfirmClick();
	}

	public override string GetConfirmClickHint()
	{
		return ((base.ViewModel.UnitProgressionVM as UnitProgressionVM)?.CurrentCareer.CurrentValue == base.ViewModel) ? UIStrings.Instance.CharGen.Next : UIStrings.Instance.CommonTexts.Select;
	}
}
