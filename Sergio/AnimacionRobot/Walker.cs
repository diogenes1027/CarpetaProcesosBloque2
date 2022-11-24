using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

//Diógenes Grajales Corona A01653251
public class Walker : MonoBehaviour
{
    public enum PARTS {
        RP_HEAP, RP_TORSO, RP_CHEST, RP_NECK, RP_HEAD,
        RTHIGH, RLEG, RFOOT,
        LTHIGH, LLEG, LFOOT,
        RSHOULDER, RARM, RFOREARM, RHAND,
        LSHOULDER, LARM, LFOREARM, LHAND
        
        
        
    };

    public struct BACK_FORTH {
        public float delta, dir, val, min, max;
        public BACK_FORTH(float _delta, float _dir, float _val, float _min, float _max) {
            delta = _delta;
            dir = _dir;
            val = _val;
            min = _min;
            max = _max;
        }

        public void Update() {
            val += delta * dir;
            if (val <= min || val >= max) dir = -dir;
        }
    };

    public static void INIT_PART(ref List<GameObject> parts,
                                ref List<Matrix4x4> locations,
                                ref List<Matrix4x4> scales,
                                PrimitiveType type,
                                int index,
                                Color color,
                                string name,
                                Vector3 scale,
                                Vector3 position
                                )
    {
     
        parts.Add(GameObject.CreatePrimitive(type));
        parts[index].GetComponent<MeshRenderer>().material.SetColor("_Color",color);
        parts[index].name = name;
        parts[index].GetComponent<BoxCollider>().enabled = false;
        scales.Add(Transformations.Scale(scale.x, scale.y, scale.z));
        locations.Add(Transformations.Translate(position.x, position.y, position.z));

    }

    List<GameObject> go_parts;
    List<Matrix4x4> m_locations;
    List<Matrix4x4> m_scales;
    Vector3[] v3_originals;
    BACK_FORTH rY;
    BACK_FORTH rk;
    BACK_FORTH rX;
    BACK_FORTH jump;

    ArmRobot leftArm;
    ArmRobot rightArm;

    LegRobot leftLeg;
    LegRobot rightLeg;

    Camera MainCamera;

    // Start is called before the first frame update
    void Start()
    {
        leftArm = gameObject.AddComponent<ArmRobot>();
        rightArm = gameObject.AddComponent<ArmRobot>();

        leftLeg = gameObject.AddComponent<LegRobot>();
        rightLeg = gameObject.AddComponent<LegRobot>();

        MainCamera = Camera.main;

        MainCamera.clearFlags = CameraClearFlags.SolidColor;

        MainCamera.backgroundColor = new Color(76f / 255f, 92f / 255f, 118f / 255f);

        MainCamera.transform.localPosition = new Vector3(-5.74f,2.86f,-3.01f);
        MainCamera.transform.localRotation = Quaternion.Euler(20.4f, 62.8f, 0f);

        rY = new BACK_FORTH(0.08f, 1f, 0f, -9f, 9f);
        rk = new BACK_FORTH(0.08f, -1f, 9f, -9f, 9f);
        jump = new BACK_FORTH(0.0005f, 1f, 0f, -0.05f, 0.05f);
        rX = new BACK_FORTH(0.05f, 1f, 0f, 0f, 8f);

        go_parts = new List<GameObject>();
        m_scales = new List<Matrix4x4>();
        m_locations = new List<Matrix4x4>();
        //HEAP
        INIT_PART(ref go_parts, ref m_locations, ref m_scales, PrimitiveType.Cube, (int)PARTS.RP_HEAP, Color.grey, "HEAP", new Vector3(0.5f, 0.35f, 1f), new Vector3(0f, 0f, 0f));
        //TORSO
        INIT_PART(ref go_parts, ref m_locations, ref m_scales, PrimitiveType.Cube, (int)PARTS.RP_TORSO, Color.white, "TORSO", new Vector3(0.5f, 0.48f, 0.75f), new Vector3(0f, 0.413f, 0f));
        //CHEST
        INIT_PART(ref go_parts, ref m_locations, ref m_scales, PrimitiveType.Cube, (int)PARTS.RP_CHEST, Color.red, "CHEST", new Vector3(0.7f, 0.67f, 1.16f), new Vector3(0f, 0.540f, 0f));
        //NECK
        INIT_PART(ref go_parts, ref m_locations, ref m_scales, PrimitiveType.Cube, (int)PARTS.RP_NECK, Color.white, "NECK", new Vector3(0.3f, 0.21f, 0.3f), new Vector3(0f, 0.450f, 0f));
        //HEAD
        INIT_PART(ref go_parts, ref m_locations, ref m_scales, PrimitiveType.Cube, (int)PARTS.RP_HEAD, Color.blue, "HEAD", new Vector3(0.45f, 0.45f, 0.45f), new Vector3(0f, 0.310f, 0f));

        v3_originals = go_parts[(int)PARTS.RP_HEAP].GetComponent<MeshFilter>().mesh.vertices;
        
        leftLeg.Init("RIGHT", ref go_parts, ref m_locations, ref m_scales);
        rightLeg.Init("LEFT", ref go_parts, ref m_locations, ref m_scales);
        
        leftArm.Init("RIGHT", ref go_parts, ref m_locations, ref m_scales);
        rightArm.Init("LEFT", ref go_parts, ref m_locations, ref m_scales);
    }

    // Update is called once per frame
    void Update()
    {

        rY.Update();
        rk.Update();
        jump.Update();
        rX.Update();

        Matrix4x4 accumT = Matrix4x4.identity;
        Matrix4x4 accumChest = Matrix4x4.identity;
        Matrix4x4 accumHeap = Matrix4x4.identity;

        for (int i = (int)PARTS.RP_HEAP; i <= (int)PARTS.RP_HEAD; i++)
        {

            Matrix4x4 m = accumT * m_locations[i] * m_scales[i];
            if (i == (int)PARTS.RP_HEAP)
            {
                Matrix4x4 t = Transformations.Translate(0f, jump.val, 0f);
                m = accumT * m_locations[i] * t * m_scales[i];
                accumT *= m_locations[i] * t;
                accumHeap = accumT;
                leftLeg.Draw(ref accumHeap, ref go_parts, m_locations, m_scales, rY, v3_originals,rk);
                rightLeg.Draw(ref accumHeap, ref go_parts, m_locations, m_scales, rY, v3_originals,rk);
            }
            else if (i == (int)PARTS.RP_CHEST)
            {
                Matrix4x4 r = Transformations.RotateY(rY.val);
                m = accumT * m_locations[i] * r * m_scales[i];
                accumT *= m_locations[i] * r;
                accumChest = accumT;
                leftArm.Draw(ref accumChest, ref go_parts, m_locations, m_scales, rY, v3_originals);
                rightArm.Draw(ref accumChest, ref go_parts, m_locations, m_scales, rY, v3_originals);
            }
            else if (i == (int)PARTS.RP_NECK)
            {
                Matrix4x4 r = Transformations.RotateY(-rY.val);
                m = accumT * m_locations[i] * r * m_scales[i];
                accumT *= m_locations[i] * r;
            }
            else 
            {
                accumT *= m_locations[i];
            }

            go_parts[i].GetComponent<MeshFilter>().mesh.vertices = Transformations.Transform(m, v3_originals);

        }

    }
}
