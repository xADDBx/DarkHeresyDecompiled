using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.DialogSystem.Blueprints;
using Owlcat.Runtime.Core.Logging;
using UnityEngine;

namespace Kingmaker.DialogSystem;

public static class DialogDebug
{
	public class DebugMessage
	{
		[NotNull]
		public readonly BlueprintScriptableObject Blueprint;

		[NotNull]
		public readonly string Message;

		public readonly Color Color;

		public DebugMessage(BlueprintScriptableObject blueprint, string message, Color color)
		{
			Blueprint = blueprint;
			Message = message;
			Color = color;
		}
	}

	private static readonly LogChannel Logger = LogChannelFactory.GetOrCreate("DialogDebug");

	public static readonly Dictionary<string, List<DebugMessage>> DebugMessages = new Dictionary<string, List<DebugMessage>>();

	public static BlueprintDialog Dialog;

	public static void Add([CanBeNull] BlueprintScriptableObject blueprint, [NotNull] string message, Color color)
	{
		Add(blueprint, color, message);
	}

	public static void Add([CanBeNull] BlueprintScriptableObject blueprint, Color color, [NotNull] string messageFormat, params object[] @params)
	{
		string message = string.Format(messageFormat, @params);
		if (Application.isEditor || !Application.isPlaying)
		{
			if (blueprint == null)
			{
				return;
			}
			if (!DebugMessages.TryGetValue(blueprint.AssetGuid, out var value))
			{
				value = new List<DebugMessage>();
				DebugMessages[blueprint.AssetGuid] = value;
			}
			else if (value.Any((DebugMessage m) => m.Blueprint == blueprint && m.Message == message && m.Color == color))
			{
				return;
			}
			value.Add(new DebugMessage(blueprint, message, color));
		}
		Logger.Log(blueprint, $"Dialog {blueprint}: {message}");
		if (Application.isPlaying)
		{
			GameHistoryLog.Instance.DialogEvent(blueprint, message);
		}
	}

	[StringFormatMethod("messageFormat")]
	public static void AddCondition([CanBeNull] BlueprintScriptableObject blueprint, bool result, [NotNull] string messageFormat, params object[] @params)
	{
		Color color = (result ? Color.green : Color.red);
		Add(blueprint, color, messageFormat, @params);
	}

	public static void Add([CanBeNull] BlueprintScriptableObject blueprint, [NotNull] string message)
	{
	}

	public static void Init(BlueprintDialog dialog)
	{
	}

	public static void Clear([CanBeNull] BlueprintScriptableObject blueprint)
	{
		if (blueprint != null && DebugMessages.TryGetValue(blueprint.AssetGuid, out var value))
		{
			value.Clear();
		}
	}
}
