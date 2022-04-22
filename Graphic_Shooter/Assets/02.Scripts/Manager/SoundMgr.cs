using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Sound 
{
    public string ScoundName = null;
    public AudioClip Clip = null;
}

public class SoundMgr : MonoBehaviour
{
    // 사운드 메니저 싱글톤설정

    static public SoundMgr instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(this.gameObject);
    }

    [Header("배경음악")]
    public Sound[] bgmSounds;
    [Header("효과음")]
    public Sound[] SoundEffects;
    [Space(10)]
    [Header("배경음악 AudioSource")]
    public AudioSource audioSourceBGM;
    [Header("효과음 AudioSource")]
    public AudioSource[] audioSourceEffect;

    public string[] playSoundName;

    // Start is called before the first frame update
    void Start()
    {
        playSoundName = new string[audioSourceEffect.Length];
    }

    public void PlaySE(string a_soundName)
    {
        for (int i = 0; i < SoundEffects.Length; i++)
        {
            if(a_soundName == SoundEffects[i].ScoundName)
            {
                for (int j = 0; j < audioSourceEffect.Length; j++)
                {
                    if (!audioSourceEffect[j].isPlaying)
                    {
                        audioSourceEffect[j].clip = SoundEffects[i].Clip;
                        audioSourceEffect[j].Play();
                        playSoundName[j] = SoundEffects[i].ScoundName;
                        return;
                    }
                }
                return;
            }
        }
    }
    public void PlayBGM(string a_soundName)
    {
        for (int i = 0; i < bgmSounds.Length; i++)
        {
            if (a_soundName == bgmSounds[i].ScoundName)
            {
                audioSourceBGM.clip = bgmSounds[i].Clip;
                audioSourceBGM.Play();
                return;
            }
        }
    }

    public void StopAllSoundEffect()
    {
        for (int i = 0; i < audioSourceEffect.Length; i++)
        {
            audioSourceEffect[i].Stop();
        }
    }

    public void StopSE(string a_soundName)
    {
        for (int i = 0; i < audioSourceEffect.Length; i++)
        {
            if (playSoundName[i] == a_soundName)
            {
                audioSourceEffect[i].Stop();
                break;
            }
        }
    }
    public void Step()
    {
     //   Debug.Log("Step");
    }
}
