using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class AbilityRestrictionWidget : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_Description;

	[SerializeField]
	private OwlcatMultiSelectable m_PassedSelectable;

	[SerializeField]
	private OwlcatMultiSelectable m_PositionSelectable;

	public TMP_Text DescriptionText => m_Description;

	public void Setup(string description, bool isPassed, bool isFirst, bool isLast)
	{
		m_Description.SetText(description);
		m_PassedSelectable.SetActiveLayer(isPassed ? "Passed" : "NotPassed");
		m_PositionSelectable.SetActiveLayer(GetPositionLayerName(isFirst, isLast));
	}

	private static string GetPositionLayerName(bool isFirst, bool isLast)
	{
		if (isFirst && isLast)
		{
			return "Single";
		}
		if (!isFirst && !isLast)
		{
			return "Default";
		}
		if (!isFirst)
		{
			return "Last";
		}
		return "First";
	}
}
