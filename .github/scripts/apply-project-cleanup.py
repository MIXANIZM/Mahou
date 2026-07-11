#!/usr/bin/env python3
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
PROJECT = ROOT / "Mahou/Mahou.csproj"


def replace_eol(old, new, expected=1):
    data = PROJECT.read_bytes()
    old_lf = old.encode("utf-8")
    new_lf = new.encode("utf-8")
    old_crlf = old_lf.replace(b"\n", b"\r\n")
    new_crlf = new_lf.replace(b"\n", b"\r\n")
    count_lf = data.count(old_lf)
    count_crlf = data.count(old_crlf)
    count = count_lf + count_crlf
    if count != expected:
        raise RuntimeError(
            "Mahou.csproj: expected %d occurrence(s), found %d (LF=%d, CRLF=%d) for %r"
            % (expected, count, count_lf, count_crlf, old)
        )
    if count_crlf:
        data = data.replace(old_crlf, new_crlf, expected)
    else:
        data = data.replace(old_lf, new_lf, expected)
    PROJECT.write_bytes(data)


replace_eol(
    "    <IsWebBootstrapper>false</IsWebBootstrapper>\n"
    "    <SignAssembly>False</SignAssembly>\n"
    "    <DelaySign>False</DelaySign>\n"
    "    <RunPostBuildEvent>OnBuildSuccess</RunPostBuildEvent>\n"
    "    <AllowUnsafeBlocks>False</AllowUnsafeBlocks>\n"
    "    <NoStdLib>False</NoStdLib>\n"
    "    <TreatWarningsAsErrors>False</TreatWarningsAsErrors>\n"
    "    <IntermediateOutputPath>obj\\$(Configuration)\\</IntermediateOutputPath>\n"
    "    <WarningLevel>4</WarningLevel>\n"
    "    <RunCodeAnalysis>False</RunCodeAnalysis>\n"
    "    <SourceAnalysisOverrideSettingsFile>C:\\Users\\BladeMight\\AppData\\Roaming\\ICSharpCode\\SharpDevelop5\\Settings.SourceAnalysis</SourceAnalysisOverrideSettingsFile>\n"
    "    <PublishUrl>опубликовать\\</PublishUrl>\n"
    "    <Install>true</Install>\n"
    "    <InstallFrom>Disk</InstallFrom>\n"
    "    <UpdateEnabled>false</UpdateEnabled>\n"
    "    <UpdateMode>Foreground</UpdateMode>\n"
    "    <UpdateInterval>7</UpdateInterval>\n"
    "    <UpdateIntervalUnits>Days</UpdateIntervalUnits>\n"
    "    <UpdatePeriodically>false</UpdatePeriodically>\n"
    "    <UpdateRequired>false</UpdateRequired>\n"
    "    <MapFileExtensions>true</MapFileExtensions>\n"
    "    <ApplicationRevision>0</ApplicationRevision>\n"
    "    <ApplicationVersion>1.0.0.%2a</ApplicationVersion>\n"
    "    <UseApplicationTrust>false</UseApplicationTrust>\n"
    "    <BootstrapperEnabled>true</BootstrapperEnabled>\n"
    "    <NoWin32Manifest>False</NoWin32Manifest>\n"
    "    <DebugType>Full</DebugType>\n",
    "    <SignAssembly>false</SignAssembly>\n"
    "    <DelaySign>false</DelaySign>\n"
    "    <AllowUnsafeBlocks>false</AllowUnsafeBlocks>\n"
    "    <NoStdLib>false</NoStdLib>\n"
    "    <TreatWarningsAsErrors>false</TreatWarningsAsErrors>\n"
    "    <IntermediateOutputPath>obj\\$(Configuration)\\</IntermediateOutputPath>\n"
    "    <WarningLevel>4</WarningLevel>\n"
    "    <RunCodeAnalysis>false</RunCodeAnalysis>\n"
    "    <BootstrapperEnabled>false</BootstrapperEnabled>\n"
    "    <NoWin32Manifest>false</NoWin32Manifest>\n",
)
replace_eol(
    "  <PropertyGroup>\n"
    "    <ApplicationIcon>Mahou.ico</ApplicationIcon>\n"
    "  </PropertyGroup>\n"
    "  <PropertyGroup>\n"
    "    <TargetZone>LocalIntranet</TargetZone>\n"
    "  </PropertyGroup>\n"
    "  <PropertyGroup>\n"
    "    <GenerateManifests>false</GenerateManifests>\n"
    "  </PropertyGroup>\n"
    "  <PropertyGroup />\n",
    "  <PropertyGroup>\n"
    "    <ApplicationIcon>Mahou.ico</ApplicationIcon>\n"
    "    <ApplicationManifest>app.manifest</ApplicationManifest>\n"
    "  </PropertyGroup>\n",
)
replace_eol(
    "  <PropertyGroup Condition=\" '$(Configuration)' == 'Release' \">\n"
    "    <Optimize>True</Optimize>\n"
    "    <DebugSymbols>false</DebugSymbols>\n"
    "    <DebugType>None</DebugType>\n"
    "    <Deterministic>true</Deterministic>\n",
    "  <PropertyGroup Condition=\" '$(Configuration)' == 'Release' \">\n"
    "    <Optimize>true</Optimize>\n"
    "    <DebugSymbols>false</DebugSymbols>\n"
    "    <DebugType>None</DebugType>\n"
    "    <Deterministic>true</Deterministic>\n"
    "    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>\n",
)
replace_eol(
    "    <Reference Include=\"System\">\n"
    "      <RequiredTargetFramework>4.0</RequiredTargetFramework>\n"
    "    </Reference>\n"
    "    <Reference Include=\"System.Drawing\">\n"
    "      <RequiredTargetFramework>4.0</RequiredTargetFramework>\n"
    "    </Reference>\n"
    "    <Reference Include=\"System.Net\">\n"
    "      <RequiredTargetFramework>4.0</RequiredTargetFramework>\n"
    "    </Reference>\n",
    "    <Reference Include=\"System\" />\n"
    "    <Reference Include=\"System.Drawing\" />\n"
    "    <Reference Include=\"System.Net\" />\n",
)
replace_eol(
    "  <ItemGroup>\n"
    "    <None Include=\"App.config\" />\n",
    "  <ItemGroup>\n"
    "    <None Include=\"App.config\" />\n"
    "    <None Include=\"app.manifest\" />\n",
)
replace_eol(
    "  <ItemGroup>\n"
    "    <BootstrapperPackage Include=\".NETFramework,Version=v4.5\">\n"
    "      <Visible>False</Visible>\n"
    "      <ProductName>Microsoft .NET Framework 4.5 %28x86 и x64%29</ProductName>\n"
    "      <Install>true</Install>\n"
    "    </BootstrapperPackage>\n"
    "    <BootstrapperPackage Include=\"Microsoft.Net.Client.3.5\">\n"
    "      <Visible>False</Visible>\n"
    "      <ProductName>Клиентский профиль .NET Framework 3.5 SP1</ProductName>\n"
    "      <Install>false</Install>\n"
    "    </BootstrapperPackage>\n"
    "    <BootstrapperPackage Include=\"Microsoft.Net.Framework.3.5.SP1\">\n"
    "      <Visible>False</Visible>\n"
    "      <ProductName>.NET Framework 3.5 SP1</ProductName>\n"
    "      <Install>false</Install>\n"
    "    </BootstrapperPackage>\n"
    "  </ItemGroup>\n",
    "",
)

Path(__file__).unlink()
print("Cleaned obsolete local/ClickOnce/bootstrapper project metadata and enabled explicit Release quality gates.")
