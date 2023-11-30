using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class acube : MonoBehaviour
{
    public new GameObject gameObject;
    public Material textMat;
    public TMP_FontAsset textFont;

    // Start is called before the first frame update
    void Start()
    {
        GameObject gameObject = new GameObject(name);

        gameObject.AddComponent<MeshFilter>();
        gameObject.AddComponent<BoxCollider>();
        gameObject.AddComponent<MeshRenderer>();

        Vector3 scale = new Vector3(0.1f, 0.1f, 0.1f);
        gameObject.transform.localScale = scale;

        gameObject.AddComponent<BoundsControl>();
        gameObject.AddComponent<RotationAxisConstraint>();
        gameObject.AddComponent<ObjectManipulator>();
        gameObject.AddComponent<NearInteractionGrabbable>();
        gameObject.AddComponent<MinMaxScaleConstraint>();
        gameObject.AddComponent<CursorContextObjectManipulator>();

        //Font font = new Font();
        //font.material = textMat;
        //TMP_FontAsset textFont = new TMP_FontAsset();
        //textFont = TMP_FontAsset.CreateFontAsset(font);

        TextMeshPro obj = gameObject.AddComponent<TextMeshPro>();
        obj.autoSizeTextContainer = true;
        obj.font = textFont;
        obj.renderer.material = textMat;
        obj.fontMaterial = textMat;
        obj.fontSize = 1;
        obj.text = "HI CUBE";

        

        //gameObject.AddComponent<TextMeshPro>();
        //gameObject.GetComponent<TextMeshPro>().fontSize = 2;
        
        //Vector3 position = gameObject.transform.position;
        //gameObject.GetComponent<TextMeshPro>().fontSharedMaterial = TestMeshPro;

        //gameObject.GetComponent<TextMeshPro>().transform.position = position;
        //gameObject.GetComponent<TextMeshPro>().text = "HI CUBE";
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
