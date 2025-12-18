using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Kingmaker.Code.View.Visual.CharacterSystem.EquipmentComponents;
using Kingmaker.EntitySystem.Persistence;
using Kingmaker.Utility.CodeTimer;
using Kingmaker.Utility.DotNetExtensions;
using Kingmaker.Visual.CharacterSystem;
using Kingmaker.Visual.MaterialEffects;
using Owlcat.Runtime.Core.Utility;
using Owlcat.Runtime.Core.Utility.Locator;
using Owlcat.Runtime.Visual.DxtCompressor;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Layouts;
using Owlcat.Runtime.Visual.XPBD.Layouts.MeshSkinning;
using RogueTrader.Code.ShaderConsts;
using UnityEngine;

namespace Kingmaker.Code.View.Visual.CharacterSystem;

public class CharacterBuilder
{
	private struct EquipmentBodyPartInfo
	{
		public BodyPart BodyPart;

		public int Layer;

		public bool KeepPreviousLayers;

		public List<VertexColorMask> VertexColorMasks;

		public int GroupIndex;
	}

	private struct CharacterBuildStatus
	{
		public bool IsMeshReady;

		public bool IsOutfitReady;

		public bool IsAtlasReadyToBuild;

		public bool IsAtlasCompressed;

		public void Reset()
		{
			IsMeshReady = false;
			IsOutfitReady = false;
			IsAtlasReadyToBuild = false;
			IsAtlasCompressed = false;
		}

		public bool IsComplete()
		{
			if (IsMeshReady && IsOutfitReady && IsAtlasReadyToBuild)
			{
				return IsAtlasCompressed;
			}
			return false;
		}
	}

	public class OutfitPartInfo
	{
		public OutfitPart OutfitPart;

		public GameObject GameObject;

		public Transform Bone;

		public GameObject PhysicsMasterMesh;

		public OutfitPartInfo(OutfitPart outfitPart, GameObject gameObject, Transform bone, GameObject physicsMasterMesh)
		{
			OutfitPart = outfitPart;
			GameObject = gameObject;
			Bone = bone;
			PhysicsMasterMesh = physicsMasterMesh;
		}

		public void Destroy()
		{
			UnityEngine.Object.Destroy(GameObject);
			if (PhysicsMasterMesh != null)
			{
				UnityEngine.Object.Destroy(PhysicsMasterMesh);
			}
		}
	}

	private class PhysicsLayoutInfo
	{
		private int m_CurrentOffset;

		private readonly List<int> m_VertexOffsets = new List<int>();

		private readonly List<List<int>> m_RemappedVertices = new List<List<int>>();

		private readonly List<TriangleSkinmap> m_SkinnedParts = new List<TriangleSkinmap>();

		private readonly List<GameObject> m_MasterPrefabs = new List<GameObject>();

		public bool HasPhysics { get; private set; }

		public void ProcessMesh(EquipmentFeatureProvider characterPart, Mesh mesh)
		{
			if (characterPart.TryGetPhysicsDeformerLayout(out var triangleSkinmap, out var prefabMesh) && triangleSkinmap != null && prefabMesh != null)
			{
				HasPhysics = true;
				m_VertexOffsets.Add(m_CurrentOffset);
				m_SkinnedParts.Add(triangleSkinmap);
				m_MasterPrefabs.Add(prefabMesh);
				List<int> list = new List<int>();
				foreach (SlaveVertex skinnedVertex in triangleSkinmap.SkinnedVertices)
				{
					list.Add(skinnedVertex.SlaveIndex + m_CurrentOffset);
				}
				m_RemappedVertices.Add(list);
			}
			m_CurrentOffset += mesh.vertexCount;
		}

		private MeshBody SetupMasterMesh(MeshLayout layout, GameObject mesh, Transform characterObj, Transform root = null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(mesh, characterObj, worldPositionStays: false);
			if (root != null)
			{
				gameObject.transform.parent = root;
			}
			gameObject.name = "[Physics] Master Body " + mesh.name;
			MeshBody meshBody = gameObject.EnsureComponent<MeshBody>();
			meshBody.Layout = layout;
			meshBody.HideRenderer = true;
			return meshBody;
		}

		public void ResetRemappedVertices()
		{
			for (int i = 0; i < m_RemappedVertices.Count; i++)
			{
				for (int j = 0; j < m_RemappedVertices[i].Count; j++)
				{
					m_RemappedVertices[i][j] = -1;
				}
			}
		}

		public void SetRemappedVertex(int oldVertex, int newVertex)
		{
			for (int i = 0; i < m_SkinnedParts.Count; i++)
			{
				if (m_VertexOffsets[i] <= oldVertex && oldVertex < m_VertexOffsets[i] + m_SkinnedParts[i].SkinnedVertices.Count)
				{
					m_RemappedVertices[i][oldVertex - m_VertexOffsets[i]] = newVertex;
				}
			}
		}

		public List<GameObject> SetupMeshDeformer(Renderer targetRenderer, Transform rootBone = null)
		{
			if (!HasPhysics)
			{
				return new List<GameObject>();
			}
			MeshDeformer meshDeformer = targetRenderer.gameObject.AddComponent<MeshDeformer>();
			List<GameObject> list = new List<GameObject>();
			for (int i = 0; i < m_SkinnedParts.Count; i++)
			{
				MeshBody meshBody = SetupMasterMesh(m_SkinnedParts[i].Master, m_MasterPrefabs[i], targetRenderer.transform, rootBone);
				list.Add(meshBody.gameObject);
				TriangleSkinmap copy = m_SkinnedParts[i].GetCopy();
				List<SlaveVertex> list2 = new List<SlaveVertex>();
				for (int j = 0; j < m_SkinnedParts[i].SkinnedVertices.Count; j++)
				{
					if (m_RemappedVertices[i][j] != -1)
					{
						SlaveVertex slaveVertex = m_SkinnedParts[i].SkinnedVertices[j];
						list2.Add(new SlaveVertex(m_RemappedVertices[i][j], slaveVertex.MasterTriangleIndex, slaveVertex.MasterVertexIndices, slaveVertex.Position, slaveVertex.Normal, slaveVertex.Tangent));
					}
				}
				copy.SetSkinnedVertices(list2);
				meshDeformer.AddBinding(new MeshDeformerBinding
				{
					Master = meshBody,
					Skinmap = copy
				});
			}
			return list;
		}
	}

	public const string CharacterMeshName = "Character";

	private readonly GameObject m_CharacterObj;

	private readonly Transform m_RootBone;

	private CharacterDisplayOptions m_DisplayOptions;

	private CharacterAtlasData m_AtlasData;

	private SkinnedMeshRenderer m_SkinnedMeshRenderer;

	private MeshRenderer m_NonSkinnedMeshRenderer;

	private readonly EquipmentEntity.PaintedTextures m_EquipmentEntitiesTextures = new EquipmentEntity.PaintedTextures();

	private readonly List<CharacterAtlas> m_Atlases = new List<CharacterAtlas>();

	private readonly List<OutfitPartInfo> m_OutfitObjectsSpawned = new List<OutfitPartInfo>();

	private readonly List<GameObject> m_Deformers = new List<GameObject>();

	private CancellationTokenSource m_AtlasBuilderCts;

	private readonly List<BodyPart> m_OverlayBodyParts = new List<BodyPart>();

	private readonly Dictionary<string, Transform> m_AttachBonesCache = new Dictionary<string, Transform>();

	private StandardMaterialController m_StandardMaterialController;

	private CharacterBuildStatus m_CharacterBuildStatus;

	public SkinnedMeshRenderer CharacterMesh => m_SkinnedMeshRenderer;

	public CharacterBuilder(GameObject charGameObject)
	{
		m_CharacterObj = charGameObject;
		m_RootBone = m_CharacterObj.GetComponentInChildren<Animator>()?.transform;
	}

	public bool IsAtlasCompressed()
	{
		return m_CharacterBuildStatus.IsAtlasCompressed;
	}

	public bool IsComplete()
	{
		return m_CharacterBuildStatus.IsComplete();
	}

	public void BuildCharacter(List<EquipmentEntity> equipmentEntities, List<Character.SelectedRampIndices> selectedRampIndicesList, CharacterAtlasData atlasData, CharacterAtlasSize maxAtlasSize, CharacterDisplayOptions displayOptions, Character.CharacterRebuildMode mode)
	{
		if ((mode & Character.CharacterRebuildMode.FullUpdate) == Character.CharacterRebuildMode.FullUpdate)
		{
			m_CharacterBuildStatus.Reset();
		}
		else
		{
			if ((mode & Character.CharacterRebuildMode.OnlyOutfit) == Character.CharacterRebuildMode.OnlyOutfit)
			{
				m_CharacterBuildStatus.IsOutfitReady = false;
			}
			if ((mode & Character.CharacterRebuildMode.OnlyAtlases) == Character.CharacterRebuildMode.OnlyAtlases)
			{
				m_CharacterBuildStatus.IsAtlasReadyToBuild = false;
				m_CharacterBuildStatus.IsAtlasCompressed = false;
			}
		}
		m_DisplayOptions = displayOptions;
		m_AtlasData = atlasData;
		if ((object)m_StandardMaterialController == null)
		{
			m_StandardMaterialController = m_CharacterObj.EnsureComponent<StandardMaterialController>();
		}
		if (!m_CharacterBuildStatus.IsMeshReady)
		{
			ClearMeshes();
			UpdateCharacter(equipmentEntities);
			m_StandardMaterialController.InvalidateRenderersAndMaterials();
			m_CharacterBuildStatus.IsMeshReady = true;
		}
		if (!m_CharacterBuildStatus.IsOutfitReady)
		{
			RebuildOutfit(equipmentEntities);
			m_CharacterBuildStatus.IsOutfitReady = true;
		}
		if (m_CharacterBuildStatus.IsAtlasReadyToBuild)
		{
			return;
		}
		ClearAtlases();
		if (m_OverlayBodyParts.Count <= 0)
		{
			return;
		}
		CreateAtlasTexturesMap(maxAtlasSize);
		m_CharacterBuildStatus.IsAtlasReadyToBuild = true;
		if (!Application.isPlaying || !LoadingProcess.Instance.IsLoadingInProcess)
		{
			BuildCharacterAtlases(equipmentEntities, selectedRampIndicesList);
			return;
		}
		if (m_AtlasBuilderCts != null)
		{
			m_AtlasBuilderCts.Cancel();
			m_AtlasBuilderCts.Dispose();
		}
		m_AtlasBuilderCts = new CancellationTokenSource();
		BuildCharacterAtlasesAsync(equipmentEntities, selectedRampIndicesList, m_AtlasBuilderCts.Token);
	}

	private void UpdateCharacter(List<EquipmentEntity> equipmentEntities)
	{
		List<EquipmentEntity> list = new List<EquipmentEntity>();
		foreach (EquipmentEntity equipmentEntity in equipmentEntities)
		{
			if (!(equipmentEntity == null) && equipmentEntity.BodyParts.Count != 0)
			{
				list.Add(equipmentEntity);
			}
		}
		Material material = null;
		Dictionary<BodyPartType, List<EquipmentBodyPartInfo>> dictionary = new Dictionary<BodyPartType, List<EquipmentBodyPartInfo>>();
		foreach (EquipmentEntity item in list)
		{
			if ((object)material == null)
			{
				material = item.Material;
			}
			if (material != item.Material)
			{
				PFLog.TechArt.Error($"Equipment entity {item} on character {m_CharacterObj} contains material different from one already used (m_MainMaterial={material}, equipmentEntity.Material={item.Material})");
				continue;
			}
			foreach (BodyPart bodyPart in item.BodyParts)
			{
				if (bodyPart != null && !bodyPart.IsHiddenByVisibilityFeatures(m_DisplayOptions))
				{
					if (!dictionary.ContainsKey(bodyPart.Type))
					{
						dictionary.Add(bodyPart.Type, new List<EquipmentBodyPartInfo>());
					}
					int layer = (bodyPart.HasFeature(EquipmentFeatureFlag.IgnoreLayer) ? 999 : item.Layer);
					dictionary[bodyPart.Type].Add(new EquipmentBodyPartInfo
					{
						BodyPart = bodyPart,
						Layer = layer,
						KeepPreviousLayers = item.KeepTexturesOfPrevLayers,
						VertexColorMasks = item.HideByVertexColorMask,
						GroupIndex = 0
					});
					if (Application.isEditor && !(bodyPart.Textures.GetSourceTexture() != null))
					{
						PFLog.TechArt.Error($"Missing texture in {bodyPart.Type} body part in {item} when merging overlays for {this}");
					}
				}
			}
		}
		List<BodyPart> list2 = new List<BodyPart>();
		m_OverlayBodyParts.Clear();
		HashSet<VertexColorMask> hashSet = new HashSet<VertexColorMask>();
		foreach (KeyValuePair<BodyPartType, List<EquipmentBodyPartInfo>> item2 in dictionary)
		{
			List<EquipmentBodyPartInfo> value = item2.Value;
			value.Sort((EquipmentBodyPartInfo x, EquipmentBodyPartInfo y) => x.Layer.CompareTo(y.Layer));
			int num = value.FindLastIndex((EquipmentBodyPartInfo eeInfo) => eeInfo.BodyPart.HasMesh());
			if (num < 0 || value[num].BodyPart == null)
			{
				continue;
			}
			EquipmentBodyPartInfo equipmentBodyPartInfo = value[num];
			if (equipmentBodyPartInfo.BodyPart.Model != null)
			{
				list2.Add(equipmentBodyPartInfo.BodyPart);
				hashSet.AddRange(equipmentBodyPartInfo.VertexColorMasks);
			}
			else
			{
				PFLog.TechArt.Error($"Skipping VertexColorMask from {equipmentBodyPartInfo.BodyPart.Type} - model not loaded");
			}
			for (int i = 0; i < value.Count; i++)
			{
				if ((i >= num || value[num].KeepPreviousLayers) && !(value[i].BodyPart.Textures.GetSourceTexture() == null))
				{
					m_OverlayBodyParts.Add(value[i].BodyPart);
				}
			}
		}
		BuildMesh(list2, hashSet, material);
	}

	private void BuildMesh(List<BodyPart> geometryBodyParts, HashSet<VertexColorMask> vertexColorMasks, Material mainMaterial)
	{
		Dictionary<string, Transform> cachedBones = CacheHierarchy();
		List<Transform> list = new List<Transform>();
		List<BoneWeight> list2 = new List<BoneWeight>();
		List<Matrix4x4> list3 = new List<Matrix4x4>();
		List<CombineInstance> list4 = new List<CombineInstance>();
		List<Vector2> list5 = new List<Vector2>();
		PhysicsLayoutInfo physicsLayoutInfo = new PhysicsLayoutInfo();
		foreach (BodyPart geometryBodyPart in geometryBodyParts)
		{
			if (geometryBodyPart.Model == null)
			{
				continue;
			}
			SkinnedMeshRenderer[] skinnedRenderers = geometryBodyPart.SkinnedRenderers;
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedRenderers)
			{
				if (!skinnedMeshRenderer.sharedMesh.isReadable)
				{
					PFLog.TechArt.Error("Body Part mesh is not readable. Check Import settings and Location. " + skinnedMeshRenderer.name);
				}
				CombineInstance combineInstance = default(CombineInstance);
				combineInstance.mesh = skinnedMeshRenderer.sharedMesh;
				combineInstance.transform = Matrix4x4.identity;
				CombineInstance item = combineInstance;
				physicsLayoutInfo.ProcessMesh(geometryBodyPart, skinnedMeshRenderer.sharedMesh);
				int[] bonesMapping = new int[skinnedMeshRenderer.sharedMesh.bindposes.Length];
				EnsureBones(skinnedMeshRenderer, list, list3, bonesMapping, cachedBones);
				InsertBoneWeights(list2, bonesMapping, skinnedMeshRenderer);
				Vector2[] uv = skinnedMeshRenderer.sharedMesh.uv;
				for (int j = 0; j < uv.Length; j++)
				{
					Vector2 vector = uv[j];
					list5.Add((geometryBodyPart.Textures.AnisotropyMasks != null) ? new Vector2(vector.x + 1f, vector.y) : vector);
				}
				list4.Add(item);
			}
		}
		Animator componentInChildren = m_CharacterObj.GetComponentInChildren<Animator>();
		GameObject gameObject = new GameObject("SkinnedMeshRenderer");
		gameObject.transform.parent = componentInChildren?.transform ?? m_CharacterObj.transform;
		gameObject.transform.localPosition = default(Vector3);
		gameObject.transform.localScale = Vector3.one;
		gameObject.transform.localRotation = Quaternion.identity;
		SkinnedMeshRenderer skinnedMeshRenderer2 = gameObject.AddComponent<SkinnedMeshRenderer>();
		Mesh mesh = new Mesh
		{
			name = "Character"
		};
		mesh.Clear();
		mesh.CombineMeshes(list4.ToArray(), mergeSubMeshes: true);
		mesh.bindposes = list3.ToArray();
		mesh.boneWeights = list2.ToArray();
		mesh.uv = list5.ToArray();
		HideMeshesByVertexColorMasks(mesh, physicsLayoutInfo, vertexColorMasks);
		mesh.RecalculateBounds();
		mesh.UploadMeshData(markNoLongerReadable: true);
		skinnedMeshRenderer2.sharedMesh = mesh;
		skinnedMeshRenderer2.bones = list.ToArray();
		skinnedMeshRenderer2.rootBone = m_RootBone;
		m_SkinnedMeshRenderer = skinnedMeshRenderer2;
		skinnedMeshRenderer2.gameObject.layer = 9;
		skinnedMeshRenderer2.sharedMaterial = mainMaterial;
		Transform transform = Character.FindBone(m_CharacterObj.transform, "Pelvis");
		List<GameObject> collection = physicsLayoutInfo.SetupMeshDeformer(skinnedMeshRenderer2, transform ?? m_RootBone);
		m_Deformers.AddRange(collection);
	}

	private void BuildNonSkinnedMesh(List<BodyPart> geometryBodyParts, HashSet<VertexColorMask> vertexColorMasks, Material mainMaterial)
	{
		List<CombineInstance> list = new List<CombineInstance>();
		List<Vector2> list2 = new List<Vector2>();
		foreach (BodyPart geometryBodyPart in geometryBodyParts)
		{
			if (geometryBodyPart.Model == null)
			{
				continue;
			}
			MeshRenderer[] componentsInChildren = geometryBodyPart.Model.GetComponentsInChildren<MeshRenderer>();
			if (componentsInChildren == null)
			{
				continue;
			}
			MeshRenderer[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				MeshFilter component = array[i].GetComponent<MeshFilter>();
				CombineInstance combineInstance = default(CombineInstance);
				combineInstance.mesh = component.sharedMesh;
				combineInstance.transform = Matrix4x4.identity;
				CombineInstance item = combineInstance;
				Vector2[] uv = component.sharedMesh.uv;
				for (int j = 0; j < uv.Length; j++)
				{
					Vector2 vector = uv[j];
					list2.Add((geometryBodyPart.Textures.AnisotropyMasks != null) ? new Vector2(vector.x + 1f, vector.y) : vector);
				}
				list.Add(item);
			}
		}
		if (list.Count != 0)
		{
			PFLog.TechArt.Warning("EE with non skinned mesh rendered detected. Such meshes are displayed in editor only.");
			Mesh mesh = new Mesh
			{
				name = "CharacterSubMesh_NON_SKINNED"
			};
			mesh.CombineMeshes(list.ToArray(), mergeSubMeshes: true);
			mesh.uv = list2.ToArray();
			HideMeshesByVertexColorMasks(mesh, null, vertexColorMasks);
			GameObject gameObject = new GameObject("EDITOR_ONLY_NonSkinnedMeshRenderer");
			gameObject.transform.parent = m_CharacterObj.transform;
			gameObject.transform.localPosition = default(Vector3);
			gameObject.transform.localScale = Vector3.one;
			gameObject.transform.localRotation = Quaternion.identity;
			MeshRenderer meshRenderer = gameObject.AddComponent<MeshRenderer>();
			meshRenderer.sharedMaterial = mainMaterial;
			gameObject.AddComponent<MeshRenderer>();
			gameObject.AddComponent<MeshFilter>().sharedMesh = mesh;
			m_NonSkinnedMeshRenderer = meshRenderer;
		}
	}

	private void FilterOutfit(Func<OutfitPart, GameObject, bool> filter)
	{
		foreach (OutfitPartInfo item in m_OutfitObjectsSpawned)
		{
			if (!(item.GameObject == null))
			{
				item.GameObject.SetActive(filter?.Invoke(item.OutfitPart, item.GameObject) ?? true);
			}
		}
	}

	private void RebuildOutfit(List<EquipmentEntity> equipmentEntities)
	{
		List<EquipmentEntity> list = new List<EquipmentEntity>();
		bool flag = false;
		bool flag2 = false;
		bool flag3 = false;
		foreach (EquipmentEntity equipmentEntity in equipmentEntities)
		{
			if (equipmentEntity == null)
			{
				continue;
			}
			foreach (OutfitPart outfitPart in equipmentEntity.OutfitParts)
			{
				if (outfitPart == null)
				{
					continue;
				}
				if (outfitPart.Type == null)
				{
					return;
				}
				if (!outfitPart.IsHiddenByVisibilityFeatures(m_DisplayOptions))
				{
					if (outfitPart.HasFeature(EquipmentFeatureFlag.IsCloak))
					{
						flag2 = true;
					}
					if (outfitPart.HasFeature(EquipmentFeatureFlag.IsCloakSquashed))
					{
						flag3 = true;
					}
					if (outfitPart.HasFeature(EquipmentFeatureFlag.IsBackpack))
					{
						flag = true;
					}
					list.Add(equipmentEntity);
				}
			}
		}
		list.Sort((EquipmentEntity x, EquipmentEntity y) => x.Layer.CompareTo(y.Layer));
		List<OutfitPart> list2 = new List<OutfitPart>();
		Dictionary<OutfitPartType, OutfitPart> dictionary = new Dictionary<OutfitPartType, OutfitPart>();
		foreach (EquipmentEntity item in list)
		{
			foreach (OutfitPart outfitPart2 in item.OutfitParts)
			{
				if ((!(flag && flag2) || !outfitPart2.HasFeature(EquipmentFeatureFlag.IsCloak)) && (!(!flag && flag2 && flag3) || !outfitPart2.HasFeature(EquipmentFeatureFlag.IsCloakSquashed)))
				{
					if (outfitPart2.HasFeature(EquipmentFeatureFlag.IgnoreLayer))
					{
						list2.Add(outfitPart2);
					}
					else
					{
						dictionary[outfitPart2.Type] = outfitPart2;
					}
				}
			}
		}
		list2.AddRange(dictionary.Values);
		int i;
		for (i = m_OutfitObjectsSpawned.Count - 1; i >= 0; i--)
		{
			if (list2.All((OutfitPart o) => o != m_OutfitObjectsSpawned[i].OutfitPart))
			{
				m_OutfitObjectsSpawned[i].Destroy();
				m_OutfitObjectsSpawned.RemoveAt(i);
			}
		}
		foreach (OutfitPart outfit in list2)
		{
			if (!m_OutfitObjectsSpawned.Contains((OutfitPartInfo x) => x.OutfitPart == outfit))
			{
				OutfitPartInfo outfitPartInfo = outfit.Attach(m_CharacterObj.transform, m_AttachBonesCache);
				if (outfitPartInfo != null)
				{
					m_AttachBonesCache.TryAdd(outfitPartInfo.Bone.name, outfitPartInfo.Bone);
					m_OutfitObjectsSpawned.Add(outfitPartInfo);
				}
			}
		}
		FilterOutfit(m_DisplayOptions.OutfitFilter);
	}

	private void CreateAtlasTexturesMap(CharacterAtlasSize maxAtlasSize)
	{
		if (m_OverlayBodyParts.Count == 0)
		{
			return;
		}
		CharacterAtlasSize atlasSize = (true ? CharacterAtlasSize.AtlasSizeForCurrentGraphicsSettings() : maxAtlasSize);
		CharacterAtlas atlas = GetAtlas(atlasSize, CharacterTextureChannel.Diffuse);
		CharacterAtlas atlas2 = GetAtlas(atlasSize, CharacterTextureChannel.Normal);
		CharacterAtlas atlas3 = GetAtlas(atlasSize, CharacterTextureChannel.Masks);
		atlas.RefreshData();
		atlas2.RefreshData();
		atlas3.RefreshData();
		foreach (BodyPart overlayBodyPart in m_OverlayBodyParts)
		{
			Texture2D diffuseTexture = overlayBodyPart.Textures.DiffuseTexture;
			if (diffuseTexture != null)
			{
				atlas.AddPrimaryTexture(overlayBodyPart.Textures, overlayBodyPart.Type);
			}
			if (overlayBodyPart.Textures.NormalTexture != null)
			{
				atlas2.AddSecondaryTexture(overlayBodyPart.Textures, diffuseTexture, overlayBodyPart.Type);
			}
			if (overlayBodyPart.Textures.MaskTexture != null)
			{
				atlas3.AddSecondaryTexture(overlayBodyPart.Textures, diffuseTexture, overlayBodyPart.Type);
			}
		}
	}

	private void RepaintTextures(List<EquipmentEntity> equipmentEntities, List<Character.SelectedRampIndices> selectedRampIndicesList)
	{
		using (ProfileScope.New("Character Builder. Repaint Textures"))
		{
			foreach (EquipmentEntity ee in equipmentEntities)
			{
				Character.SelectedRampIndices selectedRampIndices = selectedRampIndicesList.Find((Character.SelectedRampIndices x) => x.EquipmentEntity == ee);
				if (selectedRampIndices != null)
				{
					ee.RepaintTextures(m_EquipmentEntitiesTextures, selectedRampIndices.PrimaryIndex, selectedRampIndices.SecondaryIndex);
				}
				else
				{
					ee.RepaintTextures(m_EquipmentEntitiesTextures, 0, 0);
				}
			}
		}
	}

	private Dictionary<string, Transform> CacheHierarchy()
	{
		Dictionary<string, Transform> dictionary = new Dictionary<string, Transform>();
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(m_CharacterObj.transform);
		while (stack.Count > 0)
		{
			Transform transform = stack.Pop();
			if (!dictionary.ContainsKey(transform.name))
			{
				dictionary.Add(transform.name, transform);
			}
			int childCount = transform.childCount;
			for (int i = 0; i < childCount; i++)
			{
				Transform child = transform.GetChild(i);
				stack.Push(child);
			}
		}
		return dictionary;
	}

	private void OnAtlasNotCompressed(CharacterAtlas atlas)
	{
		m_CharacterBuildStatus.IsAtlasCompressed = true;
	}

	private void OnAtlasCompressed(CharacterAtlas atlas, Texture2D tex)
	{
		if (!(m_CharacterObj == null))
		{
			UpdateMaterial(atlas.Channel, tex);
			m_CharacterBuildStatus.IsAtlasCompressed = true;
			m_StandardMaterialController.InvalidateMaterialsTextures();
		}
	}

	private void UpdateMaterial(CharacterTextureChannel atlasChannel, Texture tex)
	{
		if (m_SkinnedMeshRenderer != null)
		{
			switch (atlasChannel)
			{
			case CharacterTextureChannel.Diffuse:
				m_SkinnedMeshRenderer.material.SetTexture(ShaderProps._BaseMap, tex);
				break;
			case CharacterTextureChannel.Normal:
				m_SkinnedMeshRenderer.material.SetTexture(ShaderProps._BumpMap, tex);
				break;
			case CharacterTextureChannel.Masks:
				m_SkinnedMeshRenderer.material.SetTexture(ShaderProps._MasksMap, tex);
				break;
			}
		}
		if (m_NonSkinnedMeshRenderer != null)
		{
			switch (atlasChannel)
			{
			case CharacterTextureChannel.Diffuse:
				m_NonSkinnedMeshRenderer.material.SetTexture(ShaderProps._BaseMap, tex);
				break;
			case CharacterTextureChannel.Normal:
				m_NonSkinnedMeshRenderer.material.SetTexture(ShaderProps._BumpMap, tex);
				break;
			case CharacterTextureChannel.Masks:
				m_NonSkinnedMeshRenderer.material.SetTexture(ShaderProps._MasksMap, tex);
				break;
			}
		}
	}

	private void BuildCharacterAtlases(List<EquipmentEntity> equipmentEntities, List<Character.SelectedRampIndices> selectedRampIndicesList)
	{
		RepaintTextures(equipmentEntities, selectedRampIndicesList);
		Material sharedMaterial = m_SkinnedMeshRenderer.sharedMaterial;
		MaterialProperties materialProperties = default(MaterialProperties);
		materialProperties.Roughness = sharedMaterial.GetFloat(ShaderProps._Roughness);
		materialProperties.Emission = sharedMaterial.GetFloat(ShaderProps._Emission);
		materialProperties.Metallic = sharedMaterial.GetFloat(ShaderProps._Metallic);
		MaterialProperties materialProperties2 = materialProperties;
		foreach (CharacterAtlas atlase in m_Atlases)
		{
			Texture tex = atlase.Build(m_EquipmentEntitiesTextures, materialProperties2);
			UpdateMaterial(atlase.Channel, tex);
		}
		if (Application.isPlaying)
		{
			m_StandardMaterialController.InvalidateMaterialsTextures();
			Services.GetInstance<CharacterAtlasService>().QueueAtlasRebuild(m_Atlases, OnAtlasCompressed, OnAtlasNotCompressed, m_CharacterObj.GetInstanceID(), m_CharacterObj.name);
		}
		m_EquipmentEntitiesTextures.Clear();
	}

	private async void BuildCharacterAtlasesAsync(List<EquipmentEntity> equipmentEntities, List<Character.SelectedRampIndices> selectedRampIndicesList, CancellationToken cancellationToken)
	{
		while (Services.GetInstance<CharacterAtlasService>().RequestsCount > 0 && Services.GetInstance<DxtCompressorService2>().RequestsCount > 0)
		{
			await Task.Delay(10);
			if (cancellationToken.IsCancellationRequested)
			{
				return;
			}
		}
		if (m_OverlayBodyParts.Count > 0)
		{
			BuildCharacterAtlases(equipmentEntities, selectedRampIndicesList);
		}
	}

	private CharacterAtlas GetAtlas(CharacterAtlasSize atlasSize, CharacterTextureChannel channel)
	{
		CharacterAtlas characterAtlas = m_Atlases.FirstOrDefault((CharacterAtlas a) => a.Channel == channel);
		if (characterAtlas == null)
		{
			characterAtlas = new CharacterAtlas(atlasSize, channel, m_AtlasData);
			m_Atlases.Add(characterAtlas);
		}
		return characterAtlas;
	}

	public void Cleanup()
	{
		ClearAtlases();
		ClearMeshes();
		foreach (OutfitPartInfo item in m_OutfitObjectsSpawned)
		{
			item.Destroy();
		}
		m_OutfitObjectsSpawned.Clear();
		m_EquipmentEntitiesTextures.Clear();
	}

	private void ClearAtlases()
	{
		if (m_AtlasBuilderCts != null)
		{
			m_AtlasBuilderCts.Cancel();
			m_AtlasBuilderCts.Dispose();
			m_AtlasBuilderCts = null;
		}
		foreach (CharacterAtlas atlase in m_Atlases)
		{
			atlase.Cleanup();
		}
		m_Atlases.Clear();
	}

	private void ClearMeshes()
	{
		if (m_SkinnedMeshRenderer != null)
		{
			UnityEngine.Object.Destroy(m_SkinnedMeshRenderer.sharedMesh);
			UnityEngine.Object.Destroy(m_SkinnedMeshRenderer.gameObject);
		}
		if (m_NonSkinnedMeshRenderer != null)
		{
			UnityEngine.Object.Destroy(m_NonSkinnedMeshRenderer.gameObject);
		}
		foreach (GameObject deformer in m_Deformers)
		{
			UnityEngine.Object.Destroy(deformer);
		}
	}

	private static void HideMeshesByVertexColorMasks(Mesh mesh, PhysicsLayoutInfo physicsLayout, HashSet<VertexColorMask> masks)
	{
		using (ProfileScope.New("Character Builder. Hide By Vertex Color Masks"))
		{
			if (masks.Count == 0)
			{
				return;
			}
			Color[] colors = mesh.colors;
			if (colors.Length == 0)
			{
				return;
			}
			Vector3[] vertices = mesh.vertices;
			Vector2[] uv = mesh.uv;
			BoneWeight[] boneWeights = mesh.boneWeights;
			Vector3[] normals = mesh.normals;
			Vector4[] tangents = mesh.tangents;
			bool flag = mesh.boneWeights.Length != 0;
			bool flag2 = physicsLayout?.HasPhysics ?? false;
			if (flag2)
			{
				physicsLayout.ResetRemappedVertices();
			}
			int[] array = new int[vertices.Length];
			List<int> list = new List<int>();
			for (int i = 0; i < vertices.Length; i++)
			{
				bool flag3 = false;
				foreach (VertexColorMask mask in masks)
				{
					flag3 |= MaskColor(colors[i], mask.MaskColor) > 0.5f;
				}
				if (flag3)
				{
					array[i] = -1;
					continue;
				}
				if (flag2)
				{
					physicsLayout.SetRemappedVertex(i, list.Count);
				}
				array[i] = list.Count;
				list.Add(i);
			}
			Vector3[] array2 = new Vector3[list.Count];
			Color[] array3 = new Color[list.Count];
			Vector2[] array4 = new Vector2[list.Count];
			BoneWeight[] array5 = new BoneWeight[list.Count];
			Vector3[] array6 = new Vector3[list.Count];
			Vector4[] array7 = new Vector4[list.Count];
			for (int j = 0; j < list.Count; j++)
			{
				array2[j] = vertices[list[j]];
				array3[j] = colors[list[j]];
				array4[j] = uv[list[j]];
				array6[j] = normals[list[j]];
				array7[j] = tangents[list[j]];
				if (flag)
				{
					array5[j] = boneWeights[list[j]];
				}
			}
			int[] triangles = mesh.triangles;
			List<int> list2 = new List<int>(triangles.Length);
			for (int k = 0; k < triangles.Length; k += 3)
			{
				int num = array[triangles[k]];
				int num2 = array[triangles[k + 1]];
				int num3 = array[triangles[k + 2]];
				if (num >= 0 && num2 >= 0 && num3 >= 0)
				{
					list2.Add(num);
					list2.Add(num2);
					list2.Add(num3);
				}
			}
			PFLog.TechArt.Log($"Mesh {mesh.name} apply vertex mask: old vertices {vertices.Length} | new vertices {array2.Length}");
			mesh.Clear();
			mesh.vertices = array2;
			mesh.colors = array3;
			mesh.uv = array4;
			mesh.triangles = list2.ToArray();
			mesh.boneWeights = array5;
			mesh.normals = array6;
			mesh.tangents = array7;
		}
		static float MaskColor(Color color, Color mask)
		{
			float num4 = (color.r - mask.r) * (color.r - mask.r) + (color.g - mask.g) * (color.g - mask.g) + (color.b - mask.b) * (color.b - mask.b);
			return Mathf.Clamp01(1f - num4 * 100000f);
		}
	}

	private static void EnsureBones(SkinnedMeshRenderer bodyPart, List<Transform> bones, List<Matrix4x4> bindposes, int[] bonesMapping, Dictionary<string, Transform> cachedBones)
	{
		using (ProfileScope.New("Character Builder. Ensure Bones"))
		{
			Matrix4x4[] bindposes2 = bodyPart.sharedMesh.bindposes;
			Transform[] bones2 = bodyPart.bones;
			for (int i = 0; i < bones2.Length; i++)
			{
				Transform transform = bones2[i];
				int num = -1;
				if (!cachedBones.TryGetValue(transform.name, out var value))
				{
					break;
				}
				for (int j = 0; j < bones.Count; j++)
				{
					if (CompareSkinningMatrices(bindposes[j], ref bindposes2[i]) && bones2[i].transform.name == bones[j].name)
					{
						num = j;
						break;
					}
				}
				if (num < 0)
				{
					num = bones.Count;
					bones.Add(value);
					bindposes.Add(bindposes2[i]);
				}
				bonesMapping[i] = num;
			}
		}
	}

	private static bool CompareSkinningMatrices(Matrix4x4 m1, ref Matrix4x4 m2)
	{
		if ((double)Mathf.Abs(m1.m00 - m2.m00) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m01 - m2.m01) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m02 - m2.m02) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m03 - m2.m03) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m10 - m2.m10) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m11 - m2.m11) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m12 - m2.m12) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m13 - m2.m13) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m20 - m2.m20) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m21 - m2.m21) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m22 - m2.m22) > 0.0001)
		{
			return false;
		}
		if ((double)Mathf.Abs(m1.m23 - m2.m23) > 0.0001)
		{
			return false;
		}
		return true;
	}

	private static void InsertBoneWeights(List<BoneWeight> boneWeights, int[] bonesMapping, SkinnedMeshRenderer renderer)
	{
		BoneWeight[] boneWeights2 = renderer.sharedMesh.boneWeights;
		for (int i = 0; i < boneWeights2.Length; i++)
		{
			BoneWeight item = boneWeights2[i];
			item.boneIndex0 = bonesMapping[item.boneIndex0];
			item.boneIndex1 = bonesMapping[item.boneIndex1];
			item.boneIndex2 = bonesMapping[item.boneIndex2];
			item.boneIndex3 = bonesMapping[item.boneIndex3];
			boneWeights.Add(item);
		}
	}
}
