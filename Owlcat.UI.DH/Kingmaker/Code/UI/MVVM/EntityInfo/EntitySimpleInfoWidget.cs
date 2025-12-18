using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public sealed class EntitySimpleInfoWidget : BaseEntityInfoWidget
{
	[SerializeField]
	private TMP_Text m_Title;

	[SerializeField]
	private TMP_Text m_Description;

	public override bool TryShow(IEntityInfo entityInfo)
	{
		if (!(entityInfo is EntitySimpleInfo entitySimpleInfo))
		{
			ShowInternal(show: false);
			return false;
		}
		SetPosition(entitySimpleInfo.WorldPosition);
		m_Title.SetText(entitySimpleInfo.Title);
		m_Description.SetText(entitySimpleInfo.Description);
		ShowInternal(show: true);
		return true;
	}
}
