using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.Utility;
using Kingmaker.Utility.DotNetExtensions;

namespace Kingmaker.GameModes;

public static class GameModesFactory
{
	public static readonly List<ControllerData> AllControllers = new List<ControllerData>();

	[CanBeNull]
	public static T GetController<T>() where T : class, IController
	{
		foreach (ControllerData allController in AllControllers)
		{
			if (allController.Controller is T result)
			{
				return result;
			}
		}
		return null;
	}

	public static void Register(IController controller, params GameModeType[] gameModes)
	{
		ControllerData controllerData = AllControllers.FirstItem((ControllerData i) => i.Controller == controller);
		if (controllerData != null)
		{
			string[] array = (from i in controllerData.GameModes.Intersect(gameModes)
				select i.Name).ToArray();
			if (array.Length != 0)
			{
				string name = controller.GetType().Name;
				string text = string.Join(", ", array);
				PFLog.Default.Error("Can't register controller twice for same game mode: " + name + ", same game modes: " + text);
			}
			controllerData.SetGameModes(controllerData.GameModes.Union(gameModes));
		}
		else
		{
			AllControllers.Add(new ControllerData(controller, gameModes.ToArray()));
			Type type = controller.GetType();
			typeof(Indexer<>).MakeGenericType(type).GetField("Index", BindingFlags.Static | BindingFlags.Public)?.SetValue(null, AllControllers.Count - 1);
		}
	}

	public static GameMode Create(GameModeType type)
	{
		return new GameMode(type, AllControllers.Select((ControllerData controllerData) => (!controllerData.GameModes.HasItem(type) || controllerData.Controller == null) ? null : controllerData.Controller));
	}

	public static async Task Reset()
	{
		foreach (ControllerData allController in AllControllers)
		{
			IController controller = allController.Controller;
			if (!(controller is IAsyncDisposable asyncDisposable))
			{
				if (controller is IDisposable disposable)
				{
					disposable.Dispose();
				}
			}
			else
			{
				await asyncDisposable.DisposeAsync();
			}
		}
		AllControllers.Clear();
	}
}
