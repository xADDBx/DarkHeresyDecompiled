using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Layouts;

[Serializable]
public class HalfEdgeMesh
{
	[Serializable]
	public struct HalfEdge
	{
		public int Index;

		public int IndexInFace;

		public int Face;

		public int NextHalfEdge;

		public int Pair;

		public int EndVertex;
	}

	[Serializable]
	public struct Vertex
	{
		public int Index;

		public int HalfEdge;

		public float3 Position;
	}

	[Serializable]
	public struct Face
	{
		public int Index;

		public int HalfEdge;
	}

	public Mesh InputMesh;

	public float3 Scale;

	public float Area;

	public float Volume;

	public bool ContainsData;

	public List<Vertex> Vertices = new List<Vertex>();

	public List<HalfEdge> HalfEdges = new List<HalfEdge>();

	public List<HalfEdge> BorderEdges = new List<HalfEdge>();

	public List<Face> Faces = new List<Face>();

	public List<float3> RestNormals = new List<float3>();

	public List<quaternion> RestOrientations = new List<quaternion>();

	public List<int> RawToWelded = new List<int>();

	public HalfEdgeMesh()
	{
	}

	public HalfEdgeMesh(HalfEdgeMesh halfEdge)
	{
		ContainsData = halfEdge.ContainsData;
		InputMesh = halfEdge.InputMesh;
		Scale = halfEdge.Scale;
		Vertices = new List<Vertex>(halfEdge.Vertices);
		HalfEdges = new List<HalfEdge>(halfEdge.HalfEdges);
		BorderEdges = new List<HalfEdge>(halfEdge.BorderEdges);
		Faces = new List<Face>(halfEdge.Faces);
		RestNormals = new List<float3>(halfEdge.RestNormals);
		RestOrientations = new List<quaternion>(halfEdge.RestOrientations);
		RawToWelded = new List<int>(halfEdge.RawToWelded);
	}

	public void Generate()
	{
		ContainsData = false;
		Vertices = new List<Vertex>();
		HalfEdges = new List<HalfEdge>();
		BorderEdges = new List<HalfEdge>();
		Faces = new List<Face>();
		RestNormals = new List<float3>();
		RestOrientations = new List<quaternion>();
		RawToWelded = new List<int>();
		Area = 0f;
		Volume = 0f;
		Dictionary<Vector3, Vertex> dictionary = new Dictionary<Vector3, Vertex>();
		Dictionary<Vector2Int, HalfEdge> dictionary2 = new Dictionary<Vector2Int, HalfEdge>();
		Vector3[] vertices = InputMesh.vertices;
		for (int i = 0; i < vertices.Length; i++)
		{
			Vector3 vector = Vector3.Scale(vertices[i], (Vector3)Scale);
			if (!dictionary.TryGetValue(vector, out var value))
			{
				value = default(Vertex);
				value.Position = vector;
				value.Index = Vertices.Count;
				dictionary.Add(vector, value);
				Vertices.Add(value);
			}
			RawToWelded.Add(value.Index);
		}
		int[] triangles = InputMesh.triangles;
		for (int j = 0; j < triangles.Length; j += 3)
		{
			int num = triangles[j];
			int num2 = triangles[j + 1];
			int num3 = triangles[j + 2];
			_ = ref vertices[num];
			_ = ref vertices[num2];
			_ = ref vertices[num3];
			Vertex value2 = Vertices[RawToWelded[num]];
			Vertex value3 = Vertices[RawToWelded[num2]];
			Vertex value4 = Vertices[RawToWelded[num3]];
			HalfEdge halfEdge = default(HalfEdge);
			halfEdge.Index = HalfEdges.Count;
			halfEdge.IndexInFace = 0;
			halfEdge.Face = Faces.Count;
			halfEdge.EndVertex = value2.Index;
			HalfEdge halfEdge2 = default(HalfEdge);
			halfEdge2.Index = HalfEdges.Count + 1;
			halfEdge2.IndexInFace = 1;
			halfEdge2.Face = Faces.Count;
			halfEdge2.EndVertex = value3.Index;
			HalfEdge halfEdge3 = default(HalfEdge);
			halfEdge3.Index = HalfEdges.Count + 2;
			halfEdge3.IndexInFace = 2;
			halfEdge3.Face = Faces.Count;
			halfEdge3.EndVertex = value4.Index;
			halfEdge.NextHalfEdge = halfEdge2.Index;
			halfEdge2.NextHalfEdge = halfEdge3.Index;
			halfEdge3.NextHalfEdge = halfEdge.Index;
			value2.HalfEdge = halfEdge2.Index;
			value3.HalfEdge = halfEdge3.Index;
			value4.HalfEdge = halfEdge.Index;
			Vertices[RawToWelded[num]] = value2;
			Vertices[RawToWelded[num2]] = value3;
			Vertices[RawToWelded[num3]] = value4;
			Vector2Int key = new Vector2Int(value4.Index, value2.Index);
			Vector2Int key2 = new Vector2Int(value2.Index, value3.Index);
			Vector2Int key3 = new Vector2Int(value3.Index, value4.Index);
			if (!dictionary2.ContainsKey(key) && !dictionary2.ContainsKey(key2) && !dictionary2.ContainsKey(key3))
			{
				dictionary2.Add(key, halfEdge);
				dictionary2.Add(key2, halfEdge2);
				dictionary2.Add(key3, halfEdge3);
				HalfEdges.Add(halfEdge);
				HalfEdges.Add(halfEdge2);
				HalfEdges.Add(halfEdge3);
				Face item = default(Face);
				item.Index = Faces.Count;
				item.HalfEdge = halfEdge.Index;
				Faces.Add(item);
				Area += Vector3.Cross((Vector3)(value3.Position - value2.Position), (Vector3)(value4.Position - value2.Position)).magnitude / 2f;
				Volume += Vector3.Dot(Vector3.Cross((Vector3)value2.Position, (Vector3)value3.Position), (Vector3)value4.Position) / 6f;
			}
		}
		foreach (KeyValuePair<Vector2Int, HalfEdge> item2 in dictionary2)
		{
			HalfEdge value5 = item2.Value;
			Vector2Int key4 = new Vector2Int(item2.Key.y, item2.Key.x);
			if (!dictionary2.TryGetValue(key4, out var value6))
			{
				value6 = default(HalfEdge);
				value6.IndexInFace = -1;
				value6.Face = -1;
				value6.Index = HalfEdges.Count;
				value6.EndVertex = HalfEdges[HalfEdges[value5.NextHalfEdge].NextHalfEdge].EndVertex;
				value6.Pair = value5.Index;
				Vertex value7 = Vertices[value5.EndVertex];
				value7.HalfEdge = value6.Index;
				Vertices[value5.EndVertex] = value7;
				HalfEdges.Add(value6);
				BorderEdges.Add(value6);
			}
			value5.Pair = value6.Index;
			HalfEdges[value5.Index] = value5;
		}
		for (int k = 0; k < BorderEdges.Count; k++)
		{
			HalfEdge value8 = HalfEdges[BorderEdges[k].Index];
			value8.NextHalfEdge = Vertices[value8.EndVertex].HalfEdge;
			HalfEdges[BorderEdges[k].Index] = value8;
		}
		ContainsData = true;
		CalculateRestNormals();
		CalculateRestOrientations();
	}

	private void CalculateRestNormals()
	{
		RestNormals.Capacity = Vertices.Count;
		for (int i = 0; i < Vertices.Count; i++)
		{
			RestNormals.Add(Vector3.zero);
		}
		for (int j = 0; j < Faces.Count; j++)
		{
			HalfEdge halfEdge = HalfEdges[Faces[j].HalfEdge];
			HalfEdge halfEdge2 = HalfEdges[halfEdge.NextHalfEdge];
			HalfEdge halfEdge3 = HalfEdges[halfEdge2.NextHalfEdge];
			Vector3 vector = Vertices[halfEdge.EndVertex].Position;
			Vector3 vector2 = Vertices[halfEdge2.EndVertex].Position;
			float3 @float = math.cross(y: (Vector3)Vertices[halfEdge3.EndVertex].Position - vector, x: vector2 - vector);
			RestNormals[halfEdge.EndVertex] += @float;
			RestNormals[halfEdge2.EndVertex] += @float;
			RestNormals[halfEdge3.EndVertex] += @float;
		}
		for (int k = 0; k < RestNormals.Count; k++)
		{
			RestNormals[k] = math.normalize(RestNormals[k]);
		}
	}

	private void CalculateRestOrientations()
	{
		for (int i = 0; i < Vertices.Count; i++)
		{
			Vector3 upwards = Vertices[HalfEdges[Vertices[i].HalfEdge].EndVertex].Position - Vertices[i].Position;
			RestOrientations.Add(Quaternion.Inverse(Quaternion.LookRotation((Vector3)RestNormals[i], upwards)));
		}
	}

	public List<int> GetEdgeList()
	{
		List<int> list = new List<int>();
		bool[] array = new bool[HalfEdges.Count];
		for (int i = 0; i < HalfEdges.Count; i++)
		{
			if (!array[HalfEdges[i].Pair])
			{
				list.Add(i);
				array[HalfEdges[i].Pair] = true;
				array[i] = true;
			}
		}
		return list;
	}

	public int GetHalfEdgeStartVertex(HalfEdge edge)
	{
		if (edge.Face == -1)
		{
			return HalfEdges[edge.Pair].EndVertex;
		}
		return HalfEdges[HalfEdges[edge.NextHalfEdge].NextHalfEdge].EndVertex;
	}

	public IEnumerable<HalfEdge> GetNeighbourEdgesEnumerator(Vertex vertex)
	{
		HalfEdge startEdge = HalfEdges[vertex.HalfEdge];
		HalfEdge edge = startEdge;
		do
		{
			edge = HalfEdges[edge.Pair];
			yield return edge;
			edge = HalfEdges[edge.NextHalfEdge];
			yield return edge;
		}
		while (edge.Index != startEdge.Index);
	}

	public IEnumerable<Vertex> GetNeighbourVerticesEnumerator(Vertex vertex)
	{
		HalfEdge startEdge = HalfEdges[vertex.HalfEdge];
		HalfEdge edge = startEdge;
		do
		{
			yield return Vertices[edge.EndVertex];
			edge = HalfEdges[edge.Pair];
			edge = HalfEdges[edge.NextHalfEdge];
		}
		while (edge.Index != startEdge.Index);
	}
}
