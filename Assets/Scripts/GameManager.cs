using System.Buffers.Text;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.WSA;

public class GameManager : MonoBehaviour
{
    bool playerWin = false;
    bool playerLoss = false;
    bool roundEnd = false;
    public WaveGeneration waveGeneration;
    public Core playerCore;

    float intialPlayerHP = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destroy duplicate GameManagers
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist across scenes
        }
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (roundEnd)
        {
            EvaluatePlayerPerformance();
            AdjustDifficulty();
        }
        else
            return;
    }
    void EvaluatePlayerPerformance()
    {
        float tempDiff = waveGeneration.GetCurrentDifficulty();
        //Checks Player Damage Taken
        float healthDifference = intialPlayerHP - playerCore.GetHP();
        if(healthDifference > 0)
        {

        }
        //Check Round time
        //Scales difficulty rating based on former

    }
    void AdjustDifficulty()
    {

        //    Makes it more difficult if player took less time / damage using difficulty rating inside of the wave manager
        //        Scale Enemy stats to be stronger
        //        Limit the production of turrets that did the most damage using difficulty rating inside of the wave manager
        //        Increase Wave amount

        //Makes it easier if player too more time / damage

        //    Scale Enemy stats to be weaker
    }
    public void EndRound()
    {
        roundEnd = true;
    }
    public void RoundStart()
    {
        
    }
}
