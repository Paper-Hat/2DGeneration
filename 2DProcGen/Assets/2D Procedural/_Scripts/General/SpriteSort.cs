using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSort : MonoBehaviour
{
    private SpriteRenderer _objSprite;
    // Start is called before the first frame update
    void Start(){_objSprite = GetComponent<SpriteRenderer>();    }

    void LateUpdate()
    {
        _objSprite.sortingOrder = (int)(transform.position.y + _objSprite.sprite.textureRectOffset.y);
    }
}
