using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.UI.Common.Animations;
using Owlcat.UI;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.View;

public class CharacterVisualSettingsView<TBoolEntity> : View<CharacterVisualSettingsVM>, IInitializable where TBoolEntity : CharacterVisualSettingsEntityView
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	private TextMeshProUGUI m_Header;

	[Space]
	[SerializeField]
	private TBoolEntity m_EntityTogglePrefab;

	[SerializeField]
	private RectTransform m_TogglesParent;

	[SerializeField]
	protected TextureSelectorPagedView m_OutfitMainColorSelectorView;

	private readonly List<TBoolEntity> m_PooledViews = new List<TBoolEntity>();

	protected IEnumerable<TBoolEntity> EntityToggleViews => m_PooledViews;

	public virtual void Initialize()
	{
		m_FadeAnimator.Initialize();
		m_OutfitMainColorSelectorView.Initialize();
	}

	protected override void OnBind()
	{
		m_Header.text = UIStrings.Instance.CharacterSheet.VisualSettingsTitle;
		m_FadeAnimator.AppearAnimation();
		m_OutfitMainColorSelectorView.Bind(base.ViewModel.OutfitMainColorSelector);
		for (int i = 0; i < base.ViewModel.ToggleVMs.Count; i++)
		{
			if (m_PooledViews.Count <= i)
			{
				TBoolEntity widget = WidgetFactory.GetWidget(m_EntityTogglePrefab, activate: false, strictMatching: true);
				m_PooledViews.Add(widget);
				widget.transform.SetParent(m_TogglesParent, worldPositionStays: false);
			}
			VisualSettingsToggle visualSettingsToggle = base.ViewModel.ToggleVMs[i];
			m_PooledViews[i].Initialize(visualSettingsToggle.ToggleName);
			m_PooledViews[i].Bind(visualSettingsToggle.EntityVM);
			m_PooledViews[i].gameObject.SetActive(value: true);
		}
		for (int j = base.ViewModel.ToggleVMs.Count; j < m_PooledViews.Count; j++)
		{
			m_PooledViews[j].Unbind();
			m_PooledViews[j].gameObject.SetActive(value: false);
		}
	}

	private void UnbindToggles()
	{
		foreach (TBoolEntity pooledView in m_PooledViews)
		{
			pooledView.Unbind();
		}
	}

	protected override void OnUnbind()
	{
		m_FadeAnimator.DisappearAnimation(UnbindToggles);
	}

	protected override void OnDestroy()
	{
		foreach (TBoolEntity pooledView in m_PooledViews)
		{
			WidgetFactory.DisposeWidget(pooledView);
		}
		m_PooledViews.Clear();
		base.OnDestroy();
	}
}
