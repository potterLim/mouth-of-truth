using System;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Input;
using UnityEngine;

namespace MouthOfTruth.Game.App
{
    public partial class MouthOfTruthAppController
    {
        private void requestApplicationExit()
        {
            mAnswerCaptureInputAdapter?.CancelCollection();
            mFaceCaptureInputAdapter?.CancelCollection();
            restoreSystemCursor();

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void resetAnswerTracking()
        {
            mLastObservedTranscript = AnswerTranscript.Empty;
            mGameView?.ClearAnswerTranscript();
            mAnswerCaptureInputAdapter?.Reset();
            mFaceCaptureInputAdapter?.Reset();
            mLastObservedHandAnchorState = EHandAnchorState.OutsideMouth;
            resetHandPromptDismissalTracking();
        }

        private void runObservedAsync(Func<Task> asyncOperation, string operationName)
        {
            _ = runObservedTaskAsync(asyncOperation, operationName);
        }

        private async Task runObservedTaskAsync(Func<Task> asyncOperation, string operationName)
        {
            if (asyncOperation == null)
            {
                return;
            }

            try
            {
                await asyncOperation();
            }
            catch (OperationCanceledException)
            {
                mIsTransitionBusy = false;
            }
            catch (Exception exception)
            {
                Debug.LogError("MouthOfTruth transition failed while trying to " + operationName + ". Returning to the start screen.\n" + exception);
                recoverToStartScreenAfterTransitionFailure();
            }
        }

        private void recoverToStartScreenAfterTransitionFailure()
        {
            mIsTransitionBusy = false;
            mAnswerCaptureInputAdapter?.CancelCollection();
            mFaceCaptureInputAdapter?.CancelCollection();

            try
            {
                resetAnswerTracking();
                resetInteractionSelectionState();
                mGameStateMachine?.OpenStartScreen();
                mGameView?.ShowStartScreen();
            }
            catch (Exception recoveryException)
            {
                Debug.LogError("MouthOfTruth transition recovery failed.\n" + recoveryException);
            }
        }

        private void tryCleanAllSessionArtifacts(string reason)
        {
            try
            {
                MouthOfTruthSessionArtifactCleaner.CleanAllSessionArtifacts();
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Session artifact cleanup failed during " + reason + ".\n" + exception);
            }
        }

        private void cleanSessionArtifactsOnExit()
        {
            if (mHasCleanedSessionArtifactsOnExit)
            {
                return;
            }

            mHasCleanedSessionArtifactsOnExit = true;
            tryCleanAllSessionArtifacts("application exit");
        }
    }
}
