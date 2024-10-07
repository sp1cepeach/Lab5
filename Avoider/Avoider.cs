using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using Color = UnityEngine.Color;
using Debug = UnityEngine.Debug;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(NavMeshAgent))]
public class Avoider : MonoBehaviour
{
    public Transform avoidee;
    public float range = 4;
    public bool showGizmos = true;
    private NavMeshAgent nav;
    private Vector3 nearestHidingSpot;
    private List<Vector2> samples;

    void Start()
    {
        if (avoidee == null)
        {
            Debug.LogWarning("Avoider does not have an avoidee to avoid");
        }

        nav = GetComponent<NavMeshAgent>();
        if (nav == null)
        {
            Debug.LogWarning("Avoider must have a NavMeshAgent component");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (avoidee != null)
        {
            if (WithinSightOfAvoidee(transform.position))
            {
                print("In line of sight of avoidee");

                FindNearestHidingSpot();

                if (nearestHidingSpot != Vector3.positiveInfinity)
                {
                    nav.destination = (transform.position + nearestHidingSpot);
                }
            }
            else
            {
                nav.destination = transform.position;
            }
        }
    }

    Vector3 FindNearestHidingSpot()
    {
        nearestHidingSpot = Vector3.positiveInfinity;
        PoissonDiscSampler sampler = new PoissonDiscSampler(2 * range, 2 * range, 5f);
        samples = sampler.Samples().ToList<Vector2>();
        foreach (Vector2 sample in samples)
        {
            Vector2 centered = sample - new Vector2(range, range);
            Vector3 spot = new Vector3(centered.x, 0, centered.y);

            if (!WithinSightOfAvoidee(transform.position + spot)
                && ReachableByAvoider(transform.position + spot)
                && spot.magnitude < nearestHidingSpot.magnitude)
            {
                nearestHidingSpot = spot;
            }
        }
        return nearestHidingSpot;
    }

    bool WithinSightOfAvoidee(Vector3 position)
    {
        RaycastHit hit;
        Physics.Raycast(position, avoidee.position - position, out hit);
        if (hit.transform.gameObject == avoidee.gameObject)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    bool ReachableByAvoider(Vector3 position)
    {
        NavMeshPath path = new NavMeshPath();
        nav.CalculatePath(position, path);
        return path.status == NavMeshPathStatus.PathComplete;
    }

    void OnDrawGizmos()
    {
        if (showGizmos)
        {
            if (samples != null)
            {
                foreach (Vector2 sample in samples)
                {
                    Vector2 centered = sample - new Vector2(range, range);
                    Vector3 spot = new Vector3(centered.x, 0, centered.y);
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(transform.position, transform.position + spot);
                }
            }
            if (nearestHidingSpot != Vector3.positiveInfinity)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, transform.position + nearestHidingSpot);
            }
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
