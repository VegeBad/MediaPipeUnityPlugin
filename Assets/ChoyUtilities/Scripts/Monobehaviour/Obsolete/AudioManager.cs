using System;
using System.Collections.Generic;
using EugeneC.Singleton;
using UnityEngine;
using UnityEngine.Audio;

namespace EugeneC.Obsolete
{
	public class AudioManager : GenericSingleton<AudioManager>
	{
		public AudioSource SourcePrefab;
		public AudioSfxOld[] Audiosfx;

		Dictionary<AudioType, AudioSfxOld> AudioDictionary = new();

		protected override void Awake()
		{
			base.Awake();
			foreach (AudioSfxOld sfx in Audiosfx)
			{
				AudioDictionary[sfx.AudioID] = sfx;
			}
		}

		public void PlaySourceClip(AudioType AudioId, AudioMixerGroup Channel, Vector3 position)
		{
			if (AudioDictionary.TryGetValue(AudioId, out AudioSfxOld sfx))
			{
				AudioSource source = Instantiate(SourcePrefab, position, Quaternion.identity);
				int key = UnityEngine.Random.Range(0, sfx.AudioClips.Length);
				AudioClip clip = sfx.AudioClips[key];
				source.outputAudioMixerGroup = Channel;
				source.PlayOneShot(clip);
				Destroy(source.gameObject, clip.length);
			}
		}

		public void PlayCLip(AudioType AudioId, AudioSource Source)
		{
			if (AudioDictionary.TryGetValue(AudioId, out AudioSfxOld sfx))
			{
				int key = UnityEngine.Random.Range(0, sfx.AudioClips.Length);
				AudioClip c = sfx.AudioClips[key];
				Source.PlayOneShot(c);
			}
		}

		public void PlayLoopClip(AudioType AudioId, AudioSource Source)
		{
			Source.loop = true;
			PlayCLip(AudioId, Source);
		}

		public void PlayIgnorePauseClip(AudioType AudioId, AudioSource Source)
		{
			Source.ignoreListenerPause = true;
			PlayCLip(AudioId, Source);
		}

		public void PlayOverrideClip(AudioType AudioId, AudioSource Source)
		{
			Source.loop = false;
			Source.Stop();
			PlayIgnorePauseClip(AudioId, Source);
		}
	}

	[Serializable]
	public class AudioSfxOld
	{
		public AudioType AudioID;
		public AudioClip[] AudioClips;
	}

	public enum AudioType
	{
		Bugeat,
		BugWalk,
		BugDead
	}
}