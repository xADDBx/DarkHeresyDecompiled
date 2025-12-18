using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenAttributesPhaseDetailedView : CharGenPhaseDetailedView<CharGenAttributesPhaseVM>
{
	[Header("Selector")]
	[SerializeField]
	private TextMeshProUGUI m_AvailablePointsLabel;

	[SerializeField]
	protected CharGenAttributesPhaseSelectorView m_CharGenAttributesPhaseSelectorView;

	[Header("Skills")]
	[SerializeField]
	protected CharInfoSkillsBlockCommonView m_CharInfoSkillsBlockView;

	[Header("Description")]
	[SerializeField]
	protected InfoSectionView m_InfoView;

	public override void Initialize()
	{
		base.Initialize();
		m_InfoView.Initialize();
	}

	protected override void OnBind()
	{
		base.OnBind();
		m_CharInfoSkillsBlockView.Bind(base.ViewModel.CharInfoSkillsBlock);
		m_CharGenAttributesPhaseSelectorView.Bind(base.ViewModel.SelectionGroup);
		m_InfoView.Bind(base.ViewModel.InfoVM);
		base.ViewModel.AvailablePointsLeft.Subscribe(delegate(int value)
		{
			m_AvailablePointsLabel.text = value.ToString();
		}).AddTo(this);
	}

	public override void AddInput(ref InputLayer inputLayer, ref GridConsoleNavigationBehaviour navigationBehaviour, ConsoleHintsWidget hintsWidget, ReadOnlyReactiveProperty<bool> isMainCharacter)
	{
	}
}
