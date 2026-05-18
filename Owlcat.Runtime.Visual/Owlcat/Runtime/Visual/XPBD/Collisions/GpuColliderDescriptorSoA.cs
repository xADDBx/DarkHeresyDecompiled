using Owlcat.Runtime.Visual.XPBD.DataStructures;
using Owlcat.Runtime.Visual.XPBD.SoA;
using UnityEngine.Rendering;

namespace Owlcat.Runtime.Visual.XPBD.Collisions;

public class GpuColliderDescriptorSoA : GpuStructureOfArrays<ColliderDescriptor, ColliderDescriptorSoA>
{
	public GraphicsBufferWrapper<Aabb> PrevAabb;

	public GraphicsBufferWrapper<Aabb> Aabb;

	public GraphicsBufferWrapper<AffineTransform> PrevTransform;

	public GraphicsBufferWrapper<ColliderShape> Shape;

	public GraphicsBufferWrapper<AffineTransform> Transform;

	public GraphicsBufferWrapper<int> Layer;

	public GpuColliderDescriptorSoA(int size)
		: base(size)
	{
		PrevAabb = new GraphicsBufferWrapper<Aabb>("_XpbdColliderDescriptorPrevAabbBuffer", size);
		m_Buffers.Add(PrevAabb);
		Aabb = new GraphicsBufferWrapper<Aabb>("_XpbdColliderDescriptorAabbBuffer", size);
		m_Buffers.Add(Aabb);
		PrevTransform = new GraphicsBufferWrapper<AffineTransform>("_XpbdColliderDescriptorPrevTransformBuffer", size);
		m_Buffers.Add(PrevTransform);
		Shape = new GraphicsBufferWrapper<ColliderShape>("_XpbdColliderDescriptorShapeBuffer", size);
		m_Buffers.Add(Shape);
		Transform = new GraphicsBufferWrapper<AffineTransform>("_XpbdColliderDescriptorTransformBuffer", size);
		m_Buffers.Add(Transform);
		Layer = new GraphicsBufferWrapper<int>("_XpbdColliderDescriptorLayerBuffer", size);
		m_Buffers.Add(Layer);
	}

	public override void SetData(ColliderDescriptorSoA data)
	{
		PrevAabb.SetData(data.PrevAabb);
		Aabb.SetData(data.Aabb);
		PrevTransform.SetData(data.PrevTransform);
		Shape.SetData(data.Shape);
		Transform.SetData(data.Transform);
		Layer.SetData(data.Layer);
	}

	public override void SetData(ColliderDescriptorSoA data, int offset, int count)
	{
		PrevAabb.SetData(data.PrevAabb, offset, offset, count);
		Aabb.SetData(data.Aabb, offset, offset, count);
		PrevTransform.SetData(data.PrevTransform, offset, offset, count);
		Shape.SetData(data.Shape, offset, offset, count);
		Transform.SetData(data.Transform, offset, offset, count);
		Layer.SetData(data.Layer, offset, offset, count);
	}

	public override void SetData(CommandBuffer cmd, ColliderDescriptorSoA data)
	{
		cmd.SetBufferData(PrevAabb.Buffer, data.PrevAabb);
		cmd.SetBufferData(Aabb.Buffer, data.Aabb);
		cmd.SetBufferData(PrevTransform.Buffer, data.PrevTransform);
		cmd.SetBufferData(Shape.Buffer, data.Shape);
		cmd.SetBufferData(Transform.Buffer, data.Transform);
		cmd.SetBufferData(Layer.Buffer, data.Layer);
	}

	public override void SetData(CommandBuffer cmd, ColliderDescriptorSoA data, int offset, int count)
	{
		cmd.SetBufferData(PrevAabb.Buffer, data.PrevAabb, offset, offset, count);
		cmd.SetBufferData(Aabb.Buffer, data.Aabb, offset, offset, count);
		cmd.SetBufferData(PrevTransform.Buffer, data.PrevTransform, offset, offset, count);
		cmd.SetBufferData(Shape.Buffer, data.Shape, offset, offset, count);
		cmd.SetBufferData(Transform.Buffer, data.Transform, offset, offset, count);
		cmd.SetBufferData(Layer.Buffer, data.Layer, offset, offset, count);
	}
}
