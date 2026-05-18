using System;
using System.Collections.Generic;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.JsonSystem.Helpers;
using Kingmaker.Code.View.Visual.CharacterSystem;
using Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;
using Kingmaker.ElementsSystem;
using Kingmaker.Utility.Attributes;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Visual.Highlighting;
using Owlcat.Runtime.Visual.OccludedObjectHighlighting;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;
using UnityEngine;

namespace Kingmaker.Visual.CharacterSystem;

[Serializable]
[KnowledgeDatabaseID("45241611d15846cdb63f60a3131403b1")]
public class OutfitPart : IEquipmentFeatureProvider
{
	[SerializeField]
	private OutfitPartType m_Type;

	[KDB("Prefab to spawn. DO NOT USE FBX OR OTHER MESH HERE. ONLY PREFAB.")]
	[SerializeField]
	private GameObject m_Prefab;

	[KDB("Material to apply on spawn. If empty - material from prefab will be used.")]
	[SerializeField]
	private Material m_Material;

	[KDB("R - first layer, G - second layer. B & A NOT in use.")]
	[SerializeField]
	public Texture2D ColorizeMask;

	[SerializeField]
	private bool m_ModifyTransform;

	[KDB("Local position which will be set on spawn")]
	[ShowIf("m_ModifyTransform")]
	[SerializeField]
	private Vector3 m_Position;

	[KDB("Local rotation which will be set on spawn")]
	[ShowIf("m_ModifyTransform")]
	[SerializeField]
	private Vector3 m_Rotation;

	[KDB("Local scale which will be set on spawn")]
	[ShowIf("m_ModifyTransform")]
	[SerializeField]
	private Vector3 m_Scale = Vector3.one;

	[KDB("Will attach itself to this bone in character bones hierarchy")]
	[SerializeField]
	private Skeleton.BoneType m_Bone;

	[KDB("Геймплейные особенности предмета")]
	[SerializeField]
	private BlueprintEquipmentFeatureReference[] m_Features;

	private IEquipmentFeatureProvider m_EquipmentFeatureProvider;

	private IEquipmentFeatureProvider Features => m_EquipmentFeatureProvider ?? (m_EquipmentFeatureProvider = new EquipmentFeatureProvider(m_Features));

	public OutfitPartType Type => m_Type;

	public GameObject Prefab => m_Prefab;

	public Material Material => m_Material;

	public Vector3 Position => m_Position;

	public Vector3 Rotation => m_Rotation;

	public Vector3 Scale => m_Scale;

	public Renderer[] SetupMaterialsInNewOutfitPart(GameObject newOutfitPart)
	{
		Renderer[] componentsInChildren = newOutfitPart.GetComponentsInChildren<Renderer>();
		if (componentsInChildren.Length == 0)
		{
			return null;
		}
		Renderer[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].sharedMaterial = Material;
		}
		return componentsInChildren;
	}

	public CharacterBuilder.OutfitPartInfo Attach(Transform root, Dictionary<string, Transform> attachBonesCache, EquipmentEntity sourceEe)
	{
		Transform value;
		Transform transform = (attachBonesCache.TryGetValue(m_Bone.BoneName, out value) ? value : root.FindChildRecursive(m_Bone.BoneName));
		if (!transform)
		{
			PFLog.TechArt.Error("Can't find bone with name " + m_Bone.ToString() + " in transform " + root.name);
			return null;
		}
		if (!Prefab)
		{
			PFLog.TechArt.Error("No prefab link in equipment entity, nothing to Instantiate");
			return null;
		}
		GameObject gameObject = UnityEngine.Object.Instantiate(Prefab, transform, worldPositionStays: false);
		gameObject.name = Prefab.name;
		if ((bool)Material)
		{
			SetupMaterialsInNewOutfitPart(gameObject);
		}
		if (m_ModifyTransform)
		{
			gameObject.transform.localPosition = m_Position;
			gameObject.transform.localScale = m_Scale;
			gameObject.transform.localRotation = Quaternion.Euler(m_Rotation);
		}
		GameObject physicsMasterMesh = SetupPhysicsBody(gameObject, transform, root);
		return new CharacterBuilder.OutfitPartInfo(this, gameObject, transform, physicsMasterMesh, sourceEe);
	}

	private GameObject SetupPhysicsBody(GameObject outfitObj, Transform bone, Transform root)
	{
		if (Features.TryGetPhysicsDeformerLayout(out var triangleSkinmap, out var prefabMesh))
		{
			MeshBody component = outfitObj.GetComponent<MeshBody>();
			if (component != null)
			{
				PFLog.TechArt.Error($"{outfitObj} contains MeshBody & Mesh Deformer feature. We don't use both in simulation. Please remove MeshBody from prefab");
				UnityEngine.Object.Destroy(component);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(prefabMesh, bone, worldPositionStays: false);
			gameObject.name = "[Physics] Master Body " + prefabMesh.name;
			if (m_ModifyTransform)
			{
				gameObject.transform.localPosition = m_Position;
				gameObject.transform.localScale = m_Scale;
				gameObject.transform.localRotation = Quaternion.Euler(m_Rotation);
			}
			MeshBodyBase meshBodyBase = null;
			SkinnedMeshRenderer componentInChildren = gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
			if (componentInChildren != null)
			{
				gameObject.transform.parent = root;
				meshBodyBase = componentInChildren.GetComponent<SkinnedMeshBody>();
				if (meshBodyBase == null)
				{
					PFLog.TechArt.Error($"{outfitObj} contains no SkinnedMeshBody setup. Add default layout without constrains");
					meshBodyBase = gameObject.AddComponent<SkinnedMeshBody>();
				}
				List<Transform> list = new List<Transform>();
				Transform[] bones = componentInChildren.bones;
				foreach (Transform transform in bones)
				{
					Transform transform2 = Character.FindBone(root, transform.name);
					if (transform2 == null)
					{
						PFLog.TechArt.Error("Bone for outfit physics mesh not found inside character " + transform.name);
					}
					else
					{
						list.Add(transform2);
					}
				}
				if (componentInChildren.rootBone != null)
				{
					UnityEngine.Object.Destroy(componentInChildren.rootBone.gameObject);
				}
				componentInChildren.bones = list.ToArray();
				componentInChildren.rootBone = root.GetComponentInChildren<Animator>()?.transform ?? root;
			}
			else
			{
				meshBodyBase = gameObject.GetComponentInChildren<MeshBody>();
				if (meshBodyBase == null)
				{
					PFLog.TechArt.Error($"{outfitObj} contains no MeshBody setup. Add default layout without constrains");
					meshBodyBase = gameObject.AddComponent<MeshBody>();
				}
			}
			meshBodyBase.Layout = triangleSkinmap.Master;
			meshBodyBase.HideRenderer = true;
			meshBodyBase.EnsureComponent<HighlighterBlocker>();
			meshBodyBase.EnsureComponent<OccludedObjectHighlighterBlocker>();
			outfitObj.EnsureComponent<MeshDeformer>().AddBinding(new MeshDeformerBinding
			{
				Master = meshBodyBase,
				Skinmap = triangleSkinmap
			});
			return gameObject;
		}
		return null;
	}

	public override string ToString()
	{
		return $"{Prefab?.name} on {m_Bone}";
	}

	public bool HasFeature(EquipmentFeatureFlag feature)
	{
		return Features.HasFeature(feature);
	}

	public bool TryGetPhysicsDeformerLayout(out TriangleSkinmap triangleSkinmap, out GameObject prefabMesh)
	{
		return Features.TryGetPhysicsDeformerLayout(out triangleSkinmap, out prefabMesh);
	}

	public bool IsHiddenByVisibilityFeatures(CharacterDisplayOptions displayOptions)
	{
		return Features.IsHiddenByVisibilityFeatures(displayOptions);
	}

	public IEnumerable<T> GetFeatureComponents<T>()
	{
		if (m_Features == null)
		{
			yield break;
		}
		BlueprintEquipmentFeatureReference[] features = m_Features;
		foreach (BlueprintEquipmentFeatureReference blueprintEquipmentFeatureReference in features)
		{
			foreach (T component in blueprintEquipmentFeatureReference.Get().GetComponents<T>())
			{
				yield return component;
			}
		}
	}
}
