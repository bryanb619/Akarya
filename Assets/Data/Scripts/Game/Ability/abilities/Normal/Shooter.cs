using UnityEngine;
using System.Collections;

public class Shooter : MonoBehaviour
{
    [Header("References")]
    private Transform firePoint;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private Material cleansedCrystal;

    private GameObject firePrefab, icePrefab, thunderPrefab;
    internal WeaponType _magicType;

    [SerializeField] private GameObject _areaEffect;

    [Header("Default Ability Prefabs")]
    [SerializeField] private GameObject defaultFirePrefab;
    [SerializeField] private GameObject defaultIcePrefab, defaultThunderPrefab, normalPrefab;

    [Header("Upgraded Ability Prefabs")]
    [SerializeField] private GameObject upgradedFirePrefab;
    [SerializeField] private GameObject upgradedIcePrefab, upgradedThunderPrefab;
                     internal bool wUpgraded = false, rUpgraded = false;

    [Header("Abilities options")]
    [SerializeField] private float areaAttackRadius = 5f;

    [SerializeField] private Ability normalTimer, fireTimer, iceTimer, thunderTimer;
    [SerializeField] private GameObject rAbilityTelegraph;

    internal bool normalCooldown, fireCooldown, iceCooldown, thunderCooldown = false;

    [Header("Script References")]
    private ManaManager manaManager;
    [SerializeField] private ObjectiveUI objectiveUI;
    private ValuesTextsScript valuesTexts;
    [SerializeField] private AbilityHolder targetedAttackAbilityHolder;

    private RaycastHit hit;
    private Vector3 enemyPosition;

    private bool _gameplay;

    private void Awake()
    {
        GameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
        manaManager = GetComponent<ManaManager>();
    }
    private void Start()
    {
        _magicType = WeaponType.Fire;

        firePoint = GameObject.Find("CompanionShootPos").transform;

        valuesTexts = GameObject.Find("ValuesText").GetComponent<ValuesTextsScript>();

        firePrefab = defaultFirePrefab;
        icePrefab = defaultIcePrefab;
        thunderPrefab = defaultThunderPrefab;
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
        if (firePoint != null)
        {
            //When you press the key (telegraph of the ability)
            if (Input.GetKey(KeyCode.Mouse0))
            {
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                
            }
            else if (Input.GetKey(KeyCode.W))
            {
                
            }
            else if (Input.GetKey(KeyCode.R))
            {
                rAbilityTelegraph.SetActive(true);
            }

            //--------------------------------------
            //When you release the key

            if (Input.GetKeyUp(KeyCode.Mouse0))
            {
                Debug.Log("Shot auto");
                _magicType = WeaponType.Normal;
                Shoot();
            }
            else if (Input.GetKeyUp(KeyCode.Q))
            {
                _magicType = WeaponType.Fire;
                Shoot();
            }
            else if (Input.GetKeyUp(KeyCode.W))
            {
                _magicType = WeaponType.Ice;
                Shoot();
            }
            else if (Input.GetKeyUp(KeyCode.R))
            {
                _magicType = WeaponType.Thunder;
                rAbilityTelegraph.SetActive(false);
                Shoot();
            }
        }
    }

    public void Shoot()
    {
        switch (_magicType)
        {
            case WeaponType.Normal: // input nº1
                {
                    //Cleanse Crystal
                    if (hit.collider.name == "Crystal" && hit.collider.GetComponent<Outline>().enabled == true)
                    {
                        if (Input.GetKeyDown(KeyCode.Mouse0))
                        {
                            hit.collider.GetComponent<MeshRenderer>().material.Lerp(hit.collider.GetComponent<MeshRenderer>().material, cleansedCrystal, 1f);
                            hit.collider.GetComponent<Outline>().enabled = false;
                            objectiveUI.passedSecondObjective = true;
                            valuesTexts.GetCrystal();
                            break;
                        }
                        else
                            break;
                    }
                    else
                    {
                        if (!normalCooldown && Input.GetKeyUp(KeyCode.Mouse0))
                        {
                            StartCoroutine(NormalAttackCooldown());
                            Instantiate(normalPrefab, firePoint.position, firePoint.rotation);
                        }
                        break;
                    }
                }
            case WeaponType.Fire: // input nº2 (Q)
                {
                    if(!fireCooldown)
                    { 
                        if (manaManager.ManaCheck(_magicType) == true)
                        {
                            StartCoroutine(FireAttackCooldown());
                            Instantiate(firePrefab, firePoint.position, firePoint.rotation);
                        }
                        break;
                    }
                    else
                        break;
                }
            case WeaponType.Ice: // input nº3 (W)
                {
                    if(!iceCooldown)
                        //Instantiate inside the TargetAttack function to avoid unecessary code
                        TargetAttack();
                    
                    break;
                }
            case WeaponType.Thunder: // input nº4 (R)
                {
                    if(!thunderCooldown)
                    {
                        if (manaManager.ManaCheck(_magicType) == true)
                        {
                            //Instantiate(thunderPrefab, firePoint.position, firePoint.rotation);
                            AreaAttack();
                        }
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
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, 100))
            if (hit.collider.CompareTag("Enemy"))
                hit.collider.gameObject.GetComponent<Outline>().enabled = true;
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
        if (Physics.Raycast(mainCamera.ScreenPointToRay(Input.mousePosition), out hit, 100))
        {
            if (hit.collider.CompareTag("Enemy"))
            {
                    if (manaManager.ManaCheck(_magicType) == true)
                    {
                        StartCoroutine(IceAttackCooldown());
                        Instantiate(icePrefab, hit.collider.transform.position, firePoint.rotation);
                        targetedAttackAbilityHolder.TargetedAttackCooldownUI();
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
                // Deal damage to the enemy
                Instantiate(thunderPrefab, hitCollider.transform.position, firePoint.rotation);

                Instantiate(_areaEffect, transform.position, Quaternion.identity);
            }
        }
        StartCoroutine(ThunderAttackCooldown());
    }
  
    internal void UpgradeChecker(int abilityNumber)
    {
        switch (abilityNumber)
        {
            
            case 1:
                firePrefab = upgradedFirePrefab;
                break;
            
            case 2:
                icePrefab = upgradedIcePrefab;
                wUpgraded = true;
                break;
            
            case 3:
                thunderPrefab = upgradedThunderPrefab;
                rUpgraded = true;
                break;
            
            default:
                break;
        }
    }

    #region Enumerators

    IEnumerator NormalAttackCooldown()
    {
        normalCooldown = true;
        yield return new WaitForSecondsRealtime(normalTimer.cooldownTime);
        normalCooldown = false;
    }   
    IEnumerator FireAttackCooldown()
    {
        fireCooldown = true;
        yield return new WaitForSecondsRealtime(fireTimer.cooldownTime);
        fireCooldown = false;
    }

    IEnumerator IceAttackCooldown()
    {
        iceCooldown = true;
        yield return new WaitForSecondsRealtime(iceTimer.cooldownTime);
        iceCooldown = false;
    }

    IEnumerator ThunderAttackCooldown()
    {
        thunderCooldown = true;
        yield return new WaitForSecondsRealtime(thunderTimer.cooldownTime);
        thunderCooldown = false;
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
