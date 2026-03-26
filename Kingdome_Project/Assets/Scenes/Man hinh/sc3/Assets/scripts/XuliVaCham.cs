using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class XuliVaCham : MonoBehaviour
{
  
    public int Gem = 0;
    public TextMeshProUGUI GemText;
    void Start()
    {
        GemText.SetText(Gem.ToString());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Items"))
        {
            Gem++;
            GemText.SetText(Gem.ToString());
            Destroy(collision.gameObject);

        }
        
    }

    public int CollectedGemsCount()
    {
        return Gem;
    }
}
