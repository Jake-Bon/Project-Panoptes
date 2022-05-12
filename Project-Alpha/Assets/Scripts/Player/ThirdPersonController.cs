using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    public MovementType moveType = MovementType.Normal;
    public float gravity = 10.0f;

    float speed;

    [Header("Movement Speed")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 4.5f;
    [SerializeField] private float backwardSpeed = 1.5f;
    [SerializeField] private float turningSpeed = 150f;
    [SerializeField] private float mouseSensitivity = 10f;
    [SerializeField] private Transform spawnpoint;

    float horizontalInput;
    float verticalInput;

    //States
    bool isRunning;
    bool isGrounded;
    
    Player player;
    CharacterController characterController;
    GameObject[] gameCameraList;
    GameObject gameCamera;
    GameObject prevGameCamera;
    GameObject currGameCamera;
    ChildBehavior child;
    
    bool cameraChangeFlag;

    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Player>();
        characterController = GetComponent<CharacterController>();

        gameCameraList = GameObject.FindGameObjectsWithTag("Camera");
        foreach(GameObject cam in gameCameraList){
                cam.SetActive(false);
        }
        gameCamera = gameCameraList[0];
        gameCamera.SetActive(true);
        prevGameCamera = gameCamera;
        GameObject childTest = GameObject.Find("Child");
        if(childTest!=null){
            child = childTest.GetComponent<ChildBehavior>();   
        }  
        Cursor.lockState = CursorLockMode.Locked;
        cameraChangeFlag = false;
    }

    // Update is called once per frame
    void Update()
    {
        isGrounded = CheckIfGrounded();

        if (!isGrounded) {
            Fall();
        }

        if (!player.stopInput) {
            HandleMovementInput();

            if (moveType==MovementType.TankWASD)
                ApplyTankMovementWASD();
            else if(moveType==MovementType.TankMouse)
                ApplyTankMovementMouse();
            else 
                ApplyNormalMovement();
        }

        if(gameObject.transform.position.y<-3.0f){
            Debug.Log("sdf");
            transform.position = spawnpoint.position;
            if(child!=null){
                child.ResetChild();
            }
        }

    }

    private void HandleMovementInput() {
        isRunning = Input.GetKey(KeyCode.LeftShift); // change key later?

        if (isRunning){
            speed = runSpeed;
        }else{
            speed = walkSpeed;
        }
        //child.SetSpeed(speed);

        if(moveType==MovementType.TankMouse){
            horizontalInput = Input.GetAxisRaw("Mouse X");
            if(horizontalInput<0)
                horizontalInput-=Math.Abs(Input.GetAxis("Mouse Y")); //Adds all mouse movement
            else
                horizontalInput+=Math.Abs(Input.GetAxis("Mouse Y")); //Adds all mouse movement
        }else{
            horizontalInput = Input.GetAxisRaw("Horizontal");
        }
        verticalInput = Input.GetAxisRaw("Vertical");

        horizontalInput = Mathf.Clamp(horizontalInput, -1, 1);
        verticalInput = Mathf.Clamp(verticalInput, -1, 1);
    }

    private void ApplyTankMovementMouse(){
        if (verticalInput < 0)
            speed = backwardSpeed;
        float h = horizontalInput * Time.deltaTime * turningSpeed * mouseSensitivity;
        float v = verticalInput * Time.deltaTime * speed;
        
        Vector3 move = new Vector3(0,0,v);
        move = transform.TransformDirection(move);
        characterController.Move(move);

        Vector3 turn = new Vector3(0,h,0);
        transform.Rotate(turn);
    }

    private void ApplyTankMovementWASD() {
        if (verticalInput < 0)
            speed = backwardSpeed;
        float h = horizontalInput * Time.deltaTime * turningSpeed;
        float v = verticalInput * Time.deltaTime * speed;
        
        Vector3 move = new Vector3(0,0,v);
        move = transform.TransformDirection(move);
        characterController.Move(move);

        Vector3 turn = new Vector3(0,h,0);
        transform.Rotate(turn);
    }
    
    private void ApplyNormalMovement() {
        float h = horizontalInput;
        float v = verticalInput;

        if(cameraChangeFlag && (h != 0 || v != 0)) //if camera changed & player is still moving, use previous camera as reference
            currGameCamera = prevGameCamera;
        else {//if not, use current camera as reference
            currGameCamera = gameCamera;
            cameraChangeFlag = false;
        }

        Vector3 moveX = new Vector3(currGameCamera.transform.right.x * h, 0, currGameCamera.transform.right.z * h);
        Vector3 moveZ = new Vector3(currGameCamera.transform.up.x * v, 0, currGameCamera.transform.up.z * v);
        Vector3 move = moveX + moveZ;
        move = Vector3.Normalize(move) * Time.deltaTime * speed;

        Vector3 moveTarget = new Vector3(move.x, 0, move.z);
        Transform lastCamPos = currGameCamera.transform;

        //rotate player model in direction of movement
        if (moveTarget != Vector3.zero) {
            if(lastCamPos)
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                        Quaternion.LookRotation(moveTarget),
                        turningSpeed * 2 * Time.deltaTime);
        }
        characterController.Move(move);
    }

    private bool CheckIfGrounded() {
        // Create layer mask that includes everything but the player
        int layerMask = (1 << LayerMask.NameToLayer("Player"));
        layerMask = ~layerMask;

        Vector3 castPostition = gameObject.transform.position - new Vector3(0, 0.6f, 0.0f);

        return Physics.CheckSphere(castPostition, 0.5f, layerMask);
    }

    private void Fall() {
        float gravity = 5.0f;
        float y = characterController.velocity.y;

        y -= gravity * Time.deltaTime;

        Vector3 fall = new Vector3(0, y, 0);

        characterController.Move(fall);
    }

    public void ChangeCamera(int choice){
        cameraChangeFlag = true;
        prevGameCamera = gameCamera;
        gameCamera.SetActive(false);
        gameCamera = gameCameraList[choice];
        gameCamera.SetActive(true);
    }

    public enum MovementType {Normal,TankWASD,TankMouse};
}
