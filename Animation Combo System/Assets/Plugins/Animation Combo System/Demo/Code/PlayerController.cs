using UnityEngine;

/// <summary>
/// a simple 3rd person controller
/// </summary>
public class PlayerController : MonoBehaviour
{
    [Header("Locomotion")]
    public float RotationSpeed = 5;
    public float moveSpeed = 1;

    [Header("Grounding")]
    public float RayLength = 0.5f;
    public LayerMask GroundMask;

    private Animator _anim;
    private Transform _cam;
    private Rigidbody _rb;

    private GameObject[] targets;
    private Vector3 inputV;
    private bool focusMode;

    public bool AxeEquiped { get; private set; }

    private void Start()
    {
        _anim = GetComponent<Animator>();
        _rb = GetComponent<Rigidbody>();
        _cam = FindObjectOfType<Camera>().transform; //there should be only 1 in the demo scene (the 3rd person cam)

        targets = GameObject.FindGameObjectsWithTag("Enemy");

        AxeEquiped = true;
    }

    private void FixedUpdate()
    {
        PlayerMotor();
        UpdateAnimParams();
    }
     
    private void OnAnimatorMove()
    {
        if(!Grounded()) return;

        Vector3 v = _anim.deltaPosition * moveSpeed / Time.deltaTime;
        v.y = _rb.velocity.y;

        _rb.velocity = v;
    }

    /// <summary>
    /// simple player motor
    /// </summary>
    private void PlayerMotor()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        if (Input.GetKeyDown(KeyCode.F))
        {
            focusMode = !focusMode;
        }

        Quaternion _camOri = Quaternion.LookRotation(new Vector3(_cam.forward.x, 0f, _cam.forward.z).normalized);
        inputV = _camOri * new Vector3(h, 0f, v);

        //Calculate rotation (Player)
        Vector3 _forward = focusMode ? CalculateStrafingForward() : inputV;
        float _angle = Mathf.Atan2(_forward.x, _forward.z) * Mathf.Rad2Deg;

        Quaternion _playerOrientation = Quaternion.AngleAxis(_angle, _rb.transform.up);
        Quaternion _finalOrientation = Quaternion.Lerp(_rb.rotation, _playerOrientation, Time.deltaTime * RotationSpeed);

        //apply rotation (Player)
        _rb.MoveRotation(_finalOrientation);
        inputV = _rb.transform.InverseTransformDirection(inputV);
    }

    /// <summary>
    /// update the animations params
    /// </summary>
    private void UpdateAnimParams()
    {
        _anim.SetFloat("Forward", inputV.z, 0.1f, Time.deltaTime);
        _anim.SetFloat("Right", inputV.x, 0.1f, Time.deltaTime);
        _anim.SetBool("Moving", inputV.sqrMagnitude >= 0.05f);

        if (Input.GetKeyDown(KeyCode.E))
        {
            AxeEquiped = !AxeEquiped;

            _anim.SetTrigger(AxeEquiped ? "Equip2" : "Disarm2");
        }
    }

    /// <summary>
    /// calculate a pivot to strafe relative to
    /// </summary>
    /// <returns></returns>
    private Vector3 CalculateStrafingForward()
    {
        if (targets.Length == 0) return _cam.forward;

        Transform target = targets[0].transform;
        float closestDist = Vector3.SqrMagnitude(transform.position - target.position);
        for (int i = 1; i < targets.Length; i++)
        {
            float d2 = Vector3.SqrMagnitude(targets[i].transform.position - transform.position);
            if (d2 <= closestDist)
            {
                closestDist = d2;
                target = targets[i].transform;
            }
        }

        return target.position - transform.position;
    }

    /// <summary>
    /// simple grounded
    /// </summary>
    /// <returns></returns>
    private bool Grounded()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.1f, Vector3.down);
        return Physics.Raycast(ray, RayLength, GroundMask);
    }

}
