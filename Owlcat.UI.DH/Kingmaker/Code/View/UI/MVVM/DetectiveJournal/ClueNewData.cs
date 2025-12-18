using System;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

[Serializable]
public class ClueNewData
{
	[SerializeField]
	private RectTransform m_NewIcon;

	[SerializeField]
	private RectTransform m_NewAddendumsIcon;

	[SerializeField]
	private RectTransform m_NewStudiesIcon;

	public RectTransform GetNewIcon(ClueState state)
	{
		return state switch
		{
			ClueState.Default => null, 
			ClueState.New => m_NewIcon, 
			ClueState.NewAddendums => m_NewAddendumsIcon, 
			ClueState.NewStudies => m_NewStudiesIcon, 
			_ => throw new ArgumentOutOfRangeException("state", state, null), 
		};
	}
}
