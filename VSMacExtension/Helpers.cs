using System;
using System.IO;
using System.Linq;
using System.Reflection;
using MonoDevelop.Core;
using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace VSMacExtension
{
    static class Helpers
    {
        public static string GetBundleAddinsDirectory ()
        {
            var solution = IdeApp.ProjectOperations.CurrentSelectedSolution;
            var outputdir = solution.OutputDirectory.Replace("\\", "/");
            var vsProject = GetVisualStudioProject();
            var appBundlePath = Path.Combine(outputdir, vsProject.Name + ".app", "Contents", "MonoBundle", "AddIns");
            return appBundlePath;
        }

        public static string GetOutputDirectory ()
        {
            var solution = IdeApp.ProjectOperations.CurrentSelectedSolution;
            var outputdir = solution.OutputDirectory.Replace("\\", "/");
            return outputdir;
        }

        public static string GetOriginAddinsDirectory()
        {
            var output = GetOutputDirectory();
            var result =  Path.Combine(output, "AddIns");
            return result;
        }

        public static string MakeRelativePath(string fromPath, string toPath)
        {
            if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
            if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

            Uri fromUri = new Uri(fromPath);
            Uri toUri = new Uri(toPath);

            if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

            Uri relativeUri = fromUri.MakeRelativeUri(toUri);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
            {
                relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            }

            return relativePath;
        }

        public static DotNetProject GetVisualStudioProject ()
        {
            var solution = IdeApp.ProjectOperations.CurrentSelectedSolution;
            return solution.GetAllProjects().FirstOrDefault (s => s.Name == "VisualStudio") as DotNetProject;
        }

        public static string GetOriginProjectAddinDirectory(DotNetProject selectedProject)
        {
            if ((selectedProject.DefaultConfiguration is DotNetProjectConfiguration startupProjectConfig))
            {
                return startupProjectConfig.OutputDirectory;
            }
            return string.Empty;
        }

        //public static bool IsAddinProject (DotNetProject selectedProject, out Mono.Addins.AddinAttribute addinAttribute)
        //{
        //    if (selectedProject.DefaultConfiguration is DotNetProjectConfiguration)
        //    {
        //        var outputname = selectedProject.GetOutputFileName(IdeApp.Workspace.ActiveConfiguration);
        //        if (File.Exists(outputname))
        //        {
        //            try
        //            {
        //                var assembly = Assembly.LoadFile(outputname);
        //                var attributes = (Mono.Addins.AddinAttribute)assembly.GetCustomAttribute(typeof(Mono.Addins.AddinAttribute));
        //                if (attributes != null)
        //                {
        //                    addinAttribute = attributes;
        //                    return true;
        //                }
        //            }
        //            catch (System.Exception ex)
        //            {
        //                LoggingService.LogError($"Cannot read assembly: {outputname}",ex);
        //            }
        //        }
        //    }
        //    addinAttribute = null;
        //    return false;
        //}

        public static string GetDestinationProjectAddinDirectory(DotNetProject selectedProject)
        {
            var outputFile = selectedProject.GetOutputFileName(IdeApp.Workspace.ActiveConfiguration).ParentDirectory.FullPath;
            var localAddinsDirectory = GetOriginAddinsDirectory();
            var relativeDirectoryPath = MakeRelativePath(localAddinsDirectory, outputFile);
            if (relativeDirectoryPath.StartsWith("AddIns/"))
                relativeDirectoryPath = relativeDirectoryPath.Substring("AddIns/".Length);

            var addinsDirectory = GetBundleAddinsDirectory();
            var destinationDirectory = Path.Combine(addinsDirectory, relativeDirectoryPath);
            return destinationDirectory;
        }

        public static bool IsVMacSolution()
        {
            var selectedSolution = IdeApp.ProjectOperations.CurrentSelectedSolution;
            if (selectedSolution.Name == "Main")
            {
                return true;
            }
            return false;
        }

        public static void CopyFilesRecursively(string sourcePath, string targetPath)
        {
            //Now Create all of the directories
            foreach (string dirPath in Directory.GetDirectories(sourcePath, "*", SearchOption.AllDirectories))
            {
                Directory.CreateDirectory(dirPath.Replace(sourcePath, targetPath));
            }

            //Copy all the files & Replaces any files with the same name
            foreach (string newPath in Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories))
            {
                File.Copy(newPath, newPath.Replace(sourcePath, targetPath), true);
            }
        }
    }
}
