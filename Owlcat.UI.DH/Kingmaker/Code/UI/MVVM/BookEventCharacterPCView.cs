using Owlcat.Runtime.Core.Utility;
using Owlcat.UI;
using R3;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class BookEventCharacterPCView : View<BookEventCharacterVM>
{
	[SerializeField]
	private Image m_Portrait;

	[SerializeField]
	private Toggle m_Toggle;

	protected override void OnBind()
	{
		m_Toggle.group = base.transform.parent.EnsureComponent<ToggleGroup>();
		SetPortrait();
		m_Toggle.OnValueChangedAsObservable().Subscribe(OnChoose).AddTo(this);
	}

	private void SetPortrait()
	{
		m_Portrait.gameObject.SetActive(base.ViewModel.Portrait != null);
		m_Portrait.sprite = base.ViewModel.Portrait;
	}

	private void OnChoose(bool value)
	{
		base.ViewModel.OnChooseUnit(value);
	}
}
