using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;
    public AudioMixer MusicMixer;
    public AudioMixer SoundMixer; 
    public AudioSource musicSource;
    public AudioSource soundSource;
    public AudioSource playerSource;
    public AudioClip[] MusicsWaiting;
    public AudioClip[] MusicsGaming;
    [SerializeField]
    private AudioClip hitBulletAudio, helmetAudio, upgradeAudio, boomAudio;
    [SerializeField]
    private AudioClip HandgrenadeAudio, LightSwordAudio;
    [SerializeField]
    private AudioClip handAttackAudio, speerAttackAudio, stoneAttackAudio, swordAttackAudio, bowAttackAudio, firespearAttackAudio, pistolAttackAudio, rocketAttackAudio, missleAttackAudio, laserAttackAudio;
    [SerializeField]
    private AudioClip changeWeaponAudio;
    public void Awake()
    {
        instance = this;
    }
    private void Start()
    {
        //SceneManager.activeSceneChanged += OnSceneChanged;
        if(SceneManager.GetActiveScene().name == "LobbyOffline")
        {
            PlayMusicWaiting();
        }
    }
    public void SetMusicVolume(float value)
    {
        MusicMixer.SetFloat("MusicVolume", value);
    }
    public void SetSoundVolume(float value)
    {
        SoundMixer.SetFloat("SoundVolume", value);
    }
    #region
    public void PlayMusic(AudioClip music)
    {
        musicSource.clip = music;
        musicSource.loop = true;
        musicSource.Play();
    }
    public void PlayMusicWaiting()
    {
        foreach(var mw in MusicsWaiting)
        {
            if (musicSource.clip == mw)
                return;
        }
        PlayMusic(MusicsWaiting[Random.Range(0, MusicsWaiting.Length)]);
    }
    public void PlayMusicGaming()
    {
        StartCoroutine(StartGameMusic());
    }
    IEnumerator StartGameMusic()
    {
        int index = 0;
        while (SceneManager.GetActiveScene().name == "MapRandom")
        {
            if(index >= MusicsGaming.Length)
            {
                index = 0;
            }
            var clip = MusicsGaming[index];
            index++;
            PlayMusic(clip);
            yield return new WaitForSecondsRealtime(clip.length);
        }
    }
    public void StopMusic()
    {
        musicSource.Stop();
    }
    //????
    public void HurtAudio()
    {
        //soundSource.clip = hurtAudio;
        //soundSource.Play();
    }

    //????
    public void DieAudio()
    {
        //soundSource.clip = dieAudio;
        //soundSource.Play();
    }

    //????????
    public void HelmetAudio()
    {
        soundSource.clip = helmetAudio;
        soundSource.Play();
    }

    //????????
    public void UpgradeAudio()
    {
        soundSource.clip = upgradeAudio;
        soundSource.Play();
    }
    //????????
    public void ChangeWeaponAudio()
    {
        playerSource.clip = changeWeaponAudio;
        playerSource.Play();
    }
    //??????
    //public void TrapAudio()
    //{
    //    soundSource.clip = trapAudio;
    //    soundSource.Play();
    //}


    //????
    //public void BlackholeAudio()
    //{
    //    soundSource.clip = blackholeAudio;
    //    soundSource.Play();
    //}

    //??????
    //public void TransdoorAudio()
    //{
    //    soundSource.clip = transdoorAudio;
    //    soundSource.Play();
    //}

    //????
    public void BoomAudio()
    {
        soundSource.clip = boomAudio;
        soundSource.Play();
    }

    //??????
    public void EngineAudio()
    {
        //engineSource.clip = engineAudio;
        //engineSource.loop = true;
        //engineSource.Play();
    }
    //??????
    public void HitBulletAudio()
    {
        soundSource.clip = hitBulletAudio;
        soundSource.Play();
    }
    //??????
    public void HandAttackAudio()
    {

        soundSource.clip = handAttackAudio;
        soundSource.Play();
    }

    //????????
    public void StoneAttackAudio()
    {
        soundSource.clip = stoneAttackAudio;
        soundSource.Play();
    }

    //??????
    public void StaffAttackAudio()
    {
        soundSource.clip = speerAttackAudio;
        soundSource.Play();
    }

    //??????
    public void SwordAttackAudio()
    {
        soundSource.clip = swordAttackAudio;
        soundSource.Play();
    }

    //??????
    public void BowAttackAudio()
    {
        soundSource.clip = bowAttackAudio;
        soundSource.Play();
    }


    //????????
    public void FirespearAttackAudio()
    {
        soundSource.clip = firespearAttackAudio;
        soundSource.Play();
    }

    //????????
    public void PistolAttackAudio()
    {
        soundSource.clip = pistolAttackAudio;
        soundSource.Play();
    }
    //??????????
    public void RocketAttackAudio()
    {
        soundSource.clip = rocketAttackAudio;
        soundSource.Play();
    }
    //????????
    public void HandgrenadeAttackAudio()
    {
        soundSource.clip = HandgrenadeAudio;
        soundSource.Play();
    }
    //????????
    public void MissleAttackAudio()
    {
        soundSource.clip = missleAttackAudio;
        soundSource.Play();
    }
    //????????
    public void LightSwordAttackAudio()
    {
        soundSource.clip = LightSwordAudio;
        soundSource.Play();
    }
    //??????????
    public void LaserAttackAudio()
    {
        soundSource.clip = laserAttackAudio;
        soundSource.Play();
    }
    #endregion
}

