using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MeshMaskUI : MaskableGraphic {

    private Vector2[] _positions;

    protected override void OnPopulateMesh(VertexHelper vertexHelper) {
        vertexHelper.Clear();
        
            Vector3 vec_00 = new Vector3(0, 0);
            Vector3 vec_01 = new Vector3(0, 50);
            Vector3 vec_10 = new Vector3(50, 0);
            Vector3 vec_11 = new Vector3(50, 50);   
            Vector3 vec_12 = new Vector3(75, 75);   

            vertexHelper.AddUIVertexQuad(new UIVertex[] {
            new UIVertex { position = vec_00, color  = Color.green },
            new UIVertex { position = vec_01, color  = Color.green },
            new UIVertex { position = vec_11, color  = Color.green },
             new UIVertex { position = vec_12, color  = Color.green },
            new UIVertex { position = vec_12, color  = Color.green }     
        });
             
    }

    public void SetPositions(Vector2[] positions)
    {
        _positions = positions;
    }

    private void Update() {
        SetVerticesDirty();
    }

}
