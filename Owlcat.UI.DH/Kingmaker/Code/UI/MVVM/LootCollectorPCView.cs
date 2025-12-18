using Kingmaker.Blueprints.Root;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.UI.Sound;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class LootCollectorPCView : LootCollectorView
{
	private const string Normal = "Normal";

	private const string Active = "Active";

	private const string Disabled = "Disabled";

	private const string Trash = "Trash";

	[SerializeField]
	private OwlcatMultiButton m_CollectAllButton;

	[SerializeField]
	private GameObject m_CollectAllButtonBlock;

	[SerializeField]
	private TextMeshProUGUI m_CollectAllButtonLabel;

	[SerializeField]
	private OwlcatMultiButton m_CloseButton;

	[SerializeField]
	private GameObject m_CloseButtonBlock;

	[SerializeField]
	private TextMeshProUGUI m_HeaderLabel;

	[SerializeField]
	private OwlcatMultiButton m_TrashModeButton;

	[SerializeField]
	private TextMeshProUGUI m_TrashModeLabel;

	public override void Initialize()
	{
		base.Initialize();
		m_CollectAllButtonLabel.text = UIStrings.Instance.LootWindow.CollectAll.Text ?? "";
		m_HeaderLabel.text = UIStrings.Instance.LootWindow.Loot;
		m_TrashModeLabel.text = UIStrings.Instance.LootWindow.TrashMode;
	}

	protected override void OnBind()
	{
		base.OnBind();
		UISounds.Instance.SetClickAndHoverSound(m_CloseButton, ButtonSoundsEnum.PlastickSound);
		UISounds.Instance.SetClickAndHoverSound(m_CollectAllButton, ButtonSoundsEnum.LootCollectAllSound);
		ObservableSubscribeExtensions.Subscribe(m_CollectAllButton.OnLeftClickAsObservable(), delegate
		{
			CollectAll();
		}).AddTo(this);
		base.ViewModel.NoLoot.Subscribe(SetupButtons).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_CloseButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.Close();
		}).AddTo(this);
		ObservableSubscribeExtensions.Subscribe(m_TrashModeButton.OnLeftClickAsObservable(), delegate
		{
			base.ViewModel.SetTrashMode();
		}).AddTo(this);
		base.ViewModel.IsTrashMode.Subscribe(SetTrashMoveButton).AddTo(this);
		if (base.ViewModel.NoLoot.CurrentValue)
		{
			return;
		}
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.CollectAllLoot.name, delegate
		{
			if (!base.ViewModel.NoLoot.CurrentValue)
			{
				CollectAll();
			}
		}).AddTo(this);
	}

	private void SetupButtons(bool noLoot)
	{
		m_CollectAllButtonBlock.gameObject.SetActive(!noLoot);
		m_TrashModeButton.SetActiveLayer(noLoot ? "Disabled" : "Active");
	}

	private void SetTrashMoveButton(bool isTrash)
	{
		if (isTrash)
		{
			m_TrashModeButton.SetActiveLayer("Trash");
			Game.Instance.CursorController.SetCursor(CursorType.LootTrashMode, force: true);
		}
		else
		{
			m_TrashModeButton.SetActiveLayer("Active");
			Game.Instance.CursorController.ClearCursor(force: true);
		}
	}

	protected override void OnUnbind()
	{
		base.OnUnbind();
		Game.Instance.CursorController.ClearCursor(force: true);
	}
}
