using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.View.UI.MVVM.DetectiveJournal;

public class DotWidget : MonoBehaviour
{
	[SerializeField]
	private OwlcatMultiSelectable m_StateSelectable;

	[field: Header("Elements")]
	[field: SerializeField]
	public RectTransform RectTransform { get; private set; }

	public void SetState(DotState state)
	{
		m_StateSelectable.SetActiveLayer(state.ToString());
	}
}
