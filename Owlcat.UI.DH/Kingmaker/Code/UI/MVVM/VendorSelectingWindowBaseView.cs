using System.Linq;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Root;
using Kingmaker.Code.View.UI.UIUtilities;
using Kingmaker.EntitySystem;
using Kingmaker.GameCommands;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.UI.Common.Animations;
using Kingmaker.UI.InputSystems;
using Owlcat.UI;
using R3;
using TMPro;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM;

public class VendorSelectingWindowBaseView : View<VendorSelectingWindowVM>, IInitializable
{
	[SerializeField]
	private FadeAnimator m_FadeAnimator;

	[SerializeField]
	protected WidgetList m_WidgetList;

	[SerializeField]
	private CharInfoFactionReputationItemPCView m_FactionReputationItemPCView;

	[SerializeField]
	private TextMeshProUGUI m_Header;

	public void Initialize()
	{
		m_FadeAnimator.Initialize();
	}

	protected override void OnBind()
	{
		m_FadeAnimator.AppearAnimation();
		DrawEntities();
		m_Header.text = UIStrings.Instance.Vendor.ChooseVendorForTrade;
		EventBus.Subscribe(this).AddTo(this);
		EscHotkeyManager.Instance.Subscribe(OnCloseClick).AddTo(this);
	}

	protected override void OnUnbind()
	{
		Close();
	}

	protected void OnCloseClick()
	{
		AreaPersistentState loadedAreaState = Game.Instance.LoadedAreaState;
		if (loadedAreaState != null)
		{
			_ = loadedAreaState.Settings.CapitalPartyMode;
			if (true && Game.Instance.LoadedAreaState.Settings.CapitalPartyMode && !UtilityNet.IsControlMainCharacterWithWarning())
			{
				return;
			}
		}
		Close();
		EventBus.RaiseEvent(delegate(IBeginSelectingVendorHandler h)
		{
			h.HandleExitSelectingVendor();
		});
	}

	protected virtual void Close()
	{
		m_FadeAnimator.DisappearAnimation(delegate
		{
			Game.Instance.GameCommandQueue.CloseScreen(IScreenUIHandler.ScreenType.VendorSelecting, (Game.Instance.LoadedAreaState?.Settings.CapitalPartyMode).Value);
		});
	}

	private void DrawEntities()
	{
		m_WidgetList.Clear();
		m_WidgetList.DrawEntries(base.ViewModel.FactionItems.ToArray(), m_FactionReputationItemPCView);
	}
}
