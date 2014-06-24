using MusicMinkAppLayer.Enums;
using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Models;
using System;
using System.Reflection;

namespace MusicMink.ViewModels.MixEvaluators
{
    public enum RangeEvalType
    {
        Unknown,
        Days,
    }

    class RangeMixEvaluator : IMixEvaluator
    {
        public MixType MixType { get; set; }

        public PropertyInfo TargetProperty { get; private set; }

        public int Target { get; private set; }

        public RangeEvalType EvalType { get; private set; }

        public RangeMixEvaluator(PropertyInfo targetProperty, int target, RangeEvalType evalType, MixType type)
        {
            TargetProperty = targetProperty;
            Target = target;
            EvalType = evalType;
            MixType = type;
        }

        public bool Eval(SongViewModel song)
        {
            DateTime targetValue = DebugHelper.CastAndAssert<DateTime>(TargetProperty.GetValue(song));

            switch (EvalType)
            {
                case RangeEvalType.Days:
                    DateTime target = DateTime.Now - TimeSpan.FromDays(Target);

                    return (target < targetValue);
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected MemberEvalType: {0}", EvalType);
                    return false;
            }
        }

        public bool IsMixNested(int targetId)
        {
            return false;
        }

        public bool IsPlaylistNested(int targetId)
        {
            return false;
        }

        public bool IsPropertyAffected(string propertyName)
        {
            return (TargetProperty.Name == propertyName);
        }

        public int Save(int mixId, bool isRoot)
        {
            return MixEntryModel.SaveMix(MixType, Target.ToString(), mixId, isRoot);
        }

        public static MixType RangeEvalTypeToMixTypeVariant(RangeEvalType evalType)
        {
            switch (evalType)
            {
                case RangeEvalType.Days:
                    return MixType.RANGE_DAYS_VARIANT;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected eval type");
                    return MixType.None;
            }
        }
    }
}
