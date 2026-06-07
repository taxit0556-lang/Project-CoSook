
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
// These videos take long to make so I hope this helps you out and if you want to help me out you can by leaving a like and subscribe, thanks!
 
public class Player_Movement : MonoBehaviour
{
    [SerializeField] Transform playerCamera;
    [SerializeField][Range(0.0f, 0.5f)] float mouseSmoothTime = 0.03f;
    [SerializeField] bool cursorLock = true;
    [SerializeField] float mouseSensitivity = 3.5f;
    [SerializeField] float Speed = 6.0f;
    [SerializeField][Range(0.0f, 0.5f)] float moveSmoothTime = 0.3f;
    [SerializeField] float gravity = -30f;
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask ground;  
    [SerializeField] private float maxDistance;
    [SerializeField] private Vector3 boxSizeMultiplier = Vector3.one * 0.5f;
 
    public float jumpHeight = 6f;
    float velocityY;
    bool isGrounded;
    public bool canMove = true;
    public bool VaultCheck;
    public bool isVaulting;
    public bool canVault;
    private RaycastHit hit;
    private bool hitDetected;
    private bool YgravitySwitch;
 
    float cameraCap;
    Vector2 currentMouseDelta;
    Vector2 currentMouseDeltaVelocity;
    
    CharacterController controller;
    Vector2 currentDir;
    Vector2 currentDirVelocity;
    public Vector3 velocity;

    public float LedgeY;

    public float crouchYScale;
    public float startYScale;

    public Vector3 estimatedLedge_Position;


    Rigidbody rb;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        controller = GetComponent<CharacterController>();
 
        if (cursorLock)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = true;
        }
    }
 
    void Update()
    {
        if (!canMove)
        return;
        
        UpdateMouse();
        UpdateMove();
    }
    
    void FixedUpdate()
    {
        
    }
 
    void UpdateMouse()
    {
        Vector2 targetMouseDelta = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
 
        currentMouseDelta = Vector2.SmoothDamp(currentMouseDelta, targetMouseDelta, ref currentMouseDeltaVelocity, mouseSmoothTime);
 
        cameraCap -= currentMouseDelta.y * mouseSensitivity;
 
        cameraCap = Mathf.Clamp(cameraCap, -90.0f, 90.0f);
 
        playerCamera.localEulerAngles = Vector3.right * cameraCap;
 
        transform.Rotate(Vector3.up * currentMouseDelta.x * mouseSensitivity);
    }
 
    void UpdateMove()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.2f, ground);
 
        Vector2 targetDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        targetDir.Normalize();
 
        currentDir = Vector2.SmoothDamp(currentDir, targetDir, ref currentDirVelocity, moveSmoothTime);
 
        if(!isGrounded && !Input.GetKeyDown(KeyCode.Space))
            velocityY += gravity * 2f * Time.deltaTime;
 
        velocity = (transform.forward * currentDir.y + transform.right * currentDir.x) * Speed + Vector3.up * velocityY;
 
        controller.Move(velocity * Time.deltaTime);
 
        if (isGrounded && Input.GetButton("Jump"))
        {
            velocityY = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
 
        if(isGrounded! && controller.velocity.y < -1f)
        {
            velocityY = -5.5f;
        }

        if (!isGrounded && !YgravitySwitch && controller.velocity.y < -1f || isVaulting)
        {
            YgravitySwitch = true;
            velocityY = 0;
        }
        else if(isGrounded )
        {
            YgravitySwitch = false;
        }

        velocityY = Mathf.Clamp(velocityY, -25, 100);



        if (Input.GetKeyDown(KeyCode.C))
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

            Speed = 6 / 2;
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
            Speed = 6;
        }


        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            mouseSmoothTime = 0.05f;
            moveSmoothTime = 0.2f;
            Speed = 8;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            mouseSmoothTime = 0.0f;
            moveSmoothTime = 0.1f;
            Speed = 6;
        }
    }

    void VaultingMethod()
    {
        Quaternion RayRotation = Quaternion.Euler(transform.rotation.x, playerCamera.rotation.y,0);
        Vector3 halfExtents = Vector3.Scale(transform.localScale, boxSizeMultiplier);
        hitDetected = Physics.BoxCast(transform.position,halfExtents,transform.forward,out hit,RayRotation,maxDistance,ground);
        

        if (hitDetected)
        {
            if(!isGrounded && Input.GetKeyDown(KeyCode.Space))
            {
                VaultCheck = true;
            }

            if(!isVaulting)
                canVault = true;
        }

        if (VaultCheck && canVault)
        {
            controller.enabled = false;
            LedgeY = hit.collider.gameObject.GetComponent<Collider>().bounds.max.y + 0.93f;

            StartCoroutine(VaultingEnum());
        }

        if(isGrounded || isVaulting)
        {
            canVault = false;
            VaultCheck = false;
        }
        else if (isGrounded)
        {
            isVaulting = false;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = hitDetected ? Color.red : Color.green;

        Gizmos.DrawWireCube(transform.position + transform.forward * maxDistance, boxSizeMultiplier);
    }

    IEnumerator VaultingEnum()
    {
        isVaulting = true;
        canVault = false;
        controller.enabled = false;
    
        // Make sure rigidbody is not kinematic
        rb.isKinematic = false;

        float time = 0;
        float duration = 0.35f;
        float direction = transform.position.x;
        estimatedLedge_Position = new Vector3(transform.position.x, LedgeY, transform.position.z) + (transform.forward * 1.5f);

        RaycastHit TrueLedge_Position;

        bool LedgeCast = Physics.Raycast(estimatedLedge_Position, -transform.up, out TrueLedge_Position, Mathf.Infinity);
    
        Vector3 startposition = transform.position;

        while(time < duration)
        {
            transform.position = Vector3.Lerp(startposition,TrueLedge_Position.point, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        transform.position = TrueLedge_Position.point;
        // float Vgravity = Mathf.Abs(Physics.gravity.y);
        // float heightToReach = LedgeY - transform.position.y;
        // float requiredVelocity = Mathf.Sqrt(2 * Vgravity * heightToReach);

        // // Set velocity directly instead of AddForce
        // //rb.AddForce(transform.up * requiredVelocity);
        //rb.linearVelocity = new Vector3(rb.linearVelocity.x, requiredVelocity, rb.linearVelocity.z);

       //while (transform.position.y < LedgeY - 0.2f)
            //yield return null;

        // Add forward momentum
        rb.AddForce(-transform.up * 500);
        //rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, 5f); // Adjust this value

        yield return new WaitForSeconds(0.1f);

        velocity = new Vector3(0,0,0);

        // Re-enable controller and make rigidbody kinematic again
        rb.isKinematic = true;
        controller.enabled = true;
        canVault = true;
        isVaulting = false;
    }
}