using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketController : MonoBehaviour
{
    public List<GameObject> parts;
    public GameObject explosionPrefab;
    // Start is called before the first frame update
    private bool flying, enginesRunning, started;
    private GameObject terrain;
    private Collider col;
    private AudioSource speaker;
    private List<GameObject> engines;
    private List<GameObject> fuelTanks;
    private List<GameObject> periphery;
    private List<GameObject> stackComponents;
    private GameObject front;
    void Start()
    {
        flying = enginesRunning = started = false;
        terrain = GameObject.FindWithTag("Terrain");
        col = GetComponent<Collider>();
        speaker = GetComponent<AudioSource>();
        col.enabled = false;
        // parts = new List<GameObject>();
        engines = new List<GameObject>();
        fuelTanks = new List<GameObject>();
        stackComponents = new List<GameObject>();
        periphery = new List<GameObject>();
        front = new GameObject();
        SetPartList(parts);
        Debug.Log("init parts: " + parts.Count);
        Debug.Log("init engs: " + engines.Count);
        Debug.Log("init fts: " + fuelTanks.Count);
    }

    // Update is called once per frame
    void Update()
    {
    }

    void FixedUpdate()
    {

        //Add engine thrust
        if (enginesRunning)
        {
            if (!flying && transform.position.y > 20)
            {
                StartCoroutine("Flying");
            }
            foreach (GameObject e in engines)
            {
                e.transform.GetChild(0).SendMessage("AddThrust");
            }
        }

        //Calculate drag
        if (flying)
        {
            //Calc front stack comp
            float bestDist = float.MaxValue;
            foreach (GameObject part in stackComponents)
            {
                Vector3 dist = Vector3.Project(part.transform.position, part.GetComponent<Rigidbody>().velocity);
                if (dist.magnitude < bestDist)
                {
                    front = part;
                    bestDist = dist.magnitude;
                }
            }
            //Calc front object drag
            float fdrag = .5f * front.transform.GetChild(0).GetComponent<DragInfo>().drag * front.GetComponent<Rigidbody>().velocity.magnitude * front.GetComponent<Rigidbody>().velocity.magnitude;
            front.GetComponent<Rigidbody>().AddForce(-front.GetComponent<Rigidbody>().velocity.normalized * fdrag, ForceMode.Force);

            foreach (GameObject part in periphery)
            {
                float pdrag = .5f * part.transform.GetChild(0).GetComponent<DragInfo>().drag * part.GetComponent<Rigidbody>().velocity.magnitude * part.GetComponent<Rigidbody>().velocity.magnitude;
                part.GetComponent<Rigidbody>().AddForce(-part.GetComponent<Rigidbody>().velocity.normalized * pdrag, ForceMode.Force);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.transform.gameObject.tag == "Terrain")
        {
            StartCoroutine("Explode");
        }
    }

    IEnumerator Explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
        Debug.Log("Rocket destroyed");
        yield return null;
    }

    IEnumerator Flying()
    {
        flying = true;
        col.enabled = true;
        yield return null;
    }

    void Launch()
    {
        foreach (GameObject p in parts)
        {
            p.GetComponent<Rigidbody>().drag = 0.0f;
        }
        StartCoroutine("Launching");
        // Debug.Log("Rocket Launch command received");
    }

    IEnumerator Launching()
    {
        if (!started)
        {
            started = true;
            speaker.Play();
            Debug.Log("Launch started");
            yield return new WaitForSeconds(4.0f);
            float fuel = 0.0f;
            Debug.Log("FT: " + fuelTanks.Count);
            foreach (GameObject ft in fuelTanks)
            {
                if (ft.tag == "FuelTank")
                {
                    fuel += ft.transform.GetComponent<fInfo>().capacity;
                }
                Debug.Log("ft: " + ft);
            }

            Debug.Log("Fuel calculated");
            foreach (GameObject e in engines)
            {
                if (e.tag == "Engine")
                {
                    e.transform.GetChild(0).SendMessage("SetBurnTime", fuel / engines.Count);
                    e.transform.GetChild(0).SendMessage("Launch");
                    Debug.Log("e: " + e);
                }
            }
            Debug.Log("Rocket: Engines started");
            enginesRunning = true;
        }
    }

    void SetPartList(List<GameObject> l)
    {
        parts = l;
        engines.Clear();
        fuelTanks.Clear();
        stackComponents.Clear();
        periphery.Clear();
        foreach (GameObject p in parts)
        {
            if (p.tag == "Engine")
            {
                if (!engines.Contains(p))
                {
                    engines.Add(p);
                    stackComponents.Add(p);
                }
            }
            if (p.tag == "FuelTank")
            {
                if (!fuelTanks.Contains(p))
                {
                    fuelTanks.Add(p);
                    stackComponents.Add(p);
                }
            if (!stackComponents.Contains(p))
                    stackComponents.Add(p);
                    if (!periphery.Contains(p))
                    {
                        periphery.Add(p);
                    }
                }
            if (engines.Count > 0 && fuelTanks.Count > 0)
                gameObject.tag = "LaunchableRocket";
            p.transform.parent = gameObject.transform;
        }
    }

    void AddPart (GameObject p)
    {
        if (p.tag == "Engine")
        {
            if (!engines.Contains(p))
            {
                engines.Add(p);
                stackComponents.Add(p);
            }
        }
        if (p.tag == "FuelTank")
        {
            if (!fuelTanks.Contains(p))
            {
                fuelTanks.Add(p);
                stackComponents.Add(p);
            }
            if (p.tag == "Core")
            {
                if (!stackComponents.Contains(p))
                    stackComponents.Add(p);
            }
            if (p.tag == "Periphery")
            {
                if (!periphery.Contains(p))
                {
                    periphery.Add(p);
                }
            }
        }
        if (engines.Count > 0 && fuelTanks.Count > 0)
            gameObject.tag = "LaunchableRocket";
    }

    void AddRocket(GameObject otherRocket)
    {
        List<GameObject> tempL = new List<GameObject>();
        foreach (GameObject item in parts)
        {
            tempL.Add(item);
        }
        foreach(GameObject p in otherRocket.GetComponent<RocketController>().parts)
        {
            tempL.Add(p);
        }
        SetPartList(tempL);
    }

}
