using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Owlcat.Runtime.Visual.XPBD.Utilities;
using UnityEngine;

namespace Owlcat.Runtime.Visual.XPBD.Layouts;

public class GraphColorizer
{
	private List<List<int[]>> m_ColorizedConstraints = new List<List<int[]>>();

	private int m_ParticlesPerConstraintCount;

	private List<int> m_ParticleIndices = new List<int>();

	private List<int> m_ConstraintIndices = new List<int>();

	private List<List<int>> m_ConstraintsPerParticle = new List<List<int>>();

	public List<List<int[]>> ColorizedConstraints => m_ColorizedConstraints;

	public void AddConstraint(params int[] particles)
	{
		if (m_ParticlesPerConstraintCount == 0)
		{
			m_ParticlesPerConstraintCount = particles.Length;
		}
		else if (m_ParticlesPerConstraintCount != particles.Length)
		{
			throw new ArgumentException("Different constraint sizes are not allowed.", "particles");
		}
		for (int i = 0; i < particles.Length; i++)
		{
			while (particles[i] >= m_ConstraintsPerParticle.Count)
			{
				m_ConstraintsPerParticle.Add(new List<int>());
			}
			m_ConstraintsPerParticle[particles[i]].Add(m_ConstraintIndices.Count);
		}
		m_ConstraintIndices.Add(m_ParticleIndices.Count);
		m_ParticleIndices.AddRange(particles);
	}

	public void Clear()
	{
		m_ColorizedConstraints.Clear();
		m_ParticlesPerConstraintCount = 0;
		m_ParticleIndices.Clear();
		m_ConstraintIndices.Clear();
		for (int i = 0; i < m_ConstraintsPerParticle.Count; i++)
		{
			m_ConstraintsPerParticle[i].Clear();
		}
	}

	public IEnumerator Colorize(string progressDescription)
	{
		List<int> colors = new List<int>();
		m_ConstraintIndices.Add(m_ParticleIndices.Count);
		int constraintCount = Mathf.Max(0, m_ConstraintIndices.Count - 1);
		colors.Clear();
		if (constraintCount == 0)
		{
			yield break;
		}
		colors.Capacity = constraintCount;
		bool[] availability = new bool[constraintCount];
		for (int j = 0; j < constraintCount; j++)
		{
			colors.Add(-1);
			availability[j] = true;
		}
		int i = 0;
		while (i < constraintCount)
		{
			for (int k = m_ConstraintIndices[i]; k < m_ConstraintIndices[i + 1]; k++)
			{
				foreach (int item in m_ConstraintsPerParticle[m_ParticleIndices[k]])
				{
					if (i != item && colors[item] >= 0)
					{
						availability[colors[item]] = false;
					}
				}
			}
			colors[i] = 0;
			int value;
			while (colors[i] < constraintCount && !availability[colors[i]])
			{
				int index = i;
				value = colors[index] + 1;
				colors[index] = value;
			}
			for (int l = 0; l < constraintCount; l++)
			{
				availability[l] = true;
			}
			yield return new CoroutineJob.ProgressInfo(progressDescription, (float)i / (float)constraintCount);
			value = i + 1;
			i = value;
		}
		int num = colors.Max() + 1;
		for (int m = 0; m < num; m++)
		{
			m_ColorizedConstraints.Add(new List<int[]>());
		}
		for (int n = 0; n < constraintCount; n++)
		{
			int index2 = colors[n];
			int num2 = m_ConstraintIndices[n];
			int[] array = new int[m_ParticlesPerConstraintCount];
			for (int num3 = 0; num3 < m_ParticlesPerConstraintCount; num3++)
			{
				array[num3] = m_ParticleIndices[num2 + num3];
			}
			m_ColorizedConstraints[index2].Add(array);
		}
	}
}
