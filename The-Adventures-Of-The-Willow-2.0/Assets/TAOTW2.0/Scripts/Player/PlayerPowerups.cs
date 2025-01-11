using UnityEngine;
using System.Collections;

public class PlayerPowerups : MonoBehaviour
{
    #region FirePower
    public GameObject projectile;
    public Vector2 velocity;
    bool canShoot = true;
    public Vector2 offset = new Vector2(0.4f, 0.1f);
    public float cooldown = 1f;
    #endregion
    void Start()
    {
        
    }

    void Update()
    {
        if (UserInput.instance.playerMoveAndExtraActions.PlayerActions.Shoot.WasPressedThisFrame() && canShoot)
        {

            GameObject go = (GameObject)Instantiate(projectile, (Vector2)transform.position + offset * transform.localScale.x, Quaternion.identity);

            go.GetComponent<Rigidbody2D>().linearVelocity = new Vector2(velocity.x * transform.localScale.x, velocity.y);


            StartCoroutine(CanShoot());


        }
    }
    IEnumerator CanShoot()
    {
        canShoot = false;
        yield return new WaitForSeconds(cooldown);
        canShoot = true;


    }
}
