using System;
using Kingmaker.UnitLogic.Buffs.Components;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class UnitBuffBlockGroupElement : MonoBehaviour
{
	[SerializeField]
	private BuffGroupType m_Group;

	[SerializeField]
	public Image m_Icon;

	public BuffGroupType Group => m_Group;

	public RectTransform RectTransform => m_Icon.rectTransform;

	public bool IsActive => m_Icon.enabled;

	public void SetActive(bool active)
	{
		m_Icon.enabled = active;
	}

	public IDisposable SetHint(string hint)
	{
		return m_Icon.SetHint(hint);
	}
}
