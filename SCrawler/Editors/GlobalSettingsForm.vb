﻿' Copyright (C) 2022  Andy
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
'
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY
Imports PersonalUtilities.Forms
Imports PersonalUtilities.Forms.Controls.Base
Imports PersonalUtilities.Forms.Toolbars
Namespace Editors
    Friend Class GlobalSettingsForm : Implements IOkCancelToolbar
        Private ReadOnly MyDefs As DefaultFormProps(Of FieldsChecker)
        Friend Sub New()
            InitializeComponent()
            MyDefs = New DefaultFormProps(Of FieldsChecker)
        End Sub
        Private Sub GlobalSettingsForm_Load(sender As Object, e As EventArgs) Handles Me.Load
            Try
                With MyDefs
                    .MyViewInitialize(Me, Settings.Design, True)
                    .AddOkCancelToolbar()
                    .DelegateClosingChecker()
                    With Settings
                        'Basis
                        TXT_GLOBAL_PATH.Text = .GlobalPath.Value
                        TXT_IMAGE_LARGE.Value = .MaxLargeImageHeigh.Value
                        TXT_IMAGE_SMALL.Value = .MaxSmallImageHeigh.Value
                        TXT_COLLECTIONS_PATH.Text = .CollectionsPath
                        TXT_MAX_JOBS_USERS.Value = .MaxUsersJobsCount.Value
                        TXT_MAX_JOBS_CHANNELS.Value = .ChannelsMaxJobsCount.Value
                        CH_CHECK_VER_START.Checked = .CheckUpdatesAtStart
                        'Defaults
                        CH_SEPARATE_VIDEO_FOLDER.Checked = .SeparateVideoFolder.Value
                        CH_DEF_TEMP.Checked = .DefaultTemporary
                        CH_DOWN_IMAGES.Checked = .DefaultDownloadImages
                        CH_DOWN_VIDEOS.Checked = .DefaultDownloadVideos
                        'Channels
                        TXT_CHANNELS_ROWS.Value = .ChannelsImagesRows.Value
                        TXT_CHANNELS_COLUMNS.Value = .ChannelsImagesColumns.Value
                        TXT_CHANNEL_USER_POST_LIMIT.Value = .FromChannelDownloadTop.Value
                        TXT_CHANNEL_USER_POST_LIMIT.Checked = .FromChannelDownloadTopUse.Value
                        CH_COPY_CHANNEL_USER_IMAGE.Checked = .FromChannelCopyImageToUser
                        CH_CHANNELS_USERS_TEMP.Checked = .ChannelsDefaultTemporary
                        'Channels filenames
                        CH_FILE_NAME_CHANGE.Checked = .FileReplaceNameByDate Or .FileAddDateToFileName Or .FileAddTimeToFileName
                        OPT_FILE_NAME_REPLACE.Checked = .FileReplaceNameByDate
                        OPT_FILE_NAME_ADD_DATE.Checked = Not .FileReplaceNameByDate
                        CH_FILE_DATE.Checked = .FileAddDateToFileName
                        CH_FILE_TIME.Checked = .FileAddTimeToFileName
                        OPT_FILE_DATE_START.Checked = Not .FileDateTimePositionEnd
                        OPT_FILE_DATE_END.Checked = .FileDateTimePositionEnd
                        'Reddit
                        SetChecker(CH_REDDIT_TEMP, .RedditTemporary)
                        SetChecker(CH_REDDIT_DOWN_IMG, .RedditDownloadImages)
                        SetChecker(CH_REDDIT_DOWN_VID, .RedditDownloadVideos)
                        'Twitter
                        SetChecker(CH_TWITTER_TEMP, .TwitterTemporary)
                        SetChecker(CH_TWITTER_DOWN_IMG, .TwitterDownloadImages)
                        SetChecker(CH_TWITTER_DOWN_VID, .TwitterDownloadVideos)
                        CH_TWITTER_USER_MEDIA.Checked = .TwitterDefaultGetUserMedia
                    End With
                    .MyFieldsChecker = New FieldsChecker
                    With .MyFieldsChecker
                        .AddControl(Of String)(TXT_GLOBAL_PATH, TXT_GLOBAL_PATH.CaptionText)
                        .AddControl(Of String)(TXT_COLLECTIONS_PATH, TXT_COLLECTIONS_PATH.CaptionText)
                        .EndLoaderOperations()
                    End With
                    .AppendDetectors()
                    .EndLoaderOperations()
                    ChangeFileNameChangersEnabling()
                End With
            Catch ex As Exception
                MyDefs.InvokeLoaderError(ex)
            End Try
        End Sub
        Private Sub SetChecker(ByRef CH As CheckBox, ByVal Prop As XML.Base.XMLValue(Of Boolean))
            If Prop.ValueF.Exists Then
                CH.Checked = Prop.Value
            Else
                CH.CheckState = CheckState.Indeterminate
            End If
        End Sub
        Private Sub SetPropByChecker(ByRef Prop As XML.Base.XMLValue(Of Boolean), ByRef CH As CheckBox)
            Select Case CH.CheckState
                Case CheckState.Checked : Prop.Value = True
                Case CheckState.Unchecked : Prop.Value = False
                Case CheckState.Indeterminate : Prop.ValueF = Nothing
            End Select
        End Sub
        Private Sub ToolbarBttOK() Implements IOkCancelToolbar.ToolbarBttOK
            If MyDefs.MyFieldsChecker.AllParamsOK Then
                With Settings
                    Dim a As Func(Of String, Object, Integer) =
                        Function(t, v) MsgBoxE({$"You are set up higher than default count of along {t} downloading tasks." & vbNewLine &
                                                $"Default: {SettingsCLS.DefaultMaxDownloadingTasks}" & vbNewLine &
                                                $"Your value: {CInt(v)}" & vbNewLine &
                                                "Increasing this value may lead to higher CPU usage." & vbNewLine &
                                                "Do you really want to continue?",
                                                "Increasing download tasks"},
                                               vbExclamation,,, {"Confirm", $"Set to default ({SettingsCLS.DefaultMaxDownloadingTasks})", "Cancel"})
                    If CInt(TXT_MAX_JOBS_USERS.Value) > SettingsCLS.DefaultMaxDownloadingTasks Then
                        Select Case a.Invoke("users", TXT_MAX_JOBS_USERS.Value)
                            Case 1 : TXT_MAX_JOBS_USERS.Value = SettingsCLS.DefaultMaxDownloadingTasks
                            Case 2 : Exit Sub
                        End Select
                    End If
                    If CInt(TXT_MAX_JOBS_CHANNELS.Value) > SettingsCLS.DefaultMaxDownloadingTasks Then
                        Select Case a.Invoke("channels", TXT_MAX_JOBS_CHANNELS.Value)
                            Case 1 : TXT_MAX_JOBS_CHANNELS.Value = SettingsCLS.DefaultMaxDownloadingTasks
                            Case 2 : Exit Sub
                        End Select
                    End If

                    If CH_FILE_NAME_CHANGE.Checked And (Not CH_FILE_DATE.Checked Or Not CH_FILE_TIME.Checked) Then
                        MsgBoxE({"You must select at least one option (Date and/or Time) if you want to change file names by date or disable file names changes",
                                 "File name options"}, vbCritical)
                        Exit Sub
                    End If

                    .BeginUpdate()
                    'Basis
                    .GlobalPath.Value = TXT_GLOBAL_PATH.Text
                    .MaxLargeImageHeigh.Value = CInt(TXT_IMAGE_LARGE.Value)
                    .MaxSmallImageHeigh.Value = CInt(TXT_IMAGE_SMALL.Value)
                    .CollectionsPath.Value = TXT_COLLECTIONS_PATH.Text
                    .MaxUsersJobsCount.Value = CInt(TXT_MAX_JOBS_USERS.Value)
                    .ChannelsMaxJobsCount.Value = TXT_MAX_JOBS_CHANNELS.Value
                    .CheckUpdatesAtStart.Value = CH_CHECK_VER_START.Checked
                    'Defaults
                    .SeparateVideoFolder.Value = CH_SEPARATE_VIDEO_FOLDER.Checked
                    .DefaultTemporary.Value = CH_DEF_TEMP.Checked
                    .DefaultDownloadImages.Value = CH_DOWN_IMAGES.Checked
                    .DefaultDownloadVideos.Value = CH_DOWN_VIDEOS.Checked
                    'Channels
                    .ChannelsImagesRows.Value = CInt(TXT_CHANNELS_ROWS.Value)
                    .ChannelsImagesColumns.Value = CInt(TXT_CHANNELS_COLUMNS.Value)
                    .FromChannelDownloadTop.Value = CInt(TXT_CHANNEL_USER_POST_LIMIT.Value)
                    .FromChannelDownloadTopUse.Value = TXT_CHANNEL_USER_POST_LIMIT.Checked
                    .FromChannelCopyImageToUser.Value = CH_COPY_CHANNEL_USER_IMAGE.Checked
                    .ChannelsDefaultTemporary.Value = CH_CHANNELS_USERS_TEMP.Checked

                    If CH_FILE_NAME_CHANGE.Checked Then
                        .FileReplaceNameByDate.Value = OPT_FILE_NAME_REPLACE.Checked
                        .FileAddDateToFileName.Value = CH_FILE_DATE.Checked
                        .FileAddTimeToFileName.Value = CH_FILE_TIME.Checked
                        .FileDateTimePositionEnd.Value = OPT_FILE_DATE_END.Checked
                    Else
                        .FileAddDateToFileName.Value = False
                        .FileAddTimeToFileName.Value = False
                        .FileReplaceNameByDate.Value = False
                    End If
                    'Reddit
                    SetPropByChecker(.RedditTemporary, CH_REDDIT_TEMP)
                    SetPropByChecker(.RedditDownloadImages, CH_REDDIT_DOWN_IMG)
                    SetPropByChecker(.RedditDownloadVideos, CH_REDDIT_DOWN_VID)
                    'Twitter
                    SetPropByChecker(.TwitterTemporary, CH_TWITTER_TEMP)
                    SetPropByChecker(.TwitterDownloadImages, CH_TWITTER_DOWN_IMG)
                    SetPropByChecker(.TwitterDownloadVideos, CH_TWITTER_DOWN_VID)
                    .TwitterDefaultGetUserMedia.Value = CH_TWITTER_USER_MEDIA.Checked

                    .EndUpdate()
                End With
                MyDefs.CloseForm()
            End If
        End Sub
        Private Sub ToolbarBttCancel() Implements IOkCancelToolbar.ToolbarBttCancel
            MyDefs.CloseForm(DialogResult.Cancel)
        End Sub
        Private Sub TXT_GLOBAL_PATH_ActionOnButtonClick(ByVal Sender As ActionButton) Handles TXT_GLOBAL_PATH.ActionOnButtonClick
            If Sender.DefaultButton = ActionButton.DefaultButtons.Open Then
                Dim f As SFile = SFile.SelectPath(Settings.GlobalPath.Value)
                If Not f.IsEmptyString Then TXT_GLOBAL_PATH.Text = f
            End If
        End Sub
        Private Sub TXT_MAX_JOBS_USERS_ActionOnButtonClick(ByVal Sender As ActionButton) Handles TXT_MAX_JOBS_USERS.ActionOnButtonClick
            If Sender.DefaultButton = ActionButton.DefaultButtons.Refresh Then TXT_MAX_JOBS_USERS.Value = SettingsCLS.DefaultMaxDownloadingTasks
        End Sub
        Private Sub TXT_MAX_JOBS_CHANNELS_ActionOnButtonClick(ByVal Sender As ActionButton) Handles TXT_MAX_JOBS_CHANNELS.ActionOnButtonClick
            If Sender.DefaultButton = ActionButton.DefaultButtons.Refresh Then TXT_MAX_JOBS_CHANNELS.Value = SettingsCLS.DefaultMaxDownloadingTasks
        End Sub
        Private Sub CH_FILE_NAME_CHANGE_CheckedChanged(sender As Object, e As EventArgs)
            ChangeFileNameChangersEnabling()
        End Sub
        Private Sub OPT_FILE_NAME_REPLACE_CheckedChanged(sender As Object, e As EventArgs)
            ChangePositionControlsEnabling()
        End Sub
        Private Sub OPT_FILE_NAME_ADD_DATE_CheckedChanged(sender As Object, e As EventArgs)
            ChangePositionControlsEnabling()
        End Sub
        Private Sub ChangePositionControlsEnabling()
            Dim b As Boolean = OPT_FILE_NAME_ADD_DATE.Checked And OPT_FILE_NAME_ADD_DATE.Enabled
            OPT_FILE_DATE_START.Enabled = b
            OPT_FILE_DATE_END.Enabled = b
        End Sub
        Private Sub ChangeFileNameChangersEnabling()
            Dim b As Boolean = CH_FILE_NAME_CHANGE.Checked
            OPT_FILE_NAME_REPLACE.Enabled = b
            OPT_FILE_NAME_ADD_DATE.Enabled = b
            CH_FILE_DATE.Enabled = b
            CH_FILE_TIME.Enabled = b
            ChangePositionControlsEnabling()
        End Sub
    End Class
End Namespace