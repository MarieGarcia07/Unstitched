using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pickupable : MonoBehaviour
{
    [Header("Custom Gravity Settings")]
    [Tooltip("How strong gravity pulls the object down. Default Unity gravity = 9.81.")]
    [SerializeField] private float gravityScale = 1f;

    public string itemName;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
    }

    private void FixedUpdate()
    {
        if (!rb.isKinematic)
        {
            Vector3 customGravity = Physics.gravity * gravityScale;
            rb.AddForce(customGravity, ForceMode.Acceleration);
        }
    }

    public void SetGravityScale(float newScale)
    {
        gravityScale = newScale;
    }

    public float GetGravityScale()
    {
        return gravityScale;
    }
}

