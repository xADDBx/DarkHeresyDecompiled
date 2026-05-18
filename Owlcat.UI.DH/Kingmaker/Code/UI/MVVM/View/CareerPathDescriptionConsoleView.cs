using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CareerPathDescriptionConsoleView : BaseCareerPathSelectionTabConsoleView<CareerPathVM>
{
	[Header("Info View")]
	[SerializeField]
	private InfoSectionView m_InfoView;

	[Header("Console")]
	[SerializeField]
	private HintView m_ScrollHint;

	public override void Initialize()
	{
		base.Initialize();
		m_InfoView.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		SetHeader(null);
		m_InfoView.Bind(base.ViewModel.TabInfoSectionVM);
	}

	public void AddInput()
	{
	}
}
