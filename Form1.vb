Imports MySql.Data.MySqlClient
Public Class Form1
    Dim months() As String = {"January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"}
    Dim monthdays() As Integer = {31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31}
    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        If testmysqlconnection() = True Then
            Dim cmd As New MySqlCommand
            cmd.CommandText = "DROP TABLE Events"
            cmd.CommandType = CommandType.Text
            cmd.Connection = connection
            cmd.ExecuteNonQuery()
            cmd.CommandText = "CREATE TABLE Events(id BIGINT NOT NULL AUTO_INCREMENT,Type CHAR(5),Time CHAR(2),Date TEXT,Description TEXT,PRIMARY KEY (id))"
            cmd.ExecuteNonQuery()
            For m = 0 To 11
                For d = 1 To monthdays(m)
                    Dim s As String = "http://en.wikipedia.org/wiki/" & months(m) & "_" & d
                    trawl(s, m, d)
A:
                    Console.WriteLine(constantmonthlength(m) & " " & constantdaylength(d))
                Next
            Next
            Dim q As String = ""
            For Each l In notworkinglinks
                q = l & vbNewLine
            Next
            MsgBox(q)
        End If
    End Sub
    Dim lastyear As String = ""
    Dim notworkinglinks As New List(Of String)
    Sub trawl(ByVal website As String, ByVal month As String, ByVal day As String)
        'Dim html As String = New System.Net.WebClient().DownloadString(website)
        Dim html As String = My.Computer.FileSystem.ReadAllText(website)
        Dim eventindex As Integer = html.IndexOf("mw-headline")
        Dim birthindex As Integer = html.IndexOf("mw-headline", eventindex + 12)
        Dim deathindex As Integer = html.IndexOf("mw-headline", birthindex + 12)

        Dim liindex As New List(Of Integer)
        Dim temp As Integer = 0
        While temp <> -1
            temp = html.IndexOf("<li>", temp + 1)
            liindex.Add(temp)
        End While
        For i = 0 To liindex.Count - 1
            If (liindex(i) > eventindex) And (liindex(i) < html.IndexOf("mw-headline", deathindex + 12)) Then
                Dim s As String = html.Substring(liindex(i) + 4, html.IndexOf("</li>", liindex(i) + 4) - (liindex(i) + 4))

                Dim type As String = IIf(liindex(i) > deathindex, "Death", IIf(liindex(i) > birthindex, "Birth", "Event"))
                Dim year As String = ""
                Dim description As String = ""
                If IsNumeric(s.Substring(0, 1)) = True Then
                    For r = 0 To 3
                        Do Until IsNumeric(s.Substring(r, r + 1 - (r))) = False
                            year = year & s.Substring(r, r + 1 - (r))
                            r += 1
                        Loop
                    Next
                    description = s.Substring(s.IndexOf(">") + 1)
                Else
                    year = s.Substring(s.IndexOf("title") + 7, s.IndexOf(Chr(34), s.IndexOf("title") + 7) - (s.IndexOf("title") + 7))
                    If year.Contains(" BC") Then
                    Else
                        If IsNumeric(year) = False Then
                            year = lastyear
                        End If
                    End If
                    lastyear = year
                    Try
                        'description = s.Substring(s.IndexOf("</a>") + 9)
                        description = s.Substring(s.IndexOf("</a>") + 7)
                    Catch
                        'description = s.Substring(s.IndexOf(">") + 9)
                        description = s.Substring(s.IndexOf(">") + 1)
                    End Try
                End If
                Do Until description.Contains("<a href") = False
                    Dim opening As Integer = description.IndexOf("<a href")
                    Dim closing As Integer = description.IndexOf(">", opening + 1) + 1
                    description = description.Remove(opening, closing - opening)
                Loop
                Do Until description.Contains("</a>") = False
                    description = description.Remove(description.IndexOf("</a>"), 4)
                Loop
                Dim adbc As String = IIf(year.Contains("BC"), "BC", "AD")
                If year.Contains("BC") = True Then
                    Dim index As Integer = year.IndexOf("BC") - 1
                    year = year.Remove(index)
                End If
                description = System.Security.SecurityElement.Escape(description)

                Dim cmd As New MySqlCommand
                cmd.CommandText = "INSERT INTO Events(Type,Time,Date,Description) VALUES ('" & type & "','" & adbc & "','" & constantyearlength(year) & "-" & constantmonthlength(month + 1) & "-" & constantdaylength(day) & "','" & description & "')"
                cmd.CommandType = CommandType.Text
                cmd.Connection = connection
                cmd.ExecuteNonQuery()
            End If
        Next
B:
    End Sub
    Function constantyearlength(ByVal year As String)
        Dim x As String = ""
        If year.Length < 4 Then
            Dim l As Integer = year.Length
            For i = 1 To 4 - l
                x = x & "0"
            Next
            x = x & year
        Else
            x = year
        End If
        Return x
    End Function
    Function constantmonthlength(ByVal month As String)
        Dim x As String = ""
        If month.ToString.Length < 2 Then
            x = "0" & month
        Else
            x = month
        End If
        Return x
    End Function
    Function constantdaylength(ByVal day As String)
        Dim x As String = ""
        If day.ToString.Length < 2 Then
            x = "0" & day
        Else
            x = day
        End If
        Return x
    End Function
    Function getmonth(ByVal month As String)
        Dim x As String = ""
        If month = "January" Then
            x = "01"
        End If
        If month = "February" Then
            x = "02"
        End If
        If month = "March" Then
            x = "03"
        End If
        If month = "April" Then
            x = "04"
        End If
        If month = "May" Then
            x = "05"
        End If
        If month = "June" Then
            x = "06"
        End If
        If month = "July" Then
            x = "07"
        End If
        If month = "August" Then
            x = "08"
        End If
        If month = "September" Then
            x = "09"
        End If
        If month = "October" Then
            x = "10"
        End If
        If month = "November" Then
            x = "11"
        End If
        If month = "December" Then
            x = "12"
        End If
        Return x
    End Function
    Dim connection As MySqlConnection
    Public Function testmysqlconnection()
        Dim conn As String = "Database=t;Data Source=localhost;User Id=root;Password=root"
        connection = New MySqlConnection(conn)
        Try
            connection.Open()
            Return True
        Catch
            Return False
        End Try
    End Function
End Class
