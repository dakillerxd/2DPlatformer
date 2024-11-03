using UnityEngine;


public class BoosterTrigger : MonoBehaviour
{

    public float boostSpeed = 1000f;
    public Vector3 triggerSize = new(0.6099201f, 0.6345624f, 2.86218f);
    public bool alreadyBoosted = false;

    void Start()
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        else
        {
            BoxCollider boxCollider = gameObject.AddComponent<BoxCollider>();
            boxCollider.size = triggerSize;
            boxCollider.isTrigger = true;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && (!alreadyBoosted))
        {
            Rigidbody playerRigidbody = other.GetComponent<Rigidbody>();
            if (playerRigidbody != null)
            {
                Debug.Log("Boost Player");
                Vector3 forceDirection = transform.forward;
                playerRigidbody.AddForce(boostSpeed * Time.deltaTime * forceDirection, ForceMode.Impulse);
            }

        }
    }
}