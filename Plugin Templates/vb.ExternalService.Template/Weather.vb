Imports System
Imports System.Windows.Forms
Imports WeatherDesktop.Interface
Imports System.ComponentModel.Composition
Imports WeatherDesktop.Share

Namespace ExternalService.template

    <Export(GetType(ISharedWeatherinterface))>
    <ExportMetadata("ClassName", "vb External Templete")>
    Public Class WeatherSource
        Implements ISharedWeatherinterface

        Private Function Debug() As String Implements ISharedWeatherinterface.Debug
            'SharedObjects.CompileDebug - will take a dictionary And convert it to an array of key: Value Strings
            Return "throw new NotImplementedException()"
        End Function

        Private Function Invoke() As ISharedResponse Implements ISharedWeatherinterface.Invoke
            'If SharedObjects.Cache.Exists("key") Then Return DirectCast(SharedObjects.Cache.Value("key"), WeatherResponse)
            'Dim response As String = SharedObjects.CompressedCallSite("url")
            'Dim ResponseObject As WeatherResponse = Transform(response)
            'SharedObjects.Cache.Set("key", ResponseObject, 60)
            'Return ResponseObject
            Return New WeatherResponse()
        End Function

        Private Sub Load() Implements ISharedWeatherinterface.Load
            'Acts like the class' create method, to prevent lazy load from creating new objects for no reason
        End Sub

        Private Function SettingsItems() As MenuItem() Implements ISharedWeatherinterface.SettingsItems
            'Most weather objects need a zipcode
            'SharedObjects.ZipObjects.ZipMenuItem - provides an easy way to edit zip records after load
            'SharedObjects.ZipObjects.TryGetZip - provides a popup if the zip isn't loaded and saves, otherwise will provide zip thats saved
            'SharedObjects.ZipObjects.GetZip will generate the popup to enter zip. will Not provide zip if already loaded  
            Return New MenuItem() {}
        End Function

        Private Function ThrownException() As Exception Implements ISharedWeatherinterface.ThrownException
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace
