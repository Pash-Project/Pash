
Public Class TestWriteOnlyParameterizedProperty

    Private _fileNames As Dictionary(Of Integer, String)

    Public Sub New()
        _fileNames = New Dictionary(Of Integer, String)
    End Sub

    Public Function GetFileName(ByVal index As Integer) As String
        Return _fileNames(index)
    End Function

    Public WriteOnly Property FileNames(index As Integer) As String
        Set(value As String)
            _fileNames(index) = value
        End Set
    End Property
End Class
