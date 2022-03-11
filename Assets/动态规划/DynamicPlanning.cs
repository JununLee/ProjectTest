using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DynamicPlanning : MonoBehaviour {
    //环保局某科室需要对四种水样进行检测，四种水样依次有5、3、2、4份。
    //检测设备完成四种水样每一份的检测时间依次为8分钟、4分钟、6分钟、7分钟。
    //已知该科室本日最多可使用检测设备38分钟，如今天之内要完成尽可能多数量样本的检测，问有多少种不同的检测组合方式
    /*
       有一书店引进了一套书，共有3卷，每卷书定价是60元，书店为了搞促销，推出一个活动，活动如下：
      
       如果同时购买两卷不同的，那么可以打7.5折。
       如果同时购买三卷不同的，那么可以打7折。
      
       如果小明希望购买第1卷x本，第2卷y本，第3卷z本，那么至少需要多少钱呢？（x、y、z为三个已知整数）。
    */

    private const int x = 1;
    private const int y = 2;
    private const int z = 3;
    private float[,,] v = new float[x + 1, y + 1, z + 1];
    private void Start() 
    {
        for (int i = 0; i <= x; i++)
        {
            for (int j = 0; j <= y; j++)
            {
                for (int k = 0; k <= z; k++)
                {
                    v[i, j, k] = -1;
                }
            }
        }

        List<int> nums = new List<int>();
        nums.Add(1);
        nums.Add(2);
        nums.Add(3);
        nums.Add(4);
        nums.Add(5);

        List<int> indexs = new List<int>();

        selectGroup(nums, indexs, 4);

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            sw.Start();
            float min = MinMoney(x, y, z);
            sw.Stop();
            Debug.Log(min + "  " + sw.ElapsedMilliseconds);
        }
    }
    private float MinMoney(int i,int j,int k)
    {
        float value = 0;
        float mix = float.MaxValue;
        if (v[i, j, k] != -1)
        {
            return v[i, j, k];
        }
        if (i > 0)
        {
            value = 60 * 1f + MinMoney(i - 1, j, k);
            mix = mix < value ? mix : value;
        }
        if (j > 0)
        {
            value = 60 * 1f + MinMoney(i, j - 1, k);
            mix = mix < value ? mix : value;

        }
        if (k > 0)
        {
            value = 60 * 1f + MinMoney(i, j, k - 1);
            mix = mix < value ? mix : value;
        }
        if (i > 0 && j > 0)
        {
            value = 120 * 0.75f + MinMoney(i - 1, j - 1, k);
            mix = mix < value ? mix : value;
        }
        if (i > 0 && k > 0)
        {
            value = 120 * 0.75f + MinMoney(i - 1, j, k - 1);
            mix = mix < value ? mix : value;
        }
        if (j > 0 && k > 0)
        {
            value = 120 * 0.75f + MinMoney(i, j - 1, k - 1);
            mix = mix < value ? mix : value;
        }
        if (i > 0 && j > 0 && k > 0)
        {
            value = 180 * 0.7f + MinMoney(i - 1, j - 1, k - 1);
            mix = mix < value ? mix : value;
        }
        if ((i + j + k) > 0)
        {
            v[i, j, k] = mix;
            return mix;
        }
        else
        {
            v[i, j, k] = 0;
            return 0;
        }
    }

    private void selectGroup(List<int> nums,List<int> indexs,int n)
    {

        for (int i = 0; i < nums.Count; i++)
        {
            if (indexs.Contains(i)) continue;

            List<int> cindex = new List<int>();
            for (int j = 0; j < indexs.Count; j++)
            {
                cindex.Add(indexs[j]);
            } 
            cindex.Add(i);
            if(n>1)
            {
                selectGroup(nums, cindex, n - 1);
            }
            else
            {
                string str = "";
                for (int j = 0; j < cindex.Count; j++)
                {
                    str += cindex[j] + " ";
                }
                Debug.Log(str);
            }


        }
    }
}
