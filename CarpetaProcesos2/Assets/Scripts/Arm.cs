// Diogenes Grajales Corona A01653251
// Rodolfo León Gasca A01653185

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arm : MonoBehaviour
{
    public List<MeshFilter> Cubos = new List<MeshFilter>();
    public float DX = 0.1f;
    public int CubosNum = 3;
    Vector3[] originales;
    Matrix4x4 mOriginal;
    Matrix4x4 matriz;

    float rotZ;

    bool goingUP = true;

    void Start()
    {
        mOriginal = Transformations.Translate(0.5f, 0, 0);

        for(int i = 0; i < CubosNum; i++){

            GameObject Cubo = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Cubos.Add(Cubo.GetComponent<MeshFilter>());
        }
        
        originales = Cubos[0].mesh.vertices;
    }

    void Update()
    {
        if(rotZ > 45.0f || rotZ < -45.0f){
            goingUP = !goingUP;
        }

        rotZ += goingUP? DX : -DX;
        matriz = Transformations.RotateZ(rotZ)*mOriginal;

        for(int i = 0; i < Cubos.Count; i++){
            Cubos[i].mesh.vertices = Transformations.Transform(multiplier(i) * Transformations.Scale(1,0.5f,0.5f),originales);
        }
    }

    Matrix4x4 multiplier(int index){
        Matrix4x4 result = matriz;
        if(index > 0){
        for(int i = 0; i < index; i++)
            result *= mOriginal * matriz;
        }

        return result;
    }
}

