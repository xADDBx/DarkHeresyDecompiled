using System;
using System.Collections.Generic;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.ElementsSystem;
using Kingmaker.EntitySystem.Entities.Base;
using Kingmaker.View.MapObjects;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.DetectiveSystem.Servoskull;

[Serializable]
[KDB("Интеракт, который переведет указанные следы в состояние Found и позволит взаимодействовать с ними.")]
[RequireComponent(typeof(InteractionDetectiveTrace))]
[KnowledgeDatabaseID("d84242375ea04faf831b7df6b3fb50cb")]
public class DetectiveTraceRootView : MapObjectView
{
	public List<DetectiveTraceView> RootTraces = new List<DetectiveTraceView>();

	public override Entity CreateEntityData(bool load)
	{
		return Entity.Initialize(new DetectiveTraceRootEntity(UniqueId, base.IsInGameBySettings));
	}
}
