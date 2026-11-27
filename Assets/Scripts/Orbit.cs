using UnityEngine;

public class Orbit : MonoBehaviour
{
    [Header("Orbit Settings")]
    public Transform target; //object that is the center of the radius
    public float speed = 30f; //speed of the orbit
    public float radius = 5f;

    void Start()
    {
        SetInitialPosition();
    }
    public void SetInitialPosition()
    {
        if (target != null)
        {
            transform.position = target.position + new Vector3(radius, 0f, 0f);
        }
    }
    void Update()
    {
        if (target != null)
        {
            transform.RotateAround(target.position, Vector3.up, speed * Time.deltaTime);
        }    
    }

}
