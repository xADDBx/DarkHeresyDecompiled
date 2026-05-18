using System;
using Kingmaker.Code.View.Bridge.Interfaces.Canvas;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.Code.View.Bridge.Interfaces;

public interface IRootUIContext : IBugReportContext
{
	ISelectionManager SelectionManager { get; }

	bool CanChangeInput { get; }

	bool HasMainMenuContext { get; }

	bool IsDebugBlueprintsInformationShow { get; }

	bool IsDebugStringsInformationShow { get; }

	bool IsCharGenShown { get; }

	bool IsInMainMenu { get; }

	GameObject SoundGameObject { get; }

	IMainCanvas MainCanvas { get; }

	object GetLoadingScreenVM();

	object GetFadeVM();

	void StartUI(bool showLoadingScreen = false);

	void ResetUI(Action onComplete = null);

	void CloseUI(bool taskIsInteract);

	void Clear();

	void SetLoadingArea(object area);

	void SaveLoadContextLoad(object saveInfo, Action callback);

	void SetLoadingScreenVM(object vm);

	void SwitchDebugBlueprintsInformationShow();

	bool GetDebugBlueprintsInformationShow();

	void SwitchDebugStringsInformationShow();

	bool GetDebugStringsInformationShow();

	void ChangeUIPlatform(bool nextPlatform);

	void EnterGame(Action action);

	void ClearForLoadMainMenu();

	void CreateLoadingScreen();

	bool CanChangeLanguage();

	void InitializeUIKitDependencies();

	void MaybeCollectLoot();

	ILogger CreateLogChannelLoggerWrapper(LogChannel channel, string tag);
}
