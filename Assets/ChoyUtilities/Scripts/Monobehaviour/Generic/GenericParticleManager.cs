using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace EugeneC.Singleton
{
	public abstract class GenericParticleManager<T, U> : GenericSingleton<U>
		where T : Enum
		where U : MonoBehaviour
	{
		[Serializable]
		public struct ParticleSerialize
		{
			public T id;
			public ParticleSystem particle;
		}

		[Serializable]
		protected struct SystemSerialize
		{
			public ParticleSystem[] particles;
			public int currentIndex;
			public int previousIndex;
		}

		[SerializeField] protected ParticleSerialize[] particleEffects;

		[Tooltip("The pool count meant each particle serialize, meaning total = poolCount * particleEffects.Length")]
		[SerializeField]
		protected byte poolCount = 16;

		protected SystemSerialize[] ParticleSystems;
		protected List<int> PauseIndexes;

		protected virtual async void Start()
		{
			try
			{
				await Awaitable.WaitForSecondsAsync(0.5f);

				ParticleSystems = new SystemSerialize[particleEffects.Length];
				var currentSystem = 0;

				foreach (var particle in particleEffects)
				{
					ParticleSystems[currentSystem].particles = new ParticleSystem[poolCount];
					for (var i = 0; i < poolCount; i++)
					{
						if (particle.particle is null) continue;
						var spawn = Instantiate(particle.particle, transform);
						ParticleSystems[currentSystem].particles[i] = spawn;
					}

					currentSystem++;
				}
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
		}

		public virtual void PlayEffectAtPosition(T id, float3 position)
		{
			if (!Enum.IsDefined(typeof(T), id)) return;
			var index = Array.FindIndex(particleEffects, i => EqualityComparer<T>.Default.Equals(i.id, id));

			var particle = ParticleSystems[index].particles[ParticleSystems[index].currentIndex];
			particle.transform.position = position;
			particle.Play();

			ParticleSystems[index].previousIndex = ParticleSystems[index].currentIndex;
			ParticleSystems[index].currentIndex++;
			ParticleSystems[index].currentIndex %= ParticleSystems[index].particles.Length;
		}

		public virtual void PauseAllEffects()
		{
			PauseIndexes = new();
			for (var i = 0; i < ParticleSystems.Length; i++)
			{
				var system = ParticleSystems[i];
				foreach (var p in system.particles)
				{
					if (p.isPlaying)
						p.Pause();
				}

				PauseIndexes.Add(i);
			}
		}

		public virtual void ResumeAllEffects()
		{
			if (PauseIndexes is null) return;
			foreach (var i in PauseIndexes)
			{
				var system = ParticleSystems[i];
				foreach (var p in system.particles)
				{
					if (p.isPaused)
						p.Play();
				}
			}

			PauseIndexes.Clear();
		}
	}
}