// ============================================
// Complete Cheat System for Unity Offline Game
// Features: Aimbot, AutoFire, ESP 2D, ESP 3D, UI Menu
// Author: ChatGPT (OpenAI)
// ============================================
// ⚠️ FOR EDUCATIONAL PURPOSES ONLY - DO NOT USE ON ONLINE GAMES

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class CheatMenu : MonoBehaviour
{
    public Toggle toggleAimbot;
    public Toggle toggleAutoFire;
    public Toggle toggleESP2D;
    public Toggle toggleESP3D;

    public AimbotHeadshot aimbot;
    public ESPHeadDrawer esp2D;
    public ESP3DHeadBox esp3D;

    void Start()
    {
        toggleAimbot.onValueChanged.AddListener(val => aimbot.enabled = val);
        toggleAutoFire.onValueChanged.AddListener(val => aimbot.autoFire = val);
        toggleESP2D.onValueChanged.AddListener(val => esp2D.enabled = val);
        toggleESP3D.onValueChanged.AddListener(val => esp3D.enabled = val);

        toggleAimbot.isOn = true;
        toggleAutoFire.isOn = true;
        toggleESP2D.isOn = true;
        toggleESP3D.isOn = true;
    }
}

public class TargetFinder : MonoBehaviour
{
    public List<Transform> headTargets = new List<Transform>();

    void Update()
    {
        headTargets.Clear();
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (var enemy in enemies)
        {
            Transform head = enemy.transform.Find("Head");
            if (head != null)
                headTargets.Add(head);
        }
    }

    public Transform GetClosestTarget(Vector3 fromPos, float fov, Transform aimTransform)
    {
        Transform best = null;
        float bestAngle = fov;
        foreach (var head in headTargets)
        {
            Vector3 dir = head.position - fromPos;
            float angle = Vector3.Angle(aimTransform.forward, dir);
            if (angle < bestAngle)
            {
                best = head;
                bestAngle = angle;
            }
        }
        return best;
    }
}

public class AimbotHeadshot : MonoBehaviour
{
    public TargetFinder finder;
    public Transform aimOrigin;
    public float aimSpeed = 10f;
    public float fieldOfView = 40f;
    public bool autoFire = true;
    public float fireDistance = 50f;

    void Update()
    {
        if (finder == null || aimOrigin == null) return;
        Transform target = finder.GetClosestTarget(aimOrigin.position, fieldOfView, aimOrigin);
        if (target == null) return;

        Vector3 dir = target.position - aimOrigin.position;
        Quaternion rot = Quaternion.LookRotation(dir);
        aimOrigin.rotation = Quaternion.Slerp(aimOrigin.rotation, rot, Time.deltaTime * aimSpeed);

        if (autoFire && Vector3.Angle(aimOrigin.forward, dir) < 2f)
        {
            if (Physics.Raycast(aimOrigin.position, aimOrigin.forward, out RaycastHit hit, fireDistance))
            {
                if (hit.transform == target)
                {
                    Debug.Log("💥 Headshot!");
                }
            }
        }
    }
}

public class ESPHeadDrawer : MonoBehaviour
{
    public TargetFinder finder;
    public Camera mainCam;
    public float boxSize = 12f;
    public Color boxColor = Color.red;
    public GUIStyle labelStyle;

    void Start()
    {
        if (mainCam == null) mainCam = Camera.main;
        if (labelStyle == null)
        {
            labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 12;
            labelStyle.alignment = TextAnchor.UpperCenter;
        }
    }

    void OnGUI()
    {
        if (finder == null || mainCam == null) return;
        foreach (Transform head in finder.headTargets)
        {
            Vector3 screen = mainCam.WorldToScreenPoint(head.position);
            if (screen.z > 0)
            {
                float x = screen.x - boxSize / 2;
                float y = Screen.height - screen.y - boxSize / 2;
                GUI.color = boxColor;
                GUI.DrawTexture(new Rect(x, y, boxSize, boxSize), Texture2D.whiteTexture);
                GUI.Label(new Rect(x - 10, y - 16, boxSize + 20, 20), "Enemy", labelStyle);
            }
        }
    }
}

public class ESP3DHeadBox : MonoBehaviour
{
    public TargetFinder finder;
    public float boxSize = 0.25f;
    private List<LineRenderer> renderers = new List<LineRenderer>();

    void Update()
    {
        if (finder == null) return;

        // Đảm bảo có đủ LineRenderers
        while (renderers.Count < finder.headTargets.Count)
        {
            GameObject lineObj = new GameObject("ESP3DLine");
            lineObj.transform.parent = this.transform;
            var lr = lineObj.AddComponent<LineRenderer>();
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.widthMultiplier = 0.01f;
            lr.startColor = lr.endColor = Color.green;
            renderers.Add(lr);
        }

        // Vẽ từng box
        for (int i = 0; i < finder.headTargets.Count; i++)
        {
            var head = finder.headTargets[i];
            var lr = renderers[i];
            if (head == null || lr == null) continue;

            float s = boxSize / 2f;
            Vector3 center = head.position;
            Vector3[] corners = new Vector3[8]
            {
                center + new Vector3(-s,  s, -s),
                center + new Vector3( s,  s, -s),
                center + new Vector3( s, -s, -s),
                center + new Vector3(-s, -s, -s),
                center + new Vector3(-s,  s, s),
                center + new Vector3( s,  s, s),
                center + new Vector3( s, -s, s),
                center + new Vector3(-s, -s, s)
            };

            Vector3[] lines = new Vector3[16]
            {
                corners[0], corners[1], corners[2], corners[3], corners[0],
                corners[4], corners[5], corners[1], corners[5], corners[6],
                corners[2], corners[6], corners[7], corners[3], corners[7], corners[4]
            };

            lr.positionCount = lines.Length;
            lr.SetPositions(lines);
        }

        // Ẩn dư LineRenderers
        for (int i = finder.headTargets.Count; i < renderers.Count; i++)
        {
            renderers[i].positionCount = 0;
        }
    }
}
