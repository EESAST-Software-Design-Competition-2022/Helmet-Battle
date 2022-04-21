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
    //ÊÜÉË
    public void HurtAudio()
    {
        //soundSource.clip = hurtAudio;
        //soundSource.Play();
    }

    //ËÀÍö
    public void DieAudio()
    {
        //soundSource.clip = dieAudio;
        //soundSource.Play();
    }

    //¼ñµ½Í·¿ø
    public void HelmetAudio()
    {
        soundSource.clip = helmetAudio;
        soundSource.Play();
    }

    //Ê±´úÉý¼¶
    public void UpgradeAudio()
    {
        soundSource.clip = upgradeAudio;
        soundSource.Play();
    }
    //ÇÐ»»ÎäÆ÷
    public void ChangeWeaponAudio()
    {
        playerSource.clip = changeWeaponAudio;
        playerSource.Play();
    }
    //ÏÝÚå×©
    //public void TrapAudio()
    //{
    //    soundSource.clip = trapAudio;
    //    soundSource.Play();
    //}


    //ºÚ¶´
    //public void BlackholeAudio()
    //{
    //    soundSource.clip = blackholeAudio;
    //    soundSource.Play();
    //}

    //´«ËÍÃÅ
    //public void TransdoorAudio()
    //{
    //    soundSource.clip = transdoorAudio;
    //    soundSource.Play();
    //}

    //±¬Õ¨
    public void BoomAudio()
    {
        soundSource.clip = boomAudio;
        soundSource.Play();
    }

    //·¢¶¯»ú
    public void EngineAudio()
    {
        //engineSource.clip = engineAudio;
        //engineSource.loop = true;
        //engineSource.Play();
    }
    //µ²×Óµ¯
    public void HitBulletAudio()
    {
        soundSource.clip = hitBulletAudio;
        soundSource.Play();
    }
    //ÊÖ¹¥»÷
    public void HandAttackAudio()
    {

        soundSource.clip = handAttackAudio;
        soundSource.Play();
    }

    //Ê¯Í·¹¥»÷
    public void StoneAttackAudio()
    {
        soundSource.clip = stoneAttackAudio;
        soundSource.Play();
    }

    //Ã¬¹¥»÷
    public void StaffAttackAudio()
    {
        soundSource.clip = speerAttackAudio;
        soundSource.Play();
    }

    //½£¹¥»÷
    public void SwordAttackAudio()
    {
        soundSource.clip = swordAttackAudio;
        soundSource.Play();
    }

    //åó¹¥»÷
    public void BowAttackAudio()
    {
        soundSource.clip = bowAttackAudio;
        soundSource.Play();
    }


    //»ðï¥¹¥»÷
    public void FirespearAttackAudio()
    {
        soundSource.clip = firespearAttackAudio;
        soundSource.Play();
    }

    //ÊÖÇ¹¹¥»÷
    public void PistolAttackAudio()
    {
        soundSource.clip = pistolAttackAudio;
        soundSource.Play();
    }
    //»ð¼ýÅÚ¹¥»÷
    public void RocketAttackAudio()
    {
        soundSource.clip = rocketAttackAudio;
        soundSource.Play();
    }
    //ÊÖÀ×¹¥»÷
    public void HandgrenadeAttackAudio()
    {
        soundSource.clip = HandgrenadeAudio;
        soundSource.Play();
    }
    //µ¼µ¯¹¥»÷
    public void MissleAttackAudio()
    {
        soundSource.clip = missleAttackAudio;
        soundSource.Play();
    }
    //¹â½£¹¥»÷
    public void LightSwordAttackAudio()
    {
        soundSource.clip = LightSwordAudio;
        soundSource.Play();
    }
    //ÀØÉäÅÚ¹¥»÷
    public void LaserAttackAudio()
    {
        soundSource.clip = laserAttackAudio;
        soundSource.Play();
    }
    #endregion
}

