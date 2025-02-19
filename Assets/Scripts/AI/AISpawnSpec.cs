using Invector.vCharacterController.AI;
using Invector.vCharacterController.AI.FSMBehaviour;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[Serializable]
public class AISpawnSpec
{
    [SerializeField]
    private AISpawnData _def;

    [Header("AIs Spawned")]

    public List<vAIMotor> aiSpawnedList = new List<vAIMotor>();

    public bool pauseSpawning;

    private int spawned;
    
    private int indexOfDestination;
    
    private bool inSpawn;

    private bool firstSpawnDone;

    public AISpawnData Def => _def;

    public AISpawnSpec(AISpawnData def)
    {
        _def = def;
    }

    public IEnumerator SpawnWaveRoutine(MonoBehaviour mono)
    {
        while (true)
        {
            if (!pauseSpawning)
            {
                yield return SpawnRoutine(mono);
            }
        }
    }

    public IEnumerator SpawnRoutine(MonoBehaviour mono)
    {
        aiSpawnedList.RemoveAll(ai => ai == null || ai.isDead);
        _def.SpawnPoints.RemoveAll(sp => sp == null);
        _def.SpawnDestinations.RemoveAll(sd => sd == null);
        vAIMotor ai = null;

        if (CheckCanSpawn() && !inSpawn)
        {
            inSpawn = true;

            yield return new WaitForEndOfFrame();

            var spawnPoints = _def.SpawnPoints.FindAll(sp => sp.isValid);

            if (spawnPoints.Count > 0)
            {
                _def.onStartSpawn.Invoke();

                yield return new WaitForSeconds(_def.GetSpawnTime(!firstSpawnDone));

                var randomPoint = Mathf.Clamp(Random.Range(-1, spawnPoints.Count), 0, spawnPoints.Count - 1);
                var point = spawnPoints[randomPoint];

                var prefab = _def.GetPrefabToSpawn();
                if (prefab != null)
                {
                    var aiGO = GameObject.Instantiate(prefab, point.transform.position, point.transform.rotation);
                    aiGO.TryGetComponent(out ai);
                }
                
                firstSpawnDone = true;

                if (ai)
                {
                    ai.onDead.AddListener(OnDead);
                    
                    _def.onSpawn.Invoke();

                    aiSpawnedList.Add(ai);

                    yield return new WaitForSeconds(.1f);

                    var fsm = ai.GetComponent<vIFSMBehaviourController>();
                    if (fsm != null)
                    {
                        _def.SetupFSM(fsm, mono);
                    }

                    var aiController = ai.GetComponent<vIControlAI>();
                    if (aiController != null)
                    {
                        _def.SetupAI(aiController, mono, ref indexOfDestination);
                    }

                    spawned++;
                }
            }
        }
        else
        {
            yield return new WaitForSeconds(_def.GetSpawnTime(!firstSpawnDone));
        }
        
        inSpawn = false;
    }

    private bool CheckCanSpawn()
    {
        return _def.SpawnedCountReachedLimit(aiSpawnedList.Count, spawned)
            && IsThereAnySpawnPoint()
            && _def.IsThereAnyPrefab();
    }

    private bool IsThereAnySpawnPoint()
    {
        return _def.SpawnPoints.Count > 0;
    }

    private void OnDead(GameObject obj)
    {
        _def.onDead.Invoke();
    }
}
