using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.Diagnostics;
using System.Text;
using System.IO;
using System;

// Will run the script whenever the editor loads
[InitializeOnLoad]
public class GrabCommitHash : IPreprocessBuildWithReport
{
    public int callbackOrder => -5000;

    public void OnPreprocessBuild(BuildReport report)
    {
        //TODO: This breaks the building process. Need to figure out how to make it work?.
        //Version();
    }

    static GrabCommitHash()
    {
        //Version();
    }

    /// <summary>
    /// Version is selected from last tag following the "v0" format. 
    /// Itll take the number of commits and tag it on the end so that "v1.5" with 26 commits will become "v1.5.26"
    /// </summary>
    [MenuItem("PreBuild/Record Commit From Git #b")]
    public static void Version()
    {
        try
        {
            // Get Root Folder for Git
            string gitRepository = GetRootFolder();

            // Get Git Commit Hash
            string commitHash = GetGitCommit(gitRepository);

            // Get/Make Scriptable Object
            string gitHashFileName = "GitCommitHash";
            string[] result = AssetDatabase.FindAssets(gitHashFileName);
            GitHash gitHash = null;

            if (result.Length == 0)
            {
                UnityEngine.Debug.Log("[GrabCommitHash] Creating new GitHash Asset.");
                gitHash = ScriptableObject.CreateInstance<GitHash>();
                AssetDatabase.CreateAsset(gitHash, Path.Combine("Assets","Resources",$"{gitHashFileName}.asset"));
            }
            else
            {
                string path = AssetDatabase.GUIDToAssetPath(result[0]);
                gitHash = (GitHash)AssetDatabase.LoadAssetAtPath(path, typeof(GitHash));
            }

            // Edit Object
            gitHash.CommitHash = commitHash;

            // Save Object
            EditorUtility.SetDirty(gitHash);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Leave Debug for Confirmation
            UnityEngine.Debug.Log($"[GrabCommitHash] Hash: {commitHash}");
        }
        catch (System.Exception e)
        {
            UnityEngine.Debug.LogError($"[GrabCommitHash] Could not get new hash. Hash will not be updated." + Environment.NewLine + e);
        }
    }

    private static string GetGitCommit(string repository)
    {
        string version = RunGit(@"log", repository);
        version = version.Split('\n')[0];
        version = version.Substring(6).Trim();
        return version;
    }

    public static string RunGit(string arguments, string path)
    {
        using (var process = new System.Diagnostics.Process())
        {
            int exitCode = process.Run("git", arguments, path,
                out string output, out string errors);
            if (exitCode == 0)
                return output;
            else
                throw new System.Exception($"Git Exit Code: {exitCode} - {errors}");
        }
    }

    private static string GetRootFolder()
    {
        string path = SanitizePath(Application.dataPath);
        path = Path.Combine(Application.dataPath, SanitizePath("../"));
        path = Path.GetFullPath(path);
        return path;
    }

    private static string SanitizePath(string s)
    {
        return s.Replace('/', '\\');
    }
}


public static class ProcessExtensions
{
    /// <summary>
    /// Runs the specified process and waits for it to exit. Its output and errors are
    /// returned as well as the exit code from the process.
    /// See: https://stackoverflow.com/questions/4291912/process-start-how-to-get-the-output
    /// Note that if any deadlocks occur, read the above thread (cubrman's response).
    /// </summary>
    /// <remarks>
    /// This should be run from a using block and disposed after use. It won't 
    /// work properly to keep it around.
    /// </remarks>
    public static int Run(this Process process, string application,
        string arguments, string workingDirectory, out string output,
        out string errors)
    {
        process.StartInfo = new ProcessStartInfo
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            RedirectStandardOutput = true,
            FileName = application,
            Arguments = arguments,
            WorkingDirectory = workingDirectory
        };

        // Use the following event to read both output and errors output.
        var outputBuilder = new StringBuilder();
        var errorsBuilder = new StringBuilder();
        process.OutputDataReceived += (_, args) => outputBuilder.AppendLine(args.Data);
        process.ErrorDataReceived += (_, args) => errorsBuilder.AppendLine(args.Data);

        // Start the process and wait for it to exit.
        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        process.WaitForExit();

        output = outputBuilder.ToString().TrimEnd();
        errors = errorsBuilder.ToString().TrimEnd();
        return process.ExitCode;
    }
}