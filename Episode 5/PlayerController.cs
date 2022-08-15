//  Code by Yeahlowflicker
//  for the Unity First Person Controller Series
//  of the YouTube channel Yeahlowflicker.
//
//  Feel free to visit and support my channel and website. Many thanks.
//  https://www.youtube.com/channel/UCATag7uzDpitt7lZ_9hdp4Q
//  https://yeahlowflicker.com
//
//  This code is distributed under the MIT License.


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [Header("References")]
    public Rigidbody rb;
    public Transform head;
    public Camera camera;



    [Header("Configurations")]
    public float walkSpeed;
    public float runSpeed;
    public float jumpSpeed;
    public float impactThreshold;



    [Header("Runtime")]
    Vector3 newVelocity;
    bool isGrounded = false;
    bool isJumping = false;
    float vyCache;



    // Start is called before the first frame update
    void Start() {
        //  Hide and lock the mouse cursor
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }




    // Update is called once per frame
    void Update() {
        // Horizontal rotation
        transform.Rotate(Vector3.up * Input.GetAxis("Mouse X") * 2f);   // Adjust the multiplier for different rotation speed

        newVelocity = Vector3.up * rb.velocity.y;
        float speed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;
        newVelocity.x = Input.GetAxis("Horizontal") * speed;
        newVelocity.z = Input.GetAxis("Vertical") * speed;

        if (isGrounded) {
            if (Input.GetKeyDown(KeyCode.Space) && !isJumping) {
                newVelocity.y = jumpSpeed;
                isJumping = true;
            }
        }
        rb.velocity = transform.TransformDirection(newVelocity);
    }

    void FixedUpdate() {
        //  Shoot a ray of 1 unit towards the ground
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1f)) {
            isGrounded = true;
        }
        else isGrounded = false;

        //  Cache the velocity in the y direction
        vyCache = rb.velocity.y;
    }

    void LateUpdate() {
        // Vertical rotation
        Vector3 e = head.eulerAngles;
        e.x -= Input.GetAxis("Mouse Y") * 2f;   //  Edit the multiplier to adjust the rotate speed
        e.x = RestrictAngle(e.x, -85f, 85f);    //  This is clamped to 85 degrees. You may edit this.
        head.eulerAngles = e;
    }




    //  This will be called constantly
    void OnCollisionStay(Collision col) {
        if (Vector3.Dot(col.GetContact(0).normal, Vector3.up) <= .6f)
            return;

        isGrounded = true;
        isJumping = false;
    }

    //  This will be called once only during collision
    void OnCollisionEnter(Collision col) {
        //  Prevent fall damage when hitting a vertical wall
        if (Vector3.Dot(col.GetContact(0).normal, Vector3.up) < .5f) {
            if (rb.velocity.y < -5f) {
                rb.velocity = Vector3.up * rb.velocity.y;
                return;
            }
        }

        //  Calculate impact force
        float acceleration = (rb.velocity.y - vyCache) / Time.fixedDeltaTime;
        float impactForce = rb.mass * Mathf.Abs(acceleration);

        //  This triggers the fall damage
        //  Add your code to the OnFallDamage() function below
        if (impactForce >= impactThreshold)
            OnFallDamage();
    }

    void OnCollisionExit(Collision col) {
        isGrounded = false;
    }




    //  Add your fall damage code here!
    void OnFallDamage() {
        Debug.Log("Ouch!");
    }



    //  A helper function
    //  Clamp the vertical head rotation (prevent bending backwards)
    public static float RestrictAngle(float angle, float angleMin, float angleMax) {
        if (angle > 180)
            angle -= 360;
        else if (angle < -180)
            angle += 360;

        if (angle > angleMax)
            angle = angleMax;
        if (angle < angleMin)
            angle = angleMin;

        return angle;
    }
}
