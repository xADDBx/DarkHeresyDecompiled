using System;
using JetBrains.Annotations;
using Kingmaker.Blueprints;
using Kingmaker.ElementsSystem.ContextData;
using Kingmaker.EntitySystem.Entities;
using Kingmaker.UnitLogic;
using UnityEngine.SceneManagement;

namespace Kingmaker.Code.View.Bridge.Data;

public class ChargenUnit : IDisposable
{
	public readonly BlueprintUnit Blueprint;

	public BaseUnitEntity Unit;

	public bool Used;

	public ChargenUnit([NotNull] BlueprintUnit blueprint)
	{
		Blueprint = blueprint;
		RecreateUnit();
	}

	public void RecreateUnit()
	{
		Dispose();
		using (ContextData<UnitHelper.ChargenUnit>.Request())
		{
			Unit = Blueprint.CreateEntity();
		}
		Unit.AttachToViewOnLoad(null);
		SceneManager.MoveGameObjectToScene(Unit.View.gameObject, SceneManager.GetSceneByName("UI_Common_Scene"));
		Used = false;
	}

	public void Dispose()
	{
		BaseUnitEntity unit = Unit;
		if (unit != null && !unit.IsDisposed)
		{
			Unit.Dispose();
		}
	}
}
