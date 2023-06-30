using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using ShadowResolution = UnityEngine.Rendering.Universal.ShadowResolution;

public class GraphicsUpdater : SettingsUpdater
{
    [SerializeField] Volume volume;
    [SerializeField] GameObject details;
    [SerializeField] AudioMixer mixer;
    [SerializeField] float sfxBoost = 1.2f;
    [SerializeField] float musicBoost = 0.75f;
    bool muteWhenNoFocus = true;

    protected override void OnUpdateSettings (SettingsData settings)
    {
        UnityGraphicsBullshit.RenderScale = settings.renderScale;
        //UnityGraphicsBullshit.OpaqueTextureEnabled = settings.waterQuality == Quality3Steps.High;
        UnityGraphicsBullshit.MainLightCastShadows = settings.shadows != Quality2StepsOff.Off;
        UnityGraphicsBullshit.ShadowResolution = settings.shadows switch
        {
            Quality2StepsOff.High => ShadowResolution._2048,
            Quality2StepsOff.Low => ShadowResolution._1024,
            _ => ShadowResolution._256
        };
        /*UnityGraphicsBullshit.MsaaQuality = settings.antialiasing switch
        {
            Quality2StepsOff.High => MsaaQuality._4x,
            Quality2StepsOff.Low => MsaaQuality._2x,
            _ => MsaaQuality.Disabled
        };*/

        var profile = volume.profile;

        if(profile.TryGet<Bloom>(out var bloom))
        {
            bloom.active = settings.bloom != Quality2StepsOff.Off;
            bloom.highQualityFiltering = new BoolParameter(settings.bloom == Quality2StepsOff.High, true);
        }

        details.SetActive(settings.details == Quality2Steps.High);

        mixer.SetFloat("SFX", Mathf.Log10(Mathf.Max(settings.sfx * sfxBoost, 0.00001f)) * 20f);
        mixer.SetFloat("Music", Mathf.Log10(Mathf.Max(settings.music * musicBoost, 0.00001f)) * 20f);

        muteWhenNoFocus = settings.muteWhenHidden;
    }

    public void Update ()
    {
        bool muteAll = !Application.isFocused && muteWhenNoFocus;
        mixer.SetFloat("MasterVolume", muteAll ? -20 : 0);
    }
}
