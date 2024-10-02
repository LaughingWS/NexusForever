﻿using NexusForever.Shared.GameTable;
using NexusForever.Shared.GameTable.Model;
using NexusForever.WorldServer.Game.CSI.Static;
using NexusForever.WorldServer.Game.Entity;
using NexusForever.WorldServer.Game.Spell;
using NexusForever.WorldServer.Network.Message.Model;
using NLog;
using System;
using System.Linq;

namespace NexusForever.WorldServer.Game.CSI
{
    /// <summary>
    /// Client Side Interactions (CSI) should be used when the client casts a spell that is not an ability on the LAS.
    /// </summary>
    /// <remarks>
    /// Examples of this are: Using an Item, Interacting with entities in the world, Using Quest-Specific Spells.
    /// Spells triggered in this way are handled slightly differently: 
    /// - ServerSpellStartClientInteraction (0x07FD) Packet sent to the caster.
    /// - ServerSpellStart (0x07FF) Packet sent to nearby players.
    /// </remarks>
    public class ClientSideInteraction
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        public uint ClientUniqueId { get; private set; } = 0;
        public WorldEntity ActivateUnit { get; private set; } = null;
        public CSIType CsiType { get; private set; } = CSIType.Interaction;
        public ClientSideInteractionEntry Entry { get; private set; }

        private Player Owner { get; set; } = null;

        public ClientSideInteraction(Player owner, WorldEntity activateUnit, uint clientUniqueId)
        {
            Owner = owner;
            ActivateUnit = activateUnit;
            ClientUniqueId = clientUniqueId;
        }

        public void TriggerReady()
        {
            // This should be used for things like responding after a timer, or ensuring an event happens during an interaction's cast time.
        }

        public void TriggerFail()
        {
            ActivateUnit?.OnActivateFail(Owner);
        }

        public void TriggerSuccess()
        {
            ActivateUnit?.OnActivateSuccess(Owner);
        }

        public void SetClientSideInteractionEntry(ClientSideInteractionEntry clientSideInteractionEntry)
        {
            if (clientSideInteractionEntry == null)
            {
                CsiType = CSIType.Interaction;
                return;
            }

            Entry = clientSideInteractionEntry;
            CsiType = (CSIType)Entry.InteractionType;
        }

        public void HandleSuccess(SpellParameters spellCast)
        {
            // Placeholder Method in case we need to do things prior to triggering

            TriggerSuccess();
        }
    }
}
