using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MouthOfTruth.Game.Data;
using MouthOfTruth.Game.Presentation.Runtime;
using UnityEngine;

namespace MouthOfTruth.Game.Narration
{
    public class PrerecordedQuestionNarrationService : IQuestionNarrationService
    {
        private readonly AudioSource mAudioSource;
        private readonly IQuestionNarrationService mFallbackNarrationService;
        private readonly QuestionAudioDirectoryPath mQuestionAudioDirectoryPath;

        public PrerecordedQuestionNarrationService(QuestionAudioDirectoryPath questionAudioDirectoryPath, IQuestionNarrationService fallbackNarrationService)
        {
            mQuestionAudioDirectoryPath = questionAudioDirectoryPath;
            mFallbackNarrationService = fallbackNarrationService == null
                ? new SilentQuestionNarrationService()
                : fallbackNarrationService;
            GameObject audioSourceObject = new GameObject("QuestionNarrationAudioSource");
            Object.DontDestroyOnLoad(audioSourceObject);
            mAudioSource = audioSourceObject.AddComponent<AudioSource>();
            mAudioSource.playOnAwake = false;
            mAudioSource.loop = false;
            mAudioSource.spatialBlend = 0.0f;
            mAudioSource.volume = 0.92f;
            mAudioSource.priority = 8;
            mAudioSource.dopplerLevel = 0.0f;
            mAudioSource.ignoreListenerPause = true;
        }

        public async Task SpeakQuestionAsync(QuestionDefinition questionDefinition, CancellationToken cancellationToken)
        {
            RuntimeAssetFilePath audioFilePath = getQuestionAudioFilePath(questionDefinition);

            if (audioFilePath.IsEmpty)
            {
                await mFallbackNarrationService.SpeakQuestionAsync(questionDefinition, cancellationToken);
                return;
            }

            AudioClip audioClip = await RuntimeAudioClipLoader.LoadClipOrNullAsync(audioFilePath);

            if (audioClip == null)
            {
                await mFallbackNarrationService.SpeakQuestionAsync(questionDefinition, cancellationToken);
                return;
            }

            await playClipAsync(audioClip, cancellationToken);
        }

        private async Task playClipAsync(AudioClip audioClip, CancellationToken cancellationToken)
        {
            mAudioSource.Stop();
            mAudioSource.clip = audioClip;
            mAudioSource.Play();

            try
            {
                while (mAudioSource.isPlaying)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await Task.Delay(50, cancellationToken);
                }
            }
            finally
            {
                mAudioSource.Stop();
                mAudioSource.clip = null;
            }
        }

        private RuntimeAssetFilePath getQuestionAudioFilePath(QuestionDefinition questionDefinition)
        {
            if (questionDefinition == null || string.IsNullOrWhiteSpace(questionDefinition.Id.Value))
            {
                return RuntimeAssetFilePath.Empty;
            }

            if (mQuestionAudioDirectoryPath.IsEmpty)
            {
                return RuntimeAssetFilePath.Empty;
            }

            string candidateFilePath = Path.Combine(mQuestionAudioDirectoryPath.Value, $"{questionDefinition.Id.Value}.wav");

            if (File.Exists(candidateFilePath))
            {
                return new RuntimeAssetFilePath(candidateFilePath);
            }

            return RuntimeAssetFilePath.Empty;
        }
    }
}
