using System;
using System.Collections.Generic;
using System.Linq;
using Kingmaker.Blueprints.Root;
using Kingmaker.Controllers.Interfaces;
using Kingmaker.GPUCrowd;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Utility.GeometryExtensions;
using Kingmaker.Visual.Particles;
using Kingmaker.Visual.Sound;
using UnityEngine;

namespace Kingmaker.Code.Gameplay.Controllers;

public class GPUSoundController : IControllerTick, IController, IControllerReset
{
	public enum CrowdType
	{
		TalkingLoud,
		Praying,
		Angry,
		Frightened,
		Happy,
		NormalTalking,
		QuiteTalking
	}

	[Serializable]
	public class Cluster
	{
		public HashSet<LocatorInfo> locators = new HashSet<LocatorInfo>();

		private SoundFx _soundEmitter;

		public void AddLocator(LocatorInfo locatorInfo)
		{
			locators.Add(locatorInfo);
		}

		public void RemoveLocator(LocatorInfo locatorInfo)
		{
			locators.Remove(locatorInfo);
		}

		public void MergeCluster(Cluster clusterToMerge)
		{
			foreach (LocatorInfo locator in clusterToMerge.locators)
			{
				locators.Add(locator);
				locator.containingCluster = this;
			}
			clusterToMerge.locators.Clear();
			clusterToMerge.UpdateSoundEmitter();
		}

		public Vector3 GetCenter()
		{
			Vector3 zero = Vector3.zero;
			foreach (LocatorInfo locator in locators)
			{
				zero += locator.locator.Position;
			}
			return zero / locators.Count;
		}

		public CrowdType GetCrowdType()
		{
			Dictionary<CrowdType, int> dictionary = new Dictionary<CrowdType, int>();
			foreach (LocatorInfo locator in locators)
			{
				if (dictionary.ContainsKey(locator.soundType))
				{
					dictionary[locator.soundType]++;
				}
				else
				{
					dictionary.Add(locator.soundType, 1);
				}
			}
			return dictionary.MaxBy((KeyValuePair<CrowdType, int> pair) => pair.Value).Key;
		}

		public int GetCount()
		{
			return locators.Count;
		}

		public void UpdateSoundEmitter()
		{
			CleanupEmitter();
			int count = GetCount();
			if (count < 3)
			{
				return;
			}
			SoundRoot.SoundEmitterBySizeCollection soundEmitterBySizeCollection = ConfigRoot.Instance.Sound.SoundEventsForCrowdTypes.FirstOrDefault((SoundRoot.SoundEventByCrowdType e) => e.CrowdType == GetCrowdType())?.soundEmitterCollection;
			if (soundEmitterBySizeCollection != null)
			{
				SoundFx soundFx = ((count < ConfigRoot.Instance.Sound.MinMediumCrowdSize) ? soundEmitterBySizeCollection.EmitterForSmall : ((count < ConfigRoot.Instance.Sound.MinHugeCrowdSize) ? soundEmitterBySizeCollection.EmitterForMedium : soundEmitterBySizeCollection.EmitterForLarge));
				if (!(soundFx == null))
				{
					_soundEmitter = UnityEngine.Object.Instantiate(soundFx, GetCenter(), Quaternion.identity);
					_soundEmitter.transform.SetParent(FxHelper.FxRoot);
				}
			}
		}

		public void CleanupEmitter()
		{
			if (_soundEmitter != null)
			{
				PFLog.Audio.Log($"GPUSOUND: remove emitter {_soundEmitter.transform.position}");
				UnityEngine.Object.Destroy(_soundEmitter.gameObject);
			}
		}
	}

	[Serializable]
	public class LocatorInfo
	{
		public GpuCrowdSoundInfo locator;

		public CrowdType soundType;

		public Cluster containingCluster;
	}

	private bool m_RequiresUpdate;

	private List<Cluster> ExpandedCluster = new List<Cluster>();

	private List<Cluster> ContractedCluster = new List<Cluster>();

	private Dictionary<Vector2Int, List<LocatorInfo>> CellToLocators = new Dictionary<Vector2Int, List<LocatorInfo>>();

	public static float mergeDistance => ConfigRoot.Instance.Sound.MergeDistance;

	public float CellSize => mergeDistance / 1.45f;

	public TickType GetTickType()
	{
		return TickType.EndOfFrame;
	}

	public void Tick()
	{
		if (!m_RequiresUpdate)
		{
			return;
		}
		m_RequiresUpdate = false;
		foreach (Cluster item in ExpandedCluster)
		{
			item.UpdateSoundEmitter();
		}
		ExpandedCluster.Clear();
		List<LocatorInfo> list = new List<LocatorInfo>();
		foreach (Cluster item2 in ContractedCluster)
		{
			if (item2.GetCount() != 0)
			{
				list.AddRange(item2.locators);
			}
		}
		foreach (LocatorInfo item3 in list)
		{
			item3.containingCluster.RemoveLocator(item3);
			Vector2Int cellCoords = GetCellCoords(item3.locator.Position);
			CellToLocators[cellCoords].Remove(item3);
			if (CellToLocators[cellCoords].Count == 0)
			{
				CellToLocators.Remove(cellCoords);
			}
			item3.containingCluster = null;
		}
		foreach (Cluster item4 in ContractedCluster)
		{
			item4?.UpdateSoundEmitter();
		}
		ContractedCluster.Clear();
		foreach (LocatorInfo item5 in list)
		{
			AddLocatorInfo(item5);
		}
		LogClusteringResult();
	}

	private void LogClusteringResult()
	{
		HashSet<Cluster> hashSet = new HashSet<Cluster>();
		int num = 0;
		foreach (List<LocatorInfo> value in CellToLocators.Values)
		{
			foreach (LocatorInfo item in value)
			{
				num++;
				if (item.containingCluster != null)
				{
					hashSet.Add(item.containingCluster);
				}
			}
		}
		int minMediumCrowdSize = ConfigRoot.Instance.Sound.MinMediumCrowdSize;
		int minHugeCrowdSize = ConfigRoot.Instance.Sound.MinHugeCrowdSize;
		int num2 = 0;
		int num3 = 0;
		foreach (Cluster item2 in hashSet.OrderByDescending((Cluster c) => c.GetCount()))
		{
			int count = item2.GetCount();
			if (count < 3)
			{
				num3++;
				continue;
			}
			if (count >= minMediumCrowdSize)
			{
			}
			item2.GetCenter();
			item2.GetCrowdType();
			num2++;
		}
	}

	public void OnReset()
	{
		foreach (Cluster item in ExpandedCluster)
		{
			item?.CleanupEmitter();
		}
		ExpandedCluster.Clear();
		foreach (Cluster item2 in ContractedCluster)
		{
			item2?.CleanupEmitter();
		}
		ContractedCluster.Clear();
	}

	public void RegisterCrowd(GpuCrowd crowd)
	{
		int num = 0;
		int num2 = 0;
		foreach (GpuCrowdSoundInfo crowdSoundInfo in crowd.CrowdSoundInfos)
		{
			if (crowdSoundInfo.ConsiderInSoundComputation)
			{
				num++;
			}
			else
			{
				num2++;
			}
		}
		foreach (GpuCrowdSoundInfo crowdSoundInfo2 in crowd.CrowdSoundInfos)
		{
			if (crowdSoundInfo2.ConsiderInSoundComputation)
			{
				AddLocator(crowdSoundInfo2, crowd.CrowdType);
			}
		}
	}

	public void UnregisterCrowd(GpuCrowd crowd)
	{
		crowd.CrowdSoundInfos.Count((GpuCrowdSoundInfo l) => l.ConsiderInSoundComputation);
		foreach (GpuCrowdSoundInfo crowdSoundInfo in crowd.CrowdSoundInfos)
		{
			if (crowdSoundInfo.ConsiderInSoundComputation)
			{
				RemoveLocator(crowdSoundInfo, crowd.CrowdType);
			}
		}
	}

	private void AddLocator(GpuCrowdSoundInfo crowdLocator, CrowdType crowdType)
	{
		LocatorInfo newLocator = new LocatorInfo
		{
			locator = crowdLocator,
			soundType = crowdType,
			containingCluster = null
		};
		AddLocatorInfo(newLocator);
	}

	private void AddLocatorInfo(LocatorInfo newLocator)
	{
		Vector2Int cellCoords = GetCellCoords(newLocator.locator.Position);
		if (CellToLocators.ContainsKey(cellCoords))
		{
			newLocator.containingCluster = CellToLocators[cellCoords].First().containingCluster;
			CellToLocators[cellCoords].Add(newLocator);
		}
		else
		{
			newLocator.containingCluster = new Cluster();
			CellToLocators.Add(cellCoords, new List<LocatorInfo> { newLocator });
		}
		newLocator.containingCluster.AddLocator(newLocator);
		ExpandedCluster.AddUnique(newLocator.containingCluster);
		m_RequiresUpdate = true;
		Vector2Int[] adjacentCells = GetAdjacentCells(cellCoords);
		foreach (Vector2Int key in adjacentCells)
		{
			if (!CellToLocators.ContainsKey(key) || CellToLocators[key].First().containingCluster == newLocator.containingCluster)
			{
				continue;
			}
			foreach (LocatorInfo item in CellToLocators[key])
			{
				if (!(Vector3.Distance(newLocator.locator.Position.ReplaceY(0f), item.locator.Position.ReplaceY(0f)) > mergeDistance))
				{
					Cluster containingCluster = item.containingCluster;
					newLocator.containingCluster.GetCount();
					containingCluster.GetCount();
					if (containingCluster.GetCount() > newLocator.containingCluster.GetCount())
					{
						containingCluster.MergeCluster(newLocator.containingCluster);
						ExpandedCluster.AddUnique(containingCluster);
					}
					else
					{
						newLocator.containingCluster.MergeCluster(containingCluster);
						ExpandedCluster.AddUnique(newLocator.containingCluster);
					}
					m_RequiresUpdate = true;
					break;
				}
			}
		}
	}

	private void RemoveLocator(GpuCrowdSoundInfo crowdLocator, CrowdType type)
	{
		Vector2Int cellCoords = GetCellCoords(crowdLocator.Position);
		if (!CellToLocators.ContainsKey(cellCoords))
		{
			return;
		}
		LocatorInfo locatorInfo = CellToLocators[cellCoords].FirstOrDefault((LocatorInfo l) => l.locator.Position == crowdLocator.Position && l.soundType == type);
		if (locatorInfo != null)
		{
			locatorInfo.containingCluster.GetCount();
			locatorInfo.containingCluster.RemoveLocator(locatorInfo);
			CellToLocators[cellCoords].Remove(locatorInfo);
			if (CellToLocators[cellCoords].Count == 0)
			{
				CellToLocators.Remove(cellCoords);
			}
			ContractedCluster.AddUnique(locatorInfo.containingCluster);
			m_RequiresUpdate = true;
		}
	}

	private Vector2Int GetCellCoords(Vector3 position)
	{
		return new Vector2Int(Mathf.FloorToInt(position.x / CellSize), Mathf.FloorToInt(position.z / CellSize));
	}

	private Vector2Int[] GetAdjacentCells(Vector2Int cell)
	{
		return new Vector2Int[8]
		{
			cell + Vector2Int.up,
			cell + Vector2Int.up + Vector2Int.right,
			cell + Vector2Int.right,
			cell + Vector2Int.down + Vector2Int.right,
			cell + Vector2Int.down,
			cell + Vector2Int.down + Vector2Int.left,
			cell + Vector2Int.left,
			cell + Vector2Int.up + Vector2Int.left
		};
	}
}
