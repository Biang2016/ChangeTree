using System.Collections.Generic;
using UnityEngine;

public class ProbabilityUtil
{
    /// <summary>
    /// 带权重随机数生成
    /// </summary>

    [SerializeField]
    public int[] Probabilites;//各结果的可能性权重
    private int Probability_Sum = 0;//可能性之和
    private Dictionary<int, int[]> P_Range = new Dictionary<int, int[]>();//各结果的可能性范围

    public ProbabilityUtil(int[] probabilites)
    {
        Probabilites = probabilites;
        initializeProbability();
    }

    private void initializeProbability()
    {
        Probability_Sum = 0;
        for (int i = 0; i < Probabilites.Length; i++)
        {
            int[] range = new int[2];
            range[0] = Probability_Sum;
            Probability_Sum += Probabilites[i];
            range[1] = Probability_Sum;
            P_Range.Add(i, range);
        }
    }

    public int selectRandomNum()
    {
        UnityEngine.Random.seed += 10;
        int rad = UnityEngine.Random.Range(0, Probability_Sum);
        for (int i = 0; i < Probabilites.Length; i++)
        {
            if (rad >= P_Range[i][0] && rad < P_Range[i][1])
            {
                return i;
            }
        }
        return 0;
    }


}