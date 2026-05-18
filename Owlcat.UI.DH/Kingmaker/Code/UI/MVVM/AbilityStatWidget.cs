using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class AbilityStatWidget : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_StatNameShort;

	[SerializeField]
	private TMP_Text m_StatValue;

	[SerializeField]
	private OwlcatMultiSelectable m_Selectable;

	public void Setup(string statName, int statValue, bool isFirst)
	{
		m_StatNameShort.SetText(statName);
		m_StatValue.SetText(statValue.ToString());
		m_Selectable.SetActiveLayer(isFirst ? "First" : "Default");
	}
}
