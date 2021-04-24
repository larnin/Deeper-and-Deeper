using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldCloner : MonoBehaviour
{
    class CameraInfo
    {
        public float scale;
        public Transform transform;
        public Camera camera;

        public float near;
        public float far;
    }

    [SerializeField] GameObject m_cameraPrefab = null;
    [SerializeField] float m_scale = 0.2f;
    [SerializeField] int m_cloneCount = 2;

    List<CameraInfo> m_cameras = new List<CameraInfo>();

    private void Start()
    {
        InitCameras();
    }

    void LateUpdate()
    {
        ProcessCameras();
    }

    void InitCameras()
    {
        for (int i = 0; i < m_cloneCount * 2; i++)
        {
            if (i % 2 != 0)
                continue;

            CameraInfo infos = new CameraInfo();

            var obj = Instantiate(m_cameraPrefab, transform);
            infos.transform = obj.transform;
            infos.camera = obj.GetComponent<Camera>();
            int height = i / 2 + 1;
            if (i % 2 == 0)
                infos.scale = Mathf.Pow(m_scale, height);
            else infos.scale = 1.0f / Mathf.Pow(m_scale, height);

            m_cameras.Add(infos);
        }
    }

    void ProcessCameras()
    {
        GetCameraEvent camera = new GetCameraEvent();
        Event<GetCameraEvent>.Broadcast(camera);

        Vector3 center = transform.position;
        Vector3 pos = camera.camera.transform.position;
        Vector3 forward = camera.camera.transform.forward;
        Quaternion rotation = camera.camera.transform.rotation;

        float near = camera.camera.nearClipPlane;
        float far = camera.camera.farClipPlane;

        foreach (var c in m_cameras)
        {
            c.transform.rotation = rotation;

            Vector3 zero = -center / c.scale + center;

            Vector3 cameraPos = pos / c.scale + zero;
            c.transform.position = cameraPos;

            var mat = camera.camera.projectionMatrix;

            var dir = Vector3.Project(pos - cameraPos, forward);
            float dist = dir.magnitude;
            if (Vector3.Dot(dir, forward) < 0)
                dist *= -1;

            float currentNear = near + dist;
            float currentFar = far + dist;

            c.near = currentNear;
            c.far = currentFar;

            mat.m22 = -(currentFar + currentNear) / (far - near);
            mat.m23 = -(2 * currentFar * currentNear) / (far - near);

            c.camera.projectionMatrix = mat;
        }
    }

    private void OnGUI()
    {
        for (int i = 0; i < m_cameras.Count; i++)
        {
            CameraInfo c = m_cameras[i];

            GUI.Label(new Rect(10, 20 * i, 400, 20), "S " + c.scale + " N " + c.near + " F " + c.far);
        }
    }

}
