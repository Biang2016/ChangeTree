using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BiangTree : MonoBehaviour
{
    [Rename("树枝Prefab")]
    public GameObject branchPrefab;
    [Rename("树枝迭代尺寸比例")]
    public float interateScaleRatio = 0.8f;//树枝迭代尺寸比例

    //树分支选型
    [Rename("分支转角最小值")]
    public int theta_min = -60;//分支转角最小值
    [Rename("分支转角最大值")]
    public int theta_max = 60;//分支转角最大值

    private ProbabilityUtil branchNum_Probabilites;//各分支数的可能性权重
    [Rename("频数")]
    public int[] BranchNum_probabilites;//参数

    //大树干上越级长出小枝的概率
    private ProbabilityUtil bigBranchSmallChild_Probabilites;//各越级数的可能性权重
    [Rename("频数")]
    public int[] BranchDecay_probabilites;//参数

    //动画
    [Rename("子分支出现延迟时间")]
    public float branchDelay = 0.1f;//分叉延迟
    [Rename("树最大尺寸")]
    public float maxSize = 1.4f;//树最大尺寸
    [Rename("树最小尺寸")]
    public float minSize = 0.6f;//树最大尺寸
    [Rename("树枝最小尺寸比例")]
    public float minSizeRatio = 0.2f;

    internal int BranchCount = 0;//树枝总数统计

    public void Initialize()
    {
        GameObject MainBranch = generateMainBranch();
        StartCoroutine(generateChildBranch(MainBranch, interateScaleRatio, branchNum_Probabilites.selectRandomNum()));
    }

    public void InitializeProbabilities()
    {
        bigBranchSmallChild_Probabilites = new ProbabilityUtil(BranchDecay_probabilites);
        branchNum_Probabilites = new ProbabilityUtil(BranchNum_probabilites);
    }

    /// <summary>
    /// 创建主干
    /// </summary>
    /// <returns></returns>
    private GameObject generateMainBranch()
    {
        GameObject MainBranch = Instantiate(branchPrefab, transform);
        BranchCount++;
        Branch mb = MainBranch.GetComponent<Branch>();
        mb.parentHead = transform;
        mb.Level = 0;
        mb.maxSize = UnityEngine.Random.Range(minSize, maxSize);
        mb.InterateNum = Manager.mg.treeIterNum;
        MainBranch.transform.position = new Vector3(0, 0, 0);
        MainBranch.transform.localScale = new Vector3(0, 0, 0);
        return MainBranch;
    }

    /// <summary>
    /// 创建子枝
    /// </summary>
    /// <param name="parentBranch">上级树枝</param>
    /// <param name="scaleFactor">比例因子</param>
    /// <returns></returns>
    public IEnumerator generateChildBranch(GameObject parentBranch, float scaleFactor, int branchNum)
    {
        Branch pb = parentBranch.GetComponent<Branch>();
        float pMaxSize = pb.maxSize;
        Transform branchHead = parentBranch.transform.Find("BranchHead");
        yield return new WaitForSeconds(branchDelay);
        for (int i = 0; i < branchNum; i++)
        {
            int levelUp = bigBranchSmallChild_Probabilites.selectRandomNum();
            if (pb.Level + levelUp > Manager.mg.treeIterNum) levelUp = Manager.mg.treeIterNum - pb.Level;
            int level = pb.Level + levelUp;
            GameObject childBranch = Instantiate(branchPrefab, transform);
            BranchCount++;
            childBranch.transform.rotation = branchHead.transform.rotation;
            childBranch.transform.position = branchHead.transform.TransformPoint(Vector3.up);
            childBranch.transform.localScale = new Vector3(0, 0, 0);

            Branch cb = childBranch.GetComponent<Branch>();
            pb.childrenBranchs.Add(cb);
            cb.parentHead = branchHead;
            cb.Level = level;
            cb.InterateNum = Manager.mg.treeIterNum;
            cb.maxSize = scaleFactor * pMaxSize;
            for (int j = 1; j < levelUp; j++)
            {
                cb.maxSize *= scaleFactor;
            }
            if (branchNum % 2 == 1)
                childBranch.transform.Rotate(branchHead.forward * selectRandomBranchTheta(), Space.World);
            else
            {
                int tmp = i - (branchNum / 2 - 1);
                if (tmp <= 0)
                {
                    tmp--;
                    cb.Direct = Branch.Direction.Left;
                }
                else
                {
                    cb.Direct = Branch.Direction.Right;
                }
                childBranch.transform.Rotate(transform.forward * selectRandomBranchTheta(), Space.World);
            }
            if (level < Manager.mg.treeIterNum)
            {
                StartCoroutine(generateChildBranch(childBranch, scaleFactor, branchNum_Probabilites.selectRandomNum()));
            }
        }
        yield return null;
    }

    /// <summary>
    /// 选择随机的出枝角度
    /// </summary>
    /// <returns></returns>
    private int selectRandomBranchTheta()
    {
        UnityEngine.Random.seed += 10;
        int theta = UnityEngine.Random.Range(theta_min, theta_max);
        return theta;
    }
}
