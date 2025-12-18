using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenCareerPhaseDetailedView : CharGenPhaseDetailedView<CharGenCareerPhaseVM>
{
	[SerializeField]
	protected UnitProgressionCommonView m_UnitProgressionView;

	[Header("Description")]
	[SerializeField]
	protected InfoSectionView m_InfoView;

	public override void Initialize()
	{
		base.Initialize();
		m_InfoView.Initialize();
		m_UnitProgressionView.Initialize(delegate
		{
		});
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_UnitProgressionView.Bind(base.ViewModel.UnitProgressionVM);
		m_InfoView.Bind(base.ViewModel.InfoVM);
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, ReadOnlyReactiveProperty<bool> isMainCharacter)
	{
	}
}
