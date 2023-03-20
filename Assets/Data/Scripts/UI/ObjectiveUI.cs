using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ObjectiveUI : MonoBehaviour
{
    private TextMeshProUGUI textReference;
    [SerializeField] private string firstObjective;
    [SerializeField] private string secondObjective;
    [SerializeField] private string thirdObjective;
    [SerializeField] private string fourthObjective;
    [SerializeField] private float radius = 20f;
    [SerializeField] private LayerMask AILayer;

    // Start is called before the first frame update
    void Start()
    {
        textReference = GetComponent<TextMeshProUGUI>();
        textReference.text = firstObjective;
    }

    // Update is called once per frame
    void Update()
    {
        KillEnemiesCheck();
    }

    private void KillEnemiesCheck()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemies.Length <= 0)
        {
            textReference.text = secondObjective;
        }
    }
}
