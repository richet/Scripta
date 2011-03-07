Css & JavaScript manager for handling multiple deployment scenarios with .Net MVC
Scripta makes it easy to run the uncompressed versions of scripts when debugging locally
and then switching in the compressed versions when deployed.

Group relevant css and js files into bundles and add them to the Scripta config

Then you can do things like this to include both local and CDN versions of jQuery

<%=Html.Scripta("jquery").Js()%>


##To install & configure##

	Drop the Scripta.cs into your .Net MVC or class library project
	
	Scripta uses its own section in the web.config so add the following section to your web.config
	
	The scripts listed in the *debug* section will be used if debug symbols are loaded otherwise it uses the prod scripts.
	
		
	<configuration>
	   <configSections>
	      <sectionGroup name="scripta">
	         <section name="scripts" type="Scripta.ScriptsConfig, MyClassLibDllName"/>
	      </sectionGroup>
	   <configSections>
	   <scripta>
	      <scripts>
	         <bundles>
	            <bundle name="my-page">
	               <debug>
	                  <file src="/my-mvc-site/static/css/ui.css"/>
	                  <file src="/my-mvc-site/static/css/common.css"/>
	                  <file src="/my-mvc-site/static/js/blah.js"/>
	                  <file src="/my-mvc-site/static/js/common.js"/>
	               </debug>
	               <prod>
	                  <file src="http://www.my-mvc-site.com/release/my-page.css"/>
	                  <file src="http://www.my-mvc-site.com/release/my-page-min.js"/>							
	               </prod>
	            </bundle>
	            <bundle name="jquery">
	               <debug>
	                  <file src="/my-mvc-site/static/js/jquery-1.5.1.js"/>
	               </debug>
	               <prod>
	                  <file src="https://ajax.googleapis.com/ajax/libs/jquery/1.5.1/jquery.min.js"/>						
	               </prod>
	            </bundle>					
	            ...
	         </bundles>
	      </scripts>	
	   </scripta>		
	</configuration>	
	
##Scripta in action##

	Scripta is implemented as an extension to HtmlHelper.

	Import the Scripta namespace at the top of your MVC view or add <add namespace="Scripta" /> to the *pages* section of your web.config
	
	To include script bundles in your views or masterpages etc
	
	<head>
		<title>My Page</title>
		<%= Html.Scripta("my-page").Css()%>
	</head>
	<body>
		....
		<%= Html.Scripta("jquery").Js()%>
		<%= Html.Scripta("my-page").Js()%>
	</body>
	
##Advanced configuration##

	You can add the *settings* to enable extra config options
	
	*version - Use this to append a v=[version] to the end of each script url. 
	*cdn1,cdn2 - Placeholders for using CDNs
	
	<sectionGroup name="scripta">
		<section name="settings" type="Scripta.SettingsConfig, MyClassLibDllName"/>
		<section name="scripts" type="Scripta.ScriptsConfig, MyClassLibDllName"/>
	</sectionGroup>
	
	<scripta>
	   <settings version="1" cdn1="https://my.cdn.com/" cdn2=""/>
	   <scripts>
	      <bundles>
	         <bundle name="my-page">
	            <debug>
	               <file src="/my-mvc-site/static/js/blah.js"/>
	            </debug>
	            <prod>
	               <file src="[cdn1]/blah-min.js"/>						
	            </prod>
	         </bundle>

##Keep it tidy##			 
	It's recommened to put the scripts section in a separate config file. This keeps your web.config
	clean and also makes it easier if you have multiple web.config files for different deployments
	but only want to maintain 1 version of your Scripta config.
	
	<scripta>
	   <settings version="1" cdn1="https://my.cdn.com/" cdn2=""/>
	   <scripts configSource="script.config" />		
	
	


