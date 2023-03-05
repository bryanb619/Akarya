using UnityEngine;

public class PlayerCam : MonoBehaviour
{

    [SerializeField]
    private float sensX, sensY;

    [SerializeField]
    private Transform orientation;

    private float xRotation, yRotation;



    public float range = 10f;


    private bool inView;
    public bool InView => inView;

    [SerializeField] private bool CanRotateCamera = false;

    private bool _gamePlay; 


    private void Awake()
    {
        GameManager.OnGameStateChanged += GameManager_OnGameStateChanged;
    }

    private void GameManager_OnGameStateChanged(GameState state)
    {

        switch (state)
        {
            case GameState.Gameplay:
                {
                    _gamePlay = true;
                    break;
                }
            case GameState.Paused:
                {
                    _gamePlay = false;
                    break;
                }
        }

        //throw new NotImplementedException();
    }
    private void Start()
    {

        inView = false;
    }

    private void Update()
    {
        RotateCamera(); 
    }

    private void RotateCamera()
    {
        if(_gamePlay) 
        {
            // remove if statement in the near future
            if (CanRotateCamera)
            { // mouse input
                float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
                float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

                yRotation += mouseX;
                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -90f, 90f);

                transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
                orientation.rotation = Quaternion.Euler(0, yRotation, 0);
            }
        }
       
       
    }
    private void OnDestroy()
    {
        GameManager.OnGameStateChanged -= GameManager_OnGameStateChanged;
    }

    /* UNUSED CODE TRASH
    private void GetCompanion()
    {
        Vector3 direction = Vector3.forward;
        Ray theRay = new Ray(transform.position, transform.TransformDirection(direction* range));
        Debug.DrawRay(transform.position, transform.TransformDirection(direction * range));

        // get Companion
        RaycastHit hit; 

        if(Physics.Raycast(theRay, out hit, range))
        {
            if (hit.collider.tag == "Companion")
            {
                inView= true;

                //print("in sight");
                
            }
           
            
        }
        else
        {
            inView= false;
        }

        CompanionAlphaSet();
        


    }

    private void CompanionAlphaSet()
    {
        if (inView == true)
        {
            AI_Companion.Setlow();
        }
        else if(inView == false) 
        {
            AI_Companion.SetHigh();
        }

    }
    */
}
