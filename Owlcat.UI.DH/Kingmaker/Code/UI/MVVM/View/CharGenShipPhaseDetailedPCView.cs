using Kingmaker.Blueprints.Root.Strings;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenShipPhaseDetailedPCView : CharGenShipPhaseDetailedView
{
	[SerializeField]
	private OwlcatButton m_SetNameButton;

	[SerializeField]
	private TextMeshProUGUI m_SetNameButtonLabel;

	[SerializeField]
	private OwlcatButton m_SetRandomNameButton;

	[SerializeField]
	private TextMeshProUGUI m_SetRandomNameButtonLabel;

	[SerializeField]
	private CharGenChangeNameMessageBoxPCView m_MessageBoxPCView;

	public override void Initialize()
	{
		base.Initialize();
		m_SetNameButtonLabel.text = UIStrings.Instance.CharGen.EditNameButton;
		m_SetRandomNameButtonLabel.text = UIStrings.Instance.CharGen.SetRandomNameButton;
	}

	protected override void OnBind()
	{
		base.OnBind();
		base.ViewModel.MessageBoxVM.Subscribe(m_MessageBoxPCView.Bind).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_SetNameButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ShowChangeNameMessageBox();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_SetRandomNameButton.OnLeftClickAsObservable(), delegate
		{
			GenerateRandomName();
		}).AddTo(this);
		m_SetRandomNameButton.SetHint(UIStrings.Instance.CharGen.SetRandomName).AddTo(this);
	}
}
