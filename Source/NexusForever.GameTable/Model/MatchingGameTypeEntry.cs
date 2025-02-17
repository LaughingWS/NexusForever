using NexusForever.Game.Static.Matching;

namespace NexusForever.GameTable.Model
{
    public class MatchingGameTypeEntry
    {
        public uint Id;
        public uint LocalizedTextIdName;
        public uint LocalizedTextIdDescription;
        public Game.Static.Matching.MatchType MatchTypeEnum;
        public uint MatchingGameTypeEnumFlags;
        public uint TeamSize;
        public uint MinLevel;
        public uint MaxLevel;
        public uint PreparationTimeMS;
        public uint MatchTimeMS;
        public MatchRules MatchingRulesEnum;
        public uint MatchingRulesData00;
        public uint MatchingRulesData01;
        public uint TargetItemLevel;
    }
}
