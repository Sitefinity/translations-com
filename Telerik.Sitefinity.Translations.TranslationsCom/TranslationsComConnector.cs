using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Xml.Serialization;
using GlobalLink.Connect;
using GlobalLink.Connect.Config;
using GlobalLink.Connect.Model;
using Telerik.Sitefinity.Translations;
using Telerik.Sitefinity.Translations.Events;
using Telerik.Sitefinity.Translations.TranslationsCom;
using Telerik.Sitefinity.Translations.Xliff.Model;
using static Telerik.Sitefinity.Translations.TranslationsCom.TranslationsComConnector;

[assembly: TranslationConnector(name: TranslationsComConnector.ConnectorName,
                                connectorType: typeof(TranslationsComConnector),
                                title: TranslationsComConnector.ConnectorTitle,
                                enabled: false,
                                removeHtmlTags: false,
                                parameters: new string[] { ConfigKeyConstants.UrlKey,
                                                           ConfigKeyConstants.UsernameKey,
                                                           ConfigKeyConstants.PasswordKey,
                                                           ConfigKeyConstants.UseragentKey,
                                                           ConfigKeyConstants.ProjectKey,
                                                           ConfigKeyConstants.FileFormatKey,
                                                           ConfigKeyConstants.PrefixKey })]
namespace Telerik.Sitefinity.Translations.TranslationsCom
{
    /// <summary>
    /// The Translations.com connector. The main component that connects the Sitefinity CMS with the Translations.com API. 
    /// It uses the <see cref="GLExchange"/> component that can send and receive messages to and from the Translations.com SOAP API.
    /// </summary>
    public class TranslationsComConnector : TranslationConnectorBase
    {
        /// <summary>
        /// Gets or sets the object containing the configurations for the Project Director Connector.
        /// </summary>
        /// <value>The project director connector configuration.</value>
        protected ProjectDirectorConfig ProjectDirectorConfig { get; set; }

        /// <summary>
        /// Gets or sets the prefix value of the submission names.
        /// </summary>
        /// <value>The submission name prefix.</value>
        protected object SubmissionNamePrefix { get; set; }

        /// <summary>
        /// Gets or sets the short code (something like id) of the translations.com's project that this connector uses.
        /// </summary>
        /// <value>The translations.com project short code.</value>
        protected string TranslationsComProjectShortCode { get; set; }

        /// <summary>
        /// Gets or sets the file format name used for the translations.com porject
        /// </summary>
        protected string FileFormat { get; set; }

        #region Initialization
        /// <summary>
        /// Initializes the connector.
        /// </summary>
        /// <param name="config">The configuration parameters.</param>
        protected override void InitializeConnector(NameValueCollection config)
        {
            this.ProjectDirectorConfig = this.GetProjectDirectorConfig(config);
            this.TranslationsComProjectShortCode = config[ConfigKeyConstants.ProjectKey];
            if (string.IsNullOrEmpty(this.TranslationsComProjectShortCode))
            {
                this.ThrowConfigurationMissingError(ConfigKeyConstants.ProjectKey);
            }

            this.SubmissionNamePrefix = config[ConfigKeyConstants.PrefixKey];

            this.FileFormat = TranslationsComConnector.DefaultFileFormat;
            if (!string.IsNullOrEmpty(config[ConfigKeyConstants.FileFormatKey]))
            {
                this.FileFormat = config[ConfigKeyConstants.FileFormatKey];
            }
        }
        #endregion

        #region Send methods
        /// <inheritdoc />
        protected override bool OnSendTranslationError(Exception err, ISendTranslationTaskEvent evnt, ITranslationJobContext context)
        {
            return false;
        }

        /// <inheritdoc />
        protected override bool ProcessSendTranslationEvent(ISendTranslationTaskEvent evnt, ITranslationJobContext context, out string translationId)
        {
            GLExchange projectDirectorClient;
            if (!context.TryGetItem(CurrentClientKey, out projectDirectorClient))
            {
                projectDirectorClient = new GLExchange(this.ProjectDirectorConfig);
                context.Items[CurrentClientKey] = projectDirectorClient;
            }
                        
            var translationsComDocument = new Document
            {
                fileformat = this.FileFormat,

                // Setting the translation id as the name of the document.
                name = string.Format("{0}.xlf", evnt.TranslationId),
                sourceLanguage = evnt.ActualSourceLanguage,
                targetLanguages = new string[] { evnt.TargetLanguage }
            };

            var xliffFile = evnt.GetXliffFile();
            this.ProcessXliffFile(xliffFile);
            var root = new XliffRoot();
            root.Files.Add(xliffFile);

            var xliffSerializer = new XmlSerializer(typeof(XliffRoot));
            using (var xliffStream = new MemoryStream())
            {
                xliffSerializer.Serialize(xliffStream, root);
                xliffStream.Seek(0, SeekOrigin.Begin);
                translationsComDocument.setDataFromMemoryStream(xliffStream);
                projectDirectorClient.uploadTranslatable(translationsComDocument);
            }

            // Setting a Boolean value indicating that we successfully uploaded at least one document in the current submission.
            context.Items[DocumentUploadedKey] = true;

            translationId = translationsComDocument.name;

            return false;
        }

        /// <inheritdoc />
        protected override bool ProcessStartProjectEvent(IStartProjectTaskEvent evnt, ITranslationJobContext context, out string projectId)
        {
            projectId = string.Empty;

            var projectDirectorClient = new GLExchange(this.ProjectDirectorConfig);

            var project = projectDirectorClient.getProject(this.TranslationsComProjectShortCode);

            Submission submission = new Submission();

            submission.name = string.Format("{0}{1}-UCF-{2}", this.SubmissionNamePrefix, evnt.ProjectName, DateTime.Now.ToString("yyyy-MM-dd-hh-mm"));
            submission.project = project;
            submission.dueDate = evnt.DueDate;

            projectDirectorClient.initSubmission(submission);

            context.Items[CurrentClientKey] = projectDirectorClient;
            context.Items[StartProjectEventKey] = evnt;

            return false;
        }

        /// <inheritdoc />
        protected override bool ProcessCompleteProjectEvent(ICompleteProjectTaskEvent evnt)
        {
            // Just acknowledge event
            return true;
        }

        /// <inheritdoc />
        protected override void OnEndSendTranslationJob(ITranslationJobContext context)
        {
            GLExchange projectDirectorClient;
            if (context.TryGetItem(CurrentClientKey, out projectDirectorClient))
            {
                bool isAtLeastOneDocumentUploaded;
                if (context.TryGetItem(DocumentUploadedKey, out isAtLeastOneDocumentUploaded) && isAtLeastOneDocumentUploaded)
                {
                    var submissionTickets = projectDirectorClient.getSubmissionTickets();
                    IStartProjectTaskEvent startProjEvent;
                    if (context.TryGetItem(StartProjectEventKey, out startProjEvent))
                    {
                        projectDirectorClient.startSubmission();
                        startProjEvent.Acknowledge(submissionTickets[0]);
                    }
                }

                context.Items.Remove(CurrentClientKey);
            }

            base.OnEndSendTranslationJob(context);
        }

        #endregion

        #region Sync methods

        /// <inheritdoc />
        protected override IEnumerable<object> GetRawMessages(ITranslationSyncContext context)
        {
            var client = this.GetClient(context);

            var project = client.getProject(this.TranslationsComProjectShortCode);

            return client.getCompletedTargets(project, 50);
        }

        /// <inheritdoc />
        protected override IEnumerable<SyncEventMessage> ExtractSyncEventMessages(object rawMessage, ITranslationSyncContext context)
        {
            var client = this.GetClient(context);
            var target = rawMessage as Target;

            var xliffFile = this.GetXliffFile(client, target);
            var message = new ReviewTranslationTaskEvent(xliffFile, rawMessage);

            return new SyncEventMessage[] { message };
        }

        /// <inheritdoc />
        protected override bool AcknowledgeMessage(object rawMessage, ITranslationSyncContext context)
        {
            var client = this.GetClient(context);
            var result = client.sendDownloadConfirmation((rawMessage as Target).ticket);
            return true;
        }

        /// <inheritdoc />
        protected override void OnEndSyncTranslationJob(ITranslationSyncContext context)
        {
        }

        #endregion

        #region Inner class and structs

        /// <summary>
        /// Contains the string values of the keys used to configure the translations.com connector.
        /// </summary>
        public struct ConfigKeyConstants
        {
            /// <summary>
            /// The configuration key for the Url setting.
            /// </summary>
            public const string UrlKey = "url";

            /// <summary>
            /// The configuration key for the user name setting.
            /// </summary>
            public const string UsernameKey = "username";

            /// <summary>
            /// The configuration key for the password setting.
            /// </summary>
            public const string PasswordKey = "password";

            /// <summary>
            /// The configuration key for the user agent setting.
            /// </summary>
            public const string UseragentKey = "userAgent";

            /// <summary>
            /// The configuration key for the project short code setting.
            /// </summary>
            public const string ProjectKey = "project";

            /// <summary>
            /// The configuration key for file format.
            /// </summary>
            public const string FileFormatKey = "fileFormatProfile";

            /// <summary>
            /// The configuration key for the submission name prefix.
            /// </summary>
            public const string PrefixKey = "submissionPrefix";
        }
        #endregion

        #region Private methods

        private XliffFile GetXliffFile(GLExchange client, Target target)
        {
            using (var translatedDocumentStream = client.downloadCompletedTarget(target))
            {
                var serializer = new XmlSerializer(typeof(XliffRoot));

                var xliff = (XliffRoot)serializer.Deserialize(translatedDocumentStream);
                return xliff.Files[0];
            }
        }

        private GLExchange GetClient(ITranslationContext context)
        {
            GLExchange client;
            if (!context.TryGetItem(CurrentClientKey, out client))
            {
                client = new GLExchange(this.ProjectDirectorConfig);
                context.Items[CurrentClientKey] = client;
            }

            return client;
        }

        private void ProcessXliffFile(XliffFile file)
        {
            foreach (var unit in file.Body)
            {
                unit.Target = unit.Source;
                unit.Source = null;
            }
        }

        private ProjectDirectorConfig GetProjectDirectorConfig(NameValueCollection sitefinityConfig)
        {
            ProjectDirectorConfig projectDirectorConfig = new ProjectDirectorConfig();
            string url = sitefinityConfig[ConfigKeyConstants.UrlKey];
            if (string.IsNullOrEmpty(url))
            {
                this.ThrowConfigurationMissingError(ConfigKeyConstants.UrlKey);
            }
            else
            {
                projectDirectorConfig.url = url;
            }

            string username = sitefinityConfig[ConfigKeyConstants.UsernameKey];
            if (string.IsNullOrEmpty(username))
            {
                this.ThrowConfigurationMissingError(ConfigKeyConstants.UsernameKey);
            }
            else
            {
                projectDirectorConfig.username = username;
            }

            string password = sitefinityConfig[ConfigKeyConstants.PasswordKey];
            if (string.IsNullOrEmpty(password))
            {
                this.ThrowConfigurationMissingError(ConfigKeyConstants.PasswordKey);
            }
            else
            {
                projectDirectorConfig.password = password;
            }

            string userAgent = sitefinityConfig[ConfigKeyConstants.UseragentKey];
            if (string.IsNullOrEmpty(userAgent))
            {
                userAgent = "Sitefinity";
            }

            projectDirectorConfig.userAgent = userAgent;
            return projectDirectorConfig;
        }

        private void ThrowConfigurationMissingError(string missingConfigurationKey)
        {
            throw new Exception(string.Concat("Configuration option: '", missingConfigurationKey, "' for the Translations.Com connector is not set"));
        }
        #endregion

        #region Fields & constants
        internal const string ConnectorName = "TranslationsCom";
        internal const string ConnectorTitle = "Export to Translations.com";

        private const string DefaultFileFormat = "XLIFF";
        private const string CurrentClientKey = "projectDirectorClient";
        private const string DocumentUploadedKey = "isTranslationComDocumentUploaded";
        private const string StartProjectEventKey = "startProjectEvent";
        #endregion
    }
}