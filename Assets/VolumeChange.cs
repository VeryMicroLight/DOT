using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class VolumeChange : MonoBehaviour
{
    public AudioMixer mixer;
    public Slider BGMSlider;
    public Slider FXSlider;

    private void Awake()
    {

        // 给两个滑动条绑定不同的回调方法
        BGMSlider.onValueChanged.AddListener(OnBGMSliderChanged);
        FXSlider.onValueChanged.AddListener(OnFXSliderChanged);

        // 初始化滑动条默认值
        InitSliderDefaultValue();
    }

    private void InitSliderDefaultValue()
    {
        if (mixer.GetFloat("FXVolume", out float currentDb))
        {
            float sliderValue = Mathf.InverseLerp(-80f, 20f, currentDb);
            FXSlider.value = sliderValue;
        }
        if (mixer.GetFloat("BGMVolume", out float currentDb1))
        {
            float sliderValue = Mathf.InverseLerp(-80f, 20f, currentDb1);
            BGMSlider.value = sliderValue;
        }
    }

    // BGM滑动条的专属回调
    public void OnBGMSliderChanged(float sliderValue)
    {
        float volumeDb = Mathf.Lerp(-80f, 20f, sliderValue);
        mixer.SetFloat("BGMVolume", volumeDb);
    }

    // FX滑动条的专属回调
    public void OnFXSliderChanged(float sliderValue)
    {
        float volumeDb = Mathf.Lerp(-80f, 20f, sliderValue);
        mixer.SetFloat("FXVolume", volumeDb);
    }
}