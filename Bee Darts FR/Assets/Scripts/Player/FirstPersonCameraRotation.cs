using UnityEngine;

public class FirstPersonCameraRotation : MonoBehaviour
{
    [Header("Mouse Settings")]

    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float yClampingValue = 90f;

    private Transform playerBody;

    private float xRotation = 0f;

    private void Awake()
    {
        playerBody = transform.parent;

        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        // manual deadzoning as well as cancelling out tiny rotations
        if (playerBody == null) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        if (Mathf.Abs(mouseX) < 0.01f && Mathf.Abs(mouseY) < 0.01f) return;

        mouseX *= mouseSensitivity * Time.deltaTime;
        mouseY *= mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -yClampingValue, yClampingValue);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }
}