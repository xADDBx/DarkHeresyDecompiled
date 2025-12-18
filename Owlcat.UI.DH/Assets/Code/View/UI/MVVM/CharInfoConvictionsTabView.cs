using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.UI.MVVM;
using Kingmaker.Code.UI.MVVM.View;
using R3;
using TMPro;
using UnityEngine;

namespace Assets.Code.View.UI.MVVM;

public class CharInfoConvictionsTabView : CharInfoComponentView<CharInfoConvictionsTabVM>
{
	[SerializeField]
	private CharInfoAlignmentWheelPCView m_ConvictionView;

	[SerializeField]
	private CharInfoChoicesMadeView m_ChoicesMadeView;

	[SerializeField]
	private TextMeshProUGUI m_AlignmentTitle;

	private void Awake()
	{
		m_ChoicesMadeView.Initialize();
		m_AlignmentTitle.text = UIStrings.Instance.CharacterSheet.Alignment.Text;
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.ConvictionsVM.Subscribe(m_ConvictionView.Bind).AddTo(this);
		base.ViewModel.ChoicesMadeVM.Subscribe(m_ChoicesMadeView.Bind).AddTo(this);
	}
}
