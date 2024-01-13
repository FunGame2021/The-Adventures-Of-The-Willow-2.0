using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinBlock : MonoBehaviour
{
	private Animator spriteAnim;
	private GameObject child;
	public int totalCoins;
	public Sprite disabled;
	public GameObject QuestionToDisable;

	void Start()
	{
		child = transform.GetChild(0).gameObject;
		spriteAnim = child.GetComponent<Animator>();
	}

	void OnCollisionEnter2D(Collision2D col)
	{

		if (col.collider.bounds.max.y < transform.position.y
			&& col.collider.bounds.min.x < transform.position.x + 0.5f
			&& col.collider.bounds.max.x > transform.position.x - 0.5f
			&& col.collider.tag == "Player")
		{
			if (totalCoins > 0)
			{
				spriteAnim.Play("BonusBlock_Hit");
				AudioManager.instance.PlayOneShot(FMODEvents.instance.coinCollected, this.transform.position);
				AudioManager.instance.PlayOneShot(FMODEvents.instance.emptyBlock, this.transform.position);
				CoinCollect.instance.AddCoin();
				totalCoins -= 1;
				if (totalCoins == 0)
				{
					GetComponent<Animator>().enabled = false;
					child.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().color = Color.white;
					child.transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>().sprite = disabled;
					QuestionToDisable.SetActive(false);
					AudioManager.instance.PlayOneShot(FMODEvents.instance.emptyBlock, this.transform.position);
				}
			}
			if (totalCoins == 0)
			{
				AudioManager.instance.PlayOneShot(FMODEvents.instance.emptyBlock, this.transform.position);
			}

		}

	}
}