using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletHoleDecalBehaviour : PoolingObject
{
    public static BulletHoleDecalBehaviour lastBullet;

    [SerializeField] private Material normalColor;
    [SerializeField] private Material highlightedColor;

    private MeshRenderer meshRenderer;

    private void OnValidate()
    {
        if(!meshRenderer)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }
    }

    private void OnEnable()
    {
        //ChangeToHighlightedColor();

        lastBullet?.ChangeToNormalColor();
        
        lastBullet = this;
    }

    public void ChangeToHighlightedColor()
    {
        meshRenderer.material = highlightedColor;
    }

    public void ChangeToNormalColor()
    {
        meshRenderer.material = normalColor;
    }
}
