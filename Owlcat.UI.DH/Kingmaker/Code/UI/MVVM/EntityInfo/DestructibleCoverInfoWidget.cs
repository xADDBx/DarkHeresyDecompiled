using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public sealed class DestructibleCoverInfoWidget : BaseEntityInfoWidget
{
	private const string DurabilityFormat = "{0}/<size={1}%>{2}</size>";

	[SerializeField]
	private TMP_Text m_DurabilityValue;

	[SerializeField]
	private int m_MaxDurabilityTextSize = 74;

	public override bool TryShow(IEntityInfo entityInfo)
	{
		if (!(entityInfo is DestructibleCoverInfo destructibleInfo))
		{
			ShowInternal(show: false);
			return false;
		}
		Setup(destructibleInfo);
		ShowInternal(show: true);
		return true;
	}

	private void Setup(DestructibleCoverInfo destructibleInfo)
	{
		string text = $"{destructibleInfo.CurrentDurability}/<size={m_MaxDurabilityTextSize}%>{destructibleInfo.MaxDurability}</size>";
		m_DurabilityValue.SetText(text);
		SetPosition(destructibleInfo.WorldPosition);
	}
}
