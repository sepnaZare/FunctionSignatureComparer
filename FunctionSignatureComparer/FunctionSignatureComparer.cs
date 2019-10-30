
using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace FunctionSignatureComparer
{
    public class FunctionSignatureComparer
    {
        private string GitRepositoryPath { get; set; }
        private string ResultFilePath { get; set; }

        private Regex MutipleLineParameterAdded => new Regex(@"(public|private|protected)\s+(static\s+|\s)?\w+\s+\w+\s*\((.|(\n|\r|\r\n))*?\)(\s|\w)*{");
        private Regex InlineMethodDeclarationDeleted => new Regex(@"-(\s)*(public|private|protected)\s+(static\s+|\s)?\w+\s+\w+\s*\(.*?\)(\s|\w)*");
        private Regex InlineMethodDeclarationAdded => new Regex(@"\+(\s)*(public|private|protected)\s+(static\s+|\s)?\w+\s+\w+\s*\((.|(\n|\r|\r\n))*?\)(\s|\w)*");
        private Regex CommentsRegex => new Regex(@"(?://.*)|(/\\*(?:.|[\\n\\r])*?\\*/)");

        public FunctionSignatureComparer(string repoPath, string resultPath)
        {
            GitRepositoryPath = repoPath;
            ResultFilePath = resultPath;
        }

        public void CheckParameterIsAddedToSignature()
        {
            var resultFilePath = string.IsNullOrEmpty(ResultFilePath) ? $"{GitRepositoryPath}/Result.csv" : ResultFilePath;
            StringWriter finalResult = new StringWriter();


            finalResult.WriteLine(string.Format("{0},{1},{2},{3}", "commitSHA", "JavaFile", "oldFunctionSignature", "newFunctionSignature"));

            using (var repo = new Repository(GitRepositoryPath))
            {
                foreach (var commit in repo.Head.Commits)
                {
                    var commitTree = commit.Tree;
                    var parentCommitTree = commit?.Parents.FirstOrDefault()?.Tree;

                    if (parentCommitTree == null)
                        continue;
                    try
                    {
                        Patch treeChanges = repo.Diff.Compare<Patch>(parentCommitTree, commitTree);

                        foreach (PatchEntryChanges treeEntryChanges in treeChanges)
                        {
                            var changesInFile = Regex.Split(treeEntryChanges.Patch, @"\n@@");
                            foreach (var fileChangedSection in changesInFile)
                            {
                                var changedPart = InlineMethodDeclarationDeleted.Match(fileChangedSection);
                                try
                                {
                                    if (changedPart.Success)
                                    {
                                        var result = DetectInlineParameterAdd(commit, fileChangedSection, changedPart, treeEntryChanges.Path);
                                        if (!string.IsNullOrEmpty(result))
                                            finalResult.WriteLine(result);

                                        continue;
                                    }

                                    changedPart = MutipleLineParameterAdded.Match(fileChangedSection);
                                    if (changedPart.Success)
                                    {
                                        var result = DetectNewLineParameterAdd(commit, fileChangedSection, changedPart, treeEntryChanges.Path);
                                        if (!string.IsNullOrEmpty(result))
                                            finalResult.WriteLine(result);

                                        continue;
                                    }
                                }
                                catch
                                {
                                    continue;
                                }
                            }

                        }

                        if (!string.IsNullOrEmpty(finalResult.ToString()))
                        {
                            File.AppendAllText(resultFilePath, finalResult.ToString());
                            finalResult = new StringWriter();
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
            }
        }


        private string DetectNewLineParameterAdd(Commit commit, string fileChangedSection, Match deletedPart, string javaFile)
        {
            var commitSHA = "\"" + commit.Sha + "\"";
            var newFunctionSignature = string.Empty;
            var oldFunctionSignature = string.Empty;

            Regex AllChangesDetection = new Regex(@"(public|private|protected)\s+(static\s+|\s)?\w+\s+\w+\s*\((.|(\n|\r|\r\n))*?\) (.|(\n|\r|\r\n))*?\)(\s|\w)*{");

            Match signatureAllchanges = AllChangesDetection.Match(fileChangedSection);

            if (!(deletedPart.Value.Contains("+") || deletedPart.Value.Contains("-")))
                return string.Empty;

            if (deletedPart.Value.Contains("+") && deletedPart.Value.Contains("-") && !signatureAllchanges.Success)
            {
                int indexOfMinus = deletedPart.Value.IndexOf('-');
                int indexOfPlus = deletedPart.Value.IndexOf('+', indexOfMinus + 1);
                if ((indexOfMinus < 0 || indexOfPlus < 0))
                    return string.Empty;

                var tempNewSignature = (deletedPart.Value.Substring(0, indexOfMinus) + deletedPart.Value.Substring(indexOfPlus + 1)); ;
                var tempOldSignature = deletedPart.Value.Remove(indexOfMinus, indexOfPlus - indexOfMinus);

                newFunctionSignature = ModifyFunctionSignatureFormat(tempNewSignature);
                oldFunctionSignature = ModifyFunctionSignatureFormat(tempOldSignature);
            }
            else if (deletedPart.Value.Contains("+") && !deletedPart.Value.Contains("-") && signatureAllchanges.Success)
            {
                int indexOfPlus = deletedPart.Value.IndexOf('+');
                int lenghtofChange = deletedPart.Value.Split('+')[1].Split('\n')[0].Length + 1;

                var tempOldSignature = deletedPart.Value.Remove(indexOfPlus, lenghtofChange);

                newFunctionSignature = ModifyFunctionSignatureFormat(deletedPart.Value);
                oldFunctionSignature = ModifyFunctionSignatureFormat(tempOldSignature);
            }
            else
            {
                int indexOfMinus = signatureAllchanges.Value.IndexOf('-');
                int indexOfPlus = signatureAllchanges.Value.IndexOf('+', indexOfMinus + 1);
                if (indexOfMinus < 0 || indexOfPlus < 0)
                    return string.Empty;

                var addedPart = (signatureAllchanges.Value.Substring(0, indexOfMinus) + signatureAllchanges.Value.Substring(indexOfPlus + 1));


                newFunctionSignature = ModifyFunctionSignatureFormat(addedPart);
                oldFunctionSignature = ModifyFunctionSignatureFormat(deletedPart.Value);

            }
            if (string.IsNullOrEmpty(oldFunctionSignature) || string.IsNullOrEmpty(newFunctionSignature))
                return string.Empty;

            if (string.IsNullOrEmpty(newFunctionSignature) || string.IsNullOrEmpty(oldFunctionSignature))
                return string.Empty;

            if (!IsChangedParameterCount(newFunctionSignature, oldFunctionSignature))
                return string.Empty;

            return (string.Format("{0},{1},{2},{3}", commitSHA, javaFile, "\"" + oldFunctionSignature + "\"", "\"" + newFunctionSignature + "\""));
        }


        private string DetectInlineParameterAdd(Commit commit, string fileChangedSection, Match deletedPart, string javaFile)
        {
            var addedPart = InlineMethodDeclarationAdded.Match(fileChangedSection);

            if (!addedPart.Success)
                return string.Empty;

            var commitSHA = "\"" + commit.Sha + "\"";
            var oldSignature = ModifyFunctionSignatureFormat(deletedPart.Value);
            var newSignature = ModifyFunctionSignatureFormat(addedPart.Value);

            if (oldSignature.Split('(')[0].Trim() != newSignature.Split('(')[0].Trim())
                return string.Empty;

            if (string.IsNullOrEmpty(oldSignature) || string.IsNullOrEmpty(newSignature))
                return string.Empty;

            if (!IsChangedParameterCount(newSignature, oldSignature))
                return string.Empty;

            return string.Format("{0},{1},{2},{3}", commitSHA, javaFile, "\"" + oldSignature + "\"", "\"" + newSignature + "\"");
        }

        private string ModifyFunctionSignatureFormat(string functionSignature)
        {
            var splittedPart = functionSignature.Split('(');
            var parameter = splittedPart[1].Trim();

            if (CheckParameterFormatValidation(parameter) == CheckResult.Invalid)
                return string.Empty;

            if (splittedPart[1].Contains("public ") || splittedPart[1].Contains("private") || splittedPart[1].Contains("protected"))
                return string.Empty;

            return Regex.Replace(Regex.Replace($"{splittedPart[0].TrimStart()}({parameter.Split(')')[0]})", @"\t|\n|\r|\s\s|\+|-", ""), @"//.*|/\*.*\*/", "");
        }

        private bool IsChangedParameterCount(string newFunctionSignature, string oldFunctionSignature)
        {
            if (newFunctionSignature.Split(',').Length > oldFunctionSignature.Split(',').Length)
                return true;

            return false;
        }

        private CheckResult CheckParameterFormatValidation(string param)
        {
            Regex paramRegex = new Regex(@"(\s*\w+\s*\w+(,\s*\w+\s*\w+\s*)*|\s*)\)");

            if (paramRegex.Match(param).Value == param)
                return CheckResult.Valid;

            return CheckResult.Invalid;
        }

    }
}
