using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services.Neo;
using Neo.SmartContract.Framework.Services.System;
using System;
using System.Numerics;

namespace N_FundPool
{
    public class N_FundPool : SmartContract
    {
        // asset转账 cgas
        [Appcall("74f2dc36a68fdc4682034178eb2220729231db76")]
        static extern object cgasCall(string method, object[] arr);

        public static uint slope = 1000;
        public static uint alpha = 700;
        public static uint beta = 800;

        public static object Main(string operation, object[] args)
        {
            if (Runtime.Trigger == TriggerType.Verification)
            {
                return true;
            }
            else if (Runtime.Trigger == TriggerType.Application)
            {
                if (operation == "buy")
                {
                    return Buy((byte[])args[0], (BigInteger)args[1], (BigInteger)args[2], (BigInteger)args[3]);
                }
                if (operation == "sell")
                {
                    return Sell((byte[])args[0], (BigInteger)args[1], (BigInteger)args[2], (BigInteger)args[3]);
                }
                if (operation == "revenue")
                {
                    return Revenue((byte[])args[0], (BigInteger)args[1]);
                }
            }

            return true;
        }
        #region 非对外接口
        /// <summary>
        /// 开更
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static BigInteger Sqrt(BigInteger x)
        {
            BigInteger z = (x + 1) / 2;
            BigInteger y = x;
            while (z < y)
            {
                y = z;
                z = (x / z + z) / 2;
            }
            return y;
        }
        /// <summary>
        /// cgas 转账
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static bool CgasTransfer(byte[] from, byte[] to, BigInteger value)
        {
            object[] _param = new object[3];
            _param[0] = from;
            _param[1] = to;
            _param[2] = value;
            return (bool)cgasCall("transfer", _param);
        }
        /// <summary>
        /// 获取fnd的总量
        /// </summary>
        /// <returns></returns>
        public static BigInteger GetTotalFnd()
        {
            StorageMap assetMap = Storage.CurrentContext.CreateMap("fnd");
            BigInteger totalFnd = assetMap.Get("total").AsBigInteger();
            return totalFnd;
        }
        /// <summary>
        /// 增加fnd的总额
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static bool InitOrAddTotalFnd(BigInteger amount)
        {
            if (amount <= 0)
                throw new Exception("amount need more than zero");
            StorageMap assetMap = Storage.CurrentContext.CreateMap("fnd");
            BigInteger totalFnd = assetMap.Get("total").AsBigInteger();
            totalFnd += amount;
            assetMap.Put("total", amount);
            return true;
        }
        /// <summary>
        /// 减少fnd的总额
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static bool SubTotalFnd(BigInteger amount)
        {
            if (amount <= 0)
                throw new Exception("amount need more than zero");
            StorageMap assetMap = Storage.CurrentContext.CreateMap("fnd");
            BigInteger totalFnd = assetMap.Get("total").AsBigInteger();
            totalFnd -= amount;
            if (totalFnd <= 0)
                throw new Exception("totalFnd need more than zero");
            assetMap.Put("total", amount);
            return true;
        }
        /// <summary>
        /// 获取某个人拥有的fnd的数量
        /// </summary>
        /// <param name="who"></param>
        /// <returns></returns>
        public static BigInteger GetBalanceOfFnd(byte[] who)
        {
            StorageMap assetMap = Storage.CurrentContext.CreateMap("fnd");
            BigInteger balanceOfFnd = assetMap.Get(who).AsBigInteger();
            return balanceOfFnd;
        }
        /// <summary>
        /// 增加某人fnd的数量
        /// </summary>
        /// <param name="who"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static bool InitOrAddFnd(byte[] who, BigInteger amount)
        {
            if (amount <= 0)
                throw new Exception("amount need more than zero");
            if (who.Length != 20)
                throw new Exception("address wrong");
            StorageMap assetMap = Storage.CurrentContext.CreateMap("fnd");
            BigInteger balanceOfFnd = assetMap.Get(who).AsBigInteger();
            balanceOfFnd += amount;
            assetMap.Put(who, balanceOfFnd);
            return true;
        }
        /// <summary>
        /// 减少某人fnd的数量
        /// </summary>
        /// <param name="who"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static bool SubFnd(byte[] who, BigInteger amount)
        {
            if (amount <= 0)
                throw new Exception("amount need more than zero");
            if (who.Length != 20)
                throw new Exception("address wrong");
            StorageMap assetMap = Storage.CurrentContext.CreateMap("fnd");
            BigInteger balanceOfFnd = assetMap.Get(who).AsBigInteger();
            balanceOfFnd -= amount;
            if (balanceOfFnd < 0)
                throw new Exception("cant less than 0");
            assetMap.Put(who, balanceOfFnd);
            return true;
        }
        /// <summary>
        /// 获取所有的储备cgas的数量
        /// </summary>
        /// <returns></returns>
        public static BigInteger GetTotalReserveCgas()
        {
            StorageMap assetMap = Storage.CurrentContext.CreateMap("cgas");
            BigInteger totalReserveCgas = assetMap.Get("total").AsBigInteger();
            return totalReserveCgas;
        }
        /// <summary>
        /// 增加储备cgas的总额
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static bool InitOrAddTotalReserveCgas(BigInteger amount)
        {
            if (amount <= 0)
                throw new Exception("amount need more than zero");
            StorageMap assetMap = Storage.CurrentContext.CreateMap("cgas");
            BigInteger totalReserveCgas = assetMap.Get("total").AsBigInteger();
            totalReserveCgas += amount;
            assetMap.Put("total", totalReserveCgas);
            return true;
        }
        /// <summary>
        /// 减少储备cgas的数量
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static bool SubTotalReserveCgas(BigInteger amount)
        {
            if (amount <= 0)
                throw new Exception("amount need more than zero");
            StorageMap assetMap = Storage.CurrentContext.CreateMap("cgas");
            BigInteger totalReserveCgas = assetMap.Get("total").AsBigInteger();
            totalReserveCgas -= amount;
            if (totalReserveCgas < 0)
                throw new Exception("totalReserveCgas cant less than zero");
            assetMap.Put("total", totalReserveCgas);
            return true;
        }
        #endregion
        /// <summary>
        /// 投资cgas，获取一定数量的fnd币
        /// </summary>
        /// <param name="who"></param>
        /// <param name="cgasAmount"></param>
        /// <returns></returns>
        public static bool Buy(byte[] who, BigInteger cgasAmount, BigInteger minNFDAmount, BigInteger deadLine)
        {
            //验证
            if (!Runtime.CheckWitness(who))
                throw new Exception("Forbidden");
            //看看有没有超过最后期限
            BigInteger execBlock = Blockchain.GetHeight();
            if (execBlock > deadLine)
                throw new InvalidOperationException("Exceeded the deadline");
            if (cgasAmount <= 0)
                throw new Exception("amount need more than zero");
            //获取当前fnd的总数
            BigInteger totalFnd = GetTotalFnd();
            BigInteger fndBuyAmount = Sqrt(2 * cgasAmount * 1000 / slope + totalFnd * totalFnd) - totalFnd;
            if (fndBuyAmount < minNFDAmount)
                throw new Exception("fndBuyAmount less than minNFDAmount");
            /////////算钱
            //把fnd发给购买者并增加总量
            InitOrAddFnd(who, fndBuyAmount);
            InitOrAddTotalFnd(fndBuyAmount);
            ////把cgas分两份，一部分用作储备，一部分用来自治消费  总比例尽量保持3比7
            BigInteger curReserveCgas = GetTotalReserveCgas();

            //问自治合约还有多少余粮  --------------这里等vote合约写好要改
            BigInteger curVoteCgas = 0;

            //Y
            BigInteger toReserveCgas = (alpha/1000)*(curReserveCgas + cgasAmount) - (1000 - alpha) / 1000 * curVoteCgas;
            //X
            BigInteger toVoteCgas = (1000 - alpha) / 1000 * (curVoteCgas + cgasAmount) - (alpha / 1000) * curReserveCgas;

            //对x和y进行验证
            if (toReserveCgas > cgasAmount)
                toReserveCgas = cgasAmount;
            if (toVoteCgas > cgasAmount)
                toVoteCgas = cgasAmount;
            if (toReserveCgas < 0)
                toReserveCgas = 0;
            if (toVoteCgas < 0)
                toVoteCgas = 0;
            //把自治的钱给自治模块去处理
            {
                //再这里转  暂时空缺
            }

            //把储备金存在这个合约内方便退钱
            if (!CgasTransfer(who, ExecutionEngine.ExecutingScriptHash, toReserveCgas))
                throw new Exception("cgas transfer error");
            InitOrAddTotalReserveCgas(toReserveCgas);
            return true;
        }
        /// <summary>
        /// 撤资 用fnd换取cgas
        /// </summary>
        /// <param name="who"></param>
        /// <param name="tokenAmount"></param>
        /// <param name="minCgasAmount"></param>
        /// <param name="deadLine"></param>
        /// <returns></returns>
        public static bool Sell(byte[] who, BigInteger tokenAmount, BigInteger minCgasAmount, BigInteger deadLine)
        {
            //验证
            if (!Runtime.CheckWitness(who))
                throw new Exception("Forbidden");
            if (tokenAmount <= 0)
                throw new Exception("amount need more than zero");
            //看看有没有超过最后期限
            BigInteger execBlock = Blockchain.GetHeight();
            if (execBlock > deadLine)
                throw new InvalidOperationException("Exceeded the deadline");

            //获取储备金的量
            BigInteger totalReserveCgas = GetTotalReserveCgas();
            //获取fnd的总额
            BigInteger totalFnd = GetTotalFnd();
            //计算需要退的钱
            BigInteger withdraw = totalReserveCgas * tokenAmount / totalFnd / totalFnd * (2 * totalFnd - tokenAmount);
            if (withdraw < minCgasAmount)
                throw new Exception("withdraw less than minCgasAmount");
            //退钱
            if (!CgasTransfer(ExecutionEngine.ExecutingScriptHash, who, withdraw))
                throw new Exception("cgas transfer error");
            SubTotalReserveCgas(withdraw);
            SubTotalFnd(tokenAmount);
            SubFnd(who, tokenAmount);
            return true;
        }
        /// <summary>
        /// 盈利的钱用来回购fnd
        /// </summary>
        /// <param name="who"></param>
        /// <param name="cgasAmount"></param>
        /// <returns></returns>
        public static bool Revenue(byte[] who, BigInteger cgasAmount)
        {
            //验证
            if (!Runtime.CheckWitness(who))
                throw new Exception("Forbidden");
            if (cgasAmount <= 0)
                throw new Exception("amount need more than zero");
            //获取当前fnd的总数
            BigInteger totalFnd = GetTotalFnd();
            BigInteger fndBuyAmount = Sqrt(2 * cgasAmount * 1000 / slope + totalFnd * totalFnd) - totalFnd;
            /////////算钱
            //把fnd发给购买者并增加总量
            InitOrAddFnd(who, fndBuyAmount);
            InitOrAddTotalFnd(fndBuyAmount);
            ////把cgas分两份，一部分用作储备，一部分用来自治消费
            BigInteger reserveCgas = cgasAmount * beta;
            BigInteger voteCgas = (1000 - beta) / 1000 * cgasAmount;
            //把自治的钱给自治模块去处理
            {
                //再这里转  暂时空缺
            }
            //把储备金存在这个合约内方便退钱
            if (!CgasTransfer(who, ExecutionEngine.ExecutingScriptHash, reserveCgas))
                throw new Exception("cgas transfer error");
            InitOrAddTotalReserveCgas(reserveCgas);
            return true;
        }
    }
}
