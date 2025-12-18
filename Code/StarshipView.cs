using System.Collections.Generic;
using Kingmaker.EntitySystem.Interfaces;
using Kingmaker.Items;
using Kingmaker.Items.Slots;
using Kingmaker.PubSubSystem;
using Kingmaker.PubSubSystem.Core;
using Kingmaker.PubSubSystem.Core.Interfaces;
using Kingmaker.View;
using Kingmaker.Visual.Particles;
using UnityEngine;
using UnityEngine.VFX;

public class StarshipView : MonoBehaviour, IEntitySubscriber, IUnitEquipmentHandler<EntitySubscriber>, IUnitEquipmentHandler, ISubscriber<IMechanicEntity>, ISubscriber, IEventTag<IUnitEquipmentHandler, EntitySubscriber>
{
	public UnitEntityView UnitEntityView;

	public ParticlesSnapMap particleSnapMap;

	[Tooltip("Front red, right blue, back yellow, left green")]
	public StarshipFxHitMask starshipFxHitMask;

	public Mesh mesh;

	public Renderer BaseRenderer;

	public Renderer StarMapVisualRenderer;

	public List<StarshipItemSlot> ItemSlots = new List<StarshipItemSlot>();

	private bool IsPlasmaDriveInit;

	private VisualEffect[] m_DriveVisualEffects;

	private UnitMovementAgentBase m_UnitMovementAgent;

	private bool m_HasMovementAgent;

	private void Start()
	{
		SetAllEquipment();
		m_DriveVisualEffects = GetComponentsInChildren<VisualEffect>();
		m_UnitMovementAgent = GetComponentInParent<UnitMovementAgentBase>();
		m_HasMovementAgent = m_UnitMovementAgent != null;
	}

	private void OnEnable()
	{
		EventBus.Subscribe(this);
	}

	private void Update()
	{
		if (!IsPlasmaDriveInit)
		{
			ReinitPlasmaDriveVFX();
		}
		if (!m_HasMovementAgent || m_DriveVisualEffects.Length == 0)
		{
			return;
		}
		VisualEffect[] driveVisualEffects = m_DriveVisualEffects;
		foreach (VisualEffect visualEffect in driveVisualEffects)
		{
			if (visualEffect != null && visualEffect.HasFloat("Intencity"))
			{
				visualEffect.SetFloat("Intencity", Mathf.Clamp01(m_UnitMovementAgent.SpeedIndicator));
			}
		}
	}

	public Mesh FindMesh()
	{
		if (mesh != null)
		{
			return mesh;
		}
		mesh = base.gameObject.GetComponent<MeshFilter>().sharedMesh;
		if (mesh == null)
		{
			return null;
		}
		return mesh;
	}

	public void SetAllEquipment()
	{
	}

	public void HandleEquipmentSlotUpdated(ItemSlot slot, ItemEntity previousItem)
	{
		if (slot.HasItem)
		{
			EquipItemFromItemSlot(slot.Item, isEquip: true);
		}
		else
		{
			EquipItemFromItemSlot(previousItem, isEquip: false);
		}
	}

	private void EquipItemFromItemSlot(ItemEntity itemSlot, bool isEquip)
	{
	}

	private void EquipItem(List<StarshipSlotDescription> artSlots, bool isEquip)
	{
		foreach (StarshipSlotDescription artSlot in artSlots)
		{
			GameObject prefab = artSlot.Prefab;
			foreach (RequiredSlotVariant requiredSlots in artSlot.RequiredSlots)
			{
				List<StarshipItemSlot> list = ItemSlots.FindAll((StarshipItemSlot x) => x.Type == requiredSlots.SlotType);
				foreach (StarshipItemSlot item in list)
				{
					if (item.itemPrefab != null)
					{
						Object.Destroy(item.itemPrefab);
					}
				}
				if (!isEquip)
				{
					continue;
				}
				foreach (StarshipItemSlot item2 in list)
				{
					(item2.itemPrefab = Object.Instantiate(prefab, item2.transform.position, Quaternion.identity, item2.transform)).transform.localEulerAngles = Vector3.zero;
				}
			}
		}
	}

	public void OnDisable()
	{
		EventBus.Unsubscribe(this);
	}

	public void FillItemsSlots()
	{
	}

	public void OnDrawGizmosSelected()
	{
		if (starshipFxHitMask.frontHitPositions.Count > 0)
		{
			Gizmos.color = Color.red;
			foreach (Vector3 frontHitPosition in starshipFxHitMask.frontHitPositions)
			{
				Gizmos.DrawSphere(base.transform.TransformPoint(frontHitPosition), 0.01f);
			}
		}
		if (starshipFxHitMask.leftHitPositions.Count > 0)
		{
			Gizmos.color = Color.green;
			foreach (Vector3 leftHitPosition in starshipFxHitMask.leftHitPositions)
			{
				Gizmos.DrawSphere(base.transform.TransformPoint(leftHitPosition), 0.01f);
			}
		}
		if (starshipFxHitMask.rightHitPositions.Count > 0)
		{
			Gizmos.color = Color.blue;
			foreach (Vector3 rightHitPosition in starshipFxHitMask.rightHitPositions)
			{
				Gizmos.DrawSphere(base.transform.TransformPoint(rightHitPosition), 0.01f);
			}
		}
		if (starshipFxHitMask.backHitPositions.Count <= 0)
		{
			return;
		}
		Gizmos.color = Color.yellow;
		foreach (Vector3 backHitPosition in starshipFxHitMask.backHitPositions)
		{
			Gizmos.DrawSphere(base.transform.TransformPoint(backHitPosition), 0.01f);
		}
	}

	public void ReinitPlasmaDriveVFX()
	{
		if (m_DriveVisualEffects == null || m_DriveVisualEffects.Length == 0)
		{
			return;
		}
		VisualEffect[] driveVisualEffects = m_DriveVisualEffects;
		foreach (VisualEffect visualEffect in driveVisualEffects)
		{
			if (!(visualEffect == null))
			{
				visualEffect.gameObject.SetActive(value: false);
				visualEffect.gameObject.SetActive(value: true);
			}
		}
		IsPlasmaDriveInit = true;
	}

	public IEntity GetSubscribingEntity()
	{
		return UnitEntityView.Data;
	}
}
