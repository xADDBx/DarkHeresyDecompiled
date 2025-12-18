using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class SaveSlotsExpandableTitleVM : ExpandableTitleVM
{
	private readonly Action m_DeleteAll;

	private readonly ReactiveProperty<List<ContextMenuCollectionEntity>> m_ContextMenu = new ReactiveProperty<List<ContextMenuCollectionEntity>>();

	public readonly SaveInfo SaveInfo;

	public ReadOnlyReactiveProperty<List<ContextMenuCollectionEntity>> ContextMenu => m_ContextMenu;

	public SaveSlotsExpandableTitleVM(string title, Action<bool> @switch, bool defaultExpanded = true, Action deleteAll = null, SaveInfo saveInfo = null)
		: base(title, @switch, defaultExpanded)
	{
		m_DeleteAll = deleteAll;
		SaveInfo = saveInfo;
	}

	public void SetContextMenu(List<ContextMenuCollectionEntity> entities)
	{
		m_ContextMenu.Value = entities;
	}

	public void DeleteAll()
	{
		string deleteWarning = string.Concat("<b>", UIStrings.Instance.SaveLoadTexts.AreYouSureDeleteCharacter, Environment.NewLine, UIStrings.Instance.CommonTexts.ThisActionCantBeCanceled, "</b>");
		EventBus.RaiseEvent(delegate(IDialogMessageBoxUIHandler h)
		{
			h.HandleOpen(deleteWarning.ToUpper(), DialogMessageBoxType.Dialog, delegate(DialogMessageBoxButton respond)
			{
				if (respond == DialogMessageBoxButton.Yes)
				{
					m_DeleteAll?.Invoke();
				}
			}, null, UIStrings.Instance.SaveLoadTexts.DeleteCharacter);
		});
	}
}
