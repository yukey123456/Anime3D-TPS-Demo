using Invector;
using Invector.vCharacterController.AI;
using Invector.vCharacterController.AI.FSMBehaviour;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

[Serializable]
public class AISpawnData
{
    [Header("Spawn Properties")]

    [SerializeField]
    private string _spawnID;

    [SerializeField]
    private vAIMotor _prefab;

    [SerializeField]
    private vAIMotor[] _randomPrefab;

    [SerializeField]
    private List<vSpawnPoint> _spawnPoints;

    [SerializeField]
    private float _timeToFirstSpawn = 1f;
    
    [Tooltip("Enable or Disable the FSM Controller when Spawn")]
    [SerializeField]
    private bool _enableFSMOnSpawn = true;
    
    [Tooltip("Delay to Enable the FSM Controller")]
    [SerializeField]
    private float _delayToEnableFSM = 2;

    [SerializeField]
    private bool _randomTimeToSpawn = true;

    [SerializeField]
    private float _minTimeBetweenSpawn = 1;

    [vHideInInspector(nameof(_randomTimeToSpawn))]
    [SerializeField]
    private float _maxTimeBetweenSpawn = 10;

    [SerializeField]
    private int _maxQuantity = 10;

    [SerializeField]
    private bool _keepMaxQuantity = true;
    
    [Header("AI Detection Settings")]

    [SerializeField]
    private bool _overrideDetectionSettings;

    [SerializeField]
    private vTagMask _detectionTags;

    [SerializeField]
    private LayerMask _detectionLayer;

    [SerializeField]
    private vTagMask _damageTags;

    [SerializeField]
    private LayerMask _damageLayer;

    [Header("Spawn Destination")]

    [SerializeField]
    private List<Transform> _spawnDestinations;

    [SerializeField]
    private vAIMovementSpeed _destinationSpeed = vAIMovementSpeed.Running;
    
    [SerializeField] 
    private float _setWaypointAreaDelay;

    [SerializeField]
    private vWaypointArea _waypointArea;

    [SerializeField]
    private bool _randomDestination;

    [Header("Spawn Events")]
    
    public UnityEvent onStartSpawn;
    
    public UnityEvent onSpawn;
    
    public UnityEvent onDead;
    
    public delegate void OnSpawnAI(vAIMotor ai, AISpawnData spawnProperties);

    public string SpawnID => _spawnID;

    public List<vSpawnPoint> SpawnPoints => _spawnPoints;

    public List<Transform> SpawnDestinations => _spawnDestinations;

    public float GetSpawnTime(bool isFirstSpawn)
    {
        if (isFirstSpawn)
        {
            return _timeToFirstSpawn;
        }

        if (_randomTimeToSpawn)
        {
            return Random.Range(_minTimeBetweenSpawn, _maxTimeBetweenSpawn);
        }

        return _minTimeBetweenSpawn;
    }

    public bool SpawnedCountReachedLimit(int aliveCount, int spawnedCount)
    {
        return (_keepMaxQuantity ? aliveCount : spawnedCount) < _maxQuantity;
    }

    public bool IsThereAnyPrefab()
    {
        return _prefab || _randomPrefab.Length > 0;
    }

    public vAIMotor GetPrefabToSpawn()
    {
        if (_randomPrefab.Length > 0)
        {
            return _randomPrefab[Random.Range(0, _randomPrefab.Length - 1)];
        }
        else
        {
            return _prefab;
        }
    }

    public Vector3 GetSpawnDestination(Vector3 defaultDestination, ref int indexOfDestination)
    {
        var destination = Vector3.zero;

        if (_spawnDestinations.Count > 0)
        {
            if (_randomDestination)
            {
                indexOfDestination = Mathf.Clamp(Random.Range(-1, _spawnDestinations.Count), 0, _spawnDestinations.Count - 1);
                destination = _spawnDestinations[indexOfDestination].transform.position;
            }
            else
            {
                if (!(indexOfDestination < _spawnDestinations.Count))
                {
                    indexOfDestination = 0;
                }

                destination = _spawnDestinations[indexOfDestination].transform.position;
                indexOfDestination++;
            }
        }
        else
        {
            destination = defaultDestination;
        }

        return destination;
    }

    public void SetupFSM(vIFSMBehaviourController fsm, MonoBehaviour mono)
    {
        if (_enableFSMOnSpawn)
        {
            fsm.isStopped = true;
            if (_delayToEnableFSM <= 0)
            {
                fsm.isStopped = false;
            }
            else
            {
                mono.StartCoroutine(EnableFSM(fsm));
            }
        }
    }

    public void SetupAI(vIControlAI aiController, MonoBehaviour mono, ref int indexOfDestination)
    {
        var defaultDestination = aiController.transform.position;
        var destination = GetSpawnDestination(defaultDestination, ref indexOfDestination);
        aiController.selfStartPosition = destination;
        aiController.MoveTo(destination, _destinationSpeed);
        //aiController.RotateTo();

        if (_waypointArea)
        {
            if (_setWaypointAreaDelay <= 0)
                aiController.waypointArea = _waypointArea;
            else
                mono.StartCoroutine(SetWaypointAreaToAI(aiController));
        }

        if (_overrideDetectionSettings)
        {
            aiController.SetDetectionLayer(_detectionLayer);
            aiController.SetDetectionTags(_detectionTags);
            if (aiController is vIControlAIMelee)
            {
                var melee = aiController as vIControlAIMelee;
                melee.SetMeleeHitTags(_damageTags);
            }
            if (aiController is vIControlAIShooter)
            {
                var shooter = aiController as vIControlAIShooter;
                shooter.SetShooterHitLayer(_damageLayer);
            }
        }
    }

    private IEnumerator EnableFSM(vIFSMBehaviourController vIFSM)
    {
        yield return new WaitForSeconds(_delayToEnableFSM);
        vIFSM.isStopped = false;
    }

    private IEnumerator SetWaypointAreaToAI(vIControlAI controller)
    {
        yield return new WaitForSeconds(_setWaypointAreaDelay);
        controller.waypointArea = _waypointArea;
    }
}
