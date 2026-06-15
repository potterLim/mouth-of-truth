using System;
using System.Collections.Generic;
using System.IO;
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
            CardDwellSelectionTracker tracker = new CardDwellSelectionTracker(requiredDwellSeconds: 1.0f);

            Assert.That(tracker.UpdateHoveredCardOrNull(EQuestionCardSlot.LeftCard, 0.4f), Is.Null);
            Assert.That(tracker.UpdateHoveredCardOrNull(EQuestionCardSlot.LeftCard, 0.5f), Is.Null);

            EQuestionCardSlot? confirmedSlotOrNull = tracker.UpdateHoveredCardOrNull(EQuestionCardSlot.LeftCard, 0.1f);

            Assert.That(confirmedSlotOrNull, Is.EqualTo(EQuestionCardSlot.LeftCard));
            Assert.That(tracker.HoveredDurationSeconds, Is.EqualTo(0.0f));
        }

        [Test]
        public void CardDwellSelectionTrackerResetsWhenHoverChanges()
        {
            CardDwellSelectionTracker tracker = new CardDwellSelectionTracker(requiredDwellSeconds: 1.0f);

            Assert.That(tracker.UpdateHoveredCardOrNull(EQuestionCardSlot.LeftCard, 0.9f), Is.Null);
            Assert.That(tracker.UpdateHoveredCardOrNull(EQuestionCardSlot.RightCard, 0.2f), Is.Null);
            Assert.That(tracker.HoveredDurationSeconds, Is.EqualTo(0.2f));
        }

        [Test]
        public void UiActionDwellSelectionTrackerConfirmsAndResets()
        {
            UiActionDwellSelectionTracker tracker = new UiActionDwellSelectionTracker(requiredDwellSeconds: 0.5f);

            Assert.That(tracker.UpdateHoveredTargetOrNull(EUiActionTarget.StartGame, 0.2f), Is.Null);

            EUiActionTarget? confirmedTargetOrNull = tracker.UpdateHoveredTargetOrNull(EUiActionTarget.StartGame, 0.3f);

            Assert.That(confirmedTargetOrNull, Is.EqualTo(EUiActionTarget.StartGame));
            Assert.That(tracker.HoveredDurationSeconds, Is.EqualTo(0.0f));
        }

        [Test]
        public void AnswerCollectionPolicyWaitsForGraceBeforeSilenceTimeout()
        {
            AnswerCollectionPolicy policy = new AnswerCollectionPolicy(initialSilenceGraceSeconds: 2.0f, silenceTimeoutSeconds: 1.0f, maximumAnswerDurationSeconds: 5.0f);

            AnswerCollectionTickResult tick = policy.Advance(elapsedAnswerSeconds: 0.0f, elapsedSilenceSeconds: 0.0f, deltaTimeSeconds: 1.5f, isSpeechDetected: false);

            Assert.That(tick.ShouldFinishForSilence, Is.False);

            tick = policy.Advance(tick.ElapsedAnswerSeconds, tick.ElapsedSilenceSeconds, deltaTimeSeconds: 0.5f, isSpeechDetected: false);

            Assert.That(tick.ShouldFinishForSilence, Is.True);
        }

        [Test]
        public void AnswerCollectionPolicyResetsSilenceWhenSpeechIsDetected()
        {
            AnswerCollectionPolicy policy = new AnswerCollectionPolicy(initialSilenceGraceSeconds: 2.0f, silenceTimeoutSeconds: 1.0f, maximumAnswerDurationSeconds: 5.0f);

            AnswerCollectionTickResult tick = policy.Advance(elapsedAnswerSeconds: 2.0f, elapsedSilenceSeconds: 0.9f, deltaTimeSeconds: 0.1f, isSpeechDetected: true);

            Assert.That(tick.ElapsedSilenceSeconds, Is.EqualTo(0.0f));
            Assert.That(tick.ShouldFinishForSilence, Is.False);
        }

        [Test]
        public void AnswerCollectionPolicyFinishesAtMaximumDuration()
        {
            AnswerCollectionPolicy policy = new AnswerCollectionPolicy(initialSilenceGraceSeconds: 10.0f, silenceTimeoutSeconds: 5.0f, maximumAnswerDurationSeconds: 3.0f);

            AnswerCollectionTickResult tick = policy.Advance(elapsedAnswerSeconds: 2.9f, elapsedSilenceSeconds: 0.0f, deltaTimeSeconds: 0.1f, isSpeechDetected: true);

            Assert.That(tick.ShouldFinishForTimeout, Is.True);
        }

        [Test]
        public void GameStateMachineMovesThroughQuestionSelectionAndAnswerStart()
        {
            MouthOfTruthGameStateMachine stateMachine = createStateMachine(cardDwellSeconds: 0.5f);

            stateMachine.StartGame();
            Assert.That(stateMachine.CurrentState, Is.EqualTo(EGameFlowState.PresentingCards));

            stateMachine.MarkCardPresentationCompleted();
            Assert.That(stateMachine.CurrentState, Is.EqualTo(EGameFlowState.AwaitingCardSelection));

            Assert.That(stateMachine.UpdateCardSelectionOrNull(EQuestionCardSlot.CenterCard, 0.4f), Is.Null);

            EQuestionCardSlot? selectedSlotOrNull = stateMachine.UpdateCardSelectionOrNull(EQuestionCardSlot.CenterCard, 0.1f);

            Assert.That(selectedSlotOrNull, Is.EqualTo(EQuestionCardSlot.CenterCard));
            Assert.That(stateMachine.CurrentState, Is.EqualTo(EGameFlowState.RevealingQuestionCard));
            Assert.That(stateMachine.CreateSnapshot().SelectedQuestionDefinition, Is.Not.Null);

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
            AnswerAnalysisRequest request = new AnswerAnalysisRequest(createQuestion("Q100"), "answer", string.Empty, string.Empty, faceFrameCount: 0, voiceSegmentCount: 0);

            AnswerAnalysisResult result = await client.AnalyzeAsync(request, CancellationToken.None);

            Assert.That(result.VerdictKind, Is.EqualTo(EVerdictKind.Uncertain));
            Assert.That(result.ReasonCodes, Is.EquivalentTo(new[] { "insufficient_face_data", "insufficient_voice_data" }));
        }

        [Test]
        public async Task DeterministicAnalysisUsesStableParityWhenEvidenceIsPresent()
        {
            DeterministicAnswerAnalysisClient client = new DeterministicAnswerAnalysisClient();
            AnswerAnalysisRequest firstRequest = new AnswerAnalysisRequest(createQuestion("Q101"), "same answer", string.Empty, string.Empty, faceFrameCount: 4, voiceSegmentCount: 1);
            AnswerAnalysisRequest secondRequest = new AnswerAnalysisRequest(createQuestion("Q101"), "same answer", string.Empty, string.Empty, faceFrameCount: 4, voiceSegmentCount: 1);

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
                string audioFilePath = AnswerAudioWorkspacePaths.BuildAudioFilePath("Q_TEST");
                string faceDirectoryPath = FaceFrameWorkspacePaths.BuildCaptureDirectoryPath("Q_TEST");
                Directory.CreateDirectory(Path.GetDirectoryName(audioFilePath));
                Directory.CreateDirectory(faceDirectoryPath);
                File.WriteAllText(audioFilePath, "audio");
                File.WriteAllText(Path.Combine(faceDirectoryPath, "frame_00001.jpg"), "face");

                MouthOfTruthSessionArtifactCleaner.CleanAnalysisArtifacts(audioFilePath, faceDirectoryPath);

                Assert.That(File.Exists(audioFilePath), Is.False);
                Assert.That(Directory.Exists(faceDirectoryPath), Is.False);
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

                MouthOfTruthSessionArtifactCleaner.CleanAnalysisArtifacts(outsideFilePath, outsideFaceDirectoryPath);

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
            QuestionDeckService questionDeckService = new QuestionDeckService(createQuestions(), randomSeedOrNull: 42);
            CardDwellSelectionTracker cardDwellSelectionTracker = new CardDwellSelectionTracker(cardDwellSeconds);
            AnswerCollectionPolicy answerCollectionPolicy = new AnswerCollectionPolicy();
            return new MouthOfTruthGameStateMachine(questionDeckService, cardDwellSelectionTracker, answerCollectionPolicy);
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
            return new QuestionDefinition(id, "Question text", "test", difficulty: 1, isEnabled: true);
        }

        private static string createTemporaryRuntimeRoot()
        {
            string temporaryRuntimeRoot = Path.Combine(Path.GetTempPath(), "mouth-of-truth-tests-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(temporaryRuntimeRoot);
            return temporaryRuntimeRoot;
        }
    }
}
