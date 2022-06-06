using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using TMPro;
using System;

public class StrangerIsBadAtTexturing : MonoBehaviour
{

	public Mesh          targetMesh;

	[Header("Don't touch this idiot!")]
	public float         sensitivity = 60.0f;
	public Camera        camera;
	public Transform     cameraT;
    public Mesh          currentMesh;
    public LineRenderer  lineRenderer;
    public ColorPicker   colorPicker;
    public  TMP_InputField  hexInput;
    private MeshRenderer meshRenderer;
    private MeshFilter   meshFilter;
    private MeshCollider meshCollider;


    private Vector3[]    vertices;
    private int[]        triangles;
    private Color32[]    colors;

    private int[]        selectedVertex;

	private float        rotX = 0.0f;
	private float        rotY = 0.0f;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        currentMesh = TargetMeshToMesh( targetMesh );

        meshFilter.mesh = currentMesh;
        meshCollider.sharedMesh = currentMesh;

        colorPicker.onColorChanged += OnColorPickerChanged;
    }

    public void OnColorPickerChanged( Color color )
    {
    	hexInput.text = colorToHex( colorPicker.color );
    }

    public Mesh TargetMeshToMesh( Mesh inputMesh )
    { 
    	Mesh mesh = new Mesh();
    	mesh.vertices  = inputMesh.vertices;
    	mesh.triangles = inputMesh.triangles;
    	mesh.normals   = inputMesh.normals;

    	vertices  = inputMesh.vertices;
    	triangles = inputMesh.triangles;

    	colors = new Color32[ inputMesh.vertices.Length ];
    	for (int i = 0; i < inputMesh.vertices.Length; i++)
    		colors[ i ] = new Color32(255,255,255,255);

    	mesh.SetColors( colors );

    	return mesh;
    }

    // Update is called once per frame
    void Update()
    {
    	if (Input.GetMouseButton(2))
    	{
		    rotX -= Input.GetAxisRaw("Mouse Y") * sensitivity * 0.02f;
		    rotY += Input.GetAxisRaw("Mouse X") * sensitivity * 0.02f;
		    cameraT.rotation = Quaternion.Euler(rotX, rotY, 0);

		    Vector3 direction = cameraT.rotation * new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * 64 * Time.deltaTime;   
		    cameraT.position = direction + cameraT.position; 

		    return;   
	    }    


	    if (Input.GetMouseButtonDown(0))
	    {
	    	int[] hitIndices = GetRaycastTriangles();

	    	if ( hitIndices.Length > 0 )
	    	{
		    	Vector3 position1 = vertices[ hitIndices[ 0 ] ] * transform.localScale.x;
		    	Vector3 position2 = vertices[ hitIndices[ 1 ] ] * transform.localScale.x;
		    	Vector3 position3 = vertices[ hitIndices[ 2 ] ] * transform.localScale.x;

		    	lineRenderer.SetPosition( 0, position1 );
		    	lineRenderer.SetPosition( 1, position2 );
		    	lineRenderer.SetPosition( 2, position3 );
		    	lineRenderer.SetPosition( 3, position1 );

		    	colorPicker.color = colors[ hitIndices[ 0 ] ];	    
		    	hexInput.text = colorToHex( colorPicker.color );	
	    	}
	    }

	    if (Input.GetMouseButtonDown(1))
	    {
	    	int[] hitIndices = GetRaycastTriangles();

	    	if ( hitIndices.Length > 0 )
	    	{
	    		for (int i = 0; i < 3; i++)
		    		colors[ hitIndices[ i ] ] = colorPicker.color;	
		    	currentMesh.SetColors( colors );    		
	    	}
	    }
    }

    private int[] GetRaycastTriangles()
    {
	    RaycastHit hit;
	    Ray ray = camera.ScreenPointToRay(Input.mousePosition);
	    int[] tempvertex = new int[] {};
	    if (Physics.Raycast(ray, out hit)) 
	    {        
	        int index = hit.triangleIndex * 3;
	        tempvertex = new int[] { triangles[ index ], triangles[ index + 1 ], triangles[ index + 2 ] }; 
	    }   	

	    return tempvertex;
    }

    public void OnHexChanged( string val )
    {
    	colorPicker.color = hexToColor(val);
    }

    public static string colorToHex(Color32 color)
    {
        string hex = color.r.ToString("X2") + color.g.ToString("X2") + color.b.ToString("X2");
     
        return hex;
    }

    public void OnExport()
    {
    	AssetDatabase.CreateAsset( currentMesh, "Assets/output.asset" );
    	AssetDatabase.SaveAssets();
    }

    public void Bucket()
    {
    	for (int i = 0; i < colors.Length; i++)
    		colors[ i ] = colorPicker.color;
    	currentMesh.SetColors( colors );    	
    }

    public void ChangeScale( string val )
    {
        try
        {
            int numVal = Int32.Parse( val );

            transform.localScale = new Vector3( numVal, numVal, numVal );
            
        }
        catch (FormatException e)
        {

        }    	
    }

    public static Color hexToColor(string hex)
    {
        hex = hex.Replace ("0x", "");//in case the string is formatted 0xFFFFFF
        hex = hex.Replace ("#", "");//in case the string is formatted #FFFFFF
        byte a = 255;//assume fully visible unless specified in hex
        byte r = byte.Parse(hex.Substring(0,2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2,2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4,2), System.Globalization.NumberStyles.HexNumber);
        //Only use alpha if the string has enough characters
        if(hex.Length == 8){
            a = byte.Parse(hex.Substring(6,2), System.Globalization.NumberStyles.HexNumber);
        }
        return new Color32(r,g,b,a);
    }
}
