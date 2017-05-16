using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newbe.CQP.Framework;
using Newbe.CQP.Framework.Extensions;

namespace CQP.Plugins.Test1
{
    public class Plugin : PluginBase
    {
        public override string AppId => "CQP.Plugins.Test1";

        public Plugin(ICoolQApi coolQApi) : base(coolQApi)
        {
        }
        /// <summary>
        /// 处理群里信息
        /// </summary>
        /// <param name="subType"></param>
        /// <param name="sendTime"></param>
        /// <param name="fromGroup"></param>
        /// <param name="fromQq"></param>
        /// <param name="fromAnonymous"></param>
        /// <param name="msg"></param>
        /// <param name="font"></param>
        /// <returns></returns>
        public override int ProcessGroupMessage(int subType, int sendTime, long fromGroup, long fromQq, string fromAnonymous, string msg, int font)
        {
            long targetqq = 0;

            if (CheckHasAtQQ(msg, out targetqq))
            {
                if (msg.Contains("禁言"))
                {
                    Random rand = new Random();


                    bool success = rand.Next(4200) % 3 == 1;
                    if (success)
                    {
                        CoolQApi.SendGroupMsg(fromGroup, CoolQCode.At(fromQq) + "对" + CoolQCode.At(targetqq) + "释放了禁言术！ 成功了！  "+ CoolQCode.At(targetqq) +"被禁言3分钟！");
                        CoolQApi.SetGroupBan(fromGroup, targetqq, 180);
                    }
                    else
                    {
                        CoolQApi.SendGroupMsg(fromGroup, CoolQCode.At(fromQq) + "对" + CoolQCode.At(targetqq) + "释放了禁言术！ 但是什么都没有发生！");
                    }

                }
            }

            return base.ProcessGroupMessage(subType, sendTime, fromGroup, fromQq, fromAnonymous, msg, font);
        }
        /// <summary>
        /// 处理私聊信息
        /// </summary>
        /// <param name="subType"></param>
        /// <param name="sendTime"></param>
        /// <param name="fromQq"></param>
        /// <param name="msg"></param>
        /// <param name="font"></param>
        /// <returns></returns>
        public override int ProcessPrivateMessage(int subType, int sendTime, long fromQq, string msg, int font)
        {
            if (msg.Trim() == "test")
            {
                CoolQApi.SendPrivateMsg(fromQq, "无敌乔美大机霸！无敌乔美大机霸！无敌乔美大机霸！");
            }
            return base.ProcessPrivateMessage(subType, sendTime, fromQq, msg, font);
        }


        private bool CheckHasAtQQ(string msg, out long qq)
        {
            qq = 0;
            bool flag = false;

            if (msg.Contains(CoolQCode.At(-1)))
            {
                flag = true;
                qq = -1;
                return flag;
            }

            List<string> numberStrList = new List<string>();
            StringBuilder tempstr = new StringBuilder();
            foreach (char c in msg)
            {
                if (Convert.ToInt32(c) >= 48 && Convert.ToInt32(c) <= 57)
                {
                    tempstr.Append(c);
                }
                else
                {
                    string temp = tempstr.ToString();
                    if (!string.IsNullOrEmpty(temp))
                    {
                        numberStrList.Add(temp);
                    }
                }
            }
            if (numberStrList.Count == 0)
            {
                return flag;
            }
            List<long> numbers = new List<long>();
            numberStrList.ForEach(t => numbers.Add(long.Parse(t)));

            foreach (long q in numbers)
            {
                if (msg.Contains(CoolQCode.At(q)))
                {
                    flag = true;
                    qq = q;
                    return flag;
                }
            }

            return flag;
        }
    }
}
