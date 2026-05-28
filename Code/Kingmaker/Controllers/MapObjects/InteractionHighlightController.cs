using Core.Cheats;
using Kingmaker.Code.Framework.Settings.UISettings;
using Kingmaker.Code.Middleware.Metrics;
using Kingmaker.Code.View.Bridge.Enums;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.Mechanics.Entities;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.Settings;
using Kingmaker.Utility.GameConst;
using UnityEngine;

namespace Kingmaker.Controllers.MapObjects;

public class InteractionHighlightController : IControllerEnable, IController, IControllerDisable, IFullScreenUIHandler, ISubscriber
{
	private bool m_IsGlobalHighlighting;

	private bool m_IsHighlightToggleOn;

	private bool m_FullScreenUIOpen;

	private bool m_Inactive;

	private double _highlightStartTime;

	public static InteractionHighlightController Instance { get; private set; }

	public bool DebugHighlightCovers { get; private set; }

	public bool IsGlobalHighlighting => m_IsGlobalHighlighting;

	public void OnEnable()
	{
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.HighlightObjects.name + UIConsts.SuffixOn, OnHighlightKeyDown);
		Game.Instance.Keyboard.Bind(UISettingsRoot.Instance.UIKeybindGeneralSettings.HighlightObjects.name + UIConsts.SuffixOff, OnHighlightKeyUp);
		Instance = this;
		m_Inactive = false;
		EventBus.Subscribe(this);
		if (m_IsHighlightToggleOn && !m_FullScreenUIOpen)
		{
			HighlightOn();
		}
	}

	private void OnHighlightKeyDown()
	{
		if (SettingsRoot.Controls.HighlightObjectsMode.GetValue() == HighlightObjectsMode.Toggle)
		{
			SwitchHighlight();
			Metrics.Tab.Mode(HighlightObjectsMode.Toggle).ToggleState(m_IsHighlightToggleOn).Send();
		}
		else if (HighlightOn())
		{
			_highlightStartTime = Time.realtimeSinceStartupAsDouble;
		}
	}

	private void OnHighlightKeyUp()
	{
		if (SettingsRoot.Controls.HighlightObjectsMode.GetValue() != HighlightObjectsMode.Toggle && HighlightOff() && _highlightStartTime > 0.0)
		{
			Metrics.Tab.Mode(HighlightObjectsMode.Hold).Duration(Time.realtimeSinceStartupAsDouble - _highlightStartTime).Send();
			_highlightStartTime = 0.0;
		}
	}

	[Cheat(Name = "switch_highlight_covers", ExecutionPolicy = ExecutionPolicy.PlayMode)]
	public static void SwitchHighlightCovers()
	{
		Instance.DebugHighlightCovers = !Instance.DebugHighlightCovers;
		Instance.UpdateHighlightCovers();
	}

	private void UpdateHighlightCovers()
	{
		if (m_Inactive)
		{
			return;
		}
		foreach (MapObjectEntity mapObject in Game.Instance.EntityPools.MapObjects)
		{
			mapObject.View.UpdateHighlight();
		}
	}

	public void SwitchHighlight()
	{
		Highlight(!m_IsHighlightToggleOn);
	}

	public void Highlight(bool on)
	{
		m_IsHighlightToggleOn = on;
		if (on && !m_FullScreenUIOpen)
		{
			HighlightOn();
		}
		else
		{
			HighlightOff();
		}
	}

	void IFullScreenUIHandler.HandleFullScreenUiChanged(bool state, FullScreenUIType fullScreenUIType)
	{
		m_FullScreenUIOpen = state;
		if (state)
		{
			HighlightOff();
		}
		else if (m_IsHighlightToggleOn)
		{
			HighlightOn();
		}
	}

	private bool HighlightOn()
	{
		if (!m_IsGlobalHighlighting && !m_Inactive)
		{
			m_IsGlobalHighlighting = true;
			foreach (MapObjectEntity mapObject in Game.Instance.EntityPools.MapObjects)
			{
				mapObject.View?.SetGlobalHighlight(value: true);
			}
			foreach (AbstractUnitEntity allUnit in Game.Instance.EntityPools.AllUnits)
			{
				allUnit.View.UpdateHighlight(raiseEvent: false);
			}
			EventBus.RaiseEvent(delegate(IInteractionHighlightUIHandler h)
			{
				h.HandleHighlightChange(isOn: true);
			});
			return true;
		}
		return false;
	}

	private bool HighlightOff()
	{
		if (m_IsGlobalHighlighting && !m_Inactive)
		{
			m_IsGlobalHighlighting = false;
			foreach (MapObjectEntity mapObject in Game.Instance.EntityPools.MapObjects)
			{
				mapObject.View?.SetGlobalHighlight(value: false);
			}
			foreach (AbstractUnitEntity allUnit in Game.Instance.EntityPools.AllUnits)
			{
				allUnit.View.UpdateHighlight(raiseEvent: false);
			}
			EventBus.RaiseEvent(delegate(IInteractionHighlightUIHandler h)
			{
				h.HandleHighlightChange(isOn: false);
			});
			return true;
		}
		return false;
	}

	public void OnDisable()
	{
		Game.Instance.Keyboard.Unbind(UISettingsRoot.Instance.UIKeybindGeneralSettings.HighlightObjects.name + UIConsts.SuffixOn, OnHighlightKeyDown);
		Game.Instance.Keyboard.Unbind(UISettingsRoot.Instance.UIKeybindGeneralSettings.HighlightObjects.name + UIConsts.SuffixOff, OnHighlightKeyUp);
		EventBus.Unsubscribe(this);
		HighlightOff();
		m_Inactive = true;
		Instance = null;
	}
}
