using System;
using System.Diagnostics;
using System.Reflection;

using NUnit.Framework;


namespace Tableau.HyperAPI.Test
{
    class VersionTests
    {
        [Test]
        public void TestAssemblyVersion()
        {
            // Tests if we actually baked a hapi version into our dll.
            var hapiAssembly = Assembly.GetAssembly(typeof(HyperProcess));
            var hapiFileVersion = FileVersionInfo.GetVersionInfo(hapiAssembly.Location);
            var hapiVersion = Assembly.GetAssembly(typeof(HyperProcess)).GetName().Version;
            Assert.AreEqual(hapiVersion.Major, 0);
            Assert.AreEqual(hapiVersion.Minor, 0);
            Assert.AreEqual(hapiVersion.Major, hapiFileVersion.FileMajorPart);
            Assert.AreEqual(hapiVersion.Minor, hapiFileVersion.FileMinorPart);
            Assert.AreEqual(hapiVersion.Build, hapiFileVersion.FileBuildPart);
#if HAPI_SHOULD_BE_VERSIONED
            // We already have > 10k commits, so the build number should be larger
            // than that if we use versioning
            Assert.GreaterOrEqual(hapiVersion.Build, 10000);
#else
            Assert.AreEqual(hapiVersion.Build, 0);
#endif
        }
    }
}