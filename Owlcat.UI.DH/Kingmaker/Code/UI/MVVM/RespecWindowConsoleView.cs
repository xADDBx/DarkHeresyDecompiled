using Kingmaker.UI.Common;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class RespecWindowConsoleView : RespecWindowCommonView
{
	[SerializeField]
	private ScrollRectExtended m_ScrollRect;

	protected override void OnBind()
	{
		base.OnBind();
		CreateInput();
	}

	private void CreateInput()
	{
	}

	public void Scroll(IConsoleEntity entity)
	{
		if (entity is MonoBehaviour monoBehaviour)
		{
			m_ScrollRect.EnsureVisibleVertical(monoBehaviour.transform as RectTransform, 50f, smoothly: false, needPinch: false);
		}
	}
}
