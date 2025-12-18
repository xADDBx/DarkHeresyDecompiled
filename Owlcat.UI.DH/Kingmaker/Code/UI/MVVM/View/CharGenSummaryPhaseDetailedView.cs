using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenSummaryPhaseDetailedView : CharGenPhaseDetailedView<CharGenSummaryPhaseVM>
{
	[Header("Character Info")]
	[SerializeField]
	private CharGenNameBaseView m_CharGenNameView;

	[SerializeField]
	private CharInfoLevelClassScoresPCView m_LevelClassScoresView;

	[SerializeField]
	protected CharInfoSkillsBlockCommonView m_SkillsBlockView;

	[Header("Description")]
	[SerializeField]
	protected InfoSectionView m_InfoView;

	protected override bool HasYScrollBindInternal => false;

	public override void Initialize()
	{
		base.Initialize();
		m_LevelClassScoresView.Initialize();
		m_SkillsBlockView.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_CharGenNameView.Bind(base.ViewModel.CharGenNameVM);
		m_LevelClassScoresView.Bind(base.ViewModel.LevelClassScoresVM);
		m_SkillsBlockView.Bind(base.ViewModel.CharInfoSkillsBlockVM);
		m_InfoView.Bind(base.ViewModel.InfoVM);
		base.ViewModel.InterruptHandler.Subscribe(base.ViewModel.CharGenNameVM.ShowChangeNameMessageBox).AddTo(this);
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, ReadOnlyReactiveProperty<bool> isMainCharacter)
	{
	}

	public InputLayer GetInputLayer(InputLayer inputLayer, ConsoleHintsWidget hintsWidget)
	{
		return inputLayer;
	}
}
