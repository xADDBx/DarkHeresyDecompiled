using System;
using Kingmaker.Blueprints.Attributes;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.Animation.Decorators;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.AreaLogic.Cutscenes.Commands;

[Serializable]
[ComponentName("Command/CommandControlAnimationDecorator")]
[TypeId("de6681515542433fb5c43c0c19d74b6d")]
public sealed class CommandControlAnimationDecorator : CommandBase
{
	private class Data
	{
		public bool IsFinished;

		public IDecoratorVisibilityRequest[] Requests;
	}

	[Serializable]
	private class Settings
	{
		public UnitAnimationDecoratorObject Decorator;

		public IDecoratorVisibilityRequest.RequestType Visibility;

		public string GetCaption()
		{
			return $"{Visibility} {Decorator.name}";
		}
	}

	[SerializeField]
	[ValidateNotNull]
	[SerializeReference]
	private AbstractUnitEvaluator m_Unit;

	[SerializeField]
	private Settings[] m_DecoratorSettings;

	[SerializeField]
	private bool m_IsContinuous;

	[SerializeField]
	[HideIf("m_IsContinuous")]
	private float m_Duration;

	public override bool IsContinuous => m_IsContinuous;

	protected override CommandResult OnRun(CutscenePlayerData player, bool skipping)
	{
		Data commandData = player.GetCommandData<Data>(this);
		commandData.IsFinished = false;
		commandData.Requests = new IDecoratorVisibilityRequest[m_DecoratorSettings.Length];
		UnitAnimationDecoratorManager decoratorManager = m_Unit.GetValue().AnimationManager.DecoratorManager;
		for (int i = 0; i < m_DecoratorSettings.Length; i++)
		{
			Settings settings = m_DecoratorSettings[i];
			commandData.Requests[i] = ((settings.Visibility == IDecoratorVisibilityRequest.RequestType.Show) ? decoratorManager.ShowDecorator(settings.Decorator, this) : decoratorManager.HideDecorator(settings.Decorator, this));
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSetTime(double time, CutscenePlayerData player)
	{
		if (IsContinuous)
		{
			return CommandResult.Success;
		}
		if (time > (double)m_Duration)
		{
			return Finish(player);
		}
		return CommandResult.Success;
	}

	protected override CommandResult OnSkip(CutscenePlayerData player)
	{
		return Finish(player);
	}

	protected override CommandResult OnStop(CutscenePlayerData player)
	{
		return Finish(player);
	}

	public override CommandResult Interrupt(CutscenePlayerData player)
	{
		return CommandResult.Success;
	}

	public override bool IsFinished(CutscenePlayerData player)
	{
		return player.GetCommandData<Data>(this).IsFinished;
	}

	private CommandResult Finish(CutscenePlayerData player)
	{
		Data commandData = player.GetCommandData<Data>(this);
		IDecoratorVisibilityRequest[] array = commandData.Requests.EmptyIfNull();
		for (int i = 0; i < array.Length; i++)
		{
			array[i].Release();
		}
		commandData.IsFinished = true;
		return CommandResult.Success;
	}

	public override string GetCaption()
	{
		if (m_DecoratorSettings == null || m_DecoratorSettings.Length == 0)
		{
			return "none";
		}
		Settings settings = m_DecoratorSettings.FirstOrDefault();
		return m_Unit?.GetCaptionShort() + $": <b>{settings.Visibility}</b> {settings.Decorator.name}" + ((m_DecoratorSettings.Length > 1) ? (" and " + (m_DecoratorSettings.Length - 1) + " more") : "");
	}
}
