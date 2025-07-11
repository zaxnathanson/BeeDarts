using UnityEngine;

public class CharacterControllerMovement : MonoBehaviour
{
    [Header("Player Settings")]

    [SerializeField] private float speed = 5.0f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 2f;

    [Header("Ground Check Settings")]

    [SerializeField] private LayerMask ground;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius;
    private bool isGrounded = false;

    private CharacterController controller;
    private Vector3 velocity;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    private void FixedUpdate()
    {
        GroundCheck();
        MovementControls();
    }

    private void MovementControls()
    {
        // responsible for holding the player down while going down slopes
        if (isGrounded)
        {
            velocity.y = -1f;
        }
        // manually calculating gravity to character controller
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        // mopvement
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = (transform.right * x) + (transform.forward * z);

        // horizontal movement application
        controller.Move(move * speed * Time.deltaTime);

        // jump
        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        // gravity application
        controller.Move(velocity * Time.deltaTime);
    }

    private void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * 2 * -gravity);
        }
    }

    private void GroundCheck()
    {
        if (Physics.CheckSphere(groundCheck.position, groundCheckRadius, ground))
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    private void OnDrawGizmos()
    {
        //drawing ground check sphere for debug
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheck.position, groundCheckRadius);
    }
}
