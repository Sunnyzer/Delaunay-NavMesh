using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] float moveSpeed = 5;
    [SerializeField] float rotateSpeed = 5;
    float axisX = 0;
    float axisZ = 0;
    float mouseX = 0;
    float mouseY = 0;
    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        axisX = Input.GetAxis("Horizontal");
        axisZ = Input.GetAxis("Vertical");
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        transform.Rotate(Vector3.up, mouseX);
        Camera.main.transform.Rotate(Vector3.right, -mouseY);
    }
    private void FixedUpdate()
    {
        Vector3 _direction = axisZ * transform.forward + axisX * transform.right;
        transform.position += _direction * moveSpeed * Time.fixedDeltaTime;
    }
}
