
Imports System
Imports System.Windows.Forms
Imports WeatherDesktop.[Interface]
Imports System.ComponentModel.Composition
Imports WeatherDesktop.Share

Namespace ExternalService.template

    <Export(GetType(IsharedSunRiseSetInterface))>
    <ExportMetadata("ClassName", "C# External Templete")>
    Class SunRiseSetClass
        Implements IsharedSunRiseSetInterface

        Private Function Debug() As String Implements IsharedSunRiseSetInterface.Debug
            'SharedObjects.CompileDebug - will take a dictionary and convert it to an array of key: Value strings
            Return "throw new NotImplementedException()"
        End Function

        Private Function Invoke() As ISharedResponse Implements IsharedSunRiseSetInterface.Invoke
            'If SharedObjects.Cache.Exists("key") Then Return DirectCast(SharedObjects.Cache.Value("key"), SunRiseSetResponse)
            'Dim response As String = SharedObjects.CompressedCallSite("url")
            'Dim ResponseObject As SunRiseSetResponse = Transform(response)
            'SharedObjects.Cache.Set("key", ResponseObject, 60)
            'Return ResponseObject
            Return New SunRiseSetResponse()
        End Function

        Private Sub Load() Implements IsharedSunRiseSetInterface.Load
            'Acts like the class' create method, to prevent lazy load from creating new objects for no reason
        End Sub

        Private Function SettingsItems() As MenuItem() Implements IsharedSunRiseSetInterface.SettingsItems
            'Most weather objects need a zipcode
            'SharedObjects.ZipObjects.ZipMenuItem - provides an easy way to edit zip records after load
            'SharedObjects.ZipObjects.TryGetZip - provides a popup if the zip isn't loaded and saves, otherwise will provide zip thats saved
            'SharedObjects.ZipObjects.GetZip will generate the popup to enter zip. will Not provide zip if already loaded  
            Return New MenuItem() {}
        End Function

        Private Function ThrownException() As Exception Implements IsharedSunRiseSetInterface.ThrownException
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace



