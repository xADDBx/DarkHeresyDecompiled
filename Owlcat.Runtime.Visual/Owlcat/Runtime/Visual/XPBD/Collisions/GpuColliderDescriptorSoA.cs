using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.SoA;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

public class GpuColliderDescriptorSoA : GpuStructureOfArrays<ColliderDescriptor, ColliderDescriptorSoA>
{
	public GraphicsBufferWrapper<ColliderShape> Shape;

	public GraphicsBufferWrapper<AffineTransform> PrevTransform;

	public GraphicsBufferWrapper<Aabb> PrevAabb;

	public GraphicsBufferWrapper<int> Layer;

	public GraphicsBufferWrapper<Aabb> Aabb;

	public GraphicsBufferWrapper<AffineTransform> Transform;

	public GpuColliderDescriptorSoA(int size)
		: base(size)
	{
		Shape = new GraphicsBufferWrapper<ColliderShape>("_XpbdColliderDescriptorShapeBuffer", size);
		m_Buffers.Add(Shape);
		PrevTransform = new GraphicsBufferWrapper<AffineTransform>("_XpbdColliderDescriptorPrevTransformBuffer", size);
		m_Buffers.Add(PrevTransform);
		PrevAabb = new GraphicsBufferWrapper<Aabb>("_XpbdColliderDescriptorPrevAabbBuffer", size);
		m_Buffers.Add(PrevAabb);
		Layer = new GraphicsBufferWrapper<int>("_XpbdColliderDescriptorLayerBuffer", size);
		m_Buffers.Add(Layer);
		Aabb = new GraphicsBufferWrapper<Aabb>("_XpbdColliderDescriptorAabbBuffer", size);
		m_Buffers.Add(Aabb);
		Transform = new GraphicsBufferWrapper<AffineTransform>("_XpbdColliderDescriptorTransformBuffer", size);
		m_Buffers.Add(Transform);
	}

	public override void SetData(ColliderDescriptorSoA data)
	{
		Shape.SetData(data.Shape);
		PrevTransform.SetData(data.PrevTransform);
		PrevAabb.SetData(data.PrevAabb);
		Layer.SetData(data.Layer);
		Aabb.SetData(data.Aabb);
		Transform.SetData(data.Transform);
	}

	public override void SetData(ColliderDescriptorSoA data, int offset, int count)
	{
		Shape.SetData(data.Shape, offset, offset, count);
		PrevTransform.SetData(data.PrevTransform, offset, offset, count);
		PrevAabb.SetData(data.PrevAabb, offset, offset, count);
		Layer.SetData(data.Layer, offset, offset, count);
		Aabb.SetData(data.Aabb, offset, offset, count);
		Transform.SetData(data.Transform, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, ColliderDescriptorSoA data)
	{
		cmd.SetBufferData(Shape.Buffer, data.Shape);
		cmd.SetBufferData(PrevTransform.Buffer, data.PrevTransform);
		cmd.SetBufferData(PrevAabb.Buffer, data.PrevAabb);
		cmd.SetBufferData(Layer.Buffer, data.Layer);
		cmd.SetBufferData(Aabb.Buffer, data.Aabb);
		cmd.SetBufferData(Transform.Buffer, data.Transform);
	}

	public override void SetData(CommandBuffer cmd, ColliderDescriptorSoA data, int offset, int count)
	{
		cmd.SetBufferData(Shape.Buffer, data.Shape, offset, offset, count);
		cmd.SetBufferData(PrevTransform.Buffer, data.PrevTransform, offset, offset, count);
		cmd.SetBufferData(PrevAabb.Buffer, data.PrevAabb, offset, offset, count);
		cmd.SetBufferData(Layer.Buffer, data.Layer, offset, offset, count);
		cmd.SetBufferData(Aabb.Buffer, data.Aabb, offset, offset, count);
		cmd.SetBufferData(Transform.Buffer, data.Transform, offset, offset, count);
	}
}
