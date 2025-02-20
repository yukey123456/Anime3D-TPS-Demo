using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomAISpawn : MonoBehaviour
{
    public List<AISpawnData> spawnPropertiesList;
    
    readonly WaitForSeconds waitBetweenSpawnProps = new WaitForSeconds(0.1f);

    private IEnumerator Start()
    {
        for (int i = 0; i < spawnPropertiesList.Count; i++)
        {
            yield return waitBetweenSpawnProps;
            var spawnSpec = new AISpawnSpec(spawnPropertiesList[i]);
            StartCoroutine(spawnSpec.SpawnWaveRoutine(this));
        }
    }

    /// <summary>
    /// Spawn a single AI of specific <seealso cref="AISpawnData"/>
    /// </summary>
    /// <param name="spawnName">Spawn Propertie Name</param>
    public void Spawn(string spawnName)
    {
        var spawnProp = spawnPropertiesList.Find(sp => sp.SpawnID.Equals(spawnName));
        if (spawnProp != null)
        {
            //StartCoroutine(spawnProp.Spawn(this, OnAISpawned, true));
        }
    }

    /// <summary>
    /// Spawn a single AI of specific  <seealso cref="AISpawnData"/>
    /// </summary>
    /// <param name="index">Spawn Propertie Index</param>
    public void Spawn(int index)
    {
        if (spawnPropertiesList.Count > 0 && index < spawnPropertiesList.Count)
        {
            //StartCoroutine(spawnPropertiesList[index].Spawn(this, null, true));
        }
    }

    /// <summary>
    /// Spawn a single AI of all <seealso cref="AISpawnData"/>
    /// </summary>
    public void SpawnOneOfAll()
    {
        StartCoroutine(SpawnOneOfAllRoutine());
    }

    /// <summary>
    /// Start a specific <seealso cref="AISpawnData"/>
    /// </summary>
    /// <param name="spawnID">Spawn Propertie Name</param>
    public void StartSpawn(string spawnID)
    {
        var spawnProp = spawnPropertiesList.Find(sp => sp.SpawnID.Equals(spawnID));
        //if (spawnProp != null)
        //    spawnProp.pauseSpawning = false;
    }

    /// <summary>
    /// Start a specific <seealso cref="AISpawnData"/>
    /// </summary>
    /// <param name="spawnName">Spawn Propertie Index</param>
    public void StartSpawn(int index)
    {
        if (spawnPropertiesList.Count > 0 && index < spawnPropertiesList.Count)
        {
            //spawnPropertiesList[index].pauseSpawning = false;
        }
    }

    /// <summary>
    /// Start all  <seealso cref="vAISpawnProperties"/>
    /// </summary>
    public void StartSpawnAll()
    {
        StartCoroutine(StartAllRoutine());
    }

    /// <summary>
    /// Pause a specific <seealso cref="vAISpawnProperties"/>
    /// </summary>
    /// <param name="spawnName">Spawn Propertie Name</param>
    public void PauseSpawn(string spawnName)
    {
        var spawnProp = spawnPropertiesList.Find(sp => sp.SpawnID.Equals(spawnName));
        //if (spawnProp != null)
        //    spawnProp.pauseSpawning = true;
    }

    /// <summary>
    /// Pause a specific <seealso cref="vAISpawnProperties"/>
    /// </summary>
    /// <param name="spawnName">Spawn Propertie Index</param>
    public void PauseSpawn(int index)
    {
        if (spawnPropertiesList.Count > 0 && index < spawnPropertiesList.Count)
        {
            //spawnPropertiesList[index].pauseSpawning = true;
        }
    }

    /// <summary>
    /// Pause all  <seealso cref="AISpawnData"/>
    /// </summary>
    public void PauseSpawnAll()
    {
        StartCoroutine(PauseAllRoutine());
    }

    private IEnumerator SpawnOneOfAllRoutine()
    {
        for (int i = 0; i < spawnPropertiesList.Count; i++)
        {
            yield return waitBetweenSpawnProps;
            var spawnSpec = new AISpawnSpec(spawnPropertiesList[i]);
            StartCoroutine(spawnSpec.SpawnWaveRoutine(this));
        }
    }

    private IEnumerator StartAllRoutine()
    {
        for (int i = 0; i < spawnPropertiesList.Count; i++)
        {
            yield return waitBetweenSpawnProps;
            //spawnPropertiesList[i].pauseSpawning = false;
        }
    }

    private IEnumerator PauseAllRoutine()
    {
        for (int i = 0; i < spawnPropertiesList.Count; i++)
        {
            yield return waitBetweenSpawnProps;
            //spawnPropertiesList[i].pauseSpawning = true;
        }
    }
}
