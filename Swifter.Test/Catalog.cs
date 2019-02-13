using System.Collections.Generic;

namespace Swifter.Test
{
    public class Catalog

    {

        public string manifestVersion { get; set; }

        public string engineVersion { get; set; }

        public Info info { get; set; }

        public Package[] packages { get; set; }

        public Dictionary<string, string> deprecate { get; set; }

        public Signature signature { get; set; }



        public class Package

        {

            public string id { get; set; }

            public string version { get; set; }

            public string name { get; set; }

            public string type { get; set; }

            public bool isUiGroup { get; set; }

            public string chip { get; set; }

            public string language { get; set; }

            public Payload[] payloads { get; set; }

            public VsInfo _vsInfo { get; set; }

            public string vsixId { get; set; }

            public string extensionDir { get; set; }

            public Dictionary<string, string> folderMappings { get; set; }

            public Dictionary<string, string> msiProperties { get; set; }

            public string productCode { get; set; }

            public string upgradeCode { get; set; }

            public string productVersion { get; set; }

            public int productLanguage { get; set; }

            public bool visible { get; set; }

            public string relativePath { get; set; }

            public DetectConditions detectConditions { get; set; }

            public Params installParams { get; set; }

            public Params repairParams { get; set; }

            public Params uninstallParams { get; set; }

            public Params initInstallParams { get; set; }

            public Params initRepairParams { get; set; }

            public Params initUninstallParams { get; set; }

            public Params layoutParams { get; set; }

            public Params layoutInstallParams { get; set; }

            public Params launchParams { get; set; }

            public Icon icon { get; set; }

            public string defaultInstallDirectory { get; set; }

            public string releaseNotes { get; set; }

            public string thirdPartyNotices { get; set; }

            public bool recommendSelection { get; set; }

            public Dictionary<string, ReturnCode> returnCodes { get; set; }

            public string providerKey { get; set; }

            public FileAssociation[] fileAssociations { get; set; }

            public LogFile[] logFiles { get; set; }

            public ProgId[] progIds { get; set; }

            public Shortcut[] shortcuts { get; set; }

            public Sizes installSizes { get; set; }

            public UrlAssociation[] urlAssociations { get; set; }

            public bool permanent { get; set; }

            public DefaultProgram defaultProgram { get; set; }

            public string license { get; set; }

            public Dictionary<string, object> dependencies { get; set; }

            public BreadcrumbTemplate breadcrumbTemplate { get; set; }

            public ProjectClassifier[] projectClassifiers { get; set; }

            public Requirements requirements { get; set; }

            public LocalizedResource[] localizedResources { get; set; }



            public class Payload

            {

                public string fileName { get; set; }

                public string sha256 { get; set; }

                public long size { get; set; }

                public string url { get; set; }

                public Cookie cookie { get; set; }

                public bool cache { get; set; }



                public class Cookie

                {

                    public string url { get; set; }

                    public string value { get; set; }

                }

            }



            public class VsInfo

            {

                public bool ignoreChanges { get; set; }

            }



            public class DetectConditions

            {

                public string expression { get; set; }

                public Condition[] conditions { get; set; }



                public class Condition

                {

                    public string registryKey { get; set; }

                    public string id { get; set; }

                    public string join { get; set; }

                    public string registryValue { get; set; }

                    public string registryType { get; set; }

                    public string registryData { get; set; }

                    public string filePath { get; set; }

                    public string chip { get; set; }

                    public string productCode { get; set; }

                }

            }



            public class Params

            {

                public string fileName { get; set; }

                public string parameters { get; set; }

            }



            public class Icon

            {

                public string mimeType { get; set; }

                public string base64 { get; set; }

            }



            public class ReturnCode

            {

                public int returnCode { get; set; }

                public string type { get; set; }

                public string details { get; set; }

                public string watson { get; set; }

                public int messageId { get; set; }

                public string message { get; set; }

            }



            public class Sizes

            {

                public long systemDrive { get; set; }

                public long sharedDrive { get; set; }

                public long targetDrive { get; set; }

            }



            public class UrlAssociation

            {

                public string protocol { get; set; }

                public string displayName { get; set; }

                public string progId { get; set; }

                public string defaultProgramRegistrationPath { get; set; }

            }



            public class DefaultProgram

            {

                public string id { get; set; }

                public string registrationPath { get; set; }

                public string name { get; set; }

                public string descriptionPath { get; set; }

                public int descriptionPosition { get; set; }

            }



            public class LogFile

            {

                public string directory { get; set; }

                public string pattern { get; set; }

            }



            public class FileAssociation

            {

                public string extension { get; set; }

                public string progId { get; set; }

                public string defaultProgramRegistrationPath { get; set; }

                public string perceivedType { get; set; }

                public string contentType { get; set; }

                public bool isIconOnly { get; set; }

            }



            public class ProgId

            {

                public string id { get; set; }

                public string displayName { get; set; }

                public bool alwaysShowExtension { get; set; }

                public string path { get; set; }

                public string arguments { get; set; }

                public string clsid { get; set; }

                public string defaultIconPath { get; set; }

                public long defaultIconPosition { get; set; }

                public bool dde { get; set; }

                public string ddeApplication { get; set; }

                public string ddeTopic { get; set; }

                public string iconHandler { get; set; }

                public string appUserModelId { get; set; }

            }



            public class Shortcut

            {

                public string description { get; set; }

                public string targetPath { get; set; }

                public Dictionary<string, string> shellProperties { get; set; }

                public string workingDirectory { get; set; }

                public string arguments { get; set; }

                public string displayName { get; set; }

                public string folder { get; set; }

            }



            public class Requirements

            {

                public Functors functors { get; set; }

                public string supportedOS { get; set; }

                public DetectConditions conditions { get; set; }

                public bool hyperVRequired { get; set; }



                public class Functors

                {

                    public string architecture { get; set; }

                }

            }



            public class ProjectClassifier

            {

                public string id { get; set; }

                public string extension { get; set; }

                public string factoryGuid { get; set; }

                public int priority { get; set; }

                public string[] selects { get; set; }

                public Matcher[] matcherData { get; set; }

                public string matcherId { get; set; }

                public string appliesTo { get; set; }



                public class Matcher

                {

                    public string type { get; set; }

                    public string capabilityType { get; set; }

                    public string projectPropertyId { get; set; }

                    public string regExMatchSource { get; set; }

                }



            }



            public class LocalizedResource

            {

                public string language { get; set; }

                public string title { get; set; }

                public string description { get; set; }

                public string longDescription { get; set; }

                public string category { get; set; }

                public string[] keywords { get; set; }

                public string license { get; set; }

            }



            public class BreadcrumbTemplate

            {

                public Template[] templates { get; set; }

                public LocalizedResource[] localizedResources { get; set; }



                public class Template

                {

                    public string id { get; set; }

                    public string projectSubTypeSortOrder { get; set; }

                    public int projectSortOrder { get; set; }

                    public string projectSubType { get; set; }

                    public string projectType { get; set; }

                    public string[] selects { get; set; }

                }

                public class LocalizedResource

                {

                    public string templateId { get; set; }

                    public string projectTypeDisplayName { get; set; }

                    public string projectSubTypeDisplayName { get; set; }

                    public string language { get; set; }

                    public string title { get; set; }

                    public string description { get; set; }

                }

            }

        }

        public class Info

        {

            public string id { get; set; }

            public string buildBranch { get; set; }

            public string buildVersion { get; set; }

            public string localBuild { get; set; }

            public string manifestName { get; set; }

            public string manifestType { get; set; }

            public string productDisplayVersion { get; set; }

            public string productLine { get; set; }

            public string productLineVersion { get; set; }

            public string productMilestone { get; set; }

            public string productMilestoneIsPreRelease { get; set; }

            public string productName { get; set; }

            public string productPatchVersion { get; set; }

            public string productPreReleaseMilestoneSuffix { get; set; }

            public string productRelease { get; set; }

            public string productSemanticVersion { get; set; }

        }

        public class Signature

        {

            public SignInfo signInfo { get; set; }

            public string signatureValue { get; set; }

            public KeyInfo keyInfo { get; set; }

            public CounterSign counterSign { get; set; }



            public class SignInfo

            {

                public string signatureMethod { get; set; }

                public string digestMethod { get; set; }

                public string digestValue { get; set; }

                public string canonicalization { get; set; }

            }



            public class KeyInfo

            {

                public KeyValue keyValue { get; set; }

                public string[] x509Data { get; set; }



                public class KeyValue

                {

                    public RsaKeyValue rsaKeyValue { get; set; }

                    public class RsaKeyValue

                    {

                        public string modulus { get; set; }

                        public string exponent { get; set; }

                    }

                }

            }



            public class CounterSign

            {

                public string[] x509Data { get; set; }

                public string timestamp { get; set; }

                public string counterSignatureMethod { get; set; }

                public string counterSignature { get; set; }

            }

        }

    }
}
