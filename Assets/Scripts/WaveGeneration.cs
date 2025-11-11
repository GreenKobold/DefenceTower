using System;
using System.Collections.Generic;
using Unity.Android.Types;
using UnityEngine;

public class WaveGeneration : MonoBehaviour
{
    float timeTillWaveStarts;
    float timeTillNextWave;
    bool waveActive;
    int currentWave = 0;
    short totalWaveNumber = 0;
    float difficultyValue = 2.0f;
    List<BaseEnemyAI> AI = new List<BaseEnemyAI>();
    List<GameObject> ZombieSpawnPoints = new List<GameObject>();
    ////    waveNumber = 0

    ////    AliveZombies = 0
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //Start Initial Wave
        if (!waveActive)
        {
            if(timeTillWaveStarts ==0)
            {
                StartWave();
            }
            else
            {
                timeTillNextWave += Time.deltaTime;
            }
        }
        if (currentWave < totalWaveNumber)
        {
            if (timeTillNextWave == 0)
                NextWave();
            else
                timeTillNextWave -= Time.deltaTime;
        } else
        {
           StartWave();

            }
    }
    void StartWave()
    {
        AnnounceWave();
        ZombieSpawn();
        currentWave = 0;
        currentWave++;
    }
    void NextWave()
    {
        AnnounceWave();
        ZombieSpawn();
        currentWave++;
    }
    void ZombieSpawn()
    {

    }
    void AnnounceWave()
    {

    }
    void SetWaveComposition(bool commander, short nextTotalWaveNumber, float difficulty)
    {
        totalWaveNumber = nextTotalWaveNumber;
        ///Sets the possible zombie spawn list
        ////SetWaveNumber(int number)

    }
    public float GetCurrentDifficulty()
    {
        return difficultyValue;
    }
    public void SetCurrentDifficulty(float difficulty)
    {
        difficultyValue = difficulty;
    }
    public void PlayerStart()
    {
        timeTillNextWave = 0;
    }
}
