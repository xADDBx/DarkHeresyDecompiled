using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class RespecWindowCommonView : View<RespecVM>, IInitializable
{
	[SerializeField]
	private FadeAnimator m_Animator;

	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	private TextMeshProUGUI m_RespecCost;

	[SerializeField]
	private TextMeshProUGUI m_WarningLabel;

	[SerializeField]
	protected RespecCharactersSelectorView m_RespecCharactersSelectorView;

	public void Initialize()
	{
		m_Animator.Initialize();
	}

	protected override void OnBind()
	{
		m_HeaderLabel.text = UIStrings.Instance.CharGen.RespecWindowHeader;
		m_WarningLabel.text = UIStrings.Instance.CharGen.RespecWindowWarning;
		m_Animator.AppearAnimation();
		m_RespecCharactersSelectorView.Bind(base.ViewModel.CharacterSelectionGroupRadioVM);
		base.ViewModel.RespecCost.Subscribe(delegate(int value)
		{
			m_RespecCost.text = FormatCost(value);
		}).AddTo(this);
	}

	protected override void OnUnbind()
	{
		m_Animator.DisappearAnimation();
	}

	protected void CloseWindow()
	{
		base.ViewModel.OnClose();
	}

	protected void OnConfirm()
	{
		base.ViewModel.OnConfirm();
	}

	private string FormatCost(int cost)
	{
		if (cost <= 0)
		{
			return "0";
		}
		return "-" + cost;
	}
}
