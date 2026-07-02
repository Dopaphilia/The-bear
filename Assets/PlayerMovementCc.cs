using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementCc : MonoBehaviour
{
    [SerializeField] float walkSpeed = 4f;
    [SerializeField] float runSpeed = 7f;

    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpHeight = 2f;

    [SerializeField] float mouseSpeed = 1.5f;

    float xRot;
    Vector3 velo;
    Transform camTr;

    CharacterController cc;

    void Start()
    {
        cc = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        camTr = Camera.main.transform;
    }

    void Update()
    {
        MoveAndJump();
        Look();
    }

    void MoveAndJump()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        bool grounded = cc.isGrounded;
        if (grounded && velo.y < 0) velo.y = -2f;

        float curSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        Vector3 movDir = transform.right * h + transform.forward * v;

        cc.Move(movDir * curSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && grounded) velo.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        velo.y += gravity * Time.deltaTime;

        cc.Move(velo * Time.deltaTime);
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSpeed;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSpeed;

        xRot -= mouseY;
        xRot = Mathf.Clamp(xRot, -90f, 90f);

        camTr.localRotation = Quaternion.Euler(xRot, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }  // Start is called before the first frame update

}
