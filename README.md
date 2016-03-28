# Telerik.Sitefinity.Translations.TranslationsCom
When working with the Sitefinity *Translation* module, you can benefit from a number of translation connectors that you use out-of-the-box with minimum setup. You can, however, implement your own translation connector with custom logic to serve your requirements. 

This tutorial provides you with a sample that you use to implement a custom translation connector to work with the **Translations.com** service. You first create and setup the connector and then use the *Translation* API to implement the overall translation process.   
## Requirements
Sitefinity 8.1 Enterprise license

## Prerequisites

Your Sitefinity Web site must be in multilingual mode meaning that you have added atleast one additinal language to the current website you are browsing. Otherwise you will not see the translations screen in the administrations tab of your application.

You should either use country specific languages like 'en-US' and not just 'en' or specify a mapping between the country invariant and country specific language in the translations advanced settings screen: <i>Administration >> Settings >> Advanced >> Culture mappings </i> text box.

The Translations.com project that you are using must be configured to translate XLIFF file format.
The Translations.com project that you are using must be configured for the language translations that you wish to achieve like en-US -> fr-FR; en-us -> de-DE.

Add the *Translation* sample project to your solution. To do this:

1. In Visual Studio, open your Sitefinity web application solution.
2. In the **SitefinityWebApp**, add a reference to the <strong>Telerik.Sitefinity.Translations.TranslationsCom</strong> assembly.


## Create and configure the connector

To configure the *Translation.com* connector in Sitefinity:

1. Navigate to <i>Administration >> Settings >> Advanced >> Translations >> Connectors >> Create new</i>.
2. In <i>Connector name</i>, enter <strong>Translations.com</strong>.
3. In <i>Connector title</i>, enter <strong>Translations.com</strong>.
4. In <i>Connector type</i>, enter <strong>Telerik.Sitefinity.Translations.TranslationsCom.TranslationsComConnector</strong>.
5. To enable the connector, in the <i>Enabled</i> field, enter <strong>true</strong>.
6. Save your changes.
7. Expand the <i>Parameters</i> section of the newly created connector and create the following <i>Keys</i>: 
   **NOTE:** The following parametersm except for the last one, must be provided by Translations.com.
 * <strong>url</strong> </br>In <i>Value</i>, enter the URL of the connector
 * <strong>username</strong> </br>In <i>Value</i>, enter the username
 * <strong>password</strong> </br>In <i>Value</i>, enter the password
 * <strong>userAgent</strong> </br>In <i>Value</i>, enter the agent
 * <strong>project</strong> </br>In <i>Value</i>, enter the name of the project
 * <strong>fileFormatProfile</strong> </br>In <i>Value</i>, enter the file format that the connector accepts. For example, enter **XLIFF**.
 * <strong>submissionPrefix</strong> </br>In <i>Value</i>, enter the prefix for the translation submission name that is generated and sent to the connector.

##API Overview: TranslationsComConnector
The <strong>TranslationsComConnector</strong> class has properties that hold information about the connector. The following table summarizes these API properties.

**NOTE:** In this tutorial, you do not use all of the methods listed below. You can use the table below as reference, as well.
#### TranslationsCom specific properties
<table>
	<thead>
		<tr>
			<td><strong>Type</strong></td>
			<td><strong>Property</strong></td>
			<td><strong>Description</strong></td>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td><strong><code>ProjectDirectorConfig</code></strong></td>
			<td><strong>ProjectDirectorConfig</strong></td>
			<td>
				<p>
					Object containing the configurations for the <i>Project Director Connector</i>.
				</p>
			</td>
		</tr>
		<tr>
			<td><strong><code>object</code></strong></td>
			<td><strong>SubmissionNamePrefix</strong></td>
			<td>
				<p>
					The prefix value of the submission names.
				</p>
			</td>
		</tr>
		<tr>
			<td><strong><code>string</code></strong></td>
			<td><strong>TranslationsComProjectShortCode</strong></td>
			<td>
				<p>
					The short code of the <i>translations.com</i> project that this connector uses.
				</p>
			</td>
		</tr>
	</tbody>
</table>


The <strong>TranslationsComConnector</strong> class has several overriden methods from the <strong>TranslationConnectorBase</strong>. The following tables provides more details about each method. 

**Initialization methods**
<table>
	<thead>
		<tr>
			<td><strong>Type</strong></td>
			<td><strong>Method</strong></td>
			<td><strong>Description</strong></td>
            <td><strong>Required</strong></td>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td><strong><code>void</code></strong></td>
			<td><strong>InitializeConnector</strong></td>
			<td>
				<p>
					Initializes the connector. The parameter is a <code>NameValueCollection</code> object with TranslationsCom specific configuration parameters.
				</p>
			</td>
            <td>
				Yes
            </td>
		</tr>
	</tbody>
</table>

**Send methods**
Methods below are listed in the order that they are called when the entire translation project is sent to the external translation agency.
<table>
	<thead>
		<tr>
			<td><strong>Type</strong></td>
			<td><strong>Method</strong></td>
			<td><strong>Description</strong></td>
			<td><strong>Required</strong></td>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td><strong><code>void</code></strong></td>
			<td><strong>OnInitTranslationJobContext</strong></td>
			<td>
				<p>
Override this method if you need to set initial values to the job context.
				</p>
			</td>
			<td>
				No
			</td>
		</tr>
		<tr>
			<td><strong><code>bool</code></strong></td>
			<td><strong>ProcessStartProjectEvent</strong></td>
			<td>
				<p>
					You use this method for processing the <code>IStartProjectTaskEvent</code> that informs the translation agenecy a new project started. The last parameter of the method stores the external ID for the project that is later used to associate the translations project with the Sitefinity project. Upon the event of succesfully sent message, the method returns **true**.
				</p>
			</td>
			<td>
				Yes
            </td>
		</tr>
<tr>
			<td><strong><code>bool</code></strong></td>
			<td><strong>ProcessCompleteProjectEvent</strong></td>
			<td>
				<p>
					You use this method for processing the <code>ICompleteProjectTaskEvent</code> that informs the translation agency the translation project is complete. pon the event of succesfully sent message, the method returns **true**.
				</p>
			</td>
			<td>
				Yes
            </td>
		</tr>
		<tr>
			<td><strong><code>bool</code></strong></td>
			<td><strong>OnStartProjectError</strong></td>
			<td>
				<p>
				       This method is called if any error occurs when sending the message for starting the project. The following parameters are passed to the method:
<ul>
<li>The exception returned from the connector</li>
<li><code>IStartProjectTaskEvent</code></li>
<li>The translation job context</li>
</ul>

If the exception is handled in the method, the method returns **false**. Otherwise, the method returns **true**.
				</p>
			</td>
			<td>
				No
            </td>
		</tr>
		<tr>
			<td><strong><code>bool</code></strong></td>
			<td><strong>ProcessSendTranslationEvent</strong></td>
			<td>
				<p>
					You use this method for processing the translation sent to the external translation service. The following parameters are passed to the method:
<ul>
<li><code>ISendTranslationTaskEvent</code></li> containing information about the translation to be sent
<li><code>ITranslationJobContext</code> job context.</li>
<li><code>translationId</code> store the external ID for the translation that is later used to associate the translated message with the Sitefinity translation.</li>
</ul>
If the method has successfully sent the translation, it returns **true**. 
					This method checks if the process of sending the translation is successful. 
				</p>
			</td>
			<td>
				Yes
            </td>
		</tr>
		<tr>
			<td><strong><code>bool</code></strong></td>
			<td><strong>OnSendTranslationError</strong></td>
			<td>
				<p>
					This method is called if any error occures when sending the tranlation message. The following parameters are passed to the method:
<ul>
<li>The exception that is returned from the connector</li>
<li>The send translation task event</li>
<li>The translation job context that is shared between messages sent through the translation connector.</li>
</ul>
If the exception is handled in the method, the method returns **false**, otherwise the method returns **true**.
				</p>
			</td>
			<td>
				No
            </td>
		</tr>
		<tr>
			<td><strong><code>void</code></strong></td>
			<td><strong>OnEndSendTranslationJob</strong></td>
			<td>
				<p>
					This method is called after all messages are processed. The parameter of the method is the translation job context.
				</p>
			</td>
			<td>
				No
            </td>
		</tr>
	</tbody>
</table>

**Sync methods**
Methods below are listed in the order, in which they are called during the synchronization of the translations:
<table>
	<thead>
		<tr>
			<td><strong>Type</strong></td>
			<td><strong>Method</strong></td>
			<td><strong>Description</strong></td>
			<td><strong>Required</strong></td>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td><strong><code>IEnumerable&#60;object&#62;</code></strong></td>
			<td><strong>GetRawMessages</strong></td>
			<td>
				<p>
					This method gets the raw messages provided by the external translation agency. The messages can be of any kind.
				</p>
			</td>
			<td>
				Yes
            </td>
		</tr>
		<tr>
			<td><strong><code>IEnumerable&#60;SyncEventMessage&#62;</code></strong></td>
			<td><strong>ExtractSyncEventMessages</strong></td>
			<td>
				<p>
This method is called for every raw message returned by the <code>GetRawMessages</code>. For every raw message, one or more translation events can be returned.
				</p>
			</td>
			<td>
				Yes
            </td>
		</tr>
		<tr>
			<td><strong><code>bool</code></strong></td>
			<td><strong>AcknowledgeMessage</strong></td>
			<td>
				<p>
Send a confirmation message to the connector for the succesfull delivery of the translation, so that the status of the translation is updated from completed to delivered. If the delivery is successful, the cofirmation returns **true**.
				</p>
			</td>
			<td>
				Yes
            </td>
		</tr>
		<tr>
			<td><strong><code>void</code></strong></td>
			<td><strong>OnEndSyncTranslationJob</strong></td>
			<td>
				<p>
					This method is called when the synchronization with the connector is finished.
				</p>
			</td>
			<td>
				Yes
			</td>
		</tr>
	</tbody>
</table>

**Used types**

<table>
	<thead>
		<tr>
			<td><strong>Type</strong></td>
			<td><strong>Description</strong></td>
		</tr>
	</thead>
	<tbody>
		<tr>
			<td><strong><code>ISendTranslationTaskEvent</code></strong></td>
			<td>
				<p>
					Contains the information about the translation job, source, and target languages. Holds a reference to the generated XLIFF file.
				</p>
			</td>
		</tr>
	<tr>
			<td><strong><code>ITranslationJobContext</code></strong></td>
			<td>
				<p>
					Provides contextual information about the current project. Items can be stored in the context and used during the send process execution.
				</p>
			</td>
		</tr>
	<tr>
			<td><strong><code>IStartProjectTaskEvent</code></strong></td>
			<td>
				<p>
					A message that is sent to the translation agency. The message contains meta information such as *StartDate*, *EndDate*, and the name of the project. 
				</p>
			</td>
		</tr>
	<tr>
			<td><strong><code>ICompleteProjectTaskEvent</code></strong></td>
			<td>
				<p>
					Contains references to the translation and to the project.
				</p>
			</td>
		</tr>
	<tr>
			<td><strong><code>ITranslationSyncContext</code></strong></td>
			<td>
				<p>
					Provides contextual information for the current project. Items can be stored in the context and are available during the sync execution.				
				</p>
			</td>
		</tr>
	<tr>
			<td><strong><code>SyncEventMessage</code></strong></td>
			<td>
				<p>
					This message contains the raw message that comes from the connector. The message is used to acknowledge the acceptance of the message from the external translation agency.
				</p>
			</td>
		</tr>
	</tbody>
</table>

