using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Sinan.Data;
using Sinan.Entity;
using Sinan.Util;

namespace Sinan.GameModule
{
    public class PartAccess : VariantBuilder<PartBase>
    {
        readonly static PartAccess m_instance = new PartAccess();
        /// <summary>
        /// 唯一实例.
        /// </summary>
        public static PartAccess Instance
        {
            get { return m_instance; }
        }

        PartAccess()
            : base("Part")
        {

        }

        /// <summary>
        /// 保存实例
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public override bool Save(PartBase model)
        {
            return base.Save(model);
        }


        /// <summary>
        /// 星座相关计算公工
        /// </summary>
        /// <param name="info"></param>
        /// <param name="n">学习第几颗星</param>
        /// <param name="name">
        /// A点亮该星成功率,
        /// B每颗星提供的属性值,
        /// C星座点亮耗费的星力值,
        /// D星座点亮耗费石币</param>
        /// <returns></returns>
        public void GetStarValue(Variant info, int n, out double a, out int b, out int c, out int d)
        {
            double num = 0;
            double A = 0;
            int B = 0, C = 0, D = 0;
            foreach (var item in info) 
            {
                Variant v = item.Value as Variant;
                if (v == null) 
                    continue;

                switch (item.Key)
                {
                    case "A"://点亮该星成功率
                    case "B"://每颗星提供的属性值
                        num = v.GetDoubleOrDefault("B");
                        //for (int i = 1; i <= n; i++)
                        //{
                        //    num += num * Math.Pow(v.GetDoubleOrDefault("A"), n * (n - 1) / 2);
                        //}
                        num = num * Math.Pow(v.GetDoubleOrDefault("A"), n * (n - 1) / 2);
                        if (item.Key == "A")
                        {
                            A = (double)1 / num;
                        }
                        else
                        {
                            B = Convert.ToInt32(num);
                        }
                        break;
                    case "C"://星座点亮耗费的星力值
                        C = StarForm(v.GetDoubleOrDefault("A"), n, v.GetIntOrDefault("B"), v.GetDoubleOrDefault("C"));
                        break;
                    case "D"://星座点亮耗费石币                        
                        D = StarForm(v.GetDoubleOrDefault("A"), n, v.GetIntOrDefault("B"), v.GetDoubleOrDefault("C"));
                        break;
                }

            }
            a = A;b = B;c = C;d = D;       
        }

        /// <summary>
        /// 星座点亮耗费的星力值
        /// </summary>
        /// <param name="a"></param>
        /// <param name="n"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <returns></returns>
        public int StarForm(double a, int n, int b, double c)
        {
            //num * Math.Pow(v.GetDoubleOrDefault("A"), n * (n - 1) / 2);
            return Convert.ToInt32(a * Math.Pow(n, b) + c);
        }


        /// <summary>
        /// 得到星座道具提高成率的算法
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public double GetStarLv(int number)
        {
            return 0.03 * number;
        }

        /// <summary>
        /// 星阵取得星力值
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public int StarTroopsPower(List<Pet> petlist, DateTime startTime)
        {  
            DateTime dt = DateTime.UtcNow;
            int t = 0;
            int m = Second();
            foreach (Pet p in petlist)
            {
                TimeSpan ts = dt - startTime;
                if (ts.TotalSeconds < m)
                    return 0;
                Variant v = p.Value;
                int ccd = v.GetVariantOrDefault("ChengChangDu").GetIntOrDefault("V");
                ccd = ccd > 890 ? 890 : ccd;
                t += Convert.ToInt32((1 + Math.Round((double)ccd / 150)) * Math.Round((ts.TotalSeconds / 20), 0));
            }

            return t;
        }

          //for each(var petdata:Object in App.Instance.global["player"]["Star"]["PetsList"]){
          //      var chengzhangdu:int= petdata.ChengChangDu>890?890:petdata.ChengChangDu;
          //      secondPower = secondPower+1+Math.round(chengzhangdu/150);
          //  }
          //  return secondPower;
        /// <summary>
        /// 星阵与冥想时长
        /// </summary>
        /// <returns></returns>
        public int Second()
        {
            GameConfig gc = GameConfigAccess.Instance.FindOneById("ME_00001");
            if (gc == null)
                return 0;
            Variant m = gc.Value;
            if (m == null)
                return 0;
            return m.GetIntOrDefault("Second");
        }

        /// <summary>
        /// 得冥想基本配置
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public int MedConfig(string name) 
        {
            GameConfig gc = GameConfigAccess.Instance.FindOneById("ME_00001");
            if (gc == null)
                return 0;
            Variant m = gc.Value;
            if (m == null)
                return 0;
            return m.GetIntOrDefault(name);
        }

        /// <summary>
        /// 人物星力最大值
        /// </summary>
        /// <returns></returns>
        public int PowerMax()
        {
            GameConfig gc = GameConfigAccess.Instance.FindOneById("ME_00001");
            if (gc == null)
                return 0;
            Variant v = gc.Value;
            if (v == null)
                return 0;
            return v.GetIntOrDefault("Max");
        }

        /// <summary>
        /// 星阵最大值
        /// </summary>
        /// <returns></returns>
        public int TroopsMax()
        {
            GameConfig gc = GameConfigAccess.Instance.FindOneById("ME_00001");
            if (gc == null)
                return 0;
            Variant v = gc.Value;
            if (v == null)
                return 0;
            return v.GetIntOrDefault("TroopsMax");
        }

        /// <summary>
        /// 取得活动奖励
        /// </summary>
        /// <param name="goods"></param>
        /// <returns></returns>
        public Dictionary<string, Variant> GetPartAward(Variant goods) 
        {

            Dictionary<string, Variant> dic = new Dictionary<string, Variant>();
            if (goods != null)
            {
                foreach (var item in goods)
                {
                    Variant t = item.Value as Variant;
                    int n = t.GetIntOrDefault("N");
                    int h = t.GetIntOrDefault("H");
                    Variant v;
                    if (dic.TryGetValue(item.Key, out v))
                    {
                        v.SetOrInc("Number" + h, n);
                    }
                    else
                    {
                        GameConfig gc = GameConfigAccess.Instance.FindOneById(item.Key);
                        if (gc == null)
                            continue;

                        v = new Variant();
                        v.SetOrInc("Number" + h, n);
                        GoodsAccess.Instance.TimeLines(gc, v);
                        dic.Add(item.Key, v);
                    }
                }
            }
            return dic;
        }
    }
}
