using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float movementSpeed;
    [SerializeField] float acceleration;
    [SerializeField] CharacterController characterController;
    Vector3 movement;

    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        Vector3 targetMovement = new(Input.GetAxisRaw("Horizontal") * movementSpeed, 0, Input.GetAxisRaw("Vertical") * movementSpeed);
        movement = Vector3.MoveTowards(movement, targetMovement, acceleration * Time.fixedDeltaTime);
        characterController.Move(Quaternion.AngleAxis(transform.eulerAngles.y, Vector3.up) * movement * Time.fixedDeltaTime);
    }
}
