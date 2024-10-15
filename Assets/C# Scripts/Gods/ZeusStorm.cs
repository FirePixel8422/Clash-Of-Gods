using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;


public class ZeusStorm : MonoBehaviour
{
    public int poolObjectIndex;

    public Transform boundsCenter;
    public float yPos;
    public Vector2 spawnBox;

    public int spawnAmountMin, spawnAmountMax;
    public float waveTimeMin, waveTimeMax;

    public float strikeDelayMin, strikeDelayMax;

    private AudioController audioController;
    public float soundDelay;
    public float randomPitchMin, randomPitchMax;
    public float randomVolumeMin, randomVolumeMax;


    private void Start()
    {
        audioController = GetComponent<AudioController>();
        audioController.Init();

        StartCoroutine(LightningLoop());

        SettingsManager.SingleTon.AddAudioController(audioController);

        if (boundsCenter.position.z < 0)
        {
            boundsCenter.position = new Vector3(boundsCenter.position.x, boundsCenter.position.y, -boundsCenter.position.z);
        }
    }

    private IEnumerator LightningLoop()
    {
        yield return new WaitForSeconds(2); 


        while (true)
        {
            yield return new WaitForSeconds(Random.Range(waveTimeMin, waveTimeMax) - soundDelay);

            StartCoroutine(CallLightning());

            yield return new WaitForSeconds(soundDelay);

            audioController.Play(Random.Range(randomVolumeMin, randomVolumeMax), Random.Range(randomPitchMin, randomPitchMax));
        }
    }
    private IEnumerator CallLightning()
    {
        int amount = Random.Range(spawnAmountMin, spawnAmountMax);

        HashSet<GameObject> targets = new HashSet<GameObject>();

        for (int i = 0; i < amount; i++)
        {
            yield return new WaitForSeconds(Random.Range(strikeDelayMin, strikeDelayMax));

            GameObject spawnedObj = OnHitVFXPooling.Instance.GetPulledObj(0, boundsCenter.position + new Vector3(Random.Range(-spawnBox.x, spawnBox.x), yPos, Random.Range(-spawnBox.y, spawnBox.y)), Quaternion.identity);
            targets.Add(spawnedObj);
        }


        yield return new WaitForSeconds(4);

        foreach (var target in targets)
        {
            target.GetComponent<VisualEffect>().Stop();
        }
    }


    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(boundsCenter.position, new Vector3(spawnBox.x * 2, 10, spawnBox.y * 2));
    }

    private void OnDestroy()
    {
        SettingsManager.SingleTon.RemoveAudioController(audioController);
    }
}
