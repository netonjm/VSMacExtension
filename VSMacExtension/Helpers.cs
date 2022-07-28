using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace VSMacExtension
{
    static class Helpers
    {
        public static string GetBundleMonoBundleDirectory()
        {
            var solution = IdeApp.ProjectOperations.CurrentSelectedSolution;
            var outputdir = solution.OutputDirectory.Replace("\\", "/");
            var vsProject = GetVisualStudioProject();
            var appBundlePath = Path.Combine(outputdir, vsProject.Name + ".app", "Contents", "MonoBundle");
            return appBundlePath;
        }

        public static string GetBundleAddinsDirectory ()
        {
            var bundle = GetBundleMonoBundleDirectory();
            var appBundlePath = Path.Combine(bundle, "AddIns");
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

        static int ComparePaths (string path1, string path2)
        {
            return string.Compare(
    Path.GetFullPath(path1).TrimEnd(Path.DirectorySeparatorChar),
    Path.GetFullPath(path2).TrimEnd(Path.DirectorySeparatorChar),
    StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool IsAddinProject(DotNetProject selectedProject)
        {
            var outputFile = selectedProject.GetOutputFileName(IdeApp.Workspace.ActiveConfiguration).ParentDirectory.FullPath;
            var outputDirectory = GetOutputDirectory();
            return ComparePaths(outputFile.FullPath, outputDirectory) != 0;
        }

        public static string GetDestinationProjectAddinDirectory(DotNetProject selectedProject)
        {
            string destinationDirectory;
            if (IsAddinProject(selectedProject))
            {
                //addins
                var localAddinsDirectory = GetOriginAddinsDirectory();

                var outputFile = selectedProject.GetOutputFileName(IdeApp.Workspace.ActiveConfiguration).ParentDirectory.FullPath;
                var relativeDirectoryPath = MakeRelativePath(localAddinsDirectory, outputFile);
                if (relativeDirectoryPath.StartsWith("AddIns/"))
                    relativeDirectoryPath = relativeDirectoryPath.Substring("AddIns/".Length);

                var addinsDirectory = GetBundleAddinsDirectory();
                destinationDirectory = Path.Combine(addinsDirectory, relativeDirectoryPath);
              
            }
            else
            {
                //is not an addins project
                destinationDirectory = GetBundleMonoBundleDirectory();
            }
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
