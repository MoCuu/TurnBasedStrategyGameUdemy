using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;

    private void Awake()
    {
        instance = this;

        moveTarget = transform.position;
    }

    public float moveSpeed, manualMoveSpeed = 5f;
    private Vector3 moveTarget;

    private Vector2 moveInput;

    private float targetRot;
    public float rotateSpeed;

    private int currentAngle;

    public Transform theCam;

    public float fireCamViewAngle = 30f;
    private float targetCamViewAngle;
    private bool isFireView;

    // Start is called before the first frame update
    void Start()
    {
        targetCamViewAngle = 45f;
    }

    // Update is called once per frame
    void Update()
    {
        if (moveTarget != transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position, moveTarget, moveSpeed * Time.deltaTime);
        }

        moveInput.x = Input.GetAxis("Horizontal"); // way of unity gets input from keyboard
        moveInput.y = Input.GetAxis("Vertical");
        moveInput.Normalize();

        if (moveInput != Vector2.zero)
        {
            transform.position += ((transform.forward * (moveInput.y * manualMoveSpeed)) + (transform.right * (moveInput.x * manualMoveSpeed))) * Time.deltaTime;

            moveTarget = transform.position;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetMoveTarget(GameManager.instance.activePlayer.transform.position);
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            currentAngle++;

            if (currentAngle >= 8)
            {
                currentAngle = 0;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            currentAngle--;

            if (currentAngle < 0)
            {
                currentAngle = 7;
            }
        }

        if (isFireView == false)
        {
            targetRot = (45f * currentAngle) + 45f;
        }

        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, targetRot, 0f), rotateSpeed * Time.deltaTime); // linear interpolation, log itp.

        theCam.localRotation = Quaternion.Slerp(theCam.localRotation, Quaternion.Euler(targetCamViewAngle, 0f, 0f), rotateSpeed * Time.deltaTime);
    }

    public void SetMoveTarget(Vector3 newTarget)
    {
        moveTarget = newTarget;

        targetCamViewAngle = 45f;
        isFireView = false;
    }

    public void SetFireView()
    {
        moveTarget = GameManager.instance.activePlayer.transform.position;

        targetRot = GameManager.instance.activePlayer.transform.rotation.eulerAngles.y;

        targetCamViewAngle = fireCamViewAngle;

        isFireView = true;
    }
}
