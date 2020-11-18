using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FuelUpButtonController : MonoBehaviour
{
    public List<GameObject> triggers;
    public GameObject launchButton;
    // Start is called before the first frame update

    private bool fueled, rocketready, areaClear;
    AudioSource button;
    void Start()
    {
        fueled = false;
        rocketready = false;
        areaClear = false;
        button = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {

        button.Play();
        if (!fueled)
        {
            StartCoroutine("BePressed");
            if (areaClear && rocketready)
            {
                fueled = true;
                launchButton.SendMessage("FueledUp", true);
            }
        }


    }

    IEnumerator BePressed()
    {
        for (int i = 0; i < 210; i++)
        {
            transform.Translate(new Vector3(0, (float)-0.0001, 0));
        }
        
        yield return new WaitForSeconds(2);
        StartCoroutine("Unpress");
    }

    IEnumerator Unpress()
    {
        for(int i = 0; i < 105; i++)
        {
            transform.Translate(new Vector3(0, (float)0.0002, 0));
        }
        yield return null;
    }

    void RocketReady(bool r)
    {
        rocketready = r;
    }

    void AreaClear()
    {
        areaClear = true;
    }
}
