using UnityEngine;
using System.Collections;

public class Fireball : MonoBehaviour
{

    public Rigidbody2D rb;
    public GameObject exploson;
    public Vector2 velocity;
    public float stopThreshold = 0.8f; // Limite para considerar que o movimento parou


    // Use this for initialization
    void Start()
    {
        Destroy(this.gameObject, 6);
        rb = GetComponent<Rigidbody2D>();
        velocity = rb.linearVelocity;

    }

    // Update is called once per frame
    void Update()
    {
        if (rb.linearVelocity.y < velocity.y)
            rb.linearVelocity = velocity;

        if (Mathf.Abs(rb.linearVelocity.x) < stopThreshold && Mathf.Abs(rb.linearVelocity.y) < stopThreshold)
        {
            // Se estiver parado, destrua o objeto
            Explode();
        }

    }

    void OnCollisionEnter2D(Collision2D col)
    {

        rb.linearVelocity = new Vector2(velocity.x, -velocity.y);


        if (col.collider.tag == "deadly")
        {

            Destroy(col.gameObject);
            Explode();
        }


/*/
        if (col.contacts[0].normal.x != 0)
        {
            Explode();
        }*/

    }

    void Explode()
    {

        Instantiate(exploson, transform.position, Quaternion.identity);

        Destroy(this.gameObject);

    }
}