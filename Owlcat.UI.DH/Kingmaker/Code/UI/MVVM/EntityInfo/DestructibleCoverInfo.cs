using UnityEngine;

namespace Kingmaker.Code.UI.MVVM.EntityInfo;

public class DestructibleCoverInfo : IEntityInfo
{
	public int MaxDurability;

	public int CurrentDurability;

	public Vector3 WorldPosition;
}
