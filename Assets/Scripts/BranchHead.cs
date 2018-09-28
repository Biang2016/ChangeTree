using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BranchHead : MonoBehaviour {

	void Start () {
        //初始化该GameObject，使其位于树枝顶部，为子树枝提供位置参照
        transform.localPosition = new Vector3(0, (float)(transform.parent.localScale.y*-0.5f)  ,0);
	}
	
	void Update () {

    }
}
