using System.Collections.Generic;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class ContextMenuEntityConsoleView : ContextMenuEntityView, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity
{
	public void Awake()
	{
		base.gameObject.SetActive(value: false);
	}

	protected override void OnBind()
	{
		base.OnBind();
		if (m_ButtonFx != null)
		{
			m_Button.OnFocusAsObservable().Subscribe(delegate(bool value)
			{
				m_ButtonFx.DoHovered(value);
			}).AddTo(this);
		}
		base.gameObject.SetActive(value: true);
	}

	protected override void OnUnbind()
	{
		base.gameObject.SetActive(value: false);
	}

	public void OnConfirm()
	{
		base.ViewModel.Execute();
	}

	public Vector2 GetPosition()
	{
		RectTransform rectTransform = base.transform as RectTransform;
		if (!(rectTransform != null))
		{
			return base.transform.position;
		}
		return rectTransform.anchoredPosition;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}
}
