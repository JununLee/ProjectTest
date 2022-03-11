using System.Collections;
using System.Collections.Generic; 
using UnityEngine; 

public class sd : MonoBehaviour {

    public class Value
    {
        public int trueValue = -1;
        public List<int> vals;

        public Value()
        {
            vals = new List<int>();
            for (int i = 1; i < 10; i++)
            {
                vals.Add(i);
            }
        }

        public Value copy()
        {
            Value v = new Value();
            v.vals.Clear();
            v.trueValue = trueValue;
            for (int i = 0; i < vals.Count; i++)
            {
                v.vals.Add(vals[i]);
            }
            return v;
        }
    }

    List<Value> values;
    List<List<int>> groups;
	 
	void Start () {

        values = new List<Value>();
        for (int i = 0; i < 9*9; i++)
        {
            values.Add(new Value());
        }


        groups = new List<List<int>>();

        for (int i = 0; i < 9; i++)
        {
            List<int> temp = new List<int>(); 
            for (int j = 0; j < 9; j++)
            {
                temp.Add(i * 9 + j); 
            }
            groups.Add(temp); 
        }

        for (int i = 0; i < 9; i++)
        { 
            List<int> temp1 = new List<int>();
            for (int j = 0; j < 9; j++)
            { 
                temp1.Add(i + j * 9);
            } 
            groups.Add(temp1);
        }

        for (int i = 0; i < 9; i+=3)
        {
            for (int j = 0; j < 9; j+=3)
            {
                List<int> temp = new List<int>();
                for (int k = 0; k < 3; k++)
                {
                    for (int l = 0; l < 3; l++)
                    {
                        temp.Add(i + k + (j + l) * 9);
                    }
                }
                groups.Add(temp);
            }
        }

        //string input = "046903000003050060900002003005006000800000010010780200000000050081300007000800104";
        //string input = "000300000400012083000004060002056007053700000000400006800000090015900000000008001";
        //string input = "002000090600240030300000004013007006800050007900400310100000009060074008070000400";
        string input = "010000006000102340008043000090000105002006089000000000000004000500200003940380010";

        Debug.Log(input.Length);

        int empty = 0;
        for (int i = 0; i < input.Length; i++)
        {
            int a = int.Parse(input[i].ToString());
            if(a!=0)
            {
                values[i].trueValue = a; 
            }
            else
            {
                empty++;
            }
        }
        Debug.Log("EMPTY: " + empty);

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();
        int r = myAlgos(values);
        sw.Stop();
        Debug.Log("END: " + r + "  " + sw.ElapsedMilliseconds);

        debugSD(values);
    }

    void debugSD(List<Value> vals)
    {
        string ss = "----------------------------------------------\n";
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                ss += vals[i * 9 + j].trueValue + /*"(" + vals[i * 9 + j].vals.Count + ")" +*/ "   ";
            }
            ss += "\n";
        }
        Debug.Log(ss);
    }

    void Update () {
		
	}

    int myAlgos(List<Value> vals)
    {
        int r = myAlgo(vals);
        if (r == 0)
        {
            values = new List<Value>(vals);
            return 200;
        }
        if(r==100||r==200)
        {
            return r;
        }

        int n = 0;
        for (int i = 0; i < vals.Count; i++)
        {
            if(vals[i].trueValue==-1)
            {
                n = i;
                break;
            }
        }
        int num = vals[n].vals.Count;

        List<List<Value>> values_G = new List<List<Value>>();
        for (int i = 0; i < num; i++)
        {
            List<Value> temp = new List<Value>();
            for (int j = 0; j < vals.Count; j++)
            {
                temp.Add(vals[j].copy());
            }
            values_G.Add(temp);
        }

        for (int i = 0; i < num; i++)
        {
            List<Value> temp = values_G[i];
            temp[n].trueValue = temp[n].vals[i];
            temp[n].vals.Clear();
            int res = myAlgos(temp);
            if(res==200)
            {
                return 200;
            }
        }

        return r;
    }

    int myAlgo(List<Value> vals)
    {
        while (true)
        {
            for (int i = 0; i < groups.Count; i++)
            {
                List<int> group = groups[i];
                int[] flags = { -1, -1, -1, -1, -1, -1, -1, -1, -1, -1 };
                for (int j = 0; j < group.Count; j++)
                {
                    int v = vals[group[j]].trueValue;
                    if (v != -1)
                    {
                        flags[v] = 1;
                    }
                }
                for (int j = 0; j < group.Count; j++)
                {
                    Value vv = vals[group[j]];
                    if (vv.trueValue == -1)
                    {
                        for (int k = 0; k < vv.vals.Count; k++)
                        {
                            if (flags[vv.vals[k]] == 1)
                            {
                                vv.vals.RemoveAt(k);
                                k = k - 1;
                            }
                        }
                        if(vv.vals.Count==0)
                        {
                            return 100;
                        }
                    }
                }
            }
            bool b = true;
            for (int i = 0; i < vals.Count; i++)
            {
                if(vals[i].vals.Count==1)
                {
                    vals[i].trueValue = vals[i].vals[0];
                    vals[i].vals.Clear();
                    b = false;
                    break;
                }
            }
            if(b)
            {
                break;
            }
        }
        int sum = 0;
        for (int i = 0; i < vals.Count; i++)
        {
            if (vals[i].trueValue == -1)
            {
                sum++;
            }
        }
        return sum;
    } 
}
