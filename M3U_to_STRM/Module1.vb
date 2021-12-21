Imports System.IO
Imports System.Net
Imports System.Text.RegularExpressions

Module Module1

    Sub Main()
        Try
            Console.WriteLine("© by TimTester visit https://TimTester.in")

            Dim counter_Filme As Integer = 0
            Dim counter_Serien As Integer = 0
            Dim counter_TV As Integer = 0

            Dim Main_folder As String = System.AppDomain.CurrentDomain.BaseDirectory
            Dim conf_file As String = Main_folder + "\M3U_to_STRM.conf"
            Dim local_m3u As String = Main_folder + "\M3U.m3u"

            Console.WriteLine("Reading config")
            If My.Computer.FileSystem.FileExists(conf_file) = False Then
                Console.ForegroundColor = ConsoleColor.Red
                Console.WriteLine("Config file missing!")
                Console.WriteLine("Please create the config file: " + conf_file)
                Console.WriteLine("And paste the M3U download URL in the config file")
                Console.ForegroundColor = ConsoleColor.White
            Else
                Console.WriteLine("Downloading M3U")
                Dim M3U_URL As String = My.Computer.FileSystem.ReadAllText(conf_file)
                If My.Computer.FileSystem.FileExists(local_m3u) Then
                    My.Computer.FileSystem.DeleteFile(local_m3u)
                End If

                'My.Computer.Network.DownloadFile(M3U_URL, local_m3u)

                Dim downloader_M3U As WebClient
                downloader_M3U = New WebClient
                downloader_M3U.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)")
                downloader_M3U.DownloadFile(New Uri(M3U_URL), local_m3u)


                Console.WriteLine("Download complete")
            End If



            'Datei lesen
            Dim folder_Filme As String = Main_folder + "\Filme"
            Dim folder_Serien As String = Main_folder + "\Serien"
            Dim folder_TV As String = Main_folder + "\TV"
            'Verzeichnisse vorher leeren
            Console.WriteLine("Delete (Y/N)?")
            Console.WriteLine("If no entry is made for 20 seconds, the system continues without deleting.")
            Console.WriteLine("Please enter Y or N.!")
            Dim del As String = Reader.ReadLine(20000)
            If del.ToLower = "y" Then
                If My.Computer.FileSystem.DirectoryExists(folder_Filme) Then
                    My.Computer.FileSystem.DeleteDirectory(folder_Filme, FileIO.DeleteDirectoryOption.DeleteAllContents)
                End If
                If My.Computer.FileSystem.DirectoryExists(folder_Serien) Then
                    My.Computer.FileSystem.DeleteDirectory(folder_Serien, FileIO.DeleteDirectoryOption.DeleteAllContents)
                End If
                If My.Computer.FileSystem.DirectoryExists(folder_TV) Then
                    My.Computer.FileSystem.DeleteDirectory(folder_TV, FileIO.DeleteDirectoryOption.DeleteAllContents)
                End If
            End If

            'Verzeichnisse erstellen
            My.Computer.FileSystem.CreateDirectory(folder_Filme)
            My.Computer.FileSystem.CreateDirectory(folder_Serien)
            My.Computer.FileSystem.CreateDirectory(folder_TV)

            'For Each f As String In My.Computer.FileSystem.GetFiles(Main_folder)
            'M3U
            If My.Computer.FileSystem.FileExists(local_m3u) Then
                Dim FileText As String = My.Computer.FileSystem.ReadAllText(local_m3u)


                Dim FileText_lines As String() = FileText.Split(vbLf)
                For index = 1 To FileText_lines.Length - 1
                    Dim line1 As String = FileText_lines(index)
                    If line1.ToLower.StartsWith("#extinf:") Then

                        Dim line2 As String = FileText_lines(index + 1)

                        Dim NAME As String = ""
                        Dim URL As String = ""

                        Dim Start_Name As Integer = line1.IndexOf("tvg-name=") + 10
                        Dim Start_Logo As Integer = line1.IndexOf("tvg-logo=") + 10
                        'Name
                        NAME = line1.Substring(Start_Name, Start_Logo - Start_Name - 12).Replace("&", "")
                        'URL
                        URL = line2.Replace(vbCrLf, "").Replace(vbCr, "")
                        URL = URL

                        'Illegale Charakter entfernen
                        NAME = RemoveIllegalFileNameChars(NAME).Trim()

                        'Gruppe aus URL extrahieren
                        Dim GROUP As String = ""
                        If URL.ToLower.Contains("/serie") Then
                            GROUP = "serien"
                        End If
                        If URL.ToLower.Contains("/movie") Then
                            GROUP = "filme"
                        End If
                        'Prüfen ob Serie oder Film
                        Dim folder As String = ""
                        If GROUP.ToLower.Contains("serien") Then
                            'Serien zusammenfassen
                            folder = folder_Serien & "\" & NAME
                            folder = Regex.Replace(folder, "s(\d+) e(\d+)", "", RegexOptions.IgnoreCase).Trim
                            folder = Regex.Replace(folder, "s(\d+)e(\d+)", "", RegexOptions.IgnoreCase).Trim
                            Console.WriteLine("series: " + NAME + " found")
                            counter_Serien = counter_Serien + 1
                        End If
                        If GROUP.ToLower.Contains("filme") Or GROUP.ToLower.Contains("ultra hd") Then
                            folder = folder_Filme & "\" & NAME
                            Console.WriteLine("movie: " + NAME + " found")
                            counter_Filme = counter_Filme + 1
                        End If
                        If folder = "" Then
                            folder = folder_TV & "\" & counter_TV & " " & NAME
                            NAME = counter_TV & " " & NAME
                            Console.WriteLine("TV program: " & NAME & " found")
                            counter_TV = counter_TV + 1
                        End If

                        folder = folder.Trim()
                        'Ordner erstellen
                        If My.Computer.FileSystem.DirectoryExists(folder) = False Then
                            My.Computer.FileSystem.CreateDirectory(folder)
                        End If
                        '.strm Datei erstellen
                        If My.Computer.FileSystem.FileExists(folder & "\" & NAME & ".strm") = False Then
                            My.Computer.FileSystem.WriteAllText(folder & "\" & NAME & ".strm", URL, False, Text.Encoding.ASCII)
                        End If

                    Else
                        Continue For
                    End If
                Next

                Console.WriteLine("")
                Console.WriteLine("")
                Console.WriteLine(counter_Filme & " movies found")
                Console.WriteLine(counter_Serien & " series found")
                Console.WriteLine(counter_TV & " TV programs found")
                Console.WriteLine("")
                Console.WriteLine("")



            End If
            'Next


            Console.WriteLine("© by TimTester visit https://TimTester.in")

            Console.WriteLine("The program will automatically close in 20 seconds")
            Reader.ReadLine(20000)


        Catch ex As Exception
            Console.WriteLine("Fehler: " & ex.Message)
            Console.WriteLine("Fehler: " & ex.StackTrace)
            Console.ReadLine()
        End Try




    End Sub

    Public Function RemoveIllegalFileNameChars(input As String, Optional replacement As String = "") As String
        input = input.Replace("\", "")
        Dim regexSearch = New String(Path.GetInvalidFileNameChars()) & New String(Path.GetInvalidPathChars())
        Dim r = New Regex(String.Format("[{0}]", Regex.Escape(regexSearch)))
        Return r.Replace(input, replacement).Replace(".", "").Replace("[", "").Replace("]", "")
    End Function

End Module
