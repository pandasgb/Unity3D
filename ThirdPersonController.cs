using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class ThirdPersonController2 : MonoBehaviour
{

    [SerializeField] private float m_moveSpeed = 2;
    [SerializeField] private float m_jumpForce = 4;


    private float m_currentV = 0;
    private float m_currentH = 0;

    private Animator m_animator;
    private Rigidbody m_rigidBody;

    private readonly float m_interpolation = 10;
    private readonly float m_walkScale = 0.33f;


    private bool m_wasGrounded;
    private Vector3 m_currentDirection = Vector3.zero;
    private Vector3 m_moveDirection = Vector3.zero;


    private float m_jumpTimeStamp = 0;
    private float m_minJumpInterval = 0.25f;


    private bool m_isGrounded;
    private List<Collider> m_collisions = new List<Collider>();

    // 加入所有animator中的参数
    int moveSpeedID;
    int jumpID;
    int landID;
    int pickupID;
    int groundedID;
    int waveID;

    // Start is called before the first frame update
    void Start()
    {
        m_rigidBody = GetComponent<Rigidbody>();
        m_animator = GetComponent<Animator>();


        // 所有动画参数转成HashID
        moveSpeedID = Animator.StringToHash("MoveSpeed");
        groundedID = Animator.StringToHash("Grounded");
        landID = Animator.StringToHash("Land");
        pickupID = Animator.StringToHash("Pickup");
        jumpID = Animator.StringToHash("Jump");
        waveID = Animator.StringToHash("Wave");


    }

    // Update is called once per frame
    void Update()
    {

        //AnimatorStateInfo animatorInfo;
        //animatorInfo = m_animator.GetCurrentAnimatorStateInfo(0);
        
        //if (Input.GetKey(KeyCode.C))
        //{
        //    m_animator.SetTrigger(pickupID);
        //}
        //if (animatorInfo.IsName("Pickup"))
        //{
        //    Debug.Log("running anim pickup");
        //    return;
        //}

        m_animator.SetBool(groundedID, m_isGrounded);
        MoveUpdate();
        m_wasGrounded = m_isGrounded;
    }


    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log(m_isGrounded);
        ContactPoint[] contactPoints = collision.contacts;
        
        foreach (ContactPoint contactPoint in contactPoints)
        {
            // 说明倾斜角度在60度以内，如果要在45度以内可设置为>0.7
            if (Vector3.Dot(contactPoint.normal,Vector3.up) > 0.5f)
            {
                if (!m_collisions.Contains(collision.collider)) {
                    m_collisions.Add(collision.collider);
                }
                m_isGrounded = true;
            }
            
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        ContactPoint[] contactPoints = collision.contacts;
        //Debug.DrawRay(contactPoints[0].point, contactPoints[0].normal, Color.red);
        //Debug.Log(contactPoints[0].point);
        //Debug.DrawRay(contactPoints[0].point, Vector3.up, Color.green);
        bool validSurfaceNormal = false;
        foreach (ContactPoint contactPoint in contactPoints)
        {
            if (Vector3.Dot(contactPoint.normal, Vector3.up) > 0.5f)
            {
                validSurfaceNormal = true;
                break;
            }
        }

        if (validSurfaceNormal)
        {
            m_isGrounded = true;
            if (!m_collisions.Contains(collision.collider))
            {
                m_collisions.Add(collision.collider);
            }
        }
        else
        {
            if (m_collisions.Contains(collision.collider))
            {
                m_collisions.Remove(collision.collider);
            }
            if (m_collisions.Count == 0)
            {
                m_isGrounded = false;
            }
        }
    }


    private void OnCollisionExit(Collision collision)
    {
        if (m_collisions.Contains(collision.collider))
        {
            m_collisions.Remove(collision.collider);
        }
        if (m_collisions.Count == 0)
        {
            m_isGrounded = false;
        }
    }




    private void MoveUpdate()
    {
        float v = Input.GetAxis("Vertical");
        float h = Input.GetAxis("Horizontal");

        if (Input.GetKey(KeyCode.LeftShift))
        {
            v *= m_walkScale;
            h *= m_walkScale;
        }

        m_currentV = Mathf.Lerp(m_currentV, v, Time.deltaTime * m_interpolation);
        m_currentH = Mathf.Lerp(m_currentH, h, Time.deltaTime * m_interpolation);

        m_moveDirection = Vector3.forward * m_currentV + Vector3.right * m_currentH;

        if (m_moveDirection != Vector3.zero)
        {
            m_currentDirection = Vector3.Slerp(m_currentDirection, m_moveDirection, Time.deltaTime * m_interpolation);
            transform.rotation = Quaternion.LookRotation(m_currentDirection);
            transform.position += m_currentDirection * m_moveSpeed * Time.deltaTime;

            m_animator.SetFloat(moveSpeedID, m_moveDirection.magnitude);
            string i = m_moveDirection.magnitude.ToString();
            //Debug.Log("speed:"+i);
        }

        JumpingAndLanding();


    }

    private void JumpingAndLanding()
    {

        bool jumpCooldowm = (Time.time - m_jumpTimeStamp) >= m_minJumpInterval;

        if (jumpCooldowm && m_isGrounded && Input.GetButton("Jump"))
        {
            m_jumpTimeStamp = Time.time;
            m_rigidBody.AddForce(Vector3.up * m_jumpForce, ForceMode.Impulse);
        }

        if (!m_wasGrounded && m_isGrounded)
        {
            m_animator.SetTrigger(landID);
        }

        if (!m_isGrounded && m_wasGrounded)
        {
            m_animator.SetTrigger(jumpID);
        }

     
    }
}
