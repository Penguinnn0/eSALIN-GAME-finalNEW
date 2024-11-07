using System;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeSettings : MonoBehaviour
{
    [SerializeField] private AudioMixer myMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        if (PlayerPrefs.HasKey("musicVol"))
        {
            LoadVol();
        }
        else
        {
        SetMusicVol();
        SetMusicVol();
        }
    }

    public void SetMusicVol()
    {
        float vol = musicSlider.value;
        myMixer.SetFloat("Music", Mathf.Log10(vol)*20);
        PlayerPrefs.SetFloat("musicVol", vol);
    }

    public void SetSFXVol()
    {
        float vol = sfxSlider.value;
        myMixer.SetFloat("SFX", Mathf.Log10(vol)*20);
        PlayerPrefs.SetFloat("sfxVol", vol);
    }

    public void LoadVol()
    {
        musicSlider.value = PlayerPrefs.GetFloat("musicVol");
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVol");

        SetMusicVol();
        SetSFXVol();
    }
}
