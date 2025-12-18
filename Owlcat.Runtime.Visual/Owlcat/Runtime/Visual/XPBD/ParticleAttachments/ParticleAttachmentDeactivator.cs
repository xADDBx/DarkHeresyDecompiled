using System;
using Owlcat.Runtime.Visual.XPBD.Authoring;
using Owlcat.Runtime.Visual.XPBD.Bodies;
using Owlcat.Runtime.Visual.XPBD.Layouts;
using Owlcat.Runtime.Visual.XPBD.Particles;
using Unity.Collections;

namespace Owlcat.Runtime.Visual.XPBD.ParticleAttachments;

public class ParticleAttachmentDeactivator : IDisposable
{
	public NativeList<int> ParticleIndices;

	public NativeList<float> ParticleInvMass;

	public ParticleAttachmentDeactivator()
	{
		ParticleIndices = new NativeList<int>(16, Allocator.Persistent);
		ParticleInvMass = new NativeList<float>(16, Allocator.Persistent);
	}

	public void Dispose()
	{
		ParticleIndices.Dispose();
		ParticleInvMass.Dispose();
	}

	internal void AddParticlesForRestore(ParticleAttachment attachment, in ParticleAttachmentDescriptor descriptor)
	{
		if (attachment.IsValid && attachment.Body.IsValid)
		{
			LayoutBase layoutBase = attachment.Body.LayoutBase;
			ParticleGroup particleGroup = attachment.ParticleGroup;
			for (int i = 0; i < particleGroup.ParticleIndices.Count; i++)
			{
				int num = particleGroup.ParticleIndices[i];
				ref NativeList<int> particleIndices = ref ParticleIndices;
				int value = num + descriptor.BodyParticlesRange.x;
				particleIndices.Add(in value);
				ref NativeList<float> particleInvMass = ref ParticleInvMass;
				Particle particle = layoutBase.BodyStructure.Particles[num];
				particleInvMass.Add(in particle.InvMass);
			}
		}
	}

	internal void ClearParticlesForRestore()
	{
		ParticleIndices.Clear();
		ParticleInvMass.Clear();
	}

	internal bool HasParticlesForRestore()
	{
		return ParticleIndices.Length > 0;
	}
}
