using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    // global params
    public GameObject explosion;
    public GameObject smoke;
    public GameObject bullet;
    public ApplyBlink applyBlinkObject;
    public float yPos;
    public float timeGap;

    // private variables
    private float timer;
    private float xPos, zPos;
    private int counter;

    // Start is called before the first frame update
    void Start()
    {
        timer = 0f;
        counter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // generate random snow bullets
        timer += Time.deltaTime;
        if(timer > timeGap || counter == 0)
        {
            
            timer = 0f;
            xPos = Random.Range(-5.0f, 5.0f);
            zPos = Random.Range(5.0f, 7.0f);

            Vector3 pos = new Vector3(xPos, yPos, zPos);
            Instantiate(bullet, pos, Quaternion.identity);
            counter++;

        }
        
        
    }

    // detect collision, destroy the bullet and create an explosion
    private void OnCollisionEnter(Collision collision)
    {
        GameObject temp = Instantiate(explosion, collision.transform.position, Quaternion.identity);
        temp.GetComponent<GenerateTextures>().smoke = smoke;
        temp.GetComponent<GenerateTextures>().ab = applyBlinkObject;
        Destroy(collision.gameObject);
    }
}
