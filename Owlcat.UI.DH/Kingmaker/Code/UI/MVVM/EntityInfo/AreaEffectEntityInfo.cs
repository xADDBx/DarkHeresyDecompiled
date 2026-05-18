using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public class AreaEffectEntityInfo : IEntityInfo
{
	public string Name;

	public Color? NameColor;

	public Vector3 WorldPosition;

	public IReadOnlyList<IEntityInfoDescription> Descriptions;
}
