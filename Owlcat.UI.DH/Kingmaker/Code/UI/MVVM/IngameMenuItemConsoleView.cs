using System;
using System.Collections.Generic;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class IngameMenuItemConsoleView : MonoBehaviour, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity
{
	[SerializeField]
	private TextMeshProUGUI m_Label;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	private Action m_Action;

	public void Initialize(string text)
	{
		base.gameObject.SetActive(value: false);
		m_Label.text = text;
	}

	public void Bind(Action action)
	{
		m_Action = action;
		base.gameObject.SetActive(value: true);
	}

	public void OnConfirmClick()
	{
		m_Action?.Invoke();
	}

	public void SetFocus(bool value)
	{
		m_Button.SetFocus(value);
	}

	public bool IsValid()
	{
		return m_Button.IsValid();
	}

	public Vector2 GetPosition()
	{
		return base.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}
}
