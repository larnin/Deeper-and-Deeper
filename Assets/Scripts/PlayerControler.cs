using UnityEngine;
using System.Collections;

public class PlayerControler : MonoBehaviour
{
    const string HorizontalInput = "Horizontal";
    const string VerticalInput = "Vertical";
    const string MouseXInput = "Mouse X";
    const string MouseYInput = "Mouse Y";
    const string JumpInput = "Jump";

    [SerializeField] float m_groundAcceleration = 20;
    [SerializeField] float m_airAcceleration = 10;
    [SerializeField] float m_maxSpeed = 5;
    [SerializeField] float m_rotationSensibility = 1;
    [SerializeField] float m_minY = -80;
    [SerializeField] float m_maxY = 80;
    [SerializeField] float m_groundTestDistance = 0.1f;
    [SerializeField] LayerMask m_groundLayer;
    [SerializeField] float m_jumpPower = 5;

    Rigidbody m_rigidbody = null;
    Camera m_camera = null;
    CapsuleCollider m_collider = null;

    float m_forward = 0;
    float m_straf = 0;
    float m_jumping = 100;

    bool m_grounded = false;

    void Start()
    {
        m_rigidbody = GetComponent<Rigidbody>();
        m_collider = GetComponent<CapsuleCollider>();
        if (m_collider == null)
            m_collider = GetComponentInChildren<CapsuleCollider>();
        m_camera = GetComponentInChildren<Camera>();
    }
    
    void Update()
    {
        ProcessRotation();

        ProcessInputs();
    }

    private void FixedUpdate()
    {
        ProcessGrounded();

        ProcessMovement();

        ProcessJump();
    }

    void ProcessGrounded()
    {
        Vector3 center = m_collider.center;
        if (m_collider.direction == 1)
            center.y -= m_collider.height / 2 - m_collider.radius;
        center.y -= m_groundTestDistance;
        center = transform.TransformDirection(center);
        Vector3 pos = transform.position + center;

        var colliders = Physics.OverlapSphere(pos, m_collider.radius);

        if(colliders.Length == 0)
            m_grounded = false;
        else
        {
            m_grounded = false;
            for (int i = 0; i < colliders.Length; i++)
            {
                Transform t = colliders[i].transform;
                if (t == transform)
                    continue;
                m_grounded = true;
            }
        }
    }
    
    void ProcessRotation()
    {
        float horizontal = transform.localRotation.eulerAngles.y;
        float vertical = m_camera.transform.localRotation.eulerAngles.x;

        float dirV = Input.GetAxis(MouseYInput);
        if (!Settings.instance.inverseY)
            dirV *= -1;
        float dirH = Input.GetAxis(MouseXInput);

        horizontal += dirH * m_rotationSensibility;

        vertical += dirV * m_rotationSensibility;
        if (vertical > 180)
            vertical -= 360;
        if (vertical > m_maxY)
            vertical = m_maxY;
        if (vertical < m_minY)
            vertical = m_minY;

        transform.localRotation = Quaternion.Euler(0, horizontal, 0);
        m_camera.transform.localRotation = Quaternion.Euler(vertical, 0, 0);
    }

    void ProcessInputs()
    {
        m_forward = Input.GetAxis(VerticalInput);
        m_straf = Input.GetAxis(HorizontalInput);
        m_jumping += Time.deltaTime;
        if (Input.GetButtonDown(JumpInput))
            m_jumping = 0;
    }

    void ProcessMovement()
    {
        Vector3 velocity = m_rigidbody.velocity;
        Vector3 forward = transform.forward;

        Vector2 velocityXZ = new Vector2(velocity.x, velocity.z);
        Vector2 forwardXZ = new Vector2(forward.x, forward.z);
        forwardXZ.Normalize();
        Vector2 rightXZ = new Vector2(forwardXZ.y, -forwardXZ.x);

        float forwardSpeed = Vector2.Dot(velocityXZ, forwardXZ);
        float rightSpeed = Vector2.Dot(velocityXZ, rightXZ);

        float acceleration = GetAcceleration();

        float targetForwardSpeed = m_maxSpeed * m_forward;
        float targetRightSpeed = m_maxSpeed * m_straf;

        if(forwardSpeed > targetForwardSpeed)
        {
            forwardSpeed -= acceleration * Time.deltaTime;
            if (forwardSpeed < targetForwardSpeed)
                forwardSpeed = targetForwardSpeed;
        }
        else if(forwardSpeed < targetForwardSpeed)
        {
            forwardSpeed += acceleration * Time.deltaTime;
            if (forwardSpeed > targetForwardSpeed)
                forwardSpeed = targetForwardSpeed;
        }

        if (rightSpeed > targetRightSpeed)
        {
            rightSpeed -= acceleration * Time.deltaTime;
            if (rightSpeed < targetRightSpeed)
                rightSpeed = targetRightSpeed;
        }
        else if (rightSpeed < targetRightSpeed)
        {
            rightSpeed += acceleration * Time.deltaTime;
            if (rightSpeed > targetRightSpeed)
                rightSpeed = targetRightSpeed;
        }

        Vector2 speed = forwardSpeed * forwardXZ + rightSpeed * rightXZ;

        m_rigidbody.velocity = new Vector3(speed.x, velocity.y, speed.y);
    }

    void ProcessJump()
    {
        if (m_grounded && m_jumping < 0.1f)
        {
            Vector3 velocity = m_rigidbody.velocity;
            velocity.y = m_jumpPower;
            m_rigidbody.velocity = velocity;
            m_jumping = 100;
        }
    }

    float GetAcceleration()
    {
        if (m_grounded)
            return m_groundAcceleration;
        return m_airAcceleration;
    }
}
