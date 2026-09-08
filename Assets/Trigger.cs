using UnityEngine;

public class Trigger : MonoBehaviour
{
    public bool trigger1 = false;
    public bool trigger2 = false;
    public AudioSource squeak;
    public GameObject lights;
    public GameObject GetFood;
    public GameObject Generator;
    public GameObject playerLight;
    public GameObject Intruder;
    public AudioSource switc;
    public AudioSource switch2;

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == "Trigger1" && trigger1 == false)
        {
            trigger1 = true;
            squeak.enabled = true;
        }
        
        if(other.tag == "Trigger2" && trigger1 == true)
        {
            trigger2 = true;
            lights.SetActive(false);
            Generator.SetActive(true);
            ///GetFood.SetActive(false);
            playerLight.SetActive(true);
            switc.enabled=true;

        }
    }
}
