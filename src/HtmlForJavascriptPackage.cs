using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace HtmlForJavascript
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid("0e5590ff-fcc4-4c8c-a4d7-b7bd17ddede4")]
    public sealed class HtmlForJavascriptPackage : AsyncPackage
    {
    }
}
