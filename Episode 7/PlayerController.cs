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



    [Header("Camera Effects")]
    public float baseCameraFov = 60f;
    public float baseCameraHeight = .85f;

    public float walkBobbingRate = .75f;
    public float runBobbingRate = 1f;
    public float maxWalkBobbingOffset = .2f;
    public float maxRunBobbingOffset = .3f;

    public float cameraShakeThreshold = 10f;
    [Range(0f, 0.03f)] public float cameraShakeRate = .015f;
    public float maxVerticalFallShakeAngle = 40f;
    public float maxHorizontalFallShakeAngle = 40f;



    [Header("Audio")]
    public AudioSource audioWalk;
    public AudioSource audioWalkConcrete;
    public AudioSource audioWind;
    public float windPitchMultiplier;



    [Header("Runtime")]
    Vector3 newVelocity;
    bool isGrounded = false;
    bool isJumping = false;
    float vyCache;
    string activeAudioName = "default";



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


        //  If controls are pressed and is touching the ground
        bool isMovingOnGround = (Input.GetAxis("Vertical") != 0f || Input.GetAxis("Horizontal") != 0f) && isGrounded;


        //  Head bob
        if (isMovingOnGround) {
            float bobbingRate = Input.GetKey(KeyCode.LeftShift) ? runBobbingRate : walkBobbingRate;
            float bobbingOffset = Input.GetKey(KeyCode.LeftShift) ? maxRunBobbingOffset : maxWalkBobbingOffset;
            Vector3 targetHeadPosition = Vector3.up * baseCameraHeight + Vector3.up * (Mathf.PingPong(Time.time * bobbingRate, bobbingOffset) - bobbingOffset*.5f);
            head.localPosition = Vector3.Lerp(head.localPosition, targetHeadPosition, .1f);
        }



        //  Audio
        //  You may edit the pitch values for the audio to achieve different sounds and speeds
        //  Remember, pitch is related to the playback speed of the audio
        audioWalk.enabled = isMovingOnGround && activeAudioName == "default";
        audioWalk.pitch = Input.GetKey(KeyCode.LeftShift) ? 1.75f : 1f;

        audioWalkConcrete.enabled = isMovingOnGround && activeAudioName == "concrete";
        audioWalkConcrete.pitch = Input.GetKey(KeyCode.LeftShift) ? 1.25f : .8f;

        audioWind.enabled = true;
        audioWind.pitch = Mathf.Clamp(Mathf.Abs(rb.velocity.y * windPitchMultiplier), 0f, 2f) + Random.Range(-.1f, .1f);
    }

    void FixedUpdate() {
        //  Shoot a ray of 1 unit towards the ground
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 1f)) {
            isGrounded = true;

            //  Check the ground tag for different walking sounds
            //  Edit this part to check for the other tags of yours!
            if (hit.transform.tag == "concrete")
                activeAudioName = "concrete";
            else activeAudioName = "default";
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

        // FOV
        float fovOffset = (rb.velocity.y < 0f) ? Mathf.Sqrt(Mathf.Abs(rb.velocity.y)) : 0f;
        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, baseCameraFov + fovOffset, .25f);

        // Fall Effect
        if (!isGrounded && Mathf.Abs(rb.velocity.y) >= cameraShakeThreshold) {
            Vector3 newAngle = head.localEulerAngles;
            newAngle += Vector3.right * Random.Range(-maxVerticalFallShakeAngle, maxVerticalFallShakeAngle);
            newAngle += Vector3.up * Random.Range(-maxHorizontalFallShakeAngle, maxHorizontalFallShakeAngle);
            head.localEulerAngles = Vector3.Lerp(head.localEulerAngles, newAngle, cameraShakeRate);
        }
        else {
            //  We have to reset the y-rotation of the head
            //  because we added a random value to it when falling!
            e = head.localEulerAngles;
            e.y = 0f;
            head.localEulerAngles = e;
        }
    }




    //  This will be called constantly
    void OnCollisionStay(Collision col) {
        if (Vector3.Dot(col.GetContact(0).normal, Vector3.up) <= .6f)
            return;

        isGrounded = true;
        isJumping = false;

        if (col.transform.tag == "concrete")
            activeAudioName = "concrete";
        else activeAudioName = "default";
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
