#region Libs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using LibGameAI.FSMs;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.PlayerLoop;
using State = LibGameAI.FSMs.State;
using StateMachine = LibGameAI.FSMs.StateMachine;

#if UNITY_EDITOR
using UnityEditor; 
#endif

#endregion

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyRanged : MonoBehaviour
{
    /*
    #region Variabless
    
    #region AI States

    private enum Ai                             { 
        Guard, 
        Patrol,
        Attack,
        None}
    
    [SerializeField]    private Ai               stateAi;

                        private enum HandleState                    { Stoped, None }
                        private HandleState                         _currentState;
                        private GameState                           _gamePlay;


    #endregion
                   
    #region Components
                        // Components ----------------------------------------------------------------------------------------------------->
        
                        // Reference to AI data
    [SerializeField]    private AiRangedData                        data;

                        // Reference to the state machine
                        private StateMachine                        _stateMachine;

                        // Reference to the NavMeshAgent
                        private NavMeshAgent                       _agent;
                        
                        //private WarningSystemAi                     _warn;

    [SerializeField]    private Agents                              agentAI;

                        private AiHandler                           _handlerAi;
                        private bool                                _deactivateAI;
                        
                        private ObjectiveUi                         _objectiveUIScript;
                        
                        // animator
                        private Animator                            animator;

    // Reference to the Outline component & meshes
    [SerializeField]    private Outline                             outlineDeactivation;
    [SerializeField]    private SkinnedMeshRenderer                 enemyMesh;
                        private Color                               _originalColor;
                        
                        #endregion
                        
    #region Patrol
                        // Patrol Points
                        private int                                     _destPoint = 0;
    [SerializeField]    private Transform[]                             patrolPoints;
                        private float _patrolSpeed; 
    #endregion
    
    #region Cover

    // hide code
                            
    [Header("Hide config")]
    private Collider[]                             _colliders = new Collider[10];

    [Range(-1, 1)]
    [SerializeField]        private float                                   hideSensitivity = 0;
    
    [Range(0.01f, 1f)]
    [SerializeField]        private float                                   UpdateFrequency = 0.65f;

    [SerializeField]        private LayerMask                               HidableLayers;

    [Range(0, 5f)]
    [SerializeField]        private float                                   MinObstacleHeight = 0f;

                            public SceneChecker                             LineOfSightChecker;

    private Coroutine                               _movementCoroutine;
    #endregion
    
    #region Combat
    
    // Combat  -------------------------------------------------------------------------------------------------------->

        // Attack 
                            private float _attackSpeed; 

        [SerializeField]    private Transform                           shootPos;

                            private GameObject                          _bullet, _randomBullet, _specialPower;

                            private float                               _fireRate;
                            private float                               _nextFire;
                            private float                               _percentage;

                            
        // special ability 

                            private const float                         AbilityMaxValue = 100F;
                            private float                               _currentAbilityValue;
                            private float                               _abilityIncreasePerFrame;

                            private GameObject                          _targetEffect;
                            
                            
                            // COMBAT //
                            private float                                   damageEffectTime;
                            
                            // damage over time variables
                            private float                                    _damageOverTime = 2f;
                            private float                                    _durationOfDOT = 10f;

                            // stunned variables
                            private float                                     _stunnedTime = 1.5f;
        
    
                            //TODO Add effects to Scriptable object
        // FOV -------------------------------------------------------------------------------------------------------->
               
        [Range(10, 150)]
        [SerializeField]    private float                                   radius;
                            public float                                    Radius => radius;
        [Range(50, 360)]
        [SerializeField]    private float                                   angle;
                            public float                                    Angle => angle;

        [SerializeField]    private LayerMask                               targetMask;
        [SerializeField]    private LayerMask                               obstructionMask;
        [SerializeField]    private Transform                               fov;
                            public Transform                                EefovTransform => fov; // Enemy Editor FOV

                            private bool                                    _canSeePlayer;
                            public bool                                     CanSee => _canSeePlayer;
                            
    // Drops & Death -------------------------------------------------------------------------------------------------->
                            private GameObject                              _death;

                            private bool                                    _gemSpawnOnDeath;
                            private GameObject                              _gemPrefab;

                            private bool                                    _spawnHealth;
                            private int                                     _healthItems;
                            private GameObject                              _healthDrop;

                            private bool                                    _spawnMana; 
                            private int                                     _manaItems;
                            private GameObject                              _manaDrop;
                            private int                                     _dropRadius; 

                            #endregion

    #region  Health & UI
    // Health --------------------------------------------------------------------------------------------------------->

        // UI
        [Header("UI ")]
        [SerializeField]    private Slider                                  healthSlider;
                            private float                                   _health;
                            
                            public int                                      damageBoost = 0;
                            
    [SerializeField]        private Slider                                  abilitySlider;

                            private ValuesTextsScript                        _valuesTexts;

                            
        // References to enemies
        [SerializeField]    private GameObject                              playerObject;

                            private Transform                               _playerTarget;
                            public Transform                                PlayerTarget => _playerTarget;

                            //private PlayerHealth                            _player; 
                            
                            #endregion

#if  UNITY_EDITOR
    #region Game DEBUG

    // Debug ---------------------------------------------------------------------------------------------------------->
    [SerializeField]        private bool                                   showAttackGizmos, showLabelGizmo, 
        showWireGizmo;
    #endregion
#endif
    //----------------------------------------------------------------------------------------------------------------->
    #endregion

    private void Awake()
    {
        // Accept Game Manager State
        GameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
        // accept components
        GetComponents();
        // Accept AI Profile values
        GetProfile();
    }
    #region GameState
    private void GameManager_OnGameStateChanged(GameState state)
    {

        switch (state)
        {
            case GameState.Gameplay:
            {
                    
                if(_currentState == HandleState.None)
                {
                    _gamePlay = GameState.Gameplay;
                    ResumeAgent();
                }
                    
                break;
            }
            case GameState.Paused:
            {
                    
                if (_currentState == HandleState.None)
                {
                    _gamePlay = GameState.Paused;
                    PauseAgent();
                }
                break;
            }
        }

        throw new NotImplementedException();
    }

    private void ResumeAgent()
    {
        
    }

    private void PauseAgent()
    {
        
    }
    #endregion
    
    #region Components Sync
    private void GetComponents()
    {
        _agent              = GetComponent<NavMeshAgent>();
        //_warn               = GetComponent<WarningSystemAi>();
        _handlerAi          = GetComponent<AiHandler>();

        agentAI            = GetComponentInChildren<Agents>();
        animator           = GetComponentInChildren<Animator>();
        healthSlider       = GetComponentInChildren<Slider>();
        //enemyMesh           = GetComponentInChildren<SkinnedMeshRenderer>();

        LineOfSightChecker  = GetComponentInChildren<SceneChecker>();

        
        //_player             = FindObjectOfType<PlayerHealth>();
        playerObject        = GameObject.Find("Player");
        _playerTarget       = playerObject.transform;
        //_shooterScript      = PlayerObject.GetComponent<Shooter>();

        _valuesTexts         = GameObject.Find("ValuesText").GetComponent<ValuesTextsScript>();
    }
    #endregion

    #region Profile Sync
    private void GetProfile()
    {
       
        // HEALTH //
        _health = data.Health;
       
        // ATTACK //
        _fireRate = data.AttackRate;
        
        //_minDist = data.MinDist;

        _percentage = data.Percentage; 

        // Special attack Ability

        _currentAbilityValue = data.CurrentAbilityValue;
        _abilityIncreasePerFrame = data.AbilityIncreasePerFrame;
        //specialDamage = data.SpecialDamage;

        // projectiles //

        _bullet          = data.NProjectile;
        _randomBullet    = data.RProjectile;
        _specialPower    = data.SProjectile;

        // cover //
        //fleeDistance = data.FleeDistance; 
        
        // Death & Loot //

        _gemPrefab = data.Gem;

        _spawnHealth = data.SpawnHealth;    
        _healthDrop = data.HealthDrop;
        _healthItems = data.HealthItems;    

        _spawnMana = data.SpawnMana;
        _manaDrop = data.ManaDrop;
        _manaItems = data.ManaItems;
        _dropRadius = data.DropRadius;  
        
        // UI //
        healthSlider.value = _health;
        abilitySlider.value = _currentAbilityValue; 

    }
    #endregion
    
    private 

    #region Start
    void Start()
    {
        StateCheck();
    }

    #region States
    private void GetStates()
    {
        // Create the states
        State onGuardState = new State("Guard" ,null);

        State PatrolState = new State("On Patrol", PatrolAction);

        State ChaseState = new State("Fight",AttackAction);
        
        // Add the transitions

        // GUARD -> CHASE
        onGuardState.AddTransition(
            new Transition(
                () => stateAi == Ai.Attack,
                //() => Debug.Log(" GUARD -> CHASE"),
                ChaseState));

        // CHASE->PATROL
        ChaseState.AddTransition(
            new Transition(
                () => stateAi == Ai.Patrol,
                PatrolState));
        
        //  PATROL -> CHASE 
        PatrolState.AddTransition(
            new Transition(
                () => stateAi == Ai.Attack,
                ChaseState));
        
        //state machine
        _stateMachine = new StateMachine(PatrolState);
    }
    #endregion
    
    #region State Set 
    
    private void StateCheck()
    {
        switch (stateAi)
        {
            case Ai.Patrol:
            {
                SetPatrol();
                break;
            }
            case Ai.Attack:
            {
                SetAttack();
                break; 
            }
        }
    }
    private void SetPatrol()
    {
        _agent.speed = _patrolSpeed;
    }
    
    private void SetAttack()
    {
        _agent.speed = _attackSpeed; 
    }
    
    #endregion
    #endregion
    #region Update
    // Request actions to the FSM and perform them
    private void Update()
    {
       UpdateStateAI();
    }

    #region AI Actions

    private void UpdateStateAI()
    {
        switch (_handlerAi.AgentOperate)
        {
            case true:
            {
                if (_gamePlay == GameState.Gameplay)
                {
                    //ResumeAgent(); 
                    outlineDeactivation.enabled = false;

                    //MinimalCheck(); // Tester

                    if (_currentState == HandleState.None)
                    {
                        if (stateAi != Ai.Attack)
                        {
                            //StartCoroutine(FovRoutine());
                        }

                        Action actions = _stateMachine.Update();
                        actions?.Invoke();

                    }
                }

                break;
            }
            case false:
            {
                //StopAgent();
                break;
            }
        }
    }
    
    #region Patrol
    private void PatrolAction()
    {
        
    }
    #endregion


    #region Attack
    private void AttackAction()
    {
        
    }
    #endregion
    
    #endregion

    
    #region FOV & Hide coroutines 

 
    #region FOV

    #endregion

    #region Hide

    

    #endregion
    
    #endregion
    #endregion

    #region AI Health

    #region Health Check
    public void TakeDamage(int damage, WeaponType type)
    {
        //health -= (_damage + damageBoost);
        
        if (_health > 0)
        {
            ApplyDamage(damage, type);
        }
        else
        {
            // call dead void
        }
    }

    private void ApplyDamage(int damage, WeaponType type)
    {
        //GetPlayer();
        switch (type)
        {
            case WeaponType.Normal:
            {
                _health -= damage + damageBoost;

                //_damageEffectTime = 0.5f;
                StartCoroutine(HitFlash());
                break;
            }

            case WeaponType.Fire: //Q ability
            {
                _health -= damage + damageBoost;

                StartCoroutine(HitFlash());
                break;
            }

            case WeaponType.Ice: //W ability
            {
                _health -= damage + damageBoost;

                if (_shooterScript.WUpgraded == true)
                {
                    _damageEffectTime = 1f;
                    StartCoroutine(DamageOverTime(_damageOverTime, _durationOfDot));
                }
                else
                    _health -= damage + damageBoost;

                Instantiate(targetEffect, transform.position, transform.rotation);
                StartCoroutine(HitFlash());

                break;
            }

            case WeaponType.Dash: //E ability
            {
                _health -= damage + damageBoost;

                StartCoroutine(HitFlash());

                break;
            }

            case WeaponType.Thunder: //R ability
            {
                _health -= damage + damageBoost;

                if (_shooterScript.RUpgraded == true)
                {
                    StartCoroutine(Stfs(_stunnedTime));
                }
                else
                    StartCoroutine(HitFlash());

                break;
            }

            default: break;
        }
    }
    #endregion

    #region Visual Effect Coroutintes

    
    #endregion
    #endregion

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameManager_OnGameStateChanged; 
    }
    
    */
}
