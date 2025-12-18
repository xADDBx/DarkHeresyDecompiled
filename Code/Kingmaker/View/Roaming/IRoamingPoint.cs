using System;
using JetBrains.Annotations;
using Kingmaker.Code.Framework.CutsceneSystem;
using Kingmaker.Utility.StatefulRandom;
using OwlPack.Runtime;
using UnityEngine;

namespace Kingmaker.View.Roaming;

[OwlPackable(OwlPackableMode.Generate)]
public interface IRoamingPoint : IOwlPackable, IOwlPackable<IRoamingPoint>
{
	Vector3 Position { get; }

	float? Orientation { get; }

	TimeSpan SelectIdleTime(StatefulRandom random);

	[CanBeNull]
	BlueprintCutscene SelectCutscene(StatefulRandom random);

	[CanBeNull]
	IRoamingPoint SelectNextPoint(StatefulRandom random);

	[CanBeNull]
	IRoamingPoint SelectPrevPoint(StatefulRandom random);
}
