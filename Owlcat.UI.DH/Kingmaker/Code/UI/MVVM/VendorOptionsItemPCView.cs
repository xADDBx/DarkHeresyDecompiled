using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorOptionsItemPCView : View<VendorOptionsItemVM>
{
	[SerializeField]
	private TextMeshProUGUI m_TitleText;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	protected override void OnBind()
	{
		base.ViewModel.Title.Subscribe(delegate(string value)
		{
			m_TitleText.text = value;
		}).AddTo(this);
		base.ViewModel.State.Subscribe(delegate(bool value)
		{
			m_Button.SetActiveLayer(value ? 1 : 0);
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Button.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.SwitchOption();
		}).AddTo(this);
	}
}
