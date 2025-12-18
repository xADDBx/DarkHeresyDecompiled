using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.LowLevel;

namespace Owlcat.Runtime.Visual.XPBD.Utilities;

public static class PlayerLoopHelper
{
	public static void Insert(Type parentSystemType, Type systemType, PlayerLoopSystem.UpdateFunction updateDelegate, int index = -1)
	{
		if (parentSystemType == null)
		{
			throw new ArgumentNullException("parentSystemType");
		}
		if (systemType == null)
		{
			throw new ArgumentNullException("systemType");
		}
		if (updateDelegate == null)
		{
			throw new ArgumentNullException("updateDelegate");
		}
		PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
		int num = Array.FindIndex(currentPlayerLoop.subSystemList, (PlayerLoopSystem s) => s.type == parentSystemType);
		if (num >= 0)
		{
			List<PlayerLoopSystem> list = currentPlayerLoop.subSystemList[num].subSystemList.ToList();
			PlayerLoopSystem item = default(PlayerLoopSystem);
			item.type = systemType;
			item.updateDelegate = updateDelegate;
			if (index >= 0)
			{
				list.Insert(index, item);
			}
			else
			{
				list.Add(item);
			}
			currentPlayerLoop.subSystemList[num].subSystemList = list.ToArray();
			PlayerLoop.SetPlayerLoop(currentPlayerLoop);
		}
	}

	public static void Remove(Type parentSystemType, Type systemType, PlayerLoopSystem.UpdateFunction updateDelegate)
	{
		if (parentSystemType == null)
		{
			throw new ArgumentNullException("parentSystemType");
		}
		if (systemType == null)
		{
			throw new ArgumentNullException("systemType");
		}
		if (updateDelegate == null)
		{
			throw new ArgumentNullException("updateDelegate");
		}
		PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
		int num = Array.FindIndex(currentPlayerLoop.subSystemList, (PlayerLoopSystem s) => s.type == parentSystemType);
		if (num >= 0)
		{
			List<PlayerLoopSystem> list = currentPlayerLoop.subSystemList[num].subSystemList.ToList();
			int num2 = list.FindIndex((PlayerLoopSystem s) => s.type == systemType && s.updateDelegate == updateDelegate);
			if (num2 >= 0)
			{
				list.RemoveAt(num2);
			}
			currentPlayerLoop.subSystemList[num].subSystemList = list.ToArray();
			PlayerLoop.SetPlayerLoop(currentPlayerLoop);
		}
	}
}
