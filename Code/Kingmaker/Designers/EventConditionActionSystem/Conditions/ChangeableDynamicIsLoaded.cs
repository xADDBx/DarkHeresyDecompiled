using System;
using System.Linq;
using Kingmaker.Blueprints.Area;
using Kingmaker.ElementsSystem;
using Owlcat.Runtime.Core.Utility;

namespace Kingmaker.Designers.EventConditionActionSystem.Conditions;

[Obsolete]
[TypeId("db2af32813d37d5408005f5087be64a4")]
public class ChangeableDynamicIsLoaded : Condition
{
	public SceneReference Scene;

	protected override string GetConditionCaption()
	{
		return $"Dynamic [{Scene.SceneName}] Is Loaded";
	}

	protected override bool CheckCondition()
	{
		if (Game.Instance.State.LoadedAreaState.Area.Blueprint.GetActiveDynamicScenes().Any((SceneReference s) => s.SceneName == Scene.SceneName))
		{
			return true;
		}
		return false;
	}
}
