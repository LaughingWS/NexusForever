﻿namespace NexusForever.Network.World.Message.Static
{
    // TODO: research this more
    public enum HousingResult
    {
        Success                     = 0, // Not used, and complains if sent to the client
        Failed                      = 1,
        Neighbor_PlayerNotFound     = 3,
        Neighbor_PlayerNotOnline    = 4,
        Neighbor_PlayerNotAHomeowner = 5,
        Neighbor_PlayerDoesntExist  = 6,
        Neighbor_InvalidNeighbor    = 7,
        Neighbor_AlreadyNeighbors   = 8,
        Neighbor_NoPendingInvite    = 9,
        Neighbor_PlayerWrongFaction = 10,
        Neighbor_Full               = 11,
        Neighbor_RequestTimedOut    = 12,
        Neighbor_RequestDeclined    = 13,
        InvalidPermissions          = 14,
        Visit_InvalidWorld          = 15,
        Neighbor_RequestAccepted    = 16,
        Guild_TransactionFailed     = 17,
        Guild_NotMember             = 18,
        Neighbor_InvitePending      = 19,
        Decor_InvalidPosition       = 20,
        Decor_CannotAfford          = 21,
        Decor_ExceedsDecorLimit     = 22,
        Visit_Failed                = 23,
        Decor_CouldNotValidate      = 24,
        Decor_PrereqNotMet          = 25,
        Visit_Private               = 26,
        Visit_Ignored               = 27,
        Decor_CannotCreateDecor     = 28,
        Decor_CannotModifyDecor     = 29,
        Decor_CannotDeleteDecor     = 30,
        Decor_InvalidDecor          = 31,
        Plug_PrereqNotMet           = 32,
        Plug_InvalidPlug            = 33,
        Plug_CannotAfford           = 34,
        Plug_ModifyFailed           = 35,
        Plug_MustBeUnique           = 36,
        Plug_NotActive              = 37,
        InvalidResidence            = 38,
        InvalidNeighborhood         = 39,
        Plug_CannotRotate           = 41,
        Decor_MustBeUnique          = 42,
        Neighbor_PlayerIsIgnored    = 43,
        Neighbor_IgnoredByPlayer    = 44,
        Decor_CannotOwnMore         = 45,
        Neighbor_MissingEntitlement = 46,
        InvalidResidenceName        = 47,
        MustHaveResidenceName       = 48,
        Neighbor_PrivilegeRestricted = 49,
        Decor_CannotDonate          = 54,
        Decor_ExceedsCommunityDecorLimit = 61,
        CommunityNotFound           = 62,
        InsufficientFunds           = 65
    }
}