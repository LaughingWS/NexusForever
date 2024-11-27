using NexusForever.Shared.Network.Message;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;
using System;
using System.Linq;

namespace NexusForever.WorldServer.Network.Message.Handler
{
    public static class MtxHandler
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        private static readonly bool[] itemsClaimed = new bool[3] { false, false, false };

        [MessageHandler(GameMessageOpcode.ClientGachaOpen)]
        public static void HandleClientFortuneOpen(WorldSession session, ClientGachaOpen fortuneStartRequest)
        {
            // Client expects a GachaRollResult sent on open, because they may have an existing Gacha Roll that hasn't been fully claimed.
            session.EnqueueMessageEncrypted(new ServerGachaRollResult());
            session.EnqueueMessageEncrypted(new ServerGachaInit
            {
                Items = new System.Collections.Generic.List<uint>
                {
                    81269,
                    84623,
                    84625,
                    84624,
                    84626,
                    90924,
                    90997,
                    90996,
                    90950,
                    90833,
                    90840,
                    90948,
                    90794,
                    84621,
                    90949,
                    90951,
                    90834,
                    90801,
                    90839,
                    90838,
                    90836,
                    90835,
                    90837
                }
            });
        }

        [MessageHandler(GameMessageOpcode.ClientGachaRollRequest)]
        public static void HandleClientFortuneOpen(WorldSession session, ClientGachaRollRequest rollRequest)
        {
            // Error Handling seems to be done upfront by client. i.e. No spending without coins available.
            // No error handler seems to exist in LUA to deal with server error responses.

            // Fortune Charges are consumed when they reach 15 (you fill the carrot).
            // 1 Fortune Charge is granted each roll
            Random rnd = new();
            uint[] Card1 = [160, 162, 582, 583, 585, 2432, 2433, 2436, 2437, 2438, 2439, 2440, 2441, 2442, 2443, 2453, 2360, 2466, 2467, 2468, 2469, 2475, 2476];
            uint[] Card2 = [160, 162, 582, 583, 585, 2432, 2433, 2436, 2437, 2438, 2439, 2440, 2441, 2442, 2443, 2453, 2360, 2466, 2467, 2468, 2469, 2475, 2476];
            uint[] Card3 = [160, 162, 582, 583, 585, 2432, 2433, 2436, 2437, 2438, 2439, 2440, 2441, 2442, 2443, 2453, 2360, 2466, 2467, 2468, 2469, 2475, 2476];

            int Index1 = rnd.Next(Card1.Length);
            int Index2 = rnd.Next(Card2.Length);
            int Index3 = rnd.Next(Card3.Length);

            ulong currentFortuneCharge = session.AccountCurrencyManager.GetAmount(Game.Account.Static.AccountCurrencyType.FortuneCharge);
            if (currentFortuneCharge < 14)
                session.AccountCurrencyManager.CurrencyAddAmount(Game.Account.Static.AccountCurrencyType.FortuneCharge, 1);
            else
                session.AccountCurrencyManager.CurrencySubtractAmount(Game.Account.Static.AccountCurrencyType.FortuneCharge, currentFortuneCharge);

            session.EnqueueMessageEncrypted(new ServerGachaRollResult
            {
                Unknown0 = 3,
                WinType = (byte)(currentFortuneCharge < 14 ? 1 : 2),
                AccountItemsWon = new uint[3]
                {
                    Card1[Index1],
                    Card2[Index2],
                    Card3[Index3]
                }
            });
        }

        [MessageHandler(GameMessageOpcode.ClientGachaClaimItem)]
        public static void HandleClientFortuneClaimItem(WorldSession session, ClientFortuneClaimItem claimItem)
        {
            bool complete = false;

            itemsClaimed[claimItem.Index] = true;

            if (itemsClaimed.Count(x => x == false) == 0)
                complete = true;

            if (complete)
            {
                for (int i = 0; i < itemsClaimed.Length; i++)
                    itemsClaimed[i] = false;
            }

            session.EnqueueMessageEncrypted(new ServerGachaGrantItem
            {
                Unknown1 = (byte)(complete ? 0 : 3),
                ItemsClaimed = itemsClaimed
            });
        }
    }
}