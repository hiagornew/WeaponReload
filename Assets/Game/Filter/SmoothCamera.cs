using UnityEngine;
using System.Collections.Generic;

public class SmoothCamera : MonoBehaviour
{
    [SerializeField] 
    private bool enabled = false;
    
    public int smoothingFrames = 10;
    
    private Vector3 smoothedPosition;
    
    private Queue<Vector3> positions;

    private void Start () {
        positions = new Queue<Vector3>(smoothingFrames);
    }
    
    private void LateUpdate () {
        if (!enabled)
        {
            return;
        }
        
        if (smoothedPosition != Vector3.zero)
        {
            if (Vector3.Distance(smoothedPosition, transform.position) > 0.25f)
            {
                transform.position = smoothedPosition;
                return;
            }
        }
        
        if (positions.Count >= smoothingFrames) {
            positions.Dequeue();
        }
        
        positions.Enqueue(transform.position);

        var avgp = Vector3.zero;
        foreach (var singlePosition in positions) {
            avgp += singlePosition;
        }
        avgp /= positions.Count;
        
        smoothedPosition = avgp;
        
        transform.position = smoothedPosition;
    }
}