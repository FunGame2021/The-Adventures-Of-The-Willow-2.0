using TMPro;
using UnityEngine;

public class StarsFunction : MonoBehaviour
{
    public static StarsFunction instance;
    public TextMeshProUGUI TXTStars;
    public int star;

    void Start()
    {
        TXTStars.text = star.ToString();
        if (instance == null)
        {
            instance = this;
        }

    }

    void Update()
    {
        TXTStars.text = star.ToString();
    }
    public void SaveChangeStar(int coinValue)
    {
        star = coinValue;
        TXTStars.text = star.ToString();
    }
    public void ChangeStar(int coinValue)
    {
        star += coinValue;
        TXTStars.text = star.ToString();
    }

    public void ChangeMinusStar(int coinValue)
    {
        star -= coinValue;
        TXTStars.text = star.ToString();
    }

    public void AddStar()
    {
        star += 1;
    }
}
