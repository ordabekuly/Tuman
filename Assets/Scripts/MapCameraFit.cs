using UnityEngine;

[RequireComponent(typeof(Camera))]
public sealed class MapCameraFit : MonoBehaviour
{
    private Camera view;
    private void Awake()=>view=GetComponent<Camera>();
    private void LateUpdate()=>view.orthographicSize=Mathf.Max(8.7f,12.7f/Mathf.Max(.1f,view.aspect));
}
