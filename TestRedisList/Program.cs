﻿using System;
using System.Diagnostics;

namespace TestRedisList
{
    class Program
    {
        static void Main(string[] args)
        {
            const string clickkey = "clickcount";
            RedisHelper.Initialization(new CSRedis.CSRedisClient("192.168.1.158:6379,defaultDatabase=1"));

            //PipelineMethod(clickkey);

            LtrimMethod(clickkey);

            Console.ReadKey();
        }

        private static void LtrimMethod(string clickkey)
        {
            #region 方案二
            /*
             1.获取list集合的长度
             2.根据长度获获取指定范围的值
             3.删除不在取出指定集合数据
             */
            var sp = new Stopwatch();
            Console.WriteLine("方案二[LTrim保留指定区间数据]");
            InsertRedis();
            var listlength = GetListData(clickkey);
            sp.Restart();
            RedisHelper.LTrim(clickkey, listlength, -1);
            sp.Stop();
            Console.WriteLine("LTrim删除数据,耗时:" + sp.ElapsedMilliseconds);
            #endregion
        }

        private static void PipelineMethod(string clickkey)
        {
            #region 方案一
            /*
             1.获取list集合的长度
             2.根据长度获获取指定范围的值
             3.开启pipeline逐个弹出(删除数据)
             */
            var sp = new Stopwatch();
            Console.WriteLine("方案一[pipeline逐个弹出]");
            InsertRedis();
            var listlength = GetListData(clickkey);
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
        }

        /// <summary>
        /// 初始化1000000数据
        /// </summary>
        public static void InsertRedis()
        {
            Console.WriteLine("插入1000000数据开始");
            Stopwatch sp = new Stopwatch();
            sp.Start();
            var pipe = RedisHelper.StartPipe();
            for (int i = 1; i <= 1000000; i++)
            {
                pipe.RPush("clickcount", i);
            }
            pipe.EndPipe();
            sp.Stop();
            Console.WriteLine("插入数据结束,耗时:" + sp.ElapsedMilliseconds);
        }

        /// <summary>
        /// 获取指定范围数据
        /// </summary>
        /// <param name="clickkey"></param>
        /// <param name="hasvalue">在redis中保留几个数据</param>
        /// <returns></returns>
        public static int GetListData(string clickkey, int hasvalue = 0)
        {
            Stopwatch sp = new Stopwatch();
            sp.Start();
            //var keylength = RedisHelper.LLen(clickkey);
            //sp.Stop();
            //Console.WriteLine("获取key长度:" + keylength + " 耗时:" + sp.ElapsedMilliseconds);
            //sp.Restart();
            var range = RedisHelper.LRange(clickkey, 0, -1);// LRange<int> 拆箱耗时 stop:最后一个数据也会回去,需要-1
            sp.Stop();
            Console.WriteLine("获取key数据:" + range.Length + " 耗时:" + sp.ElapsedMilliseconds);
            return range.Length;
        }
    }
}
