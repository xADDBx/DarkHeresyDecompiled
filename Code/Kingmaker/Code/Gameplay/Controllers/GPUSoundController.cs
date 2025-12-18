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

public class GPUSoundController : IControllerTick, IController
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
			if (_soundEmitter != null)
			{
				PFLog.Audio.Log($"GPUSOUND: remove emitter {_soundEmitter.transform.position}");
				UnityEngine.Object.Destroy(_soundEmitter.gameObject);
			}
			int count = GetCount();
			if (count < 3)
			{
				PFLog.Audio.Log($"GPUSOUND: crowd is too small {count}");
				return;
			}
			SoundRoot.SoundEmitterBySizeCollection soundEmitterBySizeCollection = ConfigRoot.Instance.Sound.SoundEventsForCrowdTypes.FirstOrDefault((SoundRoot.SoundEventByCrowdType e) => e.CrowdType == GetCrowdType())?.soundEmitterCollection;
			if (soundEmitterBySizeCollection == null)
			{
				PFLog.Audio.Log($"GPUSOUND: no emittercollection for {GetCrowdType()}");
				return;
			}
			SoundFx soundFx = ((count < ConfigRoot.Instance.Sound.MinMediumCrowdSize) ? soundEmitterBySizeCollection.EmitterForSmall : ((count < ConfigRoot.Instance.Sound.MinHugeCrowdSize) ? soundEmitterBySizeCollection.EmitterForMedium : soundEmitterBySizeCollection.EmitterForLarge));
			if (soundFx == null)
			{
				PFLog.Audio.Log($"GPUSOUND: no emitter for size {count} in {GetCrowdType()}");
				return;
			}
			_soundEmitter = UnityEngine.Object.Instantiate(soundFx, GetCenter(), Quaternion.identity);
			_soundEmitter.transform.SetParent(FxHelper.FxRoot);
			PFLog.Audio.Log($"GPUSOUND: spawn emitter {_soundEmitter.transform.position}");
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
	}

	public void RegisterCrowd(GpuCrowd crowd)
	{
		PFLog.Audio.Log($"GPUSOUND: register crowd {crowd.name} with {crowd.CrowdSoundInfos.Count} locators");
		foreach (GpuCrowdSoundInfo crowdSoundInfo in crowd.CrowdSoundInfos)
		{
			if (crowdSoundInfo.ConsiderInSoundComputation)
			{
				AddLocator(crowdSoundInfo, crowd.CrowdType);
			}
		}
	}

	public void UnregisterCrowd(GpuCrowd crowd)
	{
		PFLog.Audio.Log($"GPUSOUND: unregister crowd {crowd.name} with {crowd.CrowdSoundInfos.Count} locators");
		foreach (GpuCrowdSoundInfo crowdSoundInfo in crowd.CrowdSoundInfos)
		{
			if (crowdSoundInfo.ConsiderInSoundComputation)
			{
				RemoveLocator(crowdSoundInfo);
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
		PFLog.Audio.Log($"GPUSOUND: register locator {crowdLocator.Position}");
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

	private void RemoveLocator(GpuCrowdSoundInfo crowdLocator)
	{
		Vector2Int cellCoords = GetCellCoords(crowdLocator.Position);
		LocatorInfo locatorInfo = CellToLocators[cellCoords].FirstOrDefault((LocatorInfo l) => l.locator.Position == crowdLocator.Position);
		if (locatorInfo != null)
		{
			PFLog.Audio.Log($"GPUSOUND: remove locator {crowdLocator.Position}");
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
