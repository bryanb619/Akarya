using UnityEngine;
using System.Collections;
using UnityEngine.Serialization;

public class Shooter : MonoBehaviour
{
    [Header("References")]
    private Transform _firePoint;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Material cleansedCrystal;

    private GameObject _firePrefab, _icePrefab, _thunderPrefab;
    internal WeaponType MagicType;

    [FormerlySerializedAs("_areaEffect")] [SerializeField] private GameObject areaEffect;

    [Header("Default Ability Prefabs")]
    [SerializeField] private GameObject defaultFirePrefab;
    [SerializeField] private GameObject defaultIcePrefab, defaultThunderPrefab, normalPrefab;

    [Header("Upgraded Ability Prefabs")]
    [SerializeField] private GameObject upgradedFirePrefab;
    [SerializeField] private GameObject upgradedIcePrefab, upgradedThunderPrefab;
                     internal bool WUpgraded = false, RUpgraded = false;

    [Header("Abilities options")]
    [SerializeField] private float areaAttackRadius = 5f;

    [SerializeField] private Ability normalTimer, fireTimer, iceTimer, thunderTimer;
    [SerializeField] private GameObject rAbilityTelegraph;
    private KeyCode qKey = KeyCode.Q, wKey = KeyCode.W, rKey = KeyCode.R;

    internal bool NormalCooldown, FireCooldown, IceCooldown, ThunderCooldown = false;

    [Header("Script References")]
    private ManaManager _manaManager;
    [FormerlySerializedAs("objectiveUI")] [SerializeField] private ObjectiveUi objectiveUi;
    private ValuesTextsScript _valuesTexts;
    [SerializeField] private AbilityHolder targetedAttackAbilityHolder;
    private PlayerAnimationHandler _playerAnim; 
    private PlayerMovement  _coroutineCaller;

    
    private RaycastHit _hit;
    private Vector3 _enemyPosition;
    [SerializeField] private float maxDistanceToCrystal = 10f;
    private bool _gameplay;

    private void Awake()
    {
        GameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
        _manaManager = GetComponent<ManaManager>();
    }
    private void Start()
    {
        _playerAnim                         = GetComponentInChildren<PlayerAnimationHandler>();
        _coroutineCaller                     = FindObjectOfType<PlayerMovement>();

        MagicType = WeaponType.Fire;

        _firePoint = GameObject.Find("CompanionShootPos").transform;

        _valuesTexts = GameObject.Find("ValuesText").GetComponent<ValuesTextsScript>();

        _firePrefab = defaultFirePrefab;
        _icePrefab = defaultIcePrefab;
        _thunderPrefab = defaultThunderPrefab;
    }

    private void GameManager_OnGameStateChanged(GameState state)
    {
        switch (state)
        {
            case GameState.Paused:
                {
                    _gameplay = false;
                    break;
                }
            case GameState.Gameplay:
                {
                    _gameplay = true;
                    break;
                }
        }
    }

    private void Update()
    {
        if (_gameplay)
        {
            ShootInput();
            HoverHighlight();
        }

    }

    private void ShootInput()
    {
        if (_firePoint != null)
        {
            //When you press the key (telegraph of the ability)
            if (Input.GetKey(KeyCode.Mouse0))
            {
                
            }
            else if (Input.GetKey(KeyCode.Alpha1))
            {
                
            }
            else if (Input.GetKey(KeyCode.Alpha2))
            {
                
            }
            else if (Input.GetKey(KeyCode.Alpha3))
            {
                
            }
            else if (Input.GetKey(rKey) && !ThunderCooldown)
            {
                rAbilityTelegraph.SetActive(true);
            }

            //--------------------------------------
            //When you release the key

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                MagicType = WeaponType.Normal;
                Shoot();
            }
            else if (Input.GetKeyUp(qKey))    
            {
                MagicType = WeaponType.Fire;
                Shoot();
            }
            else if (Input.GetKeyUp(wKey))
            {
                MagicType = WeaponType.Ice;
                Shoot();
            }
            else if (Input.GetKeyUp(rKey))
            {
                //needs to be fixed
                _coroutineCaller.StopMovement();
                _coroutineCaller.RestartMovement();
                MagicType = WeaponType.Thunder;
                rAbilityTelegraph.SetActive(false);
                Shoot();
            }
        }
    }

    public void Shoot()
    {
        switch (MagicType)
        {
            case WeaponType.Normal: // input nº1
                {
                    //Cleanse Crystal
                    if (_hit.collider.name == "Crystal" && _hit.collider.GetComponent<Outline>().enabled == true)
                    {
                        float distance = Vector3.Distance(_hit.collider.gameObject.transform.position, 
                                                          gameObject.transform.position);
                        
                        if (Input.GetKeyUp(KeyCode.Mouse0)&& distance < maxDistanceToCrystal)
                        {
                            
                            _hit.collider.GetComponent<MeshRenderer>().material.Lerp(_hit.collider.GetComponent<MeshRenderer>().material, cleansedCrystal, 1f);
                            _hit.collider.GetComponent<Outline>().enabled = false;
                            objectiveUi.Passed();
                            _valuesTexts.GetCrystal();
                            
                            print("CLEANED CRYSTAL");
                            break;
                        }
                        else
                            break;
                    }
                    else
                    {
                        if (!NormalCooldown && Input.GetKeyUp(KeyCode.Mouse0))
                        {
                            _playerAnim.NormalAttack();
                            StartCoroutine(NormalAttackCooldown());
                            Instantiate(normalPrefab, _firePoint.position, _firePoint.rotation);
                        }
                        break;
                    }
                }
            case WeaponType.Fire: // input nº2 (Q)
                {
                    if(!FireCooldown)
                    { 
                        if (_manaManager.ManaCheck(MagicType) == true)
                        {
                            _playerAnim.QAttack();
                            StartCoroutine(FireAttackCooldown());
                            Instantiate(_firePrefab, _firePoint.position, _firePoint.rotation);
                        }
                        break;
                    }
                    else
                        break;
                }
            case WeaponType.Ice: // input nº3 (W)
                {
                    if(!IceCooldown)
                        //Instantiate inside the TargetAttack function to avoid unnecessary code
                        _playerAnim.QAttack();
                        TargetAttack();
                    
                    break;
                }
            case WeaponType.Thunder: // input nº4 (R)
                {
                    if(!ThunderCooldown)
                    {
                            AreaAttack();
                            break;
                    }
                    else
                        break;
                }

            default:
                break;
        }

    }

    private void HoverHighlight()
    {
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out _hit, 50f))
            if (_hit.collider.CompareTag("Enemy"))
            {
                _hit.collider.gameObject.GetComponent<Outline>().enabled = true;
            }
                
            else
            {
                GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

                foreach (GameObject enemy in enemies)
                {
                    enemy.GetComponent<Outline>().enabled = false;
                    
                }
            }

    }

    private void TargetAttack()
    {
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out _hit, 500))
        {
            if (_hit.collider.CompareTag("Enemy"))
            {
                    if (_manaManager.ManaCheck(MagicType))
                    {
                        StartCoroutine(IceAttackCooldown());
                        Instantiate(_icePrefab, _hit.collider.transform.position, _firePoint.rotation);
                        targetedAttackAbilityHolder.TargetedAttackCooldownUi();
                        Debug.Log("Enemy Hit with Ice");
                    }
                else
                    Debug.Log("Not enough mana");
            }
        }
    }

    private void AreaAttack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, areaAttackRadius);

        foreach (Collider hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag("Enemy"))
            {
                if (_manaManager.ManaCheck(MagicType))
                {
                    // Deal damage to the enemy
                    Instantiate(_thunderPrefab, hitCollider.transform.position, _firePoint.rotation);
                    Instantiate(areaEffect, transform.position, Quaternion.identity);
                }
            }
        }
        targetedAttackAbilityHolder.AreaAttackCooldownUi();
        StartCoroutine(ThunderAttackCooldown());
        _playerAnim.RAttack();
    }
  
    internal void UpgradeChecker(int abilityNumber)
    {
        switch (abilityNumber)
        {
            
            case 1:
                _firePrefab = upgradedFirePrefab;
                break;
            
            case 2:
                _icePrefab = upgradedIcePrefab;
                WUpgraded = true;
                break;
            
            case 3:
                _thunderPrefab = upgradedThunderPrefab;
                RUpgraded = true;
                break;
            
            default:
                break;
        }
    }

    public void KeyChanger(int option)
    {
        if (option == 1)
        {
            qKey = KeyCode.Q;
            wKey = KeyCode.W;
            rKey = KeyCode.R;
        }
        else if (option == 2)
        {
            qKey = KeyCode.Alpha1;
            wKey = KeyCode.Alpha2;
            rKey = KeyCode.Alpha4;
        }
    }
    

    #region Enumerators

    IEnumerator NormalAttackCooldown()
    {
        NormalCooldown = true;
        yield return new WaitForSecondsRealtime(normalTimer.cooldownTime);
        NormalCooldown = false;
    }   
    IEnumerator FireAttackCooldown()
    {
        FireCooldown = true;
        yield return new WaitForSecondsRealtime(fireTimer.cooldownTime);
        FireCooldown = false;
    }

    IEnumerator IceAttackCooldown()
    {
        IceCooldown = true;
        yield return new WaitForSecondsRealtime(iceTimer.cooldownTime);
        IceCooldown = false;
    }

    IEnumerator ThunderAttackCooldown()
    {
        ThunderCooldown = true;
        yield return new WaitForSecondsRealtime(thunderTimer.cooldownTime);
        ThunderCooldown = false;
    }

    #endregion

    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameManager_OnGameStateChanged;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a wireframe sphere to show the attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, areaAttackRadius);
    }

}