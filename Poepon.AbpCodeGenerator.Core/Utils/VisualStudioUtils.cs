using System;
using System.IO;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;

namespace Poepon.AbpCodeGenerator.Core.Utils
{
    internal class VisualStudioUtils
    {

        private DTE2 _dte;

        internal VisualStudioUtils()
        {
            // initialize DTE object -- the top level object for working with Visual Studio
            this._dte = Package.GetGlobalService(typeof(DTE)) as DTE2;
        }

        internal void BuildProject(Project project)
        {
            var solutionConfiguration = _dte.Solution.SolutionBuild.ActiveConfiguration.Name;
            if (project == null)
            {
                throw new NullReferenceException("project");
            }
            
            _dte.Solution.SolutionBuild.BuildProject(solutionConfiguration, project.FullName, true);
        }


    }
}
