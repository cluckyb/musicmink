using MusicMinkAppLayer.Enums;
using MusicMinkAppLayer.Diagnostics;
using MusicMinkAppLayer.Models;
using System;
using System.Reflection;

namespace MusicMink.ViewModels.MixEvaluators
{
    public enum NumericEvalType
    {
        Unknown,
        StrictLess,
        Less,
        Equal,
        More,
        StrictMore
    }

    class NumericMixEvaluator<T> : IMixEvaluator
        where T : IComparable
    {
        public MixType MixType { get; set; }

        public PropertyInfo TargetProperty { get; private set; }

        public T Target { get; private set; }

        public NumericEvalType EvalType { get; private set; }

        public NumericMixEvaluator(PropertyInfo targetProperty, T target, NumericEvalType evalType, MixType type)
        {
            TargetProperty = targetProperty;
            Target = target;
            EvalType = evalType;
            MixType = type;
        }

        public bool Eval(SongViewModel song)
        {
            T targetValue = DebugHelper.CastAndAssert<T>(TargetProperty.GetValue(song));

            switch (EvalType)
            {
                case NumericEvalType.StrictLess:
                    return targetValue.CompareTo(Target) < 0;
                case NumericEvalType.Less:
                    return targetValue.CompareTo(Target) <= 0;
                case NumericEvalType.Equal:
                    return targetValue.CompareTo(Target) == 0;
                case NumericEvalType.More:
                    return targetValue.CompareTo(Target) >= 0;
                case NumericEvalType.StrictMore:
                    return targetValue.CompareTo(Target) > 0;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected NumericEvalType: {0}", EvalType);
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

        public static MixType NumericEvalTypeToMixTypeVariant(NumericEvalType evalType)
        {
            switch (evalType)
            {
                case NumericEvalType.StrictLess:
                    return MixType.NUMERIC_STRICTLYLESS_VARIANT;
                case NumericEvalType.Less:
                    return MixType.NUMERIC_LESS_VARIANT;
                case NumericEvalType.Equal:
                    return MixType.NUMERIC_EQUAL_VARIANT;
                case NumericEvalType.More:
                    return MixType.NUMERIC_MORE_VARIANT;
                case NumericEvalType.StrictMore:
                    return MixType.NUMERIC_STRICTLYMORE_VARIANT;
                default:
                    DebugHelper.Alert(new CallerInfo(), "Unexpected eval type");
                    return MixType.None;
            }
        }
    }
}
