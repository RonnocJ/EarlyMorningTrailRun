using System.Collections;
using Unity.Cinemachine;
using UnityEngine;

public class CameraBehavior : MonoBehaviour
{
    public Transform playerTr;
    public Vector3 offset;
    public float followSpeed;
    public CinemachineCamera cam;

    public IEnumerator ShakeCamera(float duration, float magnitude)
    {
        float elapsedTime = duration;        
        while(elapsedTime > 0)
        {
            cam.GetComponent<CinemachineBasicMultiChannelPerlin>().AmplitudeGain = magnitude;

            magnitude = magnitude * elapsedTime / duration;

            elapsedTime -= Time.deltaTime;

            yield return null;
        }

        cam.GetComponent<CinemachineBasicMultiChannelPerlin>().AmplitudeGain = 0;
    }
}
