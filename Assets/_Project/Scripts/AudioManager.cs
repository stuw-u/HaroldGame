using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AudioClipName
{
    CrateCollect,
    CrateCollect10,
    MenuClick,
    MenuStart,
    PlayerDepthScream,
    LaserRunOff,
    LaserRecragedCue,
    MineBlowUp,
    MineDrop,
    WasteBucketSwoosh,
    PlayerLand,
    RoseWaddle,
    RoseDeath,
    PineLegTick,
    PineTreeDamage,
    PineTreeDeath,
    AloesAttackSound,
    AloesDamaged,
    AloesDeath,
    LotusSurge,
    LotusBite,
    LotusDamaged,
    LotusDeath,
    ReachEnd,
    BurnTick,
    SuperScore
}

[System.Serializable]
public class AudioEntry
{
    public AudioClip clip;
    public AudioClipName key;
}

public class AudioManager : MonoBehaviour
{
    [SerializeField] AudioSource sourcePrefab;
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource sirenSource;
    [SerializeField] AudioEntry[] entries;
    Dictionary<AudioClipName, AudioClip> clipDict;
    Queue<AudioSource> queue;
    float musicPitchFade = 0f;

    private static AudioManager inst;
    private void Awake ()
    {
        inst = this;
        clipDict = new Dictionary<AudioClipName, AudioClip>();
        queue = new Queue<AudioSource>();
        foreach(var kvp in entries)
        {
            if (clipDict.ContainsKey(kvp.key)) continue;
            clipDict.Add(kvp.key, kvp.clip);
        }
    }

    private void Update ()
    {
        musicPitchFade = Mathf.Clamp01(musicPitchFade + Time.deltaTime * 
            (!GameManager.ShouldMusicPlay ? -1 : 1));
        musicSource.pitch = musicPitchFade;
        musicSource.volume = musicPitchFade;

        sirenSource.volume = EnemyManager.InDanger() ? 0.2f : 0;
    }

    public static void Play (AudioClipName name, Vector3 position, bool global = false, float pitch = 1f, float volume = 1f)
    {
        if (!inst.clipDict.ContainsKey(name)) return;
        if (inst.clipDict[name] == null) return;

        float length = pitch * inst.clipDict[name].length * 1.5f;
        if(!inst.queue.TryDequeue(out AudioSource source))
        {
            source = Instantiate(inst.sourcePrefab);
        }
        source.enabled = true;
        source.spatialBlend = global ? 0 : 1;
        source.transform.position = position;
        source.pitch = pitch;
        source.volume = volume;
        source.PlayOneShot(inst.clipDict[name]);
        inst.StartCoroutine(inst.Collect(source, length));
    }

    IEnumerator Collect (AudioSource source, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        source.enabled = false;
        queue.Enqueue(source);
    }
}
