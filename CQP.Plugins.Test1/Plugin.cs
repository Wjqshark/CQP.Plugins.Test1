using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newbe.CQP.Framework;
using Newbe.CQP.Framework.Extensions;

namespace CQP.Plugins.Test1
{
    public class Plugin : PluginBase
    {
        private ICoolQApi m_API;

        /// <summary>
        /// 随机数生成
        /// </summary>
        private Random m_Rand = new Random();
        /// <summary>
        /// qq群里qq号字典
        /// </summary>
        private Dictionary<long, List<long>> m_QQdics = new Dictionary<long, List<long>>();

        public override string AppId => "CQP.Plugins.Test1";

        public Plugin(ICoolQApi coolQApi) : base(coolQApi)
        {
            m_API = coolQApi;
        }

        #region 处理各种消息 入口

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
            DealWithGroupMessageThread(subType, sendTime, fromGroup, fromQq, fromAnonymous, msg, font);
            return base.ProcessGroupMessage(subType, sendTime, fromGroup, fromQq, fromAnonymous, msg, font);
        }

        /// <summary>
        /// 处理群成员增加
        /// </summary>
        /// <param name="subType"></param>
        /// <param name="sendTime"></param>
        /// <param name="fromGroup"></param>
        /// <param name="fromQq"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public override int ProcessGroupMemberIncrease(int subType, int sendTime, long fromGroup, long fromQq, long target)
        {
            UpdateQQListThread(fromGroup);
            return base.ProcessGroupMemberIncrease(subType, sendTime, fromGroup, fromQq, target);
        }

        public override int ProcessMenuClickA()
        {
            //todo:do something 
            return base.ProcessMenuClickA();
        }

        public override int Enabled()
        {
            return base.Enabled();
        }

        #endregion

        /// <summary>
        /// 开启处理群消息进程
        /// </summary>
        /// <param name="subType"></param>
        /// <param name="sendTime"></param>
        /// <param name="fromGroup"></param>
        /// <param name="fromQq"></param>
        /// <param name="fromAnonymous"></param>
        /// <param name="msg"></param>
        /// <param name="font"></param>
        private void DealWithGroupMessageThread(int subType, int sendTime, long fromGroup, long fromQq, string fromAnonymous, string msg, int font)
        {
            object[] paramaters = new object[] { subType, sendTime, fromGroup, fromQq, fromAnonymous, msg, font };
            Thread mainThread = new Thread(new ParameterizedThreadStart(DealWithGroupMessage));
            mainThread.Start(paramaters);
        }
        /// <summary>
        /// 处理群消息方法
        /// </summary>
        /// <param name="obj"></param>
        private void DealWithGroupMessage(object obj)
        {
            try
            {
                object[] paramaters = (object[])obj;

                long fromGroup = (long)paramaters[2];
                long fromQq = (long)paramaters[3];
                string msg = (string)paramaters[5];

                if (!m_QQdics.ContainsKey(fromGroup))
                {
                    //获取群成员 需要api权限160
                    List<GroupMemberInfo> memebers = CoolQApi.GetGroupMemberList(fromGroup).Model.ToList();
                    m_QQdics.Add(fromGroup, memebers.Select(t => t.Number).ToList());
                }

                List<long> qqList = CheckHasAtQQ(m_QQdics[fromGroup], msg);

                if (qqList.Count > 0)
                {
                    //禁言部分
                    if (msg.Contains("禁言") || msg.Contains("闭嘴") || msg.Contains("别说话") || msg.Contains("让你说话了么") || msg.Contains("ShutUp"))
                    {
                        foreach (var targetqq in qqList)
                        {
                            //全体成员跳过
                            if (targetqq == -1)
                            {
                                continue;
                            }

                            //对该机器人
                            if (targetqq == CoolQApi.GetLoginQQ())
                            {
                                CoolQApi.SendGroupMsg(fromGroup, CoolQCode.At(fromQq) + " 小逼，想禁言我？找死是吧！3分钟不谢！");
                                CoolQApi.SetGroupBan(fromGroup, fromQq, 180);
                                break;
                            }

                            //成功概率1/3
                            bool success = m_Rand.Next(4200) % 3 == 1;
                            if (success)
                            {
                                CoolQApi.SendGroupMsg(fromGroup, CoolQCode.At(fromQq) + "对" + CoolQCode.At(targetqq) + "释放了禁言术！ 成功了！  " + CoolQCode.At(targetqq) + "被禁言3分钟！");
                                CoolQApi.SetGroupBan(fromGroup, targetqq, 180);
                            }
                            else
                            {
                                CoolQApi.SendGroupMsg(fromGroup, CoolQCode.At(fromQq) + "对" + CoolQCode.At(targetqq) + "释放了禁言术！ 但是什么都没有发生！");
                            }
                        }
                    }
                    //踢出群部分
                    if (msg.Contains("滚") || msg.Contains("KickOut") || msg.Contains("踢了") || msg.Contains("飞踢") || msg.Contains("ThaiKick"))
                    {
                        foreach (var targetqq in qqList)
                        {
                            //全体成员跳过
                            if (targetqq == -1)
                            {
                                continue;
                            }
                            //对该机器人
                            if (targetqq == CoolQApi.GetLoginQQ())
                            {
                                CoolQApi.SendGroupMsg(fromGroup, CoolQCode.At(fromQq) + " 小逼，想踢我？找死是吧！翻滚吧！");
                                CoolQApi.SetGroupKick(fromGroup, fromQq, false);
                                break;
                            }
                            //成功概率1/5
                            bool success = m_Rand.Next(4200) % 5 == 1;
                            if (success)
                            {
                                CoolQApi.SendGroupMsg(fromGroup, CoolQCode.At(fromQq) + "狠狠踢了" + CoolQCode.At(targetqq) + "一脚！ " + CoolQCode.At(targetqq) + "飞了好远，直到飞出了群");
                                CoolQApi.SetGroupKick(fromGroup, targetqq, false);
                            }
                            else
                            {
                                CoolQApi.SendGroupMsg(fromGroup, CoolQCode.At(fromQq) + "想要狠狠踢" + CoolQCode.At(targetqq) + "一脚，结果没有踢着，把脚崴了。好惨！");
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                CoolQApi.AddLog(1, CoolQLogLevel.Error, e.Message);
            }

        }

        /// <summary>
        /// 获取信息中@的qq列表
        /// </summary>
        /// <param name="allqqList">群里所有qq成员</param>
        /// <param name="msg">信息</param>
        /// <returns></returns>
        private List<long> CheckHasAtQQ(List<long> allqqList, string msg)
        {
            List<long> qqList = new List<long>();
            bool flag = false;

            Dictionary<long, string> QQAtDic = new Dictionary<long, string>();

            QQAtDic.Add(-1, CoolQCode.At(-1));

            foreach (var tempqq in allqqList)
            {
                QQAtDic.Add(tempqq, CoolQCode.At(tempqq));
            }

            foreach (var qqkey in QQAtDic.Keys)
            {
                if (msg.Contains(QQAtDic[qqkey]))
                {
                    qqList.Add(qqkey);
                }
            }
            return qqList;
        }

        /// <summary>
        /// 更新QQ列表进程
        /// </summary>
        /// <param name="groupqq"></param>
        private void UpdateQQListThread(long groupqq)
        {
            object[] paramaters = new object[] { groupqq };
            Thread mainThread = new Thread(new ParameterizedThreadStart(UpdateQQList));
            mainThread.Start(paramaters);
        }

        /// <summary>
        /// 更新群里qq列表
        /// </summary>
        /// <param name="groupQQ"></param>
        private void UpdateQQList(object obj)
        {
            object[] paramaters = (object[])obj;

            long groupQQ = (long)paramaters[0];

            //获取群成员 需要api权限160
            List<GroupMemberInfo> memebers = CoolQApi.GetGroupMemberList(groupQQ).Model.ToList();

            if (!m_QQdics.ContainsKey(groupQQ))
            {
                m_QQdics.Add(groupQQ, memebers.Select(t => t.Number).ToList());
            }
            else
            {
                m_QQdics[groupQQ] = memebers.Select(t => t.Number).ToList();
            }
        }
    }
}
