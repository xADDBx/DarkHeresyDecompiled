using System;
using System.Linq;
using Code.View.UI.UIUtils;
using Kingmaker.Blueprints.Base;
using Kingmaker.Blueprints.Root.Strings;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Code.View.Bridge.OBSOLETE;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.Settings;
using Owlcat.UI;
using R3;

namespace Kingmaker.Code.UI.MVVM;

public class GameOverVM : ViewModel
{
	private readonly ReactiveProperty<string> m_Reason = new ReactiveProperty<string>(string.Empty);

	private readonly ReactiveProperty<bool> m_CanQuickLoad = new ReactiveProperty<bool>(value: false);

	public ReadOnlyReactiveProperty<string> Reason => m_Reason;

	public ReadOnlyReactiveProperty<bool> CanQuickLoad => m_CanQuickLoad;

	private SaveManager SaveManager => Game.Instance.SaveManager;

	public bool IsIronMan => SettingsRoot.Difficulty.OnlyOneSave;

	public bool HasDowngradedIronManSave => SaveManager.HasDowngradedIronManSave;

	public GameOverVM()
	{
		m_Reason.Value = GetReasonString();
		MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
		{
			m_CanQuickLoad.Value = SaveManager.GetLatestSave() != null;
		}));
	}

	private string GetReasonString()
	{
		string result = string.Empty;
		switch (Game.Instance.Player.GameOverReason)
		{
		case Player.GameOverReasonType.PartyIsDefeated:
			result = UIGameOverScreen.Instance.PartyIsDefeatedLabel;
			break;
		case Player.GameOverReasonType.EssentialUnitIsDead:
		{
			BaseUnitEntity baseUnitEntity = Game.Instance.EntityPools.AllBaseUnits.FirstOrDefault((BaseUnitEntity c) => c.LifeState.IsFinallyDead && c.IsEssentialForGame);
			result = ((baseUnitEntity != null) ? string.Format((baseUnitEntity.Gender == Gender.Female) ? UIGameOverScreen.Instance.FemaleDeadLabel : UIGameOverScreen.Instance.MaleDeadLabel, baseUnitEntity.CharacterName) : ((string)UIGameOverScreen.Instance.PartyIsDefeatedLabel));
			break;
		}
		case Player.GameOverReasonType.KingdomIsDestroyed:
			result = UIGameOverScreen.Instance.PartyIsDefeatedLabel;
			break;
		case Player.GameOverReasonType.QuestFailed:
			result = UIGameOverScreen.Instance.QuestIsFailedLabel;
			break;
		default:
			throw new ArgumentOutOfRangeException();
		case Player.GameOverReasonType.Won:
			break;
		}
		return result;
	}

	public void OnButtonLoadGame()
	{
		EventBus.RaiseEvent(delegate(ISaveLoadUIHandler h)
		{
			h.HandleOpenSaveLoad(SaveLoadMode.Load, singleMode: true);
		});
	}

	public void OnQuickLoad()
	{
		MainThreadDispatcher.StartCoroutine(UIUtilitySaves.WaitForSaveUpdated(delegate
		{
			Game.Instance.LoadGame(SaveManager.GetLatestSave());
		}));
	}

	public void OnButtonMainMenu()
	{
		Game.Instance.ResetToMainMenu();
	}

	public void OnIronManDeleteSave()
	{
		if (HasDowngradedIronManSave)
		{
			SaveManager.DeleteDowngradedIronManSave();
		}
		OnButtonMainMenu();
	}

	public void OnIronManContinueGame()
	{
		if (HasDowngradedIronManSave)
		{
			SaveManager.LoadDowngradedIronManSave();
		}
	}
}
