using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    internal static Manager mg; //静态化

    public GameObject Forest;
    public GameObject Tree_Prefab;
    public GameObject mainCamera;

    public GameObject[] BranchNumSliders; //对应同名UI
    Slider[] bnSliders; //对应同名UI
    public GameObject[] BigToSmallSliders; //对应同名UI
    Slider[] btsSliders; //对应同名UI

    public int treeIterNum = 7; //树分枝最大迭代数

    public enum TreeIterNumber
    {
        Five_Poor = 0,
        Six_Fast = 1,
        Seven_Medium = 2,
        Eight_Good = 3,
        Nine_High = 4,
        Ten_Slow = 5
    }

    public GameObject TreeIterNumberDropdown; //树分枝最大迭代数
    internal Dropdown treeIterNumberDropdown; //树分枝最大迭代数

    public void ChangeTreeIterNum()
    {
        treeIterNum = treeIterNumberDropdown.value + 5;
    }

    private void Awake()
    {
        if (mg == null)
        {
            mg = gameObject.GetComponent<Manager>();
        }
    }

    float ratio = 0.3f;

    void Start()
    {
        bnSliders = new Slider[BranchNumSliders.Length];
        btsSliders = new Slider[BigToSmallSliders.Length];
        for (int i = 0; i < BranchNumSliders.Length; i++)
        {
            bnSliders[i] = BranchNumSliders[i].GetComponent<Slider>();
        }

        for (int i = 0; i < BigToSmallSliders.Length; i++)
        {
            btsSliders[i] = BigToSmallSliders[i].GetComponent<Slider>();
        }

        rotateToggle = RotateToggle.GetComponent<Toggle>();
        rotateSpeedSlider = RotateSpeedSlider.GetComponent<Slider>();

        colorStyleDropdown = ColorStyleDropdown.GetComponent<Dropdown>();
        colorStyleDropdown.options.Clear();
        foreach (int cs_index in Enum.GetValues(typeof(ColorStyle)))
        {
            string strName = Enum.GetName(typeof(ColorStyle), cs_index);
            colorStyleDropdown.options.Add(new Dropdown.OptionData(strName));
        }

        colorStyleDropdown.value = (int) ColorStyle.Deep;

        treeNumberDropdown = TreeNumberDropdown.GetComponent<Dropdown>();
        treeNumberDropdown.options.Clear();
        foreach (int tn_index in Enum.GetValues(typeof(TreeNumber)))
        {
            string strName = Enum.GetName(typeof(TreeNumber), tn_index);
            treeNumberDropdown.options.Add(new Dropdown.OptionData(strName));
        }

        treeNumberDropdown.value = (int) TreeNumber.Three;

        treeIterNumberDropdown = TreeIterNumberDropdown.GetComponent<Dropdown>();
        treeIterNumberDropdown.options.Clear();
        foreach (int tin_index in Enum.GetValues(typeof(TreeIterNumber)))
        {
            string strName = Enum.GetName(typeof(TreeIterNumber), tin_index);
            treeIterNumberDropdown.options.Add(new Dropdown.OptionData(strName));
        }

        treeIterNumberDropdown.value = (int) TreeIterNumber.Eight_Good;

        createForest();
    }

    void Update()
    {
        branchEditor();
        viewChange();
    }

    #region 鼠标交互操作

    GameObject selectBranch;
    Vector3 mousePosition_last;
    float dragRotateTimeTicker = 0f;
    float dragRotateTimeInterval = 0.1f;
    float mouseClickTreshold = 0.2f;
    float mouseClickTimeTicker = 0f;

    /// <summary>
    ///  树枝编辑动作
    /// </summary>
    void branchEditor()
    {
        //鼠标左键点击，增加
        if (Input.GetMouseButtonUp(0))
        {
            if (mouseClickTimeTicker <= mouseClickTreshold)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    Debug.DrawLine(ray.origin, hitInfo.point);
                    selectBranch = hitInfo.collider.gameObject;
                    if (selectBranch.layer == 8)
                    {
                        branch(selectBranch); //增加新枝(随机出枝方向)
                    }
                }
                else
                {
                    selectBranch = null;
                }
            }
        }

        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            mouseClickTimeTicker = 0f;
        }

        mouseClickTimeTicker += Time.deltaTime;

        //鼠标右键点击，修剪
        if (Input.GetMouseButtonUp(1))
        {
            if (mouseClickTimeTicker <= mouseClickTreshold)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hitInfo;
                if (Physics.Raycast(ray, out hitInfo))
                {
                    Debug.DrawLine(ray.origin, hitInfo.point);
                    selectBranch = hitInfo.collider.gameObject;
                    if (selectBranch.layer == 8)
                    {
                        trim(selectBranch); //修剪
                    }
                }
                else
                {
                    selectBranch = null;
                }
            }
        }

        //鼠标拖动，调整树枝方向
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hitInfo;
            if (Physics.Raycast(ray, out hitInfo))
            {
                Debug.DrawLine(ray.origin, hitInfo.point);
                selectBranch = hitInfo.collider.gameObject;
                mousePosition_last = Input.mousePosition;
            }
            else
            {
                selectBranch = null;
            }
        }
        else if (Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            if (selectBranch)
            {
                dragRotateTimeTicker += Time.deltaTime;
                if (dragRotateTimeTicker >= dragRotateTimeInterval)
                {
                    Vector3 mousePosition_current = Input.mousePosition;
                    rotate(selectBranch, mousePosition_last, mousePosition_current);
                    mousePosition_last = mousePosition_current;
                    dragRotateTimeTicker = 0f;
                }
            }
        }
    }

    /// <summary>
    /// 画面缩放平移
    /// </summary>
    void viewChange()
    {
        mainCamera.transform.Translate(Vector3.forward * Input.GetAxis("Mouse ScrollWheel") * 3);
        if (Input.GetMouseButton(2))
        {
            mainCamera.transform.Translate(Vector3.left * Input.GetAxis("Mouse X") * 0.1f);
            mainCamera.transform.Translate(Vector3.up * -Input.GetAxis("Mouse Y") * 0.1f);
        }
    }

    #endregion

    #region 色彩风格

    public enum ColorStyle
    {
        Red,
        Green,
        Deep
    }

    public ColorStyle colorStyle;
    public GameObject ColorStyleDropdown; //色彩风格
    internal Dropdown colorStyleDropdown; //色彩风格
    internal bool colorChangeTrigger = false;

    public void ChangeColor()
    {
        colorChangeTrigger = true;
        colorStyle = (ColorStyle) colorStyleDropdown.value;
        switch (colorStyle)
        {
            case ColorStyle.Deep:
                mainCamera.GetComponent<Camera>().backgroundColor = new Color(0.6f, 0.6f, 0.6f);
                break;
            case ColorStyle.Red:
                mainCamera.GetComponent<Camera>().backgroundColor = new Color(0.125f, 0.125f, 0.125f);
                break;
            case ColorStyle.Green:
                mainCamera.GetComponent<Camera>().backgroundColor = new Color(0.125f, 0.125f, 0.125f);
                break;
            default:
                break;
        }

        StartCoroutine(resetColorChangeTrigger());
    }

    IEnumerator resetColorChangeTrigger()
    {
        yield return new WaitForEndOfFrame(); //帧末复原，所有树枝颜色更改完毕
        colorChangeTrigger = false;
    }

    #endregion

    #region 树飘动

    internal bool isRotating = false; //是否飘动

    public void RotateSwitch()
    {
        isRotating = rotateToggle.isOn;
    }

    public GameObject RotateSpeedSlider; //飘动幅度
    internal Slider rotateSpeedSlider; //飘动幅度
    public GameObject RotateToggle; //飘动开关
    Toggle rotateToggle; //飘动开关

    #endregion

    #region 森林增减

    public float TreeDistance = 5f; //树间距

    public enum TreeNumber
    {
        One = 0,
        Two = 1,
        Three = 2,
        Four = 3,
        Five = 4
    }

    int TreeNum = 1;
    public GameObject TreeNumberDropdown; //树数量
    internal Dropdown treeNumberDropdown; //树数量

    public void ChangeTreeNum()
    {
        TreeNum = treeNumberDropdown.value + 1;
    }

    List<BiangTree> biangTrees = new List<BiangTree>();

    /// <summary>
    /// 创造森林
    /// </summary>
    void createForest()
    {
        for (int i = 0; i < TreeNum; i++)
        {
            GameObject Tree = Instantiate(Tree_Prefab, Forest.transform);
            Tree.transform.position = new Vector3(TreeDistance * ratio * 4 * (i - TreeNum / 2), 0, 0);
            BiangTree bt = Tree.GetComponent<BiangTree>();
            biangTrees.Add(bt);
            bt.BranchNum_probabilites = new int[bnSliders.Length];
            bt.BranchDecay_probabilites = new int[btsSliders.Length + 1];
            bt.BranchDecay_probabilites[0] = 0;
            refreshProbabilities(bt);
            bt.Initialize();
        }
    }

    //更改出枝概率后对tree类进行刷新
    void refreshProbabilities(BiangTree bt)
    {
        for (int j = 0; j < bnSliders.Length; j++)
        {
            bt.BranchNum_probabilites[j] = (int) (bnSliders[j].value * 100) + 1;
        }

        for (int j = 0; j < btsSliders.Length - 1; j++)
        {
            bt.BranchDecay_probabilites[j + 1] = (int) (btsSliders[j].value * 100) + 1;
        }

        bt.InitializeProbabilities();
    }

    public void RefreshAllTreeProbabilities()
    {
        foreach (BiangTree bt in biangTrees)
        {
            refreshProbabilities(bt);
        }
    }

    /// <summary>
    /// 重建森林
    /// </summary>
    public void RecreateForest()
    {
        isRotating = false;
        rotateToggle.isOn = false;
        for (int i = 0; i < Forest.transform.childCount; i++)
        {
            Destroy(Forest.transform.GetChild(i).gameObject, 0.1f);
        }

        createForest();
    }

    #endregion

    #region 树枝修改

    /// <summary>
    /// 修剪树枝
    /// </summary>
    /// <param name="branch"></param>
    void trim(GameObject branch)
    {
        isRotating = false;
        rotateToggle.isOn = false;
        for (int i = 0; i < branch.transform.childCount; i++)
        {
            Destroy(branch.transform.GetChild(i).gameObject, 0.1f);
        }

        Destroy(branch, 0.1f);
    }

    /// <summary>
    /// 长出新枝
    /// </summary>
    /// <param name="branch"></param>
    void branch(GameObject branch)
    {
        isRotating = false;
        rotateToggle.isOn = false;

        BiangTree tree = branch.transform.parent.gameObject.GetComponent<BiangTree>();
        StartCoroutine(tree.generateChildBranch(branch, tree.interateScaleRatio, 1));
    }

    /// <summary>
    /// 旋转树枝
    /// </summary>
    /// <param name="branch"></param>
    void rotate(GameObject branch, Vector3 mousePosition_last, Vector3 mousePosition_current)
    {
        branch.GetComponent<Branch>().rotate(mousePosition_last, mousePosition_current);
    }

    #endregion
}