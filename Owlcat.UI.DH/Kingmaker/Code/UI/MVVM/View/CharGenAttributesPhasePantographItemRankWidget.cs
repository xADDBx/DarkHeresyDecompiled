using Owlcat.UI;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharGenAttributesPhasePantographItemRankWidget : MonoBehaviour
{
	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	public void SetState(bool state)
	{
		m_Selectable.SetActiveLayer(state ? "Active" : "Inactive");
	}
}
