using System.Collections.Generic;
using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class SingleLinkMultiButton : MonoBehaviour, IFloatConsoleNavigationEntity, IConsoleNavigationEntity, IConsoleEntity, IPrerequisiteLinkEntity, IConfirmClickHandler
{
	[SerializeField]
	private OwlcatMultiButton m_FocusMultiButton;

	public OwlcatMultiButton Button => m_FocusMultiButton;

	public string LinkId { get; private set; }

	public void Initialize(string linkId)
	{
		LinkId = linkId;
	}

	public void SetFocus(bool value)
	{
		m_FocusMultiButton.SetFocus(value);
		m_FocusMultiButton.SetActiveLayer(value ? "Selected" : "Normal");
	}

	public bool IsValid()
	{
		if (m_FocusMultiButton.IsValid())
		{
			return m_FocusMultiButton.Interactable;
		}
		return false;
	}

	public Vector2 GetPosition()
	{
		return base.transform.position;
	}

	public List<IFloatConsoleNavigationEntity> GetNeighbours()
	{
		return null;
	}

	public bool CanConfirmClick()
	{
		return m_FocusMultiButton.CanConfirmClick();
	}

	public void OnConfirmClick()
	{
		m_FocusMultiButton.OnConfirmClick();
	}

	public string GetConfirmClickHint()
	{
		return string.Empty;
	}
}
