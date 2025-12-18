using System.Collections.Generic;
using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public class AreaEffectEntityInfo : IEntityInfo
{
	public Vector3 WorldPosition;

	public IReadOnlyList<IEntityInfoDescription> Descriptions;
}
