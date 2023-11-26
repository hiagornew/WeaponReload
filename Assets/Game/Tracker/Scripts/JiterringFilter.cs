using System.Collections;
using System.Collections.Generic;
using MyBox;
using UnityEngine;

public class JiterringFilter
{
    private struct State
    {
        internal double timestamp;
        internal Vector3 pos;
        internal Quaternion rot;
    }

    // We store twenty states with "playback" information
    private State[] m_BufferedState = new State[20];
    private int m_TimestampCount;
    private float _realTimeSinceStart;

    private void Interpolation(Vector3 pos)
    {
        // Shift buffer contents, oldest data erased, 18 becomes 19, ... , 0 becomes 1
        for (int i = m_BufferedState.Length - 1; i >= 1; i--)
        {
            m_BufferedState[i] = m_BufferedState[i - 1];
        }

        // Save currect received state as 0 in the buffer, safe to overwrite after shifting
        State state = new State();
        state.timestamp = _realTimeSinceStart;
        state.pos = pos;

        m_BufferedState[0] = state;

        // Increment state count but never exceed buffer size
        m_TimestampCount = Mathf.Min(m_TimestampCount + 1, m_BufferedState.Length);
    }

    public Vector3 SyncMovment(Vector3 pos, double delay)
    {
        Interpolation(pos);
        
        _realTimeSinceStart = Time.realtimeSinceStartup;
        
        double currentTime = Time.realtimeSinceStartup;
        double interpolationTime = currentTime - delay;

        // We have a window of InterpolationDelay where we basically play back old updates.
        // By having InterpolationDelay the average ping, you will usually use interpolation.
        // And only if no more data arrives we will use the latest known position.

        // Use interpolation, if the interpolated time is still "behind" the update timestamp time:
        if (m_BufferedState[0].timestamp > interpolationTime)
        {
            for (int i = 0; i < m_TimestampCount; i++)
            {
                // Find the state which matches the interpolation time (time+0.1) or use last state
                if (m_BufferedState[i].timestamp <= interpolationTime || i == m_TimestampCount - 1)
                {
                    // The state one slot newer (<100ms) than the best playback state
                    State rhs = m_BufferedState[Mathf.Max(i - 1, 0)];

                    // The best playback state (closest to 100 ms old (default time))
                    State lhs = m_BufferedState[i];

                    // Use the time between the two slots to determine if interpolation is necessary
                    double diffBetweenUpdates = rhs.timestamp - lhs.timestamp;
                    float t = 0.0F;

                    // As the time difference gets closer to 100 ms t gets closer to 1 in 
                    // which case rhs is only used
                    if (diffBetweenUpdates > 0.0001)
                    {
                        t = (float) ((interpolationTime - lhs.timestamp) / diffBetweenUpdates);
                    }

                    // if t=0 => lhs is used directly
                    return Vector3.Lerp(lhs.pos, rhs.pos, t);
                }
            }
        }

        State latest = m_BufferedState[0];
        return Vector3.Lerp(pos, latest.pos, Time.deltaTime * 20);
    }
}
