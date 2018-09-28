using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Branch : MonoBehaviour
{
    public int Level;//当前树枝层级
    internal int InterateNum;//总迭代层级

    internal Direction Direct;
    internal enum Direction
    {
        Left = -1,
        Right = 1
    }

    internal Transform parentHead;//父树枝顶部
    internal List<Branch> childrenBranchs = new List<Branch>();

    void Start()
    {
        setBranchColor();
    }

    /// <summary>
    /// 根据色彩风格为树枝定色
    /// </summary>
    private void setBranchColor()
    {
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        gameObject.GetComponent<Renderer>().GetPropertyBlock(propertyBlock);
        switch (Manager.mg.colorStyle)
        {
            case Manager.ColorStyle.Green:
                propertyBlock.SetColor("_Color", new Color(0, (InterateNum - Level) / InterateNum * 0.2f + 0.8f, Level * 0.5f / InterateNum));
                break;
            case Manager.ColorStyle.Red:
                propertyBlock.SetColor("_Color", new Color((InterateNum - Level + 2) / InterateNum + 2, Mathf.Sqrt(Level * 0.5f / InterateNum), Mathf.Sqrt(Level / InterateNum)));
                break;
            case Manager.ColorStyle.Deep:
                propertyBlock.SetColor("_Color", new Color((Level + 2) / (InterateNum + 5), (Level + 2) / (InterateNum + 5), (Level + 2) / (InterateNum + 5)));
                break;
            default:
                propertyBlock.SetColor("_Color", new Color(0, (InterateNum - Level) / InterateNum * 0.2f + 0.8f, Level * 0.5f / InterateNum));
                break;
        }
        gameObject.GetComponent<Renderer>().SetPropertyBlock(propertyBlock);
    }

    float updateTimeInteval = 0.5f;//刷新间隔
    float updateTimeTick = 0f;//Ticker
    void Update()
    {
        if (parentHead == null) Destroy(gameObject, 0.01f);
        if (updateTimeTick < updateTimeInteval)
        {
            updateTimeTick += Time.deltaTime;
            growUp();
            rotate();
        }
        else
        {
            updateTimeTick = 0f;
        }

        if (Manager.mg.colorChangeTrigger) setBranchColor();
    }

    float growUpSpeed = 0.05f;//成长速度
    internal float maxSize;//最大大小
    /// <summary>
    /// 成长
    /// </summary>
    void growUp()
    {
        if (transform.localScale.x < maxSize)
        {
            transform.localScale = new Vector3(transform.localScale.x + growUpSpeed, transform.localScale.y + growUpSpeed * 1.5f, transform.localScale.z + growUpSpeed);
        }
    }

    float countRotate = 0f;//总旋转量限制
    bool clockWise = true;//旋转方向
    /// <summary>
    /// 飘舞
    /// </summary>
    void rotate()
    {
        //飘动一定幅度后反向飘动
        if (Manager.mg.isRotating)
        {
            if ((clockWise && countRotate < 5 * Level) || (!clockWise && countRotate < 0))
            {
                clockWise = true;
                transform.Rotate(Manager.mg.transform.forward * Level * (int)Direct * Manager.mg.rotateSpeedSlider.value * 0.2f, Space.World);
                countRotate += Level * 0.1f;
            }
            else
            {
                clockWise = false;
                transform.Rotate(Manager.mg.transform.forward * Level * -(int)Direct * Manager.mg.rotateSpeedSlider.value * 0.2f, Space.World);
                countRotate -= Level * 0.1f;
            }
        }
        FollowParent();
    }

    /// <summary>
    /// 跟随父树枝
    /// </summary>
    void FollowParent()
    {
        if (parentHead != null)
        {
            transform.position = parentHead.TransformPoint(Vector3.up);//跟随父树枝的位置
        }
    }

    /// <summary>
    /// 根据鼠标拖动方向旋转树枝
    /// </summary>
    /// <param name="mousePosition_last"></param>
    /// <param name="mousePosition_current"></param>
    public void rotate(Vector3 mousePosition_last, Vector3 mousePosition_current)
    {
        Vector3 pivot = Camera.main.WorldToScreenPoint(transform.position);
        Vector3 v1 = pivot - mousePosition_last;
        Vector3 v2 = pivot - mousePosition_current;
        int sign = Vector3.Cross(v1, v2).z > 0 ? 1 : -1;
        float tmp = Mathf.Sqrt((v1.x * v1.x + v1.y * v1.y) * (v2.x * v2.x + v2.y * v2.y));
        if (tmp < 0.001f) return;
        float Theta = Mathf.Acos((v1.x * v2.x + v1.y * v2.y) / tmp) / Mathf.PI * 180;
        iterRotate(Manager.mg.transform.forward * sign * Theta, Space.World);
    }

    /// <summary>
    /// 递归
    /// </summary>
    /// <param name="theta"></param>
    /// <param name="space"></param>
    public void iterRotate(Vector3 theta, Space space)
    {
        transform.Rotate(theta, space);
        foreach (Branch childBranch in childrenBranchs)
        {
            if (childBranch == null) continue;
            childBranch.FollowParent();
            childBranch.iterRotate(theta, space);
        }
    }

}
