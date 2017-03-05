/*
 * The MeshRenderer component and Graphics.DrawMesh API are supported.
 *
 * GPU instancing is available on the following platforms: 
 * Windows: DX11 and DX12 with SM 4.0 and above / OpenGL 4.1 and above
 * OS X and Linux: OpenGL 4.1 and above
 * Mobile: OpenGL ES 3.0 and above / Metal (Support Android in version 5.5)
 * PlayStation 4
 * Xbox One
 * 
*/

using UnityEngine;

/// <summary>
/// Draw multiple meshes via GPU instancing.
/// Note: Temporary the GPU instancing does not support SkinMesh.
/// Known issue: Occur error message if editor is using the Framedebug (Unity verion 5.4)
/// </summary>

[ExecuteInEditMode]
public class DrawInstanced : MonoBehaviour {

	public GameObject m_Prefab;

    [Tooltip ("The default value of this max instance count is 500. For OpenGL, the actual value is one quarter of the value you specify, so 125 by default.")]
    [Range(1, 1023)]
    public int InstanceCount = 500;

#if UNITY_5_6_OR_NEWER
    public bool EnableGPUInstancing = true;
#endif

    MaterialPropertyBlock props;

    Vector4[] vec4Array;
    Matrix4x4[] matrixArray;

    Mesh mesh;
    Material mat;
    int oldCount = 0;

    void OnEnable ()
    {
        if (m_Prefab == null)
            return;

        mesh = m_Prefab.GetComponent<MeshFilter>().sharedMesh;
        mat = m_Prefab.GetComponent<Renderer>().sharedMaterial;

        oldCount = InstanceCount;
        updateEnableInstancingState();

        randomlData();
    }

    /// Cache the random value.
    void randomlData()
    {
        props = new MaterialPropertyBlock();
        matrixArray = new Matrix4x4[InstanceCount];

        vec4Array = new Vector4[InstanceCount];

        /// The count may not the same as instanced prefab numnber in FrameDebug, 
        /// that due to some instanced game objects position are behind the camera,
        /// or they are outside of the camera Frustum Culling area.
        for (int i = 0; i < InstanceCount; i++)
        {
            /// random color
            float r = Random.Range(0, 1f);
            float g = Random.Range(0, 1f);
            float b = Random.Range(0, 1f);

            // MaterialPropertyBlock does not have "Set Color Array", so using vector instead.
            vec4Array[i] = new Vector4(r,g,b,1);

            /// random position
            float x = Random.Range(-20, 20);
            float y = Random.Range(-10, 10);
            float z = Random.Range(-10, 10);
            matrixArray[i] = Matrix4x4.identity;                    /// set default identity
            matrixArray[i].SetColumn(3, new Vector4(x, y, z, 1));	/// 4th colummn: set position

        }
#if UNITY_5_5_OR_NEWER
        props.SetVectorArray("_Color", vec4Array);
#endif

        oldCount = InstanceCount;
    }

    void Update()
    {
        if (mesh == null && mat == null)
            return;

#if UNITY_5_5_OR_NEWER
    #if UNITY_5_6_OR_NEWER
        if(EnableGPUInstancing)
    #endif

        /// Since version 5.5, You can specify the number of instances to draw, 
        /// or by default it is the length of the matrices array.
        /// Note: You can only draw a maximum of 1023 instances at once.
        /// This API requires Enabled GPU Instancing.
        Graphics.DrawMeshInstanced(mesh, 0, mat, matrixArray, matrixArray.Length, props);  /// 5.5 api
            //Graphics.DrawMeshInstancedIndirect (...);                                         /// 5.6 api

    #if UNITY_5_6_OR_NEWER
        else
            /// fallback for non-instance draw mesh.
            DrawMesh ();
    #endif
#else
        DrawMesh();
#endif

    }

    void DrawMesh ()
    {
        for (int i = 0; i < InstanceCount; i++)
        {
            /// Adding per-instance data
            props.SetColor("_Color", vec4Array[i]);
            Graphics.DrawMesh(mesh, matrixArray[i], mat, 0, null, 0, props);                    /// 5.4 api
        }
    }

    void updateEnableInstancingState ()
    {
        if (mat == null)
            return;

#if UNITY_5_6_OR_NEWER
        /// 5.6 api only
        mat.enableInstancing = EnableGPUInstancing? true : false;
#endif

    }

    void OnValidate()
    {
        /// check if the objects count state is dirty (Editor only)
        if (oldCount != InstanceCount)
            randomlData();

        updateEnableInstancingState();
    }

#if UNITY_EDITOR
    /// Is GPU draw call instancing supported? 
    void OnGUI (){
        if (!SystemInfo.supportsInstancing){
            GUI.color = Color.red;
            GUILayout.Label(" Your graphic card does not support GPU instancing." );
        }
        // Debug.Log(SystemInfo.supportsInstancing? true : false);

    }
#endif
}
