using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TextureSelectorTabsVM : BaseCharGenAppearancePageComponentVM
{
	private readonly ReactiveProperty<int> m_CurrentIndex = new ReactiveProperty<int>();

	private readonly ReactiveProperty<TextureSelectorVM> m_CurrentTabSelector = new ReactiveProperty<TextureSelectorVM>();

	private CharGenContext m_ChargenContext;

	private readonly AutoDisposingList<TextureSelectorVM> m_TabsSelectorVms = new AutoDisposingList<TextureSelectorVM>();

	private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>();

	private readonly ReactiveCommand<Unit> m_OnSetValues = new ReactiveCommand<Unit>();

	private readonly ReactiveProperty<int> m_TotalItems = new ReactiveProperty<int>();

	public ReadOnlyReactiveProperty<int> CurrentIndex => m_CurrentIndex;

	public ReadOnlyReactiveProperty<TextureSelectorVM> CurrentTabSelector => m_CurrentTabSelector;

	public Observable<Unit> OnSetValues => m_OnSetValues;

	public ReadOnlyReactiveProperty<int> TotalItems => m_TotalItems;

	public ReadOnlyReactiveProperty<string> Title => m_Title;

	public TextureSelectorTabsVM(CharGenContext charGenContext)
	{
		m_ChargenContext = charGenContext;
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		Clear();
	}

	private void Clear()
	{
		m_TabsSelectorVms.Clear();
	}

	public void SetTitle(string title)
	{
		m_Title.Value = title;
	}

	public void SetValues(IEnumerable<TextureSelectorVM> tabsSelectors)
	{
		if (tabsSelectors.Any())
		{
			m_TabsSelectorVms.Clear();
			foreach (TextureSelectorVM tabsSelector in tabsSelectors)
			{
				m_TabsSelectorVms.Add(tabsSelector);
				AddDisposable(tabsSelector.OnChanged.Subscribe(delegate
				{
					Changed();
				}));
			}
		}
		m_TotalItems.Value = m_TabsSelectorVms.Count;
		SetIndex((CurrentIndex.CurrentValue < TotalItems.CurrentValue) ? CurrentIndex.CurrentValue : 0);
		m_OnSetValues.Execute(Unit.Default);
	}

	public void SetIndex(int index)
	{
		if (index < 0 || index >= m_TabsSelectorVms.Count)
		{
			throw new ArgumentOutOfRangeException();
		}
		m_CurrentTabSelector.Value = m_TabsSelectorVms[index];
		m_CurrentIndex.Value = index;
		UpdateIndexForClient(index);
		EventBus.RaiseEvent(delegate(ICharGenTextureSelectorTabChangeHandler h)
		{
			h.HandleTextureSelectorTabChange(base.Type, index);
		});
		EventBus.RaiseEvent(delegate(ICharGenAppearanceComponentUpdateHandler h)
		{
			h.HandleAppearanceComponentUpdate(base.Type);
		});
	}

	protected override void SetUITattoo(EquipmentEntityLink equipmentEntityLink, int index, int tattooTabIndex)
	{
		if (base.Type == CharGenAppearancePageComponent.Tattoo && !UtilityNet.IsControlMainCharacter())
		{
			PFLog.UI.Log($"{equipmentEntityLink.AssetId} / {index} / {tattooTabIndex}");
			SetIndex(tattooTabIndex);
			CurrentTabSelector.CurrentValue.SelectionGroup.TrySelectEntity(CurrentTabSelector.CurrentValue.SelectionGroup.EntitiesCollection[index]);
		}
	}

	private void UpdateIndexForClient(int index)
	{
		if (UtilityNet.IsControlMainCharacter() && base.Type == CharGenAppearancePageComponent.Tattoo)
		{
			int num = CurrentTabSelector.CurrentValue.SelectionGroup.EntitiesCollection.IndexOf(CurrentTabSelector.CurrentValue.SelectionGroup.SelectedEntity.Value);
			if (m_ChargenContext.Doll.Tattoos.Any() && index < m_ChargenContext.Doll.Tattoos.Count && num >= 0 && num < m_ChargenContext.Doll.Tattoos[index].Paints.Count)
			{
				EquipmentEntityLink tattoo = m_ChargenContext.Doll.Tattoos[index].Paints[num];
				Game.Instance.GameCommandQueue.CharGenSetTattoo(tattoo, num, index);
			}
		}
	}
}
