using System;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using ObservableCollections;
using Owlcat.UI;
using R3;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class TextureSelectorVM : BaseCharGenAppearancePageComponentVM, IVirtualListElementIdentifier
{
	private readonly ReactiveProperty<string> m_Description = new ReactiveProperty<string>();

	private readonly bool m_HideIfNoElements;

	private readonly ReactiveProperty<string> m_NoItemsDesc = new ReactiveProperty<string>();

	private readonly ReactiveProperty<string> m_Title = new ReactiveProperty<string>();

	public readonly SelectionGroupRadioVM<TextureSelectorItemVM> SelectionGroup;

	public ReadOnlyReactiveProperty<string> Title => m_Title;

	public ReadOnlyReactiveProperty<string> Description => m_Description;

	public ReadOnlyReactiveProperty<string> NoItemsDesc => m_NoItemsDesc;

	public int GroupIndex { get; private set; }

	public int VirtualListTypeId { get; }

	public TextureSelectorVM(SelectionGroupRadioVM<TextureSelectorItemVM> selectionGroup, TextureSelectorType typeId, bool hideIfNoElements = true, int groupIndex = 0)
	{
		m_HideIfNoElements = hideIfNoElements;
		GroupIndex = groupIndex;
		AddDisposable(SelectionGroup = selectionGroup);
		AddDisposable(SelectionGroup.SelectedEntity.Subscribe(delegate
		{
			Changed();
		}));
		AddDisposable(SelectionGroup.EntitiesCollection.ObserveAdd().Subscribe(delegate
		{
			UpdateState();
		}));
		AddDisposable(SelectionGroup.EntitiesCollection.ObserveRemove().Subscribe(delegate
		{
			UpdateState();
		}));
		VirtualListTypeId = (int)typeId;
		UpdateState();
		AddDisposable(EventBus.Subscribe(this));
	}

	protected override void DisposeImplementation()
	{
		SelectionGroup.EntitiesCollection.ForEach(delegate(TextureSelectorItemVM vm)
		{
			vm.Dispose();
		});
	}

	public void SetTitle(string title)
	{
		m_Title.Value = title;
	}

	public void SetDescription(string description)
	{
		m_Description.Value = description;
	}

	public void SetNoItemsDescription(string description)
	{
		m_NoItemsDesc.Value = description;
	}

	private void UpdateState()
	{
		bool activeState = SelectionGroup.EntitiesCollection.Count > 0 || !m_HideIfNoElements;
		SetActiveState(activeState);
		m_IsAvailable.Value = SelectionGroup.EntitiesCollection.Count > 0;
	}

	public void SetActiveState(bool state)
	{
		Active.Value = state;
	}

	public override void CaptureDefaults()
	{
		m_DefaultIndex = SelectionGroup.SelectedEntity.CurrentValue?.Number ?? (-1);
	}

	public override void Randomize()
	{
		ObservableList<TextureSelectorItemVM> entitiesCollection = SelectionGroup.EntitiesCollection;
		if (entitiesCollection.Count != 0)
		{
			SelectionGroup.TrySelectEntity(entitiesCollection[UnityEngine.Random.Range(0, entitiesCollection.Count)]);
		}
	}

	public override void ResetToDefault()
	{
		m_DefaultIndex = Math.Max(0, m_DefaultIndex);
		if (m_DefaultIndex < SelectionGroup.EntitiesCollection.Count)
		{
			SelectionGroup.TrySelectEntity(SelectionGroup.EntitiesCollection[m_DefaultIndex]);
		}
	}

	public void SelectFirst()
	{
		if (SelectionGroup.EntitiesCollection.Count > 0)
		{
			SelectionGroup.TrySelectEntity(SelectionGroup.EntitiesCollection[0]);
		}
	}

	protected override void SetSelectUISkinColor(int index)
	{
		if (base.Type == CharGenAppearancePageComponent.SkinColour)
		{
			SelectEntityImpl(index);
		}
	}

	protected override void SetSelectUIHair(EquipmentEntityLink equipmentEntityLink, int index)
	{
		if (base.Type == CharGenAppearancePageComponent.HairType)
		{
			SelectEntityImpl(index);
		}
	}

	protected override void SetSelectUIHairColor(int index)
	{
		if (base.Type == CharGenAppearancePageComponent.HairColour)
		{
			SelectEntityImpl(index);
		}
	}

	protected override void SetUIEyebrows(EquipmentEntityLink equipmentEntityLink, int index)
	{
		if (base.Type == CharGenAppearancePageComponent.EyebrowType)
		{
			SelectEntityImpl(index);
		}
	}

	protected override void SetUIEyebrowsColor(int index)
	{
		if (base.Type == CharGenAppearancePageComponent.EyebrowColour)
		{
			SelectEntityImpl(index);
		}
	}

	protected override void SetUIBeard(EquipmentEntityLink equipmentEntityLink, int index)
	{
		if (base.Type == CharGenAppearancePageComponent.BeardType)
		{
			SelectEntityImpl(index);
		}
	}

	protected override void SetUIBeardColor(int index)
	{
		if (base.Type == CharGenAppearancePageComponent.BeardColour)
		{
			SelectEntityImpl(index);
		}
	}

	protected override void SetUITattoo(EquipmentEntityLink equipmentEntityLink, int index, int tattooTabIndex)
	{
		if (base.Type == CharGenAppearancePageComponent.Tattoo || GroupIndex == index)
		{
			SelectEntityImpl(index);
		}
	}

	protected override void SetUITattooColor(int rampIndex, int index)
	{
		if (base.Type == CharGenAppearancePageComponent.TattooColor || GroupIndex == index)
		{
			SelectEntityImpl(rampIndex);
		}
	}

	protected override void SetUIPort(EquipmentEntityLink equipmentEntityLink, int index, int portNumber)
	{
		if (base.Type == CharGenAppearancePageComponent.PortType1 || base.Type == CharGenAppearancePageComponent.PortType2 || GroupIndex == index)
		{
			SelectEntityImpl(index);
		}
	}

	private void SelectEntityImpl(int index)
	{
		if (!UtilityNet.IsControlMainCharacter())
		{
			SelectionGroup.TrySelectEntity(SelectionGroup.EntitiesCollection[index]);
		}
	}
}
