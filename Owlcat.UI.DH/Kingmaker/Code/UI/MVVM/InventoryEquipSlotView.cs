using System.Collections.Generic;
using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Owlcat.Runtime.Core.Utility;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Kingmaker.Code.UI.MVVM;

public abstract class InventoryEquipSlotView : ItemSlotView<EquipSlotVM>
{
	[Space]
	[SerializeField]
	public Image TypeIcon;

	[SerializeField]
	private GameObject m_PossibleTargetHighlight;

	[SerializeField]
	private GameObject m_NetLock;

	[SerializeField]
	private GameObject m_IsNotRemovable;

	[Header("UsableStacks")]
	[SerializeField]
	private GameObject m_UsableStacksContainer;

	[SerializeField]
	private TextMeshProUGUI m_UsableStacksCount;

	private bool m_IconIsSet;

	protected override void BindViewImplementation()
	{
		base.BindViewImplementation();
		AddDisposable(base.ViewModel.CanBeFakeItem.Subscribe(delegate(bool value)
		{
			m_Icon.color = (value ? new Color(1f, 1f, 1f, 0.5f) : Color.white);
		}));
		if (m_PossibleTargetHighlight != null)
		{
			AddDisposable(base.ViewModel.PossibleTarget.Subscribe(delegate(bool value)
			{
				m_PossibleTargetHighlight.SetActive(value);
			}));
		}
		AddDisposable(base.ViewModel.NetLock.Subscribe(delegate(bool value)
		{
			m_NetLock.Or(null)?.SetActive(value);
		}));
		AddDisposable(base.ViewModel.IsNotRemovable.Subscribe(delegate(bool value)
		{
			m_IsNotRemovable.Or(null)?.SetActive(value);
		}));
		AddDisposable(base.ViewModel.UsableCount.Subscribe(delegate
		{
			UpdateUsableCountText();
		}));
	}

	protected override void DestroyViewImplementation()
	{
		base.DestroyViewImplementation();
		m_IconIsSet = false;
		m_UsableStacksContainer.Or(null)?.SetActive(value: false);
	}

	protected override void SetupIcon(Sprite value)
	{
		base.SetupIcon(value);
		TypeIcon.sprite = UIConfig.Instance.TypeIcons.GetTypeIcon(base.ViewModel.SlotType, base.ViewModel.SlotSubtype);
		TypeIcon.gameObject.SetActive(value == null);
		m_IconIsSet = true;
		UpdateUsableCountText();
	}

	private void UpdateUsableCountText()
	{
		if (m_IconIsSet)
		{
			int num = base.ViewModel.UsableCount?.CurrentValue ?? 0;
			m_UsableStacksContainer.Or(null)?.SetActive(num > 0);
			if ((bool)m_UsableStacksCount)
			{
				m_UsableStacksCount.text = num.ToString();
			}
		}
	}

	protected override void SetupContextMenu()
	{
		UIContextMenu contextMenu = UIStrings.Instance.ContextMenu;
		base.SetupContextMenu();
		base.ViewModel.SetContextMenu(new List<ContextMenuCollectionEntity>
		{
			new ContextMenuCollectionEntity(contextMenu.TakeOff, delegate
			{
				TryUnequip();
			}, base.ViewModel.IsEquipPossible && !base.ViewModel.IsNotRemovable.CurrentValue),
			new ContextMenuCollectionEntity(contextMenu.Information, base.ViewModel.ShowInfo, base.ViewModel.HasItem)
		});
	}

	protected bool TryUnequip()
	{
		return UIInventoryHelper.TryUnequip(base.ViewModel);
	}
}
