using Kingmaker.Blueprints.Root.Strings;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CareerPathSelectionsSummaryConsoleView : BaseCareerPathSelectionTabConsoleView<CareerPathVM>
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
		SetHeader(UIStrings.Instance.CharacterSheet.HeaderSummaryTab);
		m_InfoView.Bind(base.ViewModel.TabInfoSectionVM);
	}

	public override void UpdateState()
	{
	}

	protected override void HandleClickNext()
	{
		base.ViewModel?.Commit();
	}

	protected override void HandleClickBack()
	{
		base.ViewModel.SelectPreviousItem();
	}
}
