using Owlcat.UI;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class DialogSystemAnswerConsoleView : DialogSystemAnswerBaseView, IConfirmClickHandler, IConsoleEntity
{
	[SerializeField]
	private Image m_Image;

	protected override void OnBind()
	{
		base.OnBind();
	}

	public override void SetFocus(bool value)
	{
		base.SetFocus(value);
		m_Image.gameObject.SetActive(value);
	}

	public bool CanConfirmClick()
	{
		return m_Image.IsActive();
	}

	public void OnConfirmClick()
	{
		base.ViewModel.OnChooseAnswer();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}

	public void ShowAnswerHint(bool value)
	{
		m_Image.gameObject.SetActive(value);
	}
}
