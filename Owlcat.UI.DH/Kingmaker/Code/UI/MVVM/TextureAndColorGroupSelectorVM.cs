using System.Collections.Generic;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.ResourceLinks;
using ObservableCollections;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class TextureAndColorGroupSelectorVM : BaseCharGenAppearancePageComponentVM
{
	private CharGenContext m_ChargenContext;

	private readonly List<TextureSelectorVM> m_TexturesSelectorVms = new List<TextureSelectorVM>();

	private readonly List<TextureSelectorVM> m_ColorsSelectorVms = new List<TextureSelectorVM>();

	private readonly List<TextureAndColorSelectorItemVM> m_AllItems = new List<TextureAndColorSelectorItemVM>();

	private readonly ReactiveProperty<bool> m_CanAdd = new ReactiveProperty<bool>(value: true);

	public readonly ObservableList<TextureAndColorSelectorItemVM> Items = new ObservableList<TextureAndColorSelectorItemVM>();

	public ReadOnlyReactiveProperty<bool> CanAdd => m_CanAdd;

	public TextureAndColorGroupSelectorVM(CharGenContext charGenContext)
	{
		m_ChargenContext = charGenContext;
		EventBus.Subscribe(this).AddTo(this);
	}

	public override void CaptureDefaults()
	{
		m_TexturesSelectorVms.ForEach(delegate(TextureSelectorVM vm)
		{
			vm.CaptureDefaults();
		});
		m_ColorsSelectorVms.ForEach(delegate(TextureSelectorVM vm)
		{
			vm.CaptureDefaults();
		});
	}

	public override void Randomize()
	{
		m_TexturesSelectorVms.ForEach(delegate(TextureSelectorVM vm)
		{
			vm.Randomize();
		});
		m_ColorsSelectorVms.ForEach(delegate(TextureSelectorVM vm)
		{
			vm.Randomize();
		});
	}

	protected override void SetUITattoo(EquipmentEntityLink equipmentEntityLink, int index, int tattooTabIndex)
	{
		if (tattooTabIndex >= 0 && tattooTabIndex < m_ColorsSelectorVms.Count)
		{
			CharGenAppearanceComponentFactory.RefreshTattooColorSelector(m_ChargenContext, tattooTabIndex, m_ColorsSelectorVms[tattooTabIndex]);
			base.ContentChanged.Execute(Unit.Default);
		}
	}

	public override void ResetToDefault()
	{
		m_TexturesSelectorVms.ForEach(delegate(TextureSelectorVM vm)
		{
			vm.ResetToDefault();
		});
		m_ColorsSelectorVms.ForEach(delegate(TextureSelectorVM vm)
		{
			vm.ResetToDefault();
		});
	}

	protected override void DisposeImplementation()
	{
		Clear();
	}

	private void Clear()
	{
		m_TexturesSelectorVms.ForEach(delegate(TextureSelectorVM x)
		{
			x.Dispose();
		});
		m_TexturesSelectorVms.Clear();
		m_ColorsSelectorVms.ForEach(delegate(TextureSelectorVM x)
		{
			x.Dispose();
		});
		m_ColorsSelectorVms.Clear();
		m_AllItems.ForEach(delegate(TextureAndColorSelectorItemVM x)
		{
			x.Dispose();
		});
		m_AllItems.Clear();
		Items.Clear();
	}

	public void SetValues(IEnumerable<TextureSelectorVM> textures, IEnumerable<TextureSelectorVM> colors)
	{
		Clear();
		foreach (TextureSelectorVM texture in textures)
		{
			m_TexturesSelectorVms.Add(texture);
			texture.OnChanged.Subscribe(delegate
			{
				Changed();
			}).AddTo(this);
		}
		foreach (TextureSelectorVM color in colors)
		{
			m_ColorsSelectorVms.Add(color);
			color.OnChanged.Subscribe(delegate
			{
				Changed();
			}).AddTo(this);
		}
		for (int i = 0; i < m_TexturesSelectorVms.Count; i++)
		{
			TextureSelectorVM texturesSelectorVm = ((m_TexturesSelectorVms.Count > i) ? m_TexturesSelectorVms[i] : null);
			TextureSelectorVM colorsSelectorVm = ((m_ColorsSelectorVms.Count > i) ? m_ColorsSelectorVms[i] : null);
			TextureAndColorSelectorItemVM item = new TextureAndColorSelectorItemVM(i, texturesSelectorVm, colorsSelectorVm);
			ObservableSubscribeExtensions.Subscribe(item.OnRemoveRequested, delegate
			{
				RemoveItem(item);
			}).AddTo(this);
			m_AllItems.Add(item);
		}
		if (m_AllItems.Count > 0)
		{
			Items.Add(m_AllItems[0]);
		}
		base.ContentChanged.Execute(Unit.Default);
	}

	public void AddGroup()
	{
		foreach (TextureAndColorSelectorItemVM allItem in m_AllItems)
		{
			if (!Items.Contains(allItem))
			{
				Items.Add(allItem);
				base.ContentChanged.Execute(Unit.Default);
				break;
			}
		}
		m_CanAdd.Value = m_TexturesSelectorVms.Count > Items.Count;
	}

	private void RemoveItem(TextureAndColorSelectorItemVM item)
	{
		if (item.CanRemove && Items.Remove(item))
		{
			item.TexturesSelectorVm?.SelectFirst();
			item.ColorsSelectorVm?.SelectFirst();
			base.ContentChanged.Execute(Unit.Default);
			m_CanAdd.Value = m_TexturesSelectorVms.Count > Items.Count;
		}
	}
}
