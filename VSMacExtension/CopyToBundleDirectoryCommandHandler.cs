using System.IO;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using MonoDevelop.Projects;

namespace VSMacExtension
{
    public class OpenPluginsDirectoryCommandHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            if (Helpers.IsVMacSolution() && IdeApp.ProjectOperations.CurrentSelectedItem is Project)
            {
                if (Helpers.GetVisualStudioProject() != null)
                {
                    info.Text = $"Open VS Bundle Addins folder ...";
                    info.Visible = info.Enabled = true;
                }
                else
                {
                    info.Visible = true;
                    info.Enabled = false;
                    info.Text = "VisualStudio project not found";
                }
               return;
            }
            info.Visible = info.Enabled = false;
        }

        protected override void Run()
        {
            var addinsDirectory = Helpers.GetBundleAddinsDirectory();
            if (!Directory.Exists(addinsDirectory))
            {
                MessageService.ShowMessage($"Error VS Addins Directory in path '{addinsDirectory}' not found"); 
            }
            IdeServices.DesktopService.OpenFolder(addinsDirectory);
        }
    }

    public class OpenPluginDirectoryCommandHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            if (Helpers.IsVMacSolution() && IdeApp.ProjectOperations.CurrentSelectedItem is DotNetProject project)
            {
                var destinationDirectory = Helpers.GetDestinationProjectAddinDirectory(project);
                if (Directory.Exists(destinationDirectory))
                {
                    info.Text = $"Open '{project.Name}' Addin output directory...";
                    info.Visible = info.Enabled = true;
                }
                else
                {
                    info.Visible = true;
                    info.Enabled = false;
                    info.Text = $"'{project.Name}' Addin is not copied to the Bundle";
                }
            }
            else
            {
                info.Visible = info.Enabled = false;
            }
        }

        protected override void Run()
        {
            var vsmacProject = Helpers.GetVisualStudioProject();
            if (vsmacProject == null)
            {
                MessageService.ShowMessage("Error. VisualStudio project not found");
                return;
            }

            if (IdeApp.ProjectOperations.CurrentSelectedItem is DotNetProject project)
            {
                var directory = Helpers.GetDestinationProjectAddinDirectory(project);
                if (!Directory.Exists(directory))
                {
                    MessageService.ShowMessage($"Error. addin directory path '{directory}' not found");
                }
                IdeServices.DesktopService.OpenFolder(directory);
            }
        }
    }

    public class CopyToBundleDirectoryCommandHandler : CommandHandler
    {
        protected override void Update(CommandInfo info)
        {
            if (Helpers.IsVMacSolution() && IdeApp.ProjectOperations.CurrentSelectedItem is DotNetProject project)
            {
                var destinationDirectory = Helpers.GetDestinationProjectAddinDirectory(project);

                if (Directory.Exists(destinationDirectory))
                {
                    var directory = Helpers.GetDestinationProjectAddinDirectory(project);
                    var addin = Path.GetFileNameWithoutExtension(directory);
                    info.Text = $"Copy '{addin}' Addin output files to Bundle...";
                    info.Visible = info.Enabled = true;
                }
                else
                {
                    info.Visible = true;
                    info.Enabled = false;
                    info.Text = "This Addin is not copied to the Bundle";
                }
            }
            else
            {
                info.Visible = info.Enabled = false;
            }
        }

        protected override void Run()
        {
            var vsmacProject = Helpers.GetVisualStudioProject();
            if (vsmacProject == null)
            {
                MessageService.ShowMessage("Error. VisualStudio project not found");
                return;
            }

            if (IdeApp.ProjectOperations.CurrentSelectedItem is DotNetProject selectedProject)
            {
                var sourceDirectory = Helpers.GetOriginProjectAddinDirectory(selectedProject);
                if (!System.IO.Directory.Exists(sourceDirectory))
                {
                    MessageService.ShowMessage($"Source directory: {sourceDirectory} doesn't exists");
                    return;
                }

                var destinationDirectory = Helpers.GetDestinationProjectAddinDirectory(selectedProject);
                if (!System.IO.Directory.Exists(destinationDirectory))
                {
                    MessageService.ShowMessage($"Output directory: {destinationDirectory} doesn't exists");
                    return;
                }
              
                Helpers.CopyFilesRecursively(sourceDirectory, destinationDirectory);
            }
        }
       
    }
}
