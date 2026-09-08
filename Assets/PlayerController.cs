using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    public float speed;
    float xRotation;
    float yRotation;

    [SerializeField]
    float sens;

    [SerializeField]
    Transform orientation;

    void Start()
    {
        rb = this.gameObject.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sens;
        yRotation += mouseX;
        orientation.rotation = Quaternion.Euler(0f, yRotation, 0f);
        
    }
    private void FixedUpdate()
    {
        Movement();
    }
    private void Movement()
    {

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Debug.Log(x);
        Vector3 move = orientation.forward * z + orientation.right * x;
        rb.linearVelocity = speed * move;
    }
}
