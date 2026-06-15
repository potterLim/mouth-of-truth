using UnityEditor.Build;

namespace MouthOfTruth.Editor
{
    public readonly struct ReleaseRuntimeRootPath
    {
        public ReleaseRuntimeRootPath(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new BuildFailedException("Release runtime root path is empty.");
            }

            Value = value;
        }

        public string Value { get; }
    }
}
