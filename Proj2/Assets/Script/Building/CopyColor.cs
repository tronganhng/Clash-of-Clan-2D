using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyColor : MonoBehaviour
{
    public SpriteRenderer parentSprite;

    // Update is called once per frame
    void Update()
    {
        GetComponent<SpriteRenderer>().color = parentSprite.color;
    }
}
