using System.Collections.Generic;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class CharInfoBuffGroupView : VirtualListElementViewBase<CharacterInfoBuffGroupVM>
{
	[SerializeField]
	private TMP_Text m_TitleText;

	[SerializeField]
	private Image m_Icon;

	[SerializeField]
	private WidgetList m_WidgetList;

	[SerializeField]
	private StatusEffectBaseView m_BuffPrefab;

	protected override void BindViewImplementation()
	{
		m_TitleText.SetText(base.ViewModel.GroupTitle);
		m_Icon.sprite = base.ViewModel.GroupIcon;
		base.ViewModel.Buffs.Subscribe(HandleBuffsChanged).AddTo(this);
	}

	protected override void DestroyViewImplementation()
	{
		m_WidgetList.Clear();
	}

	private void HandleBuffsChanged(IReadOnlyList<CharInfoFeatureVM> buffs)
	{
		m_WidgetList.DrawEntries(buffs, m_BuffPrefab);
	}
}
