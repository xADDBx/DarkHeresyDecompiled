using System;
using Kingmaker.Blueprints;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Gameplay.Features.AoEPatterns.Blueprints;

[Serializable]
[TypeId("97977599bd244af88f94f2f82605c1b8")]
public sealed class AoEPatternRoot : BlueprintScriptableObject
{
	[Tooltip("Определяет какие клетки отсекаются по перепаду высот для НЕ Directional паттернов. Перепад высот считается относительно клетки применения паттерна.")]
	public float SameLevelDiff = 1.6f;

	[Tooltip("Определяет какие клетки отсекаются по перепаду высот для Directional паттернов. Перепад высот считается относительно луча из позиции кастера в позицию цели.")]
	public float RayConeThickness = 0.3f;
}
