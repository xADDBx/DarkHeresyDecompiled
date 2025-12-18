using System;
using Kingmaker.ResourceLinks;
using Kingmaker.UI;
using Kingmaker.Utility.Attributes;
using Kingmaker.View.Equipment;
using Kingmaker.View.MapObjects;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.MaterialEffects.Dissolve;
using Owlcat.QA.Validation;
using Owlcat.Runtime.Core.Utility;
using UnityEngine;

namespace Kingmaker.Blueprints.Root;

[Serializable]
[TypeId("a14239b588256324da50c3fb6910e428")]
public class Prefabs : BlueprintScriptableObject
{
	public Mesh UnitCoreCollider;

	public float CoreColliderHeight = 0.7f;

	public float SecondaryColliderWidthCoeff = 1.3f;

	public GameObject PersonalEnemyFxPrefab;

	public DroppedLoot DroppedLootBag;

	public PrefabLink DroppedLootBagAttachedLink;

	public DroppedLoot BreathOfMoneyLootBag;

	[ValidateIsPrefab]
	[ValidateHasComponent(typeof(EquipmentOffsets))]
	public EquipmentOffsets DefaultConsumableOffsets;

	public DissolveSettings FogOfWarDissolveSettings;

	[AssetPicker("")]
	public UICamera UICamera;

	public CharacterBonesSetup CharacterBonesSetup;

	public GameObject DefaultInteractWithMeltaChargeFxPrefab;

	public GameObject StealthEffectPrefab;

	public GameObject ExitStealthEffectPrefab;

	public Texture2D DefaultDissolveTexture;

	public Texture2D DefaultCharWhiteMaskTexture;

	public Texture2D DefaultCharAnisotropyFallbackTexture;
}
