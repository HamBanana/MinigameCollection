using System.Collections.Generic;
using UnityEngine;

using Ham.GameControl;
using System;

namespace Ham.GameControl.Sound
{

    public class SoundManager : Manager
    {
        [Header("Audio Sources")]
        [SerializeField]
        public AudioSource MusicSource;

        [SerializeField]
        public AudioSource SfxSource;

        [Header("Audio Clips")]
        public List<AudioClip> MusicClips;
        public List<AudioClip> SfxClips;

        public bool IsMusicEnabled = true;
        public bool IsSfxEnabled = true;

        // Event specific sounds.
        public AudioClip ValidActionSfx;
        public AudioClip InvalidActionSfx;
        public AudioClip GameStartedSfx;
        public AudioClip SceneStartedSfx;
        public AudioClip GameEndedSfx;

        private Dictionary<string, AudioClip> musicLookup = new();
        private Dictionary<string, AudioClip> sfxLookup = new();

        protected override void Awake()
        {
            base.Awake();
            Debug.Log("SoundManager: Awake()");

            foreach (AudioClip clip in SfxClips)
            {
                if (clip != null)
                {
                    sfxLookup[clip.name] = clip;
                }
            }

            foreach (AudioClip clip in MusicClips)
            {
                if (clip != null)
                {
                    musicLookup[clip.name] = clip;
                }
            }

            if (!MusicSource)
            {
                MusicSource = this.gameObject.AddComponent<AudioSource>();
            }

            if (!SfxSource)
            {
                SfxSource = this.gameObject.AddComponent<AudioSource>();
            }
        }

        protected override void Start()
        {
            this.onControllerAction += (sender, e) => {
                Debug.Log("SoundManager: Received controller action event.");
                Controller controller = (Controller)sender;
                this.SfxSource.PlayOneShot(controller.TestSound);
            };
        }

        public void PlaySFX(string clipname)
        {
            if (sfxLookup.TryGetValue(clipname, out AudioClip clip))
            {
                SfxSource.PlayOneShot(clip);
            }
            else
            {
                Debug.LogWarning("SoundManager: SFX clip not found: " + clipname);
            }
        }

        public void PlayMusic(string clipname)
        {
            if (musicLookup.TryGetValue(clipname, out AudioClip clip))
            {
                MusicSource.clip = clip;
                MusicSource.Play();
            }
            else
            {
                Debug.LogWarning("SoundManager: Music clip not found: " + clipname);
            }
        }

        public void StopMusic()
        {
            MusicSource.Stop();
        }

        public void SetVolume(float sfx, float music)
        {
            SfxSource.volume = Mathf.Clamp01(sfx);
            MusicSource.volume = Mathf.Clamp01(music);
        }

        public void MuteMusic()
        {
            this.IsMusicEnabled = false;
            this.MusicSource.mute = true;
        }

        public void UnmuteMusic()
        {
            this.IsMusicEnabled = true;
            this.MusicSource.mute = false;
        }

        public void MuteSfx()
        {
            this.IsSfxEnabled = false;
            this.MusicSource.mute = true;
        }

        public void UnmuteSfx()
        {
            this.IsSfxEnabled = true;
            this.MusicSource.mute = true;
        }

        private void playFromGameobject(PlayFromGameobjectEventArgs e)
        {
            AudioSource src = e.AudioSourceGameObject.AddComponent<AudioSource>();
            if (sfxLookup.TryGetValue(e.AudioClipName, out AudioClip clip))
            {
                src.PlayOneShot(clip);
            } else {
                Debug.Log("SoundManager: playFromGameobject: Sound '" + e.AudioClipName +"' not found.");
            }
            Destroy(src);
        }
    }

    public class PlayFromGameobjectEventArgs : EventArgs
    {
        public GameObject AudioSourceGameObject;
        public string AudioClipName;
    }
}
