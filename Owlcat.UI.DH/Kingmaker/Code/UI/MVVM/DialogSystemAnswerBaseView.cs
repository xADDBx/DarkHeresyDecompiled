using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class DialogSystemAnswerBaseView : View<AnswerVM>, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	protected TextMeshProUGUI m_Text;

	[SerializeField]
	protected OwlcatMultiButton m_Button;

	public void Initialize()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.gameObject.SetActive(value: true);
		m_Text.text = base.ViewModel.AnswerRawText;
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}

	public virtual void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}
}
