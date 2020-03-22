using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace TestRedisList
{
    class Program
    {
        static void Main(string[] args)
        {
            const string clickkey = "clickcount";
            int listlength = 0;
            RedisHelper.Initialization(new CSRedis.CSRedisClient("127.0.0.1:6379,defaultDatabase=1"));
            Stopwatch sp = new Stopwatch();
            #region 方案一
            /*
             1.获取list集合的长度
             2.根据长度获获取指定范围的值
             3.开启pipeline逐个弹出(删除数据)
             */
            Console.WriteLine("方案一[pipeline逐个弹出]");
            InsertRedis();
            listlength = GetListData(clickkey);
            sp.Start();
            Console.WriteLine("pipeline弹出数据开始");
            var pipe1 = RedisHelper.StartPipe();
            for (int i = 0; i < listlength; i++)
            {
                pipe1.LPop(clickkey);
            }
            pipe1.EndPipe();
            sp.Stop();
            Console.WriteLine("pipeline弹出数据结束,耗时:" + sp.ElapsedMilliseconds);
            #endregion
            Console.WriteLine("----------------------------------------------");
            #region 方案二
            /*
             1.获取list集合的长度
             2.根据长度获获取指定范围的值
             3.删除不在取出指定集合数据
             */
            Console.WriteLine("方案二[LTrim保留指定区间数据]");
            InsertRedis();
            listlength = GetListData(clickkey);
            sp.Restart();
            RedisHelper.LTrim(clickkey, listlength, -1);
            sp.Stop();
            Console.WriteLine("LTrim删除数据,耗时:" + sp.ElapsedMilliseconds);
            #endregion
            Console.ReadKey();
        }

        public static void InsertRedis()
        {
            Console.WriteLine("插入1000000数据开始");
            Stopwatch sp = new Stopwatch();
            sp.Start();
            var pipe = RedisHelper.StartPipe();
            for (int i = 0; i < 1000000; i++)
            {
                pipe.RPush("clickcount", i);
            }
            pipe.EndPipe();
            sp.Stop();
            Console.WriteLine("插入数据结束,耗时:" + sp.ElapsedMilliseconds);
        }

        public static int GetListData(string clickkey)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            var keylength = RedisHelper.LLen(clickkey);
            sp.Stop();
            Console.WriteLine("获取key长度:" + keylength + " 耗时:" + sp.ElapsedMilliseconds);
            sp.Restart();
            var range = RedisHelper.LRange(clickkey, 0, keylength - 1);// LRange<int> 拆箱耗时
            sp.Stop();
            Console.WriteLine("获取key数据:" + range.Length + " 耗时:" + sp.ElapsedMilliseconds);
            return range.Length;
        }
    }
}
