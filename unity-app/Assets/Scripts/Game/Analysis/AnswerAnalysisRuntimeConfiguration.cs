using System;

namespace MouthOfTruth.Game.Analysis
{
    public class AnswerAnalysisRuntimeConfiguration
    {
        public const string ANALYSIS_MODE_ENVIRONMENT_VARIABLE_NAME = "MOUTH_OF_TRUTH_ANALYSIS_MODE";
        public const string ANALYSIS_FAILURE_POLICY_ENVIRONMENT_VARIABLE_NAME = "MOUTH_OF_TRUTH_ANALYSIS_FAILURE_POLICY";

        public AnswerAnalysisRuntimeConfiguration(EAnswerAnalysisMode analysisMode, EAnswerAnalysisFailurePolicy failurePolicy)
        {
            AnalysisMode = analysisMode;
            FailurePolicy = failurePolicy;
        }

        public EAnswerAnalysisMode AnalysisMode
        {
            get;
        }

        public EAnswerAnalysisFailurePolicy FailurePolicy
        {
            get;
        }

        public bool ShouldFallBackToDeterministicAnalysis =>
            FailurePolicy == EAnswerAnalysisFailurePolicy.FallBackToDeterministic;

        public static AnswerAnalysisRuntimeConfiguration LoadFromEnvironment()
        {
            return new AnswerAnalysisRuntimeConfiguration(
                ParseAnalysisMode(Environment.GetEnvironmentVariable(ANALYSIS_MODE_ENVIRONMENT_VARIABLE_NAME)),
                ParseFailurePolicy(Environment.GetEnvironmentVariable(ANALYSIS_FAILURE_POLICY_ENVIRONMENT_VARIABLE_NAME)));
        }

        public static EAnswerAnalysisMode ParseAnalysisMode(string rawAnalysisMode)
        {
            string normalizedAnalysisMode = normalizeConfigurationValue(rawAnalysisMode);

            if (string.IsNullOrEmpty(normalizedAnalysisMode) || normalizedAnalysisMode == "auto")
            {
                return EAnswerAnalysisMode.Auto;
            }

            if (normalizedAnalysisMode == "python")
            {
                return EAnswerAnalysisMode.Python;
            }

            if (normalizedAnalysisMode == "deterministic")
            {
                return EAnswerAnalysisMode.Deterministic;
            }

            throw new InvalidOperationException(
                "Unsupported answer analysis mode '"
                + rawAnalysisMode
                + "'. Use auto, python, or deterministic.");
        }

        public static EAnswerAnalysisFailurePolicy ParseFailurePolicy(string rawFailurePolicy)
        {
            string normalizedFailurePolicy = normalizeConfigurationValue(rawFailurePolicy);

            if (string.IsNullOrEmpty(normalizedFailurePolicy)
                || normalizedFailurePolicy == "failfast"
                || normalizedFailurePolicy == "fail-fast"
                || normalizedFailurePolicy == "strict")
            {
                return EAnswerAnalysisFailurePolicy.FailFast;
            }

            if (normalizedFailurePolicy == "deterministic"
                || normalizedFailurePolicy == "fallback"
                || normalizedFailurePolicy == "fallback-to-deterministic")
            {
                return EAnswerAnalysisFailurePolicy.FallBackToDeterministic;
            }

            throw new InvalidOperationException(
                "Unsupported answer analysis failure policy '"
                + rawFailurePolicy
                + "'. Use fail-fast or deterministic.");
        }

        private static string normalizeConfigurationValue(string rawValue)
        {
            return string.IsNullOrWhiteSpace(rawValue)
                ? string.Empty
                : rawValue.Trim().ToLowerInvariant();
        }
    }
}
