using Owlcat.UI;
using R3;
using R3.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public class ChargenProgressionHeaderView : View<ChargenProgressionHeaderVM>
{
	[SerializeField]
	private WidgetList m_LevelList;

	[SerializeField]
	private ChargenProgressionHeaderLevelView m_LevelPrefab;

	[SerializeField]
	private RectTransform m_CurrentSelector;

	[SerializeField]
	private OwlcatMultiButton m_Button;

	[SerializeField]
	private Button m_AlternativeButton;

	protected override void OnBind()
	{
		base.OnBind();
		m_LevelList.Clear();
		m_LevelList.DrawEntries(base.ViewModel.Levels, m_LevelPrefab);
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_LevelList.Container as RectTransform);
		base.ViewModel.CurrentLevel.DelayFrame(2).Subscribe(UpdateCurrentSelector).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_Button?.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.ToggleOpen();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_AlternativeButton?.OnClickAsObservable(), delegate
		{
			base.ViewModel.ToggleOpen();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(this.OnEnableAsObservable().DelayFrame(1), delegate
		{
			UpdateCurrentSelector(base.ViewModel.CurrentLevel.CurrentValue);
		}).AddTo(this);
	}

	private void UpdateCurrentSelector(int index)
	{
		if (m_LevelList.Entries.Count >= index && index > 0)
		{
			m_CurrentSelector.position = ((MonoBehaviour)m_LevelList.Entries[index - 1]).transform.position;
		}
	}
}
