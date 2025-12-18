using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public sealed class AreaEffectInfoWidget : BaseEntityInfoWidget<EntityInfoElementView>
{
	[SerializeField]
	private EntityInfoElementView m_ElementPrefab;

	public override bool TryShow(IEntityInfo entityInfo)
	{
		if (!(entityInfo is AreaEffectEntityInfo areaEffectInfo))
		{
			ShowInternal(show: false);
			return false;
		}
		Setup(areaEffectInfo);
		ShowInternal(show: true);
		return true;
	}

	private void Setup(AreaEffectEntityInfo areaEffectInfo)
	{
		SetPosition(areaEffectInfo.WorldPosition);
		HideAllElements();
		int count = areaEffectInfo.Descriptions.Count;
		for (int i = 0; i < count; i++)
		{
			EntityInfoElementView elementView = GetElementView(i, m_ElementPrefab);
			IEntityInfoDescription descriptionEntry = areaEffectInfo.Descriptions[i];
			SetupElement(elementView, descriptionEntry);
			elementView.ShowUnderline(i != count - 1);
			elementView.gameObject.SetActive(value: true);
		}
	}

	private void SetupElement(EntityInfoElementView element, IEntityInfoDescription descriptionEntry)
	{
		if (descriptionEntry is AreaEffectInfoEntry areaEffectInfoEntry)
		{
			element.SetText(areaEffectInfoEntry.Text);
			element.SetIcon(areaEffectInfoEntry.Icon);
			element.SetDOT(null);
		}
		else if (descriptionEntry is DOTEffectInfoEntry dOTEffectInfoEntry)
		{
			element.SetText(dOTEffectInfoEntry.Text);
			element.SetDOT(dOTEffectInfoEntry.Type);
			element.SetIcon(null);
		}
	}
}
