using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonController : MonoBehaviour
{
    public bool tankControls = true;

    float speed;

    [Header("Movement Speed")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float runSpeed = 4.5f;
    [SerializeField] private float backwardSpeed = 1.5f;
    [SerializeField] private float turningSpeed = 150f;

    float horizontalInput;
    float verticalInput;

    //States
    bool isRunning;
    
    Player player;
    CharacterController characterController;
    GameObject gameCamera;
    
    // Start is called before the first frame update
    void Start()
    {
        player = GetComponent<Player>();
        characterController = GetComponent<CharacterController>();

        //probably need to change this later to work with dynamically changing cameras?
        gameCamera = GameObject.FindGameObjectWithTag("MainCamera");
    }

    // Update is called once per frame
    void Update()
    {
        if (!player.stopInput) {
            HandleMovementInput();

            if (tankControls)
                ApplyTankMovement();
            else 
                ApplyNormalMovement();
        }

    }

    private void HandleMovementInput() {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        horizontalInput = Mathf.Clamp(horizontalInput, -1, 1);
        verticalInput = Mathf.Clamp(verticalInput, -1, 1);

        isRunning = Input.GetKey(KeyCode.LeftShift); // change key later?

        if (isRunning)
            speed = runSpeed;
        else
            speed = walkSpeed;
    }

    private void ApplyTankMovement() {
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

        Vector3 moveX = new Vector3(gameCamera.transform.right.x * h, 0, gameCamera.transform.right.z * h);
        Vector3 moveZ = new Vector3(gameCamera.transform.up.x * v, 0, gameCamera.transform.up.z * v);
        Vector3 move = moveX + moveZ;
        move = Vector3.Normalize(move) * Time.deltaTime * speed;

        Vector3 moveTarget = new Vector3(move.x, 0, move.z);
        Transform lastCamPos = gameCamera.transform;

        //rotate player model in direction of movement
        if (moveTarget != Vector3.zero) {
            if(lastCamPos)
                transform.rotation = Quaternion.RotateTowards(transform.rotation,
                        Quaternion.LookRotation(moveTarget),
                        turningSpeed * 2 * Time.deltaTime);
        }
        characterController.Move(move);
    }


}
