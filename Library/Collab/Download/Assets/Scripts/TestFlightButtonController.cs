using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestFlightButtonController : MonoBehaviour
{

    public GameObject engine;
    public List<GameObject> triggers;
    // Start is called before the first frame update
    private bool launched;
    void Start()
    {
        launched = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        if (!launched && triggers.Contains(other.gameObject))
        {
            StartCoroutine("Launch");
        }
    }

    IEnumerator BePressed()
    {
        for (int i = 0; i < 105; i++)
        {
            transform.Translate(new Vector3(0, (float)-0.0002, 0));
            yield return new WaitForSeconds((float)0.0002);
        }

        yield return new WaitForSeconds(2);
        StartCoroutine("Unpress");
    }

    IEnumerator Unpress()
    {
        for (int i = 0; i < 53; i++)
        {
            transform.Translate(new Vector3(0, (float)0.0004, 0));
            yield return new WaitForSeconds((float)0.0002);
        }
        yield return null;
    }

    IEnumerator Launch()
    {
        engine.SendMessage("Launch");
        yield return null;
    }
}
