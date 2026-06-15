using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MouthOfTruth.Game.Analysis;
using MouthOfTruth.Game.App;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Face;
using MouthOfTruth.Game.Input;
using MouthOfTruth.Game.Session;
using MouthOfTruth.Game.Voice;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace MouthOfTruth.Editor.Tests.Game
{
    public class CoreGameLogicTests
    {
        [Test]
        public void CardDwellSelectionTrackerConfirmsOnlyAfterContinuousDwell()
        {
            CardDwellSelectionTracker tracker = new CardDwellSelectionTracker(new SecondsDuration(1.0f));

            Assert.That(tracker.UpdateHoveredCardOrNull(EQuestionCardSlot.LeftCard, new SecondsDuration(0.4f)), Is.Null);
            Assert.That(tracker.UpdateHoveredCardOrNull(EQuestionCardSlot.LeftCard, new SecondsDuration(0.5f)), Is.Null);

            EQuestionCardSlot? confirmedSlotOrNull = tracker.UpdateHoveredCardOrNull(EQuestionCardSlot.LeftCard, new SecondsDuration(0.1f));

            Assert.That(confirmedSlotOrNull, Is.EqualTo(EQuestionCardSlot.LeftCard));
            Assert.That(tracker.HoveredDuration.Value, Is.EqualTo(0.0f));
        }

        [Test]
        public void CardDwellSelectionTrackerResetsWhenHoverChanges()
        {
            CardDwellSelectionTracker tracker = new CardDwellSelectionTracker(new SecondsDuration(1.0f));

            Assert.That(tracker.UpdateHoveredCardOrNull(EQuestionCardSlot.LeftCard, new SecondsDuration(0.9f)), Is.Null);
            Assert.That(tracker.UpdateHoveredCardOrNull(EQuestionCardSlot.RightCard, new SecondsDuration(0.2f)), Is.Null);
            Assert.That(tracker.HoveredDuration.Value, Is.EqualTo(0.2f));
        }

        [Test]
        public void UiActionDwellSelectionTrackerConfirmsAndResets()
        {
            UiActionDwellSelectionTracker tracker = new UiActionDwellSelectionTracker(new SecondsDuration(0.5f));

            Assert.That(tracker.UpdateHoveredTargetOrNull(EUiActionTarget.StartGame, new SecondsDuration(0.2f)), Is.Null);

            EUiActionTarget? confirmedTargetOrNull = tracker.UpdateHoveredTargetOrNull(EUiActionTarget.StartGame, new SecondsDuration(0.3f));

            Assert.That(confirmedTargetOrNull, Is.EqualTo(EUiActionTarget.StartGame));
            Assert.That(tracker.HoveredDuration.Value, Is.EqualTo(0.0f));
        }

        [Test]
        public void AnswerCollectionPolicyWaitsForGraceBeforeSilenceTimeout()
        {
            AnswerCollectionPolicy policy = createAnswerCollectionPolicy(initialSilenceGraceSeconds: 2.0f, silenceTimeoutSeconds: 1.0f, maximumAnswerDurationSeconds: 5.0f);

            AnswerCollectionTickResult tick = policy.Advance(new SecondsDuration(0.0f), new SecondsDuration(0.0f), new SecondsDuration(1.5f), ESpeechDetectionState.Silent);

            Assert.That(tick.ShouldFinishForSilence, Is.False);

            tick = policy.Advance(tick.ElapsedAnswerDuration, tick.ElapsedSilenceDuration, new SecondsDuration(0.5f), ESpeechDetectionState.Silent);

            Assert.That(tick.ShouldFinishForSilence, Is.True);
        }

        [Test]
        public void AnswerCollectionPolicyResetsSilenceWhenSpeechIsDetected()
        {
            AnswerCollectionPolicy policy = createAnswerCollectionPolicy(initialSilenceGraceSeconds: 2.0f, silenceTimeoutSeconds: 1.0f, maximumAnswerDurationSeconds: 5.0f);

            AnswerCollectionTickResult tick = policy.Advance(new SecondsDuration(2.0f), new SecondsDuration(0.9f), new SecondsDuration(0.1f), ESpeechDetectionState.SpeechDetected);

            Assert.That(tick.ElapsedSilenceDuration.Value, Is.EqualTo(0.0f));
            Assert.That(tick.ShouldFinishForSilence, Is.False);
        }

        [Test]
        public void AnswerCollectionPolicyFinishesAtMaximumDuration()
        {
            AnswerCollectionPolicy policy = createAnswerCollectionPolicy(initialSilenceGraceSeconds: 10.0f, silenceTimeoutSeconds: 5.0f, maximumAnswerDurationSeconds: 3.0f);

            AnswerCollectionTickResult tick = policy.Advance(new SecondsDuration(2.9f), SecondsDuration.Zero, new SecondsDuration(0.1f), ESpeechDetectionState.SpeechDetected);

            Assert.That(tick.ShouldFinishForTimeout, Is.True);
        }

        [Test]
        public void NormalizedProgressClampsUntrustedPresentationValues()
        {
            Assert.That(NormalizedProgress.FromUnclamped(-0.5f).Value, Is.EqualTo(0.0f));
            Assert.That(NormalizedProgress.FromUnclamped(1.5f).Value, Is.EqualTo(1.0f));
            Assert.That(NormalizedProgress.FromUnclamped(float.NaN).Value, Is.EqualTo(0.0f));
        }

        [Test]
        public void LoopedAudioClipReaderCalculatesWrappedSampleDistance()
        {
            Assert.That(LoopedAudioClipReader.CalculateLoopedSampleDistance(new AudioSamplePosition(2), new AudioSamplePosition(8), new AudioSampleCount(10)).Value, Is.EqualTo(6));
            Assert.That(LoopedAudioClipReader.CalculateLoopedSampleDistance(new AudioSamplePosition(8), new AudioSamplePosition(2), new AudioSampleCount(10)).Value, Is.EqualTo(4));
            Assert.That(LoopedAudioClipReader.CalculateLoopedSampleDistance(new AudioSamplePosition(8), new AudioSamplePosition(2), AudioSampleCount.Zero).Value, Is.EqualTo(0));
        }

        [Test]
        public void SpeechEvidenceDetectorSeparatesNoiseFromSpeechEvidence()
        {
            SpeechEvidenceDetector detector = new SpeechEvidenceDetector(new AudioSampleRate(16000));

            Assert.That(detector.EvaluateSpeechState(createConstantSamples(1600, 0.001f)), Is.EqualTo(ESpeechDetectionState.Silent));
            Assert.That(detector.ContainsSpeechEvidence(createConstantSamples(1600, 0.001f)), Is.False);
            Assert.That(detector.EvaluateSpeechState(createConstantSamples(1600, 0.03f)), Is.EqualTo(ESpeechDetectionState.SpeechDetected));
            Assert.That(detector.ContainsSpeechEvidence(createConstantSamples(1600, 0.03f)), Is.True);
        }

        [Test]
        public void GameStateMachineMovesThroughQuestionSelectionAndAnswerStart()
        {
            MouthOfTruthGameStateMachine stateMachine = createStateMachine(cardDwellSeconds: 0.5f);

            stateMachine.StartGame();
            Assert.That(stateMachine.CurrentState, Is.EqualTo(EGameFlowState.PresentingCards));

            stateMachine.MarkCardPresentationCompleted();
            Assert.That(stateMachine.CurrentState, Is.EqualTo(EGameFlowState.AwaitingCardSelection));

            Assert.That(stateMachine.UpdateCardSelectionOrNull(EQuestionCardSlot.CenterCard, new SecondsDuration(0.4f)), Is.Null);

            EQuestionCardSlot? selectedSlotOrNull = stateMachine.UpdateCardSelectionOrNull(EQuestionCardSlot.CenterCard, new SecondsDuration(0.1f));

            Assert.That(selectedSlotOrNull, Is.EqualTo(EQuestionCardSlot.CenterCard));
            Assert.That(stateMachine.CurrentState, Is.EqualTo(EGameFlowState.RevealingQuestionCard));
            Assert.That(stateMachine.CreateSnapshot().SelectedQuestionDefinitionOrNull, Is.Not.Null);

            stateMachine.MarkQuestionRevealCompleted();
            Assert.That(stateMachine.CurrentState, Is.EqualTo(EGameFlowState.NarratingQuestion));

            stateMachine.MarkQuestionNarrationCompleted();
            Assert.That(stateMachine.CurrentState, Is.EqualTo(EGameFlowState.AwaitingHandInsertion));

            stateMachine.NotifyHandReachedFrontAnchor();
            Assert.That(stateMachine.CurrentState, Is.EqualTo(EGameFlowState.InsertingHand));

            stateMachine.NotifyHandReachedInnerAnchor();
            Assert.That(stateMachine.CurrentState, Is.EqualTo(EGameFlowState.Answering));
        }

        [Test]
        public async Task DeterministicAnalysisReturnsUncertainWhenEvidenceIsMissing()
        {
            DeterministicAnswerAnalysisClient client = new DeterministicAnswerAnalysisClient();
            AnswerAnalysisRequest request = createAnalysisRequest("Q100", "answer", faceFrameCount: 0, voiceSegmentCount: 0);

            AnswerAnalysisResult result = await client.AnalyzeAsync(request, CancellationToken.None);

            Assert.That(result.VerdictKind, Is.EqualTo(EVerdictKind.Uncertain));
            Assert.That(
                result.ReasonCodes.Select(reasonCode => reasonCode.Value),
                Is.EquivalentTo(new[] { "insufficient_face_data", "insufficient_voice_data" }));
        }

        [Test]
        public async Task DeterministicAnalysisUsesStableParityWhenEvidenceIsPresent()
        {
            DeterministicAnswerAnalysisClient client = new DeterministicAnswerAnalysisClient();
            AnswerAnalysisRequest firstRequest = createAnalysisRequest("Q101", "same answer", faceFrameCount: 4, voiceSegmentCount: 1);
            AnswerAnalysisRequest secondRequest = createAnalysisRequest("Q101", "same answer", faceFrameCount: 4, voiceSegmentCount: 1);

            AnswerAnalysisResult firstResult = await client.AnalyzeAsync(firstRequest, CancellationToken.None);
            AnswerAnalysisResult secondResult = await client.AnalyzeAsync(secondRequest, CancellationToken.None);

            Assert.That(firstResult.VerdictKind, Is.EqualTo(secondResult.VerdictKind));
            Assert.That(firstResult.VerdictKind, Is.Not.EqualTo(EVerdictKind.Uncertain));
            Assert.That(firstResult.ReasonCodes, Is.Empty);
        }

        [Test]
        public void SessionArtifactCleanerDeletesAnalysisArtifactsInsideWorkspace()
        {
            string previousRuntimeRoot = Environment.GetEnvironmentVariable("MOUTH_OF_TRUTH_RUNTIME_ROOT");
            string temporaryRuntimeRoot = createTemporaryRuntimeRoot();

            try
            {
                Environment.SetEnvironmentVariable("MOUTH_OF_TRUTH_RUNTIME_ROOT", temporaryRuntimeRoot);
                AnswerAudioFilePath audioFilePath = AnswerAudioWorkspacePaths.BuildAudioFilePath(new QuestionId("Q_TEST"));
                FaceFramesDirectoryPath faceDirectoryPath = FaceFrameWorkspacePaths.BuildCaptureDirectoryPath(new QuestionId("Q_TEST"));
                Directory.CreateDirectory(Path.GetDirectoryName(audioFilePath.Value));
                Directory.CreateDirectory(faceDirectoryPath.Value);
                File.WriteAllText(audioFilePath.Value, "audio");
                File.WriteAllText(Path.Combine(faceDirectoryPath.Value, "frame_00001.jpg"), "face");

                MouthOfTruthSessionArtifactCleaner.CleanAnalysisArtifacts(audioFilePath, faceDirectoryPath);

                Assert.That(File.Exists(audioFilePath.Value), Is.False);
                Assert.That(Directory.Exists(faceDirectoryPath.Value), Is.False);
            }
            finally
            {
                Environment.SetEnvironmentVariable("MOUTH_OF_TRUTH_RUNTIME_ROOT", previousRuntimeRoot);
                Directory.Delete(temporaryRuntimeRoot, recursive: true);
            }
        }

        [Test]
        public void SessionArtifactCleanerDoesNotDeletePathsOutsideWorkspace()
        {
            string previousRuntimeRoot = Environment.GetEnvironmentVariable("MOUTH_OF_TRUTH_RUNTIME_ROOT");
            string temporaryRuntimeRoot = createTemporaryRuntimeRoot();
            string outsideDirectoryPath = Path.Combine(Path.GetTempPath(), "mouth-of-truth-outside-" + Guid.NewGuid().ToString("N"));

            try
            {
                Environment.SetEnvironmentVariable("MOUTH_OF_TRUTH_RUNTIME_ROOT", temporaryRuntimeRoot);
                Directory.CreateDirectory(outsideDirectoryPath);
                string outsideFilePath = Path.Combine(outsideDirectoryPath, "answer.wav");
                string outsideFaceDirectoryPath = Path.Combine(outsideDirectoryPath, "face");
                Directory.CreateDirectory(outsideFaceDirectoryPath);
                File.WriteAllText(outsideFilePath, "audio");
                File.WriteAllText(Path.Combine(outsideFaceDirectoryPath, "frame_00001.jpg"), "face");
                LogAssert.Expect(LogType.Warning, "Skipped deleting session artifact outside the allowed directory: " + outsideFilePath);
                LogAssert.Expect(LogType.Warning, "Skipped deleting session artifact directory outside the allowed directory: " + outsideFaceDirectoryPath);

                MouthOfTruthSessionArtifactCleaner.CleanAnalysisArtifacts(new AnswerAudioFilePath(outsideFilePath), new FaceFramesDirectoryPath(outsideFaceDirectoryPath));

                Assert.That(File.Exists(outsideFilePath), Is.True);
                Assert.That(Directory.Exists(outsideFaceDirectoryPath), Is.True);
            }
            finally
            {
                Environment.SetEnvironmentVariable("MOUTH_OF_TRUTH_RUNTIME_ROOT", previousRuntimeRoot);
                Directory.Delete(temporaryRuntimeRoot, recursive: true);

                if (Directory.Exists(outsideDirectoryPath))
                {
                    Directory.Delete(outsideDirectoryPath, recursive: true);
                }
            }
        }

        [Test]
        public void SessionArtifactCleanerClearsPreviousRunDirectories()
        {
            string previousRuntimeRoot = Environment.GetEnvironmentVariable("MOUTH_OF_TRUTH_RUNTIME_ROOT");
            string temporaryRuntimeRoot = createTemporaryRuntimeRoot();

            try
            {
                Environment.SetEnvironmentVariable("MOUTH_OF_TRUTH_RUNTIME_ROOT", temporaryRuntimeRoot);
                string audioDirectoryPath = AnswerAudioWorkspacePaths.GetAudioDirectoryPath();
                string faceDirectoryPath = FaceFrameWorkspacePaths.GetFaceFramesDirectoryPath();
                Directory.CreateDirectory(audioDirectoryPath);
                Directory.CreateDirectory(Path.Combine(faceDirectoryPath, "Q_TEST"));
                File.WriteAllText(Path.Combine(audioDirectoryPath, "answer.wav"), "audio");
                File.WriteAllText(Path.Combine(faceDirectoryPath, "Q_TEST", "frame_00001.jpg"), "face");

                MouthOfTruthSessionArtifactCleaner.CleanAllSessionArtifacts();

                Assert.That(Directory.GetFiles(audioDirectoryPath), Is.Empty);
                Assert.That(Directory.GetDirectories(faceDirectoryPath), Is.Empty);
            }
            finally
            {
                Environment.SetEnvironmentVariable("MOUTH_OF_TRUTH_RUNTIME_ROOT", previousRuntimeRoot);
                Directory.Delete(temporaryRuntimeRoot, recursive: true);
            }
        }

        private static MouthOfTruthGameStateMachine createStateMachine(float cardDwellSeconds)
        {
            QuestionDeckService questionDeckService = new QuestionDeckService(createQuestions(), new QuestionDeckRandomSeed(42));
            CardDwellSelectionTracker cardDwellSelectionTracker = new CardDwellSelectionTracker(new SecondsDuration(cardDwellSeconds));
            AnswerCollectionPolicy answerCollectionPolicy = new AnswerCollectionPolicy();
            return new MouthOfTruthGameStateMachine(questionDeckService, cardDwellSelectionTracker, answerCollectionPolicy);
        }

        private static AnswerCollectionPolicy createAnswerCollectionPolicy(float initialSilenceGraceSeconds, float silenceTimeoutSeconds, float maximumAnswerDurationSeconds)
        {
            return new AnswerCollectionPolicy(
                new SecondsDuration(initialSilenceGraceSeconds),
                new SecondsDuration(silenceTimeoutSeconds),
                new SecondsDuration(maximumAnswerDurationSeconds));
        }

        private static IReadOnlyList<QuestionDefinition> createQuestions()
        {
            return new[]
            {
                createQuestion("Q001"),
                createQuestion("Q002"),
                createQuestion("Q003"),
                createQuestion("Q004"),
            };
        }

        private static QuestionDefinition createQuestion(string id)
        {
            return new QuestionDefinition(
                new QuestionId(id),
                new QuestionText("Question text"),
                new QuestionCategory("test"),
                new QuestionDifficulty(1),
                EQuestionAvailability.Enabled);
        }

        private static AnswerAnalysisRequest createAnalysisRequest(string questionId, string answerTranscript, int faceFrameCount, int voiceSegmentCount)
        {
            return new AnswerAnalysisRequest(
                createQuestion(questionId),
                new AnswerTranscript(answerTranscript),
                AnswerAudioFilePath.Empty,
                FaceFramesDirectoryPath.Empty,
                new FaceFrameCount(faceFrameCount),
                new VoiceSegmentCount(voiceSegmentCount));
        }

        private static float[] createConstantSamples(int sampleCount, float sampleValue)
        {
            float[] samples = new float[sampleCount];

            for (int sampleIndex = 0; sampleIndex < samples.Length; sampleIndex += 1)
            {
                samples[sampleIndex] = sampleValue;
            }

            return samples;
        }

        private static string createTemporaryRuntimeRoot()
        {
            string temporaryRuntimeRoot = Path.Combine(Path.GetTempPath(), "mouth-of-truth-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporaryRuntimeRoot);
            return temporaryRuntimeRoot;
        }
    }
}
